using Forth.Core.Binding;
using Forth.Core.Execution;
using System.Collections.Immutable; // added
using System.Reflection;
using System.Text;
using System.Globalization;
using System.IO;

namespace Forth.Core.Interpreter;

public class ForthInterpreter : IForthInterpreter
{
    internal readonly ForthStack _stack = new();
    internal readonly ForthStack _rstack = new();
    internal ImmutableDictionary<(string? Module, string Name), Word> _dict = CorePrimitives.Words;
    private readonly IForthIO _io;
    private bool _exitRequested;
    internal bool _isCompiling; // made internal
    internal string? _currentDefName; // internal
    internal List<Func<ForthInterpreter, Task>>? _currentInstructions; // internal
    internal readonly Stack<CompileFrame> _controlStack = new(); // internal
    internal readonly Dictionary<long,long> _mem = new(); // internal
    internal long _nextAddr = 1; // internal

    // File handle table
    private readonly Dictionary<int, FileStream> _openFiles = new();
    private int _nextFileHandle = 1;

    public enum FileOpenMode
    {
        Read = 0,
        Write = 1,
        Append = 2
    }

    internal int OpenFileHandle(string path, FileOpenMode mode = FileOpenMode.Read)
    {
        FileStream fs;
        // Use FileShare.ReadWrite for read streams to allow external inspection during tests
        switch (mode)
        {
            case FileOpenMode.Read:
                // Allow other processes to read/write while we have it open for read
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                break;
            case FileOpenMode.Write:
                // Create or truncate for write; allow readers and writers, use write-through
                fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                // Ensure we start at beginning
                fs.Seek(0, SeekOrigin.Begin);
                break;
            case FileOpenMode.Append:
                // Append with read/write access and write-through; position at end
                fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                fs.Seek(0, SeekOrigin.End);
                break;
            default:
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                break;
        }

        var h = _nextFileHandle++;
        _openFiles[h] = fs;
        return h;
    }

    internal void CloseFileHandle(int handle)
    {
        if (!TryCloseFileHandle(handle))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
    }

