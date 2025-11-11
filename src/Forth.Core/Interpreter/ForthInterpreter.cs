using System.Threading.Tasks;
using System; // added for Type reflection
using System.Collections.Concurrent; // added for IL cache
using System.Globalization; // needed for NumberStyles in BIND parsing
using System.Reflection; // for LoadAssemblyWords
using Forth.Core;
using Forth.Core.Binding;
using Forth.Core.Execution;

namespace Forth.Core.Interpreter;

public class ForthInterpreter : Forth.Core.IForthInterpreter
{
    private readonly ForthStack _stack = new();
    private readonly ForthStack _rstack = new();
    private readonly Dictionary<string, Word> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly Forth.Core.IForthIO _io;
    private bool _exitRequested;
    private bool _isCompiling;
    private string? _currentDefName;
    private List<Func<ForthInterpreter, Task>>? _currentInstructions;
    private readonly Stack<CompileFrame> _controlStack = new();
    private readonly Dictionary<long,long> _mem = new();
    private long _nextAddr = 1;
    private readonly HashSet<object> _consumedTaskResults = new();

    // VALUE storage
    private readonly Dictionary<string,long> _values = new(StringComparer.OrdinalIgnoreCase);

    // Module support
    private readonly Dictionary<string, Dictionary<string, Word>> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _usingModules = new();
    private string? _currentModule;

    private readonly ConcurrentDictionary<string, Func<ForthInterpreter, Task>> _ilCache = new();

    // Components
    private readonly ControlFlowRuntime _controlFlow = new();

    // STATE and BASE cells
    private readonly long _stateAddr;
    internal long StateAddr => _stateAddr;
    private readonly long _baseAddr;
    internal long BaseAddr => _baseAddr;

    // Deferred word bindings: name -> target Word
    private readonly Dictionary<string, Word?> _deferred = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Create a new interpreter instance. Optionally provide custom I/O.</summary>
    public ForthInterpreter(Forth.Core.IForthIO? io = null)
    {
        _io = io ?? new Forth.Core.ConsoleForthIO();
        // Allocate special cells before installing primitives
        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0; // interpret by default
        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = 10; // decimal by default
        InstallPrimitives();
    }

    public IReadOnlyList<object> Stack => _stack.AsReadOnly();
    public void Push(object value) => _stack.Push(value);
    public object Pop() => _stack.Pop();
    public object Peek() => _stack.Peek();

    public void AddWord(string name, Action<Forth.Core.IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        TargetDict()[name] = new Word(intr => body(intr));
    }
    public void AddWordAsync(string name, Func<Forth.Core.IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        TargetDict()[name] = new Word(intr => body(intr));
    }

