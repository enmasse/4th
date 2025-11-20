using Forth.Core.Binding;
using Forth.Core.Execution;
using System.Collections.Immutable; // added
using System.Reflection;
using System.Text;
using System.Globalization;
using System.IO; // ...existing using (may be needed by other parts)

namespace Forth.Core.Interpreter;

/// <summary>
/// The core Forth interpreter implementation. Provides evaluation, dictionary management,
/// and memory/stack primitives used by core primitives and tests.
/// </summary>
public partial class ForthInterpreter : IForthInterpreter // made partial
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

    private readonly long _sourceAddr;
    private readonly long _inAddr;
    internal long SourceAddr => _sourceAddr;
    internal long InAddr => _inAddr;

    // Current source tracking (line and index within it)
    private string? _currentSource;
    private int _currentIn;
    internal string? CurrentSource => _currentSource;
    internal int CurrentIn => _currentIn;

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

    /// <summary>
    /// Create a new interpreter instance.
    /// </summary>
    /// <param name="io">Optional IO implementation to use for input/output. If null, a console IO is used.</param>
    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();

        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0;

        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = 10;

        // Allocate memory cells for SOURCE and >IN variables
        _sourceAddr = _nextAddr++;
        _mem[_sourceAddr] = 0; // not storing string; reserved
        _inAddr = _nextAddr++;
        _mem[_inAddr] = 0; // >IN initial value

        _currentSource = null;
        _currentIn = 0;

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
                if (_dict.ContainsKey(key))
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

    /// <summary>
    /// Gets a read-only snapshot of the data stack contents (top at end of list).
    /// </summary>
    public IReadOnlyList<object> Stack =>
        _stack.AsReadOnly();

    /// <summary>
    /// Pushes a raw object onto the data stack.
    /// </summary>
    /// <param name="value">Value to push.</param>
    public void Push(object value) =>
        _stack.Push(value);

    /// <summary>
    /// Pops and returns the top item on the data stack.
    /// </summary>
    /// <returns>The popped value.</returns>
    public object Pop() =>
        _stack.Pop();

    /// <summary>
    /// Peeks at the top item on the data stack without removing it.
    /// </summary>
    /// <returns>The top stack value.</returns>
    public object Peek() =>
        _stack.Peek();

    /// <summary>
    /// Adds a new synchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Delegate executed when the word runs.</param>
    public void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _currentModule };
        _dict = _dict.SetItem((_currentModule, name), w);
        RegisterDefinition(name);
    }

    /// <summary>
    /// Adds a new asynchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Async delegate executed when the word runs.</param>
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
        bool bo => bo ? -1L : 0L,
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

    /// <summary>
    /// Evaluates a single line of Forth source text, loading prelude first if not yet loaded.
    /// </summary>
    /// <param name="line">Source line to evaluate.</param>
    /// <returns>True if interpreter continues running; false if exit requested.</returns>
    public async Task<bool> EvalAsync(string line)
    {
        await _loadPrelude;
        return await EvalInternalAsync(line);
    }

    private async Task<bool> EvalInternalAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        // Track current source and reset input index for this evaluation
        _currentSource = line;
        _currentIn = 0;
        // reset memory >IN cell
        _mem[_inAddr] = 0;
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

        while (TryReadNextToken(out var tok))
        {
            if (tok.Length == 0)
            {
                continue;
            }

            if (!_isCompiling)
            {
                if (_doesCollecting)
                {
                    _doesTokens!.Add(tok);
                    continue;
                }

                if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryReadNextToken(out var maybeMsg) && maybeMsg.Length >= 2 && maybeMsg[0] == '"' && maybeMsg[^1] == '"')
                    {
                        throw new ForthException(ForthErrorCode.Unknown, maybeMsg[1..^1]);
                    }

                    throw new ForthException(ForthErrorCode.Unknown, "ABORT");
                }

                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    Push(tok[1..^1]);
                    continue;
                }

                if (TryParseDouble(tok, out var dnum))
                {
                    Push(dnum);
                    continue;
                }

                if (TryParseNumber(tok, out var num))
                {
                    Push(num);
                    continue;
                }

                if (TryResolveWord(tok, out var word) && word is not null)
                {
                    await word.ExecuteAsync(this);
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                if (tok != ";")
                {
                    _currentDefTokens?.Add(tok);
                }

                if (tok == ";")
                {
                    FinishDefinition();
                    continue;
                }

                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(tok[1..^1]);
                        return Task.CompletedTask;
                    });

                    continue;
                }

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
        value = 0.0;
        if (string.IsNullOrEmpty(token)) return false;
        if (!token.Contains('.') && !token.Contains('e') && !token.Contains('E')) return false;
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

    /// <summary>
    /// Registers all words marked by <see cref="PrimitiveAttribute"/> in a given assembly.
    /// </summary>
    /// <param name="asm">Assembly to scan.</param>
    /// <returns>Number of words registered.</returns>
    public int LoadAssemblyWords(Assembly asm) =>
        AssemblyWordLoader.RegisterFromAssembly(this, asm);

    // Re-added IO/query methods used by primitives after refactor
    internal int ReadKey() => _io.ReadKey();
    internal bool KeyAvailable() => _io.KeyAvailable();
    internal string? ReadLineFromIO() => _io.ReadLine();

    internal ImmutableList<string?> GetOrder()
    {
        var list = new List<string?>();
        for (int i = _usingModules.Count - 1; i >= 0; i--)
            list.Add(_usingModules[i]);
        list.Add(null);
        return list.ToImmutableList();
    }

    internal void SetOrder(IEnumerable<string?> order)
    {
        _usingModules.Clear();
        foreach (var m in order)
        {
            if (m is null) break;
            if (!string.IsNullOrWhiteSpace(m) && !_usingModules.Contains(m))
                _usingModules.Add(m);
        }
    }

    private sealed class ExitWordException : Exception { }
}