    internal bool TryCloseFileHandle(int handle)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            return false;
        try { fs.Flush(); fs.Dispose(); } catch { }
        _openFiles.Remove(handle);
        return true;
    }

    internal void RepositionFileHandle(int handle, long offset)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (offset < 0 || offset > fs.Length) throw new ForthException(ForthErrorCode.CompileError, "Invalid file offset");
        fs.Seek(offset, SeekOrigin.Begin);
    }

    // Read up to 'count' bytes from handle into memory starting at addr. Returns number of bytes read.
    internal int ReadFileIntoMemory(int handle, long addr, int count)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanRead) throw new ForthException(ForthErrorCode.CompileError, "File not open for reading");
        if (count <= 0) return 0;

        var buffer = new byte[count];
        int read = fs.Read(buffer, 0, count);
        for (int i = 0; i < read; i++)
        {
            _mem[addr + i] = buffer[i];
        }
        return read;
    }

    // Write 'count' bytes from memory starting at addr to handle. Returns number of bytes written.
    internal int WriteMemoryToFile(int handle, long addr, int count)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanWrite) throw new ForthException(ForthErrorCode.CompileError, "File not open for writing");
        if (count <= 0) return 0;

        var buffer = new byte[count];
        for (int i = 0; i < count; i++)
        {
            MemTryGet(addr + i, out var v);
            buffer[i] = (byte)v;
        }
        // Ensure write goes to current position (caller controls positioning via REPOSITION-FILE)
        fs.Write(buffer, 0, count);
        try { fs.Flush(true); } catch { fs.Flush(); }
        return count;
    }

    internal readonly Dictionary<string,long> _values = new(StringComparer.OrdinalIgnoreCase); // internal
    internal readonly List<string> _usingModules = new(); // internal
    internal string? _currentModule; // internal

    private readonly ControlFlowRuntime _controlFlow = new();

    private readonly long _stateAddr;
    internal long StateAddr =>
        _stateAddr;
    private readonly long _baseAddr;
    internal long BaseAddr =>
        _baseAddr;

    private StringBuilder? _picBuf;
    internal readonly Dictionary<string, Word?> _deferred = new(StringComparer.OrdinalIgnoreCase); // internal
    internal readonly Dictionary<string, string> _decompile = new(StringComparer.OrdinalIgnoreCase); // internal
    internal List<string>? _currentDefTokens; // internal
    internal Word? _lastDefinedWord; // internal

    // CREATE ... DOES> tracking
    internal string? _lastCreatedName; // internal
    internal long _lastCreatedAddr; // internal
    internal bool _doesCollecting; // internal
    internal List<string>? _doesTokens; // internal

    internal List<string>? _tokens; // internal current token stream
    internal int _tokenIndex;       // internal current token index

    // definition order tracking for classic FORGET
    private readonly List<DefinitionRecord> _definitions = new();
    private int _baselineCount; // protect core/compiler words
    private readonly Task _loadPrelude;

    private readonly struct DefinitionRecord
    {
        public readonly string Name;
        public readonly string? Module;
        public DefinitionRecord(string name, string? module)
        {
            Name = name;
            Module = module;
        }
    }

    internal void RegisterDefinition(string name) =>
        _definitions.Add(new DefinitionRecord(name, _currentModule));

    internal bool TryReadNextToken(out string token)
    {
        token = string.Empty;
        if (_tokens is null)
        {
            return false;
        }

        while (_tokenIndex < _tokens.Count)
        {
            var t = _tokens[_tokenIndex++];
            if (t.Length == 0)
            {
                continue;
            }

            token = t;
            return true;
        }

        return false;
    }

    internal string ReadNextTokenOrThrow(string context)
    {
        if (!TryReadNextToken(out var t))
        {
            throw new ForthException(ForthErrorCode.CompileError, context);
        }

        return t;
    }

    // Definition helpers used by immediate words
    internal void BeginDefinition(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ForthException(ForthErrorCode.CompileError, "Invalid word name");
        }

        _currentDefName = name;
        _currentInstructions = new();
        _controlStack.Clear();
        _isCompiling = true;
        _currentDefTokens = new();
        _mem[_stateAddr] = 1;
    }

    internal void FinishDefinition()
    {
        if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
        {
            throw new ForthException(ForthErrorCode.CompileError, "No open definition to end");
        }

        if (_controlStack.Count != 0)
        {
            throw new ForthException(ForthErrorCode.CompileError, "Unmatched control structure");
        }

        var body = _currentInstructions;
        var nameForDecomp = _currentDefName;

        var compiled = new Word(async intr =>
        {
            try
            {
                foreach (var a in body)
                {
                    await a(intr);
                }
            }
            catch (ExitWordException)
            {
            }
        });

        if (_currentDefTokens is { Count: > 0 })
        {
            compiled.BodyTokens = new List<string>(_currentDefTokens);
        }

        compiled.Name = _currentDefName;
        compiled.Module = _currentModule;

        // Store into tuple-keyed global dictionary
        _dict = _dict.SetItem((_currentModule, _currentDefName), compiled);
        RegisterDefinition(_currentDefName);
        _lastDefinedWord = compiled;

        var bodyText = _currentDefTokens is { Count: > 0 }
            ? string.Join(' ', _currentDefTokens)
            : string.Empty;

        if (string.IsNullOrEmpty(bodyText))
        {
            _decompile[nameForDecomp] = $": {nameForDecomp} ;";
        }
        else
        {
            _decompile[nameForDecomp] = $": {nameForDecomp} {bodyText} ;";
        }

        _isCompiling = false;
        _mem[_stateAddr] = 0;
        _currentDefName = null;
        _currentInstructions = null;
        _currentDefTokens = null;
    }

    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();

        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0;

        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = 10;

        // No per-module lazy dictionaries anymore; use tuple-keyed _dict directly
        _baselineCount = _definitions.Count; // record baseline for core/compiler words
        _loadPrelude = LoadPreludeAsync(); // Load pure Forth definitions
    }

    private async Task LoadPreludeAsync()
    {
        // Try to load embedded prelude resource
        var asm = typeof(ForthInterpreter).Assembly;
        var resourceName = "Forth.Core.prelude.4th";
        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new System.IO.StreamReader(stream);
            var prelude = await reader.ReadToEndAsync();
            await LoadPreludeText(prelude);
            return;
        }

        // Fallback: try loading from file system (for development)
        var preludePath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(asm.Location) ?? ".",
            "prelude.4th");

        if (System.IO.File.Exists(preludePath))
        {
            var prelude = await System.IO.File.ReadAllTextAsync(preludePath);
            await LoadPreludeText(prelude);
        }
    }

    private async Task LoadPreludeText(string prelude)
    {
        var lines = prelude.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('\\'))
            {
                continue;
            }

            // Skip inline comments ( ... )
            if (trimmed.StartsWith('(') && trimmed.EndsWith(')'))
            {
                continue;
            }

            await EvalInternalAsync(trimmed);
        }
    }

    internal void ForgetWord(string token)
    {
        string? mod = null;
        string name = token;

        var cidx = token.IndexOf(':');
        if (cidx > 0)
        {
            mod = token[..cidx];
            name = token[(cidx + 1) ..];
        }

        for (int idx = _definitions.Count - 1; idx >= 0; idx--)
        {
            var def = _definitions[idx];
            if (!name.Equals(def.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (mod != null && !string.Equals(mod, def.Module, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (idx < _baselineCount)
            {
                throw new ForthException(ForthErrorCode.CompileError, "Cannot FORGET core word");
            }

            for (int j = _definitions.Count - 1; j >= idx; j--)
            {
                var d = _definitions[j];
                var key = (d.Module, d.Name);
                if (!_dict.ContainsKey(key))
                {
                    // nothing in global dict; continue
                }
                else
                {
                    _dict = _dict.Remove(key);
                }

                _decompile.Remove(d.Name);
                _deferred.Remove(d.Name);
                _values.Remove(d.Name);
            }

            _definitions.RemoveRange(idx, _definitions.Count - idx);
            _lastDefinedWord = null;
            return;
        }

        throw new ForthException(ForthErrorCode.UndefinedWord, $"FORGET target not found: {token}");
    }

    public IReadOnlyList<object> Stack =>
        _stack.AsReadOnly();

    public void Push(object value) =>
        _stack.Push(value);

    public object Pop() =>
        _stack.Pop();

    public object Peek() =>
        _stack.Peek();

    public void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _currentModule };
        _dict = _dict.SetItem((_currentModule, name), w);
        RegisterDefinition(name);
    }

    public void AddWordAsync(string name, Func<IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _currentModule };
        _dict = _dict.SetItem((_currentModule, name), w);
        RegisterDefinition(name);
    }

    internal bool TryResolveWord(string token, out Word? word)
    {
        var idx = token.IndexOf(':');
        if (idx > 0)
        {
            var mod = token[..idx];
            var wname = token[(idx + 1) ..];
            if (_dict.TryGetValue((mod, wname), out var wq))
            {
                word = wq;
                return true;
            }

            word = null;
            return false;
        }

        // Search current module
        if (!string.IsNullOrWhiteSpace(_currentModule)
            && _dict.TryGetValue((_currentModule, token), out var wc))
        {
            word = wc;
            return true;
        }

        // Search using modules in reverse
        for (int i = _usingModules.Count - 1; i >= 0; i--)
        {
            var mn = _usingModules[i];
            if (_dict.TryGetValue((mn, token), out var wu))
            {
                word = wu;
                return true;
            }
        }

        // Try module-less core dictionary entry
        if (_dict.TryGetValue((null, token), out var wdef))
        {
            word = wdef;
            return true;
        }

        word = null;
        return false;
    }

    internal object PopInternal() =>
        Pop();

    internal void EnsureStack(int needed, string word)
    {
        if (_stack.Count < needed)
        {
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
        }
    }

    internal static long ToLongPublic(object v) =>
        ToLong(v);

    private static long ToLong(object v) => v switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        char c => c,
        bool bo => bo ? 1L : 0L,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
    };

    // Return stack helpers
    internal void RPush(object value) =>
        _rstack.Push(value);

    internal object RPop() =>
        _rstack.Pop();

    internal int RCount =>
        _rstack.Count;

    // Peek top of return stack without removing it
    internal object RTop() =>
        _rstack.Peek();

    internal long ValueGet(string name) =>
        _values.TryGetValue(name, out var v) ? v : 0L;

    internal void ValueSet(string name, long v) =>
        _values[name] = v;

    internal void PushLoopIndex(long idx) =>
        _controlFlow.Push(idx);

    internal void PopLoopIndexMaybe() =>
        _controlFlow.PopMaybe();

    internal void Unloop() =>
        _controlFlow.Unloop();

    internal long CurrentLoopIndex() =>
        _controlFlow.Current();

    internal void PicturedBegin() =>
        _picBuf = new StringBuilder();

    internal void PicturedHold(char ch)
    {
        _picBuf ??= new StringBuilder();
        _picBuf.Insert(0, ch);
    }

    internal void PicturedHoldDigit(long digit)
    {
        int d = (int)digit;
        char ch = (char)(d < 10 ? '0' + d : 'A' + (d - 10));
        PicturedHold(ch);
    }

    internal string PicturedEnd()
    {
        var s = _picBuf?.ToString() ?? string.Empty;
        _picBuf = null;
        return s;
    }

    internal IEnumerable<string> GetAllWordNames()
    {
        var names = new List<string>();
        foreach (var kv in _dict)
        {
            var w = kv.Value;

            if (w is null)
            {
                continue;
            }

            if (w.IsHidden)
            {
                continue;
            }

            var n = w.Name ?? kv.Key.Name;

            if (string.IsNullOrWhiteSpace(kv.Key.Module))
            {
                names.Add(n);
            }
            else
            {
                names.Add($"{kv.Key.Module}:{n}");
            }
        }

        names.Sort(StringComparer.OrdinalIgnoreCase);
        return names;
    }

    public async Task<bool> EvalAsync(string line)
    {
        await _loadPrelude;
        return await EvalInternalAsync(line);
    }

    private async Task<bool> EvalInternalAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        _tokens = Tokenizer.Tokenize(line);

        // Preprocess idiomatic compound tokens like "['] name" or "[']name" into
        // the equivalent sequence: "[" "'" name "]" so existing primitives
        // ([, ', ]) handle the compile-time behaviour without adding new words.
        if (_tokens is not null && _tokens.Count > 0)
        {
            var processed = new List<string>();
            for (int ti = 0; ti < _tokens.Count; ti++)
            {
                var t = _tokens[ti];
                if (t == "[']")
                {
                    // If a following token exists, consume it as the target name.
                    if (ti + 1 < _tokens.Count)
                    {
                        processed.Add("[");
                        processed.Add("'");
                        processed.Add(_tokens[ti + 1]);
                        processed.Add("]");
                        ti++; // skip the consumed name
                        continue;
                    }
                    // Fallback: expand to [ and ' tokens and continue
                    processed.Add("[");
                    processed.Add("'");
                    continue;
                }
                processed.Add(t);
            }
            _tokens = processed;
        }

        _tokenIndex = 0;

        // IL fast-path removed; interpret tokens normally
        while (TryReadNextToken(out var tok))
        {
            if (tok.Length == 0)
            {
                continue;
            }

            if (!_isCompiling)
            {
                // Collect tokens for DOES> body when in create-does> sequence
                if (_doesCollecting)
                {
                    _doesTokens!.Add(tok);
                    continue;
                }

                // ABORT with optional quoted message is kept as interpret-time sugar
                if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryReadNextToken(out var maybeMsg) && maybeMsg.Length >= 2 && maybeMsg[0] == '"' && maybeMsg[^1] == '"')
                    {
                        throw new ForthException(ForthErrorCode.Unknown, maybeMsg[1..^1]);
                    }

                    throw new ForthException(ForthErrorCode.Unknown, "ABORT");
                }

                // Bare quoted literal token => push string
                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    Push(tok[1..^1]);
                    continue;
                }

                // Floating-point literal (interpret-time)
                if (TryParseDouble(tok, out var dnum))
                {
                    Push(dnum);
                    continue;
                }

                // Numeric literal (integer)
                if (TryParseNumber(tok, out var num))
                {
                    Push(num);
                    continue;
                }

                // Delegate to dictionary for all other words (including compiler/defining words)
                if (TryResolveWord(tok, out var word) && word is not null)
                {
                    await word.ExecuteAsync(this);
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                // While compiling, record tokens for decompilation (exclude ';')
                if (tok != ";")
                {
                    _currentDefTokens?.Add(tok);
                }

                // End of definition marker
                if (tok == ";")
                {
                    FinishDefinition();
                    continue;
                }

                // Compile-time quoted literal token
                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(tok[1..^1]);
                        return Task.CompletedTask;
                    });

                    continue;
                }

                // Compile-time floating/integer numeric literal
                if (TryParseDouble(tok, out var dlit))
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(dlit);
                        return Task.CompletedTask;
                    });

                    continue;
                }

                if (TryParseNumber(tok, out var lit))
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(lit);
                        return Task.CompletedTask;
                    });

                    continue;
                }

                // Resolve words; immediate words execute now (may manipulate control stack/token stream),
                // non-immediate words are compiled by appending their execution
                if (TryResolveWord(tok, out var cw) && cw is not null)
                {
                    if (cw.IsImmediate)
                    {
                        if (cw.BodyTokens is { Count: > 0 })
                        {
                            _tokens!.InsertRange(_tokenIndex, cw.BodyTokens);
                            continue;
                        }

                        await cw.ExecuteAsync(this);
                        continue;
                    }

                    CurrentList().Add(async intr => await cw.ExecuteAsync(intr));
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }

        if (_doesCollecting && !string.IsNullOrEmpty(_lastCreatedName) && _doesTokens is { Count: > 0 })
        {
            var bodyLine = string.Join(' ', _doesTokens);
            var addr = _lastCreatedAddr;

            var newWord = new Word(async intr =>
            {
                intr.MemTryGet(addr, out var cur);
                intr.Push(addr);
                intr.Push(cur);
                await intr.EvalAsync(bodyLine).ConfigureAwait(false);
            })
            {
                Name = _lastCreatedName,
                Module = _currentModule
            };

            _dict = _dict.SetItem((_currentModule, _lastCreatedName), newWord);
            RegisterDefinition(_lastCreatedName);
            _lastDefinedWord = newWord;
            _doesCollecting = false;
            _doesTokens = null;
            _lastCreatedName = null;
        }

        _tokens = null; // clear stream
        return !_exitRequested;
    }

    internal List<Func<ForthInterpreter, Task>> CurrentList() =>
        _controlStack.Count == 0 ? _currentInstructions! : _controlStack.Peek().GetCurrentList();

    private bool TryParseNumber(string token, out long value)
    {
        long GetBase(long def)
        {
            MemTryGet(_baseAddr, out var b);
            return b <= 0 ? def : b;
        }

        return NumberParser.TryParse(token, GetBase, out value);
    }

    private bool TryParseDouble(string token, out double value)
    {
        // Only treat as double if token contains decimal point or exponent to avoid capturing integers
        value = 0.0;
        if (string.IsNullOrEmpty(token)) return false;
        if (!token.Contains('.') && !token.Contains('e') && !token.Contains('E')) return false;
        // Accept typical floating formats with invariant culture (decimal point, exponent)
        return double.TryParse(token, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    internal object StackTop() =>
        _stack.Top();
    internal object StackNthFromTop(int n) =>
        _stack.NthFromTop(n);
    internal void DropTop() =>
        _stack.DropTop();
    internal void SwapTop2() =>
        _stack.SwapTop2();

    internal void MemTryGet(long addr, out long v) =>
        _mem.TryGetValue(addr, out v);

    internal void MemSet(long addr,long v) =>
        _mem[addr] = v;

    internal void RequestExit() =>
        _exitRequested = true;

    internal void WriteNumber(long n) =>
        _io.PrintNumber(n);

    internal void NewLine() =>
        _io.NewLine();

    internal void WriteText(string s) =>
        _io.Print(s);

    internal void ThrowExit() =>
        throw new ExitWordException();

    internal void WithModule(string name, Action action)
    {
        var prev = _currentModule;
        _currentModule = name;

        try
        {
            action();
        }
        finally
        {
            _currentModule = prev;
        }
    }

    public int LoadAssemblyWords(Assembly asm) =>
        AssemblyWordLoader.RegisterFromAssembly(this, asm);

    private sealed class ExitWordException : Exception { }

    internal abstract class CompileFrame
    {
        public abstract List<Func<ForthInterpreter, Task>> GetCurrentList();
    }

    internal sealed class IfFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> ThenPart { get; } = new();
        public List<Func<ForthInterpreter, Task>>? ElsePart { get; set; }
        public bool InElse { get; set; }

        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InElse ? (ElsePart ??= new()) : ThenPart;
    }

    internal sealed class BeginFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> PrePart { get; } = new();
        public List<Func<ForthInterpreter, Task>> MidPart { get; } = new();
        public bool InWhile { get; set; }

        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InWhile ? MidPart : PrePart;
    }

    internal sealed class DoFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> Body { get; } = new();
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            Body;
    }

    internal sealed class LoopLeaveException : Exception { }

    // ----- MARKER snapshots (for dictionary/time-travel) -----
    internal sealed class MarkerSnapshot
    {
        public ImmutableDictionary<(string? Module, string Name), Word> Dict { get; }
        public ImmutableList<string> UsingModules { get; }
        public ImmutableDictionary<string, long> Values { get; }
        public ImmutableDictionary<long, long> Memory { get; }
        public ImmutableDictionary<string, Word?> Deferred { get; }
        public ImmutableDictionary<string, string> Decompile { get; }
        public ImmutableArray<(string Name, string? Module)> Definitions { get; }
        public long NextAddr { get; }

        public MarkerSnapshot(
            ImmutableDictionary<(string? Module, string Name), Word> dict,
            ImmutableList<string> usingModules,
            ImmutableDictionary<string, long> values,
            ImmutableDictionary<long, long> memory,
            ImmutableDictionary<string, Word?> deferred,
            ImmutableDictionary<string, string> decompile,
            ImmutableArray<(string Name, string? Module)> definitions,
            long nextAddr)
        {
            Dict = dict;
            UsingModules = usingModules;
            Values = values;
            Memory = memory;
            Deferred = deferred;
            Decompile = decompile;
            Definitions = definitions;
            NextAddr = nextAddr;
        }
    }

    internal MarkerSnapshot CreateMarkerSnapshot()
    {
        var dict = _dict.ToImmutableDictionary();
        var usingMods = _usingModules.ToImmutableList();
        var values = _values.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        var memory = _mem.ToImmutableDictionary();
        var deferred = _deferred.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        var decompile = _decompile.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        var defs = _definitions.Select(d => (d.Name, d.Module)).ToImmutableArray();
        return new MarkerSnapshot(dict, usingMods, values, memory, deferred, decompile, defs, _nextAddr);
    }

    /// <summary>
    /// Create a new interpreter with captured snapshot from parent (used by SPAWN)
    /// </summary>
    internal ForthInterpreter(MarkerSnapshot snapshot, IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0;

        // Get BASE value from snapshot
        long baseValue = 10;
        if (snapshot.Memory.TryGetValue(snapshot.Memory.Keys.FirstOrDefault(k => k == 2), out var snapBase))
        {
            baseValue = snapBase;
        }

        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = baseValue;

        // Restore dictionary
        _dict = snapshot.Dict;

        _loadPrelude = Task.CompletedTask; // no prelude load for child snapshot

        // Restore modules list
        _usingModules.AddRange(snapshot.UsingModules);

        // Restore values
        foreach (var kvp in snapshot.Values)
        {
            _values[kvp.Key] = kvp.Value;
        }

        // Restore memory (adjust addresses for child interpreter)
        var maxSnapAddr = snapshot.NextAddr;
        if (maxSnapAddr >= _nextAddr)
        {
            _nextAddr = maxSnapAddr + 1;
        }

        foreach (var kvp in snapshot.Memory)
        {
            // Skip special addresses that were already initialized
            if (kvp.Key != _stateAddr && kvp.Key != _baseAddr)
            {
                _mem[kvp.Key] = kvp.Value;
            }
        }

        // Restore deferred
        foreach (var kvp in snapshot.Deferred)
        {
            _deferred[kvp.Key] = kvp.Value;
        }

        // Restore decompile
        foreach (var kvp in snapshot.Decompile)
        {
            _decompile[kvp.Key] = kvp.Value;
        }

        // Restore definitions list
        foreach (var (name, module) in snapshot.Definitions)
        {
            _definitions.Add(new DefinitionRecord(name, module));
        }
    }

    internal void RestoreSnapshot(MarkerSnapshot snap)
    {
        // Clear compile state
        _isCompiling = false;
        _currentDefName = null;
        _currentInstructions = null;
        _currentDefTokens = null;
        _controlStack.Clear();
        _doesCollecting = false;
        _doesTokens = null;
        _lastCreatedName = null;
        _lastCreatedAddr = 0;
        _tokens = null;
        _tokenIndex = 0;

        // Replace dictionary
        _dict = snap.Dict;

        _usingModules.Clear();
        _usingModules.AddRange(snap.UsingModules);

        _values.Clear();
        foreach (var kv in snap.Values) _values[kv.Key] = kv.Value;

        _deferred.Clear();
        foreach (var kv in snap.Deferred) _deferred[kv.Key] = kv.Value;

        _decompile.Clear();
        foreach (var kv in snap.Decompile) _decompile[kv.Key] = kv.Value;

        // Memory and addresses
        _mem.Clear();
        foreach (var kv in snap.Memory) _mem[kv.Key] = kv.Value;
        // Ensure STATE goes to interpret mode
        _mem[_stateAddr] = 0;
        _nextAddr = snap.NextAddr;

        // Definition order
        _definitions.Clear();
        foreach (var (name, module) in snap.Definitions)
            _definitions.Add(new DefinitionRecord(name, module));
        // Update last defined
        _lastDefinedWord = null;
        if (_definitions.Count > 0)
        {
            var last = _definitions[^1];
            _dict.TryGetValue((last.Module, last.Name), out _lastDefinedWord);
        }
    }

    internal int ReadKey() => _io.ReadKey();

    internal bool KeyAvailable() => _io.KeyAvailable();

    internal string? ReadLineFromIO() => _io.ReadLine();

    // Return a snapshot of the current search order (list of module names, null represents core)
    internal ImmutableList<string?> GetOrder()
    {
        var list = new List<string?>();
        // Using modules (most recent first) then root
        for (int i = _usingModules.Count - 1; i >= 0; i--)
            list.Add(_usingModules[i]);
        // add core as null (FORTH)
        list.Add(null);
        return list.ToImmutableList();
    }

    internal void SetOrder(IEnumerable<string?> order)
    {
        _usingModules.Clear();
        foreach (var m in order)
        {
            if (m is null) break; // stop at FORTH sentinel
            if (!string.IsNullOrWhiteSpace(m) && !_usingModules.Contains(m))
                _usingModules.Add(m);
        }
    }
}