    private Dictionary<string, Word> TargetDict()
    {
        if (!string.IsNullOrWhiteSpace(_currentModule))
        {
            if (!_modules.TryGetValue(_currentModule!, out var d))
            {
                d = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);
                _modules[_currentModule!] = d;
            }
            return d;
        }
        return _dict;
    }

    private bool TryResolveWord(string token, out Word? word)
    {
        var idx = token.IndexOf(':');
        if (idx > 0)
        {
            var mod = token.Substring(0, idx);
            var wname = token.Substring(idx + 1);
            if (_modules.TryGetValue(mod, out var mdict) && mdict.TryGetValue(wname, out var wq))
            {
                word = wq;
                return true;
            }
            word = null;
            return false;
        }
        if (!string.IsNullOrWhiteSpace(_currentModule) && _modules.TryGetValue(_currentModule!, out var cur) && cur.TryGetValue(token, out var wc))
        {
            word = wc;
            return true;
        }
        for (int i = _usingModules.Count - 1; i >= 0; i--)
        {
            var mn = _usingModules[i];
            if (_modules.TryGetValue(mn, out var md) && md.TryGetValue(token, out var wu))
            {
                word = wu;
                return true;
            }
        }
        return _dict.TryGetValue(token, out word);
    }

    internal object PopInternal() => Pop();
    internal static void EnsureStack(ForthInterpreter interp, int needed, string word)
    {
        if (interp._stack.Count < needed)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
    }

    internal static long ToLongPublic(object v) => ToLong(v);
    private static long ToLong(object v) => v switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        char c => c,
        bool bo => bo ? 1L : 0L,
        _ => throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
    };
    private static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        _ => throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };
    private bool IsResultConsumed(object taskRef) => _consumedTaskResults.Contains(taskRef);
    private void MarkResultConsumed(object taskRef) => _consumedTaskResults.Add(taskRef);

    // Return stack helpers
    internal void RPush(object value) => _rstack.Push(value);
    internal object RPop() => _rstack.Pop();
    internal int RCount => _rstack.Count;

    // VALUE helpers
    internal long ValueGet(string name) => _values.TryGetValue(name, out var v) ? v : 0L;
    internal void ValueSet(string name, long v) => _values[name] = v;

    // Control-flow wrappers
    internal void PushLoopIndex(long idx) => _controlFlow.Push(idx);
    internal void PopLoopIndex() => _controlFlow.Pop();
    internal void PopLoopIndexMaybe() => _controlFlow.PopMaybe();
    internal void Unloop() => _controlFlow.Unloop();
    internal long CurrentLoopIndex() => _controlFlow.Current();

    /// <summary>
    /// Interpret one line synchronously by awaiting <see cref="EvalAsync"/>.
    /// </summary>
    public bool Interpret(string line) => EvalAsync(line).GetAwaiter().GetResult();

    /// <summary>
    /// Interpret one line asynchronously. Returns false if BYE/QUIT requested exit.
    /// Supports async words and AWAIT of Task/Task&lt;T&gt;/ValueTask words.
    /// </summary>
    public async Task<bool> EvalAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        // Fast path: attempt IL compilation cache for trivial expressions (numbers and + - * /)
        if (_ilCache.TryGetValue(line, out var cached))
        {
            await cached(this);
            return !_exitRequested;
        }
        var tokens = Tokenizer.Tokenize(line);
        if (!_isCompiling && _ilCache.Count < 1024)
        {
            if (IlScriptCompiler.TryCompile(tokens, out var runner))
            {
                _ilCache[line] = runner;
                await runner(this);
                return !_exitRequested;
            }
        }
        if (!_isCompiling)
        {
            var script = CompileSync(line);
            if (script is not null)
            {
                script.Run(this);
                return !_exitRequested;
            }
        }
        int i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i++];
            if (tok.Length == 0) continue;
            if (!_isCompiling)
            {
                // Module management (immediate)
                if (tok.Equals("MODULE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after MODULE");
                    _currentModule = tokens[i++];
                    if (string.IsNullOrWhiteSpace(_currentModule)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Invalid module name");
                    if (!_modules.ContainsKey(_currentModule)) _modules[_currentModule] = new(StringComparer.OrdinalIgnoreCase);
                    continue;
                }
                if (tok.Equals("END-MODULE", StringComparison.OrdinalIgnoreCase)) { _currentModule = null; continue; }
                if (tok.Equals("USING", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after USING");
                    var m = tokens[i++];
                    if (!_usingModules.Contains(m)) _usingModules.Add(m);
                    continue;
                }
                if (tok.Equals("LOAD-ASM", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected path after LOAD-ASM");
                    var path = tokens[i++];
                    var count = AssemblyWordLoader.Load(this, path);
                    Push((long)count); // push number of modules loaded
                    continue;
                }
                if (tok.Equals("LOAD-ASM-TYPE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected type after LOAD-ASM-TYPE");
                    var typeName = tokens[i++];
                    Type? t = Type.GetType(typeName, throwOnError:false, ignoreCase:false);
                    if (t == null)
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            t = asm.GetType(typeName, throwOnError:false, ignoreCase:false);
                            if (t != null) break;
                        }
                    }
                    if (t == null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, $"Type not found: {typeName}");
                    var count = LoadAssemblyWords(t.Assembly);
                    Push((long)count);
                    continue;
                }

                // Definition start
                if (tok == ":")
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after ':'");
                    _currentDefName = tokens[i++];
                    if (string.IsNullOrWhiteSpace(_currentDefName)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Invalid word name");
                    _currentInstructions = new List<Func<ForthInterpreter, Task>>();
                    _controlStack.Clear();
                    _isCompiling = true;
                    _mem[_stateAddr] = 1;
                    continue;
                }
                if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after VARIABLE");
                    var name = tokens[i++];
                    var addr = _nextAddr++;
                    _mem[addr] = 0;
                    TargetDict()[name] = new Word(intr => intr.Push(addr));
                    continue;
                }
                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after CONSTANT");
                    var name = tokens[i++];
                    EnsureStack(this,1,"CONSTANT");
                    var value = PopInternal();
                    TargetDict()[name] = new Word(intr => intr.Push(value));
                    continue;
                }
                if (tok.Equals("VALUE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after VALUE");
                    var name = tokens[i++];
                    if (!_values.ContainsKey(name)) _values[name] = 0L;
                    TargetDict()[name] = new Word(intr => intr.Push(intr.ValueGet(name)));
                    continue;
                }
                if (tok.Equals("TO", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after TO");
                    var name = tokens[i++];
                    EnsureStack(this,1,"TO");
                    var v = ToLong(PopInternal());
                    ValueSet(name, v);
                    continue;
                }
                if (tok.Equals("DEFER", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after DEFER");
                    var name = tokens[i++];
                    _deferred[name] = null;
                    TargetDict()[name] = new Word(async intr =>
                    {
                        if (!_deferred.TryGetValue(name, out var target) || target is null)
                            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Deferred word not set: {name}");
                        await target.ExecuteAsync(intr);
                    });
                    continue;
                }
                if (tok.Equals("IS", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected deferred name after IS");
                    var name = tokens[i++];
                    EnsureStack(this,1,"IS");
                    var xtObj = PopInternal();
                    if (xtObj is not Word xt)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "IS expects an execution token");
                    if (!_deferred.ContainsKey(name))
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"No such deferred: {name}");
                    _deferred[name] = xt;
                    continue;
                }
                if (tok == "'")
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected word after '\''");
                    var wname = tokens[i++];
                    if (!TryResolveWord(wname, out var wtok) || wtok is null)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word: {wname}");
                    Push(wtok);
                    continue;
                }
                if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected char after CHAR");
                    var s = tokens[i++];
                    Push(s.Length > 0 ? (long)s[0] : 0L);
                    continue;
                }
                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    var s = tok.Substring(1, tok.Length - 2);
                    Push(s);
                    continue;
                }
                if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
                {
                    if (i < tokens.Count && tokens[i].Length >= 2 && tokens[i][0] == '"' && tokens[i][^1] == '"')
                    {
                        var st = tokens[i++];
                        var msg = st.Substring(1, st.Length - 2);
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.Unknown, msg);
                    }
                    throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.Unknown, "ABORT");
                }
                if (tok.Equals("BIND", StringComparison.OrdinalIgnoreCase) || tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase))
                {
                    bool asyncBind = tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase);
                    if (i + 3 >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, (asyncBind?"BINDASYNC":"BIND") + " requires: type method argCount name");
                    var typeName = tokens[i++];
                    var methodName = tokens[i++];
                    if (!int.TryParse(tokens[i++], NumberStyles.Integer, CultureInfo.InvariantCulture, out var argCount))
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Invalid arg count");
                    var forthName = tokens[i++];
                    TargetDict()[forthName] = asyncBind ? ClrBinder.CreateBoundTaskWord(typeName, methodName, argCount) : ClrBinder.CreateBoundWord(typeName, methodName, argCount);
                    continue;
                }
                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this,1,"AWAIT");
                    var obj = PopInternal();
                    if (obj is not Task t) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"AWAIT expects a Task id");
                    await t.ConfigureAwait(false);
                    var tt = t.GetType();
                    if (tt.IsGenericType && !IsResultConsumed(obj))
                    {
                        var resultProp = tt.GetProperty("Result");
                        var resultType = resultProp?.PropertyType;
                        if (resultType?.FullName != "System.Threading.Tasks.VoidTaskResult")
                        {
                            var val = resultProp!.GetValue(t);
                            Push(NumberParser.Normalize(val));
                            if (_stack.Count >= 2 && _stack.NthFromTop(2) is Task)
                            {
                                var top = _stack.Pop();
                                var below = _stack.Pop();
                                _stack.Push(top);
                                _stack.Push(below);
                            }
                        }
                        MarkResultConsumed(obj);
                    }
                    continue;
                }
                if (tok.Equals("TASK?", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this,1,"TASK?");
                    var obj = PopInternal();
                    var tt = obj as Task;
                    Push(tt is not null && tt.IsCompleted ? 1L : 0L);
                    continue;
                }
                if (tok.Equals("[", StringComparison.Ordinal)) { _isCompiling = false; _mem[_stateAddr] = 0; continue; }
                if (tok.Equals("]", StringComparison.Ordinal)) { _isCompiling = true; _mem[_stateAddr] = 1; continue; }

                if (TryParseNumber(tok, out var num)) { Push(num); continue; }
                if (TryResolveWord(tok, out var w) && w is not null) { await w.ExecuteAsync(this); continue; }
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                if (tok == ";")
                {
                    if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "No open definition to end");
                    if (_controlStack.Count != 0)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Unmatched control structure");
                    var body = _currentInstructions;
                    var compiled = new Word(async intr =>
                    {
                        try
                        {
                            foreach (var a in body)
                                await a(intr);
                        }
                        catch (ExitWordException) { }
                    });
                    TargetDict()[_currentDefName] = compiled;
                    _isCompiling = false;
                    _mem[_stateAddr] = 0;
                    _currentDefName = null;
                    _currentInstructions = null;
                    continue;
                }
                if (tok.Equals("IF", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new IfFrame()); continue; }
                if (tok.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not IfFrame ifr)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "ELSE without IF");
                    if (ifr.ElsePart is not null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Multiple ELSE");
                    ifr.ElsePart = new List<Func<ForthInterpreter, Task>>();
                    ifr.InElse = true;
                    continue;
                }
                if (tok.Equals("THEN", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not IfFrame ifr)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "THEN without IF");
                    _controlStack.Pop();
                    var thenPart = ifr.ThenPart; var elsePart = ifr.ElsePart;
                    CurrentList().Add(async intr =>
                    {
                        EnsureStack(intr,1,"IF");
                        var flagObj = intr.PopInternal();
                        if (ToBool(flagObj))
                        {
                            foreach (var a in thenPart) await a(intr);
                        }
                        else if (elsePart is not null)
                        {
                            foreach (var a in elsePart) await a(intr);
                        }
                    });
                    continue;
                }
                if (tok.Equals("BEGIN", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new BeginFrame()); continue; }
                if (tok.Equals("WHILE", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "WHILE without BEGIN");
                    if (bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Multiple WHILE");
                    bf.InWhile = true; continue;
                }
                if (tok.Equals("REPEAT", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "REPEAT without BEGIN");
                    if (!bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "REPEAT requires WHILE");
                    _controlStack.Pop();
                    var pre = bf.PrePart; var mid = bf.MidPart;
                    CurrentList().Add(async intr =>
                    {
                        while (true)
                        {
                            foreach (var a in pre) await a(intr);
                            EnsureStack(intr,1,"WHILE"); var flagObj = intr.PopInternal(); if (!ToBool(flagObj)) break;
                            foreach (var b in mid) await b(intr);
                        }
                    });
                    continue;
                }
                if (tok.Equals("UNTIL", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "UNTIL without BEGIN");
                    if (bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "UNTIL after WHILE use REPEAT");
                    _controlStack.Pop();
                    var body = bf.PrePart;
                    CurrentList().Add(async intr =>
                    {
                        while (true)
                        {
                            foreach (var a in body) await a(intr);
                            EnsureStack(intr,1,"UNTIL"); var flagObj = intr.PopInternal(); if (ToBool(flagObj)) break;
                        }
                    });
                    continue;
                }
                if (tok.Equals("DO", StringComparison.OrdinalIgnoreCase))
                {
                    _controlStack.Push(new DoFrame());
                    continue;
                }
                if (tok.Equals("LOOP", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not DoFrame df)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "LOOP without DO");
                    _controlStack.Pop();
                    var body = df.Body;
                    CurrentList().Add(async intr =>
                    {
                        EnsureStack(intr,2,"DO");
                        var start = ToLongPublic(intr.Pop());
                        var limit = ToLongPublic(intr.Pop());
                        long step = start <= limit ? 1L : -1L;
                        for (long idx = start; idx != limit; idx += step)
                        {
                            intr.PushLoopIndex(idx);
                            try
                            {
                                foreach (var a in body) await a(intr);
                            }
                            catch (LoopLeaveException)
                            {
                                break;
                            }
                            finally
                            {
                                intr.PopLoopIndexMaybe();
                            }
                        }
                    });
                    continue;
                }
                if (tok.Equals("LEAVE", StringComparison.OrdinalIgnoreCase))
                {
                    bool insideDo = false;
                    foreach (var frame in _controlStack)
                    {
                        if (frame is DoFrame) { insideDo = true; break; }
                    }
                    if (!insideDo)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "LEAVE used outside DO...LOOP");
                    CurrentList().Add(intr => { throw new LoopLeaveException(); });
                    continue;
                }
                if (tok.Equals("DEFER", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected name after DEFER");
                    var name = tokens[i++];
                    _deferred[name] = null;
                    TargetDict()[name] = new Word(async intr =>
                    {
                        if (!_deferred.TryGetValue(name, out var target) || target is null)
                            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Deferred word not set: {name}");
                        await target.ExecuteAsync(intr);
                    });
                    continue;
                }
                if (tok.Equals("IS", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected deferred name after IS");
                    var name = tokens[i++];
                    _currentInstructions!.Add(intr =>
                    {
                        ForthInterpreter.EnsureStack(intr,1,"IS");
                        var xtObj = intr.PopInternal();
                        if (xtObj is not Word xt)
                            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "IS expects an execution token");
                        if (!intr._deferred.ContainsKey(name))
                            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"No such deferred: {name}");
                        intr._deferred[name] = xt;
                        return Task.CompletedTask;
                    });
                    continue;
                }
                if (tok == "'")
                {
                    if (i >= tokens.Count) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Expected word after '\''");
                    var wname = tokens[i++];
                    if (!TryResolveWord(wname, out var wtok) || wtok is null)
                        throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word: {wname}");
                    var wt = wtok;
                    _currentInstructions!.Add(intr => { intr.Push(wt); return Task.CompletedTask; });
                    continue;
                }
                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    _currentInstructions!.Add(async intr =>
                    {
                        EnsureStack(intr,1,"AWAIT");
                        var obj = intr.PopInternal();
                        if (obj is not Task t) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"AWAIT expects a Task id");
                        await t.ConfigureAwait(false);
                        var tt = t.GetType();
                        if (tt.IsGenericType && !intr.IsResultConsumed(obj))
                        {
                            var resultProp = tt.GetProperty("Result");
                            var resultType = resultProp?.PropertyType;
                            if (resultType?.FullName != "System.Threading.Tasks.VoidTaskResult")
                            {
                                var val = resultProp!.GetValue(t);
                                intr.Push(NumberParser.Normalize(val));
                                if (intr._stack.Count >= 2 && intr._stack.NthFromTop(2) is Task)
                                {
                                    var top = intr._stack.Pop();
                                    var below = intr._stack.Pop();
                                    intr._stack.Push(top);
                                    intr._stack.Push(below);
                                }
                            }
                            intr.MarkResultConsumed(obj);
                        }
                    });
                    continue;
                }
                if (tok.Equals("LITERAL", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this,1,"LITERAL");
                    var value = PopInternal();
                    _currentInstructions!.Add(intr => { intr.Push(value); return Task.CompletedTask; });
                    continue;
                }
                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    var s = tok.Substring(1, tok.Length - 2);
                    _currentInstructions!.Add(intr => { intr.Push(s); return Task.CompletedTask; });
                    continue;
                }
                if (TryParseNumber(tok, out var lit)) { CurrentList().Add(intr => { intr.Push(lit); return Task.CompletedTask; }); continue; }
                if (TryResolveWord(tok, out var cw) && cw is not null) { CurrentList().Add(async intr => await cw.ExecuteAsync(intr)); continue; }
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }
        return !_exitRequested;
    }

    private List<Func<ForthInterpreter, Task>> CurrentList() => _controlStack.Count == 0 ? _currentInstructions! : _controlStack.Peek().GetCurrentList();

    private bool TryParseNumber(string token, out long value)
    {
        long GetBase(long @default)
        {
            MemTryGet(_baseAddr, out var b);
            return b <= 0 ? @default : b;
        }
        return NumberParser.TryParse(token, GetBase, out value);
    }

    private void InstallPrimitives() => CorePrimitives.Install(this, _dict);

    internal object StackTop() => _stack.Top();
    internal object StackNthFromTop(int n) => _stack.NthFromTop(n);
    internal void DropTop() => _stack.DropTop();
    internal void SwapTop2() => _stack.SwapTop2();
    internal void MemTryGet(long addr, out long v) { _mem.TryGetValue(addr, out v); }
    internal void MemSet(long addr, long v) { _mem[addr] = v; }
    internal void RequestExit() { _exitRequested = true; }
    internal void WriteNumber(long n) => _io.PrintNumber(n);
    internal void NewLine() => _io.NewLine();
    internal void WriteText(string s) => _io.Print(s);
    internal void ThrowExit() => throw new ExitWordException();

    internal Forth.Core.Execution.ForthCompiledScript? CompileSync(string line)
    {
        var tokens = Tokenizer.Tokenize(line);
        var steps = new List<Action<ForthInterpreter>>();
        int i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i++];
            if (tok.Length == 0) continue;
            if (tok is ":" or ";" or "AWAIT" or "SPAWN" or "[" or "]" or "IF" or "ELSE" or "THEN" or "BEGIN" or "WHILE" or "REPEAT" or "UNTIL")
                return null;
            if (tok.Equals("MODULE", StringComparison.OrdinalIgnoreCase) || tok.Equals("END-MODULE", StringComparison.OrdinalIgnoreCase) || tok.Equals("USING", StringComparison.OrdinalIgnoreCase))
                return null;
            if (tok.Equals("LOAD-ASM", StringComparison.OrdinalIgnoreCase) || tok.Equals("LOAD-ASM-TYPE", StringComparison.OrdinalIgnoreCase))
                return null;
            if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase) || tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase)) { return null; }
            if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase))
            {
                if (i >= tokens.Count) return null;
                var s = tokens[i++];
                steps.Add(intr => intr.Push((long)(s.Length > 0 ? s[0] : '\0')));
                continue;
            }
            if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"') { var s = tok.Substring(1, tok.Length - 2); steps.Add(intr => intr.Push(s)); continue; }
            if (TryParseNumber(tok, out var numVal)) { steps.Add(intr => intr.Push(numVal)); continue; }

            if (TryResolveWord(tok, out var w) && w is not null && !w.IsAsync)
            {
                switch (tok.ToUpperInvariant())
                {
                    case "+": steps.Add(intr => { EnsureStack(intr,2,"+"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2+b2); }); break;
                    case "-": steps.Add(intr => { EnsureStack(intr,2,"-"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2-b2); }); break;
                    case "*": steps.Add(intr => { EnsureStack(intr,2,"*"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2*b2); }); break;
                    case "/": steps.Add(intr => { EnsureStack(intr,2,"/"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); if (b2==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); intr.Push(a2/b2); }); break;
                    case "DUP": steps.Add(intr => { EnsureStack(intr,1,"DUP"); intr.Push(intr.StackTop()); }); break;
                    case "DROP": steps.Add(intr => { EnsureStack(intr,1,"DROP"); intr.DropTop(); }); break;
                    case "SWAP": steps.Add(intr => { EnsureStack(intr,2,"SWAP"); intr.SwapTop2(); }); break;
                    case "OVER": steps.Add(intr => { EnsureStack(intr,2,"OVER"); intr.Push(intr.StackNthFromTop(2)); }); break;
                    case "ROT": steps.Add(intr => { EnsureStack(intr,3,"ROT"); var c=intr.PopInternal(); var b3=intr.PopInternal(); var a3=intr.PopInternal(); intr.Push(b3); intr.Push(c); intr.Push(a3); }); break;
                    case "-ROT": steps.Add(intr => { EnsureStack(intr,3,"-ROT"); var c=intr.PopInternal(); var b3=intr.PopInternal(); var a3=intr.PopInternal(); intr.Push(c); intr.Push(a3); intr.Push(b3); }); break;
                    default:
                        return null;
                }
                continue;
            }
            return null;
        }
        return new Forth.Core.Execution.ForthCompiledScript(steps);
    }

    internal void WithModule(string name, Action action)
    {
        var previous = _currentModule;
        _currentModule = name;
        try { action(); }
        finally { _currentModule = previous; }
    }

    public int LoadAssemblyWords(Assembly asm) => AssemblyWordLoader.RegisterFromAssembly(this, asm);

    public void BeginModuleScope(string moduleName, Action register)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentNullException.ThrowIfNull(register);
        WithModule(moduleName, register);
    }

    internal sealed class Word
    {
        private readonly Func<ForthInterpreter, Task> _run;
        public bool IsAsync { get; }
        public Word(Action<ForthInterpreter> sync) { _run = intr => { sync(intr); return Task.CompletedTask; }; IsAsync = false; }
        public Word(Func<ForthInterpreter, Task> asyncRun) { _run = asyncRun; IsAsync = true; }
        public Task ExecuteAsync(ForthInterpreter intr) => _run(intr);
    }
    private abstract class CompileFrame { public abstract List<Func<ForthInterpreter, Task>> GetCurrentList(); }
    private sealed class IfFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> ThenPart { get; } = new();
        public List<Func<ForthInterpreter, Task>>? ElsePart { get; set; }
        public bool InElse { get; set; }
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => InElse ? (ElsePart ??= new()) : ThenPart;
    }
    private sealed class BeginFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> PrePart { get; } = new();
        public List<Func<ForthInterpreter, Task>> MidPart { get; } = new();
        public bool InWhile { get; set; }
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => InWhile ? MidPart : PrePart;
    }
    private sealed class DoFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> Body { get; } = new();
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => Body;
    }
    private sealed class ExitWordException : Exception {}
    private sealed class LoopLeaveException : Exception {}
}
