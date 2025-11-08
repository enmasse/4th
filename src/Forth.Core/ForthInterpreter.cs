using System.Globalization;
using System.Threading.Tasks;

namespace Forth;

public class ForthInterpreter : IForthInterpreter
{
    private readonly ForthStack _stack = new();
    private readonly Dictionary<string, Word> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly IForthIO _io;
    private bool _exitRequested;

    // Compilation state (when inside ':' ... ';')
    private bool _isCompiling;
    private string? _currentDefName;
    private List<Func<ForthInterpreter, Task>>? _currentInstructions;
    private readonly Stack<CompileFrame> _controlStack = new();

    // Simple variable storage (VARIABLE allocates address, CONSTANT captures value)
    private readonly Dictionary<long,long> _mem = new();
    private long _nextAddr = 1;

    // Track awaited tasks to avoid pushing result multiple times when same Task object appears twice
    private readonly HashSet<object> _consumedTaskResults = new();

    /// <summary>Create a new interpreter instance. Optionally supply an <see cref="IForthIO"/> for I/O redirection.</summary>
    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        InstallPrimitives();
    }

    /// <summary>Parameter stack (top is last element). Contains boxed numeric values and Task instances.</summary>
    public IReadOnlyList<object> Stack => _stack.AsReadOnly();

    /// <summary>Push a value onto the parameter stack.</summary>
    public void Push(object value) => _stack.Push(value);

    /// <summary>Pop and return top-of-stack value.</summary>
    public object Pop() => _stack.Pop();

    /// <summary>Return top-of-stack value without removing it.</summary>
    public object Peek() => _stack.Peek();

    /// <summary>Register a new synchronous word by name.</summary>
    public void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        _dict[name] = new Word(intr => body(intr));
    }

    /// <summary>Register a new asynchronous word by name.</summary>
    public void AddWordAsync(string name, Func<IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        _dict[name] = new Word(intr => body(intr));
    }

    internal object PopInternal() => Pop();

    internal static void EnsureStack(ForthInterpreter interp, int needed, string word)
    {
        if (interp._stack.Count < needed)
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
    }

    internal static long ToLongPublic(object v) => ToLong(v);
    private static long ToLong(object v)
    {
        return v switch
        {
            long l => l,
            int i => i,
            short s => s,
            byte b => b,
            char c => c,
            bool bo => bo ? 1L : 0L,
            _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
        };
    }

    private static bool ToBool(object v)
    {
        return v switch
        {
            bool b => b,
            long l => l != 0,
            int i => i != 0,
            short s => s != 0,
            byte b8 => b8 != 0,
            char c => c != '\0',
            _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
        };
    }

    private bool IsResultConsumed(object taskRef) => _consumedTaskResults.Contains(taskRef);
    private void MarkResultConsumed(object taskRef) => _consumedTaskResults.Add(taskRef);

    /// <summary>
    /// Interpret one line synchronously. This blocks on any async work (awaits internally).
    /// Returns false if BYE / QUIT was executed.
    /// </summary>
    public bool Interpret(string line) => EvalAsync(line).GetAwaiter().GetResult();

    /// <summary>
    /// Interpret one line asynchronously. Supports AWAIT for Task / Task&lt;T&gt; / ValueTask / ValueTask&lt;T&gt; objects pushed by BINDASYNC.
    /// Returns false if BYE / QUIT was executed.
    /// </summary>
    public async Task<bool> EvalAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        var tokens = Tokenizer.Tokenize(line);
        int i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i++];
            if (tok.Length == 0) continue;
            if (!_isCompiling)
            {
                // Immediate (interpret) mode
                if (tok == ":")
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after ':'");
                    _currentDefName = tokens[i++];
                    if (string.IsNullOrWhiteSpace(_currentDefName)) throw new ForthException(ForthErrorCode.CompileError, "Invalid word name");
                    _currentInstructions = new List<Func<ForthInterpreter, Task>>();
                    _controlStack.Clear();
                    _isCompiling = true;
                    continue;
                }
                if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after VARIABLE");
                    var name = tokens[i++];
                    var addr = _nextAddr++;
                    _mem[addr] = 0;
                    _dict[name] = new Word(intr => intr.Push(addr));
                    continue;
                }
                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CONSTANT");
                    var name = tokens[i++];
                    EnsureStack(this,1,"CONSTANT");
                    var value = PopInternal();
                    _dict[name] = new Word(intr => intr.Push(value));
                    continue;
                }
                if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected char after CHAR");
                    var s = tokens[i++];
                    Push(s.Length > 0 ? (long)s[0] : 0L);
                    continue;
                }
                if (tok.Equals("BIND", StringComparison.OrdinalIgnoreCase) || tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase))
                {
                    bool asyncBind = tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase);
                    if (i + 3 >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, (asyncBind?"BINDASYNC":"BIND") + " requires: type method argCount name");
                    var typeName = tokens[i++];
                    var methodName = tokens[i++];
                    if (!int.TryParse(tokens[i++], NumberStyles.Integer, CultureInfo.InvariantCulture, out var argCount))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid arg count");
                    var forthName = tokens[i++];
                    _dict[forthName] = asyncBind ? ClrBinder.CreateBoundTaskWord(typeName, methodName, argCount) : ClrBinder.CreateBoundWord(typeName, methodName, argCount);
                    continue;
                }
                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this,1,"AWAIT");
                    var obj = PopInternal();
                    if (obj is not Task t) throw new ForthException(ForthErrorCode.CompileError,"AWAIT expects a Task id");
                    await t.ConfigureAwait(false);
                    var tt = t.GetType();
                    if (tt.IsGenericType && !IsResultConsumed(obj))
                    {
                        var resultProp = tt.GetProperty("Result");
                        var resultType = resultProp?.PropertyType;
                        if (resultType?.FullName != "System.Threading.Tasks.VoidTaskResult")
                        {
                            var val = resultProp!.GetValue(t);
                            Push(Normalize(val));
                            // If there is another Task just beneath, keep it on top for consecutive AWAIT
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
                if (tok.Equals("[", StringComparison.Ordinal)) { _isCompiling = false; continue; }
                if (tok.Equals("]", StringComparison.Ordinal)) { _isCompiling = true; continue; }

                if (TryParseNumber(tok, out var num)) { Push(num); continue; }
                if (_dict.TryGetValue(tok, out var w)) { await w.ExecuteAsync(this); continue; }
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                // Compilation mode (building new word)
                if (tok == ";")
                {
                    if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
                        throw new ForthException(ForthErrorCode.CompileError, "No open definition to end");
                    if (_controlStack.Count != 0)
                        throw new ForthException(ForthErrorCode.CompileError, "Unmatched control structure");
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
                    _dict[_currentDefName] = compiled;
                    _isCompiling = false;
                    _currentDefName = null;
                    _currentInstructions = null;
                    continue;
                }
                if (tok.Equals("IF", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new IfFrame()); continue; }
                if (tok.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not IfFrame ifr)
                        throw new ForthException(ForthErrorCode.CompileError, "ELSE without IF");
                    if (ifr.ElsePart is not null) throw new ForthException(ForthErrorCode.CompileError, "Multiple ELSE");
                    ifr.ElsePart = new List<Func<ForthInterpreter, Task>>();
                    ifr.InElse = true;
                    continue;
                }
                if (tok.Equals("THEN", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not IfFrame ifr)
                        throw new ForthException(ForthErrorCode.CompileError, "THEN without IF");
                    _controlStack.Pop();
                    var thenPart = ifr.ThenPart;
                    var elsePart = ifr.ElsePart;
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
                        throw new ForthException(ForthErrorCode.CompileError, "WHILE without BEGIN");
                    if (bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "Multiple WHILE");
                    bf.InWhile = true; continue;
                }
                if (tok.Equals("REPEAT", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT without BEGIN");
                    if (!bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "REPEAT requires WHILE");
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
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL without BEGIN");
                    if (bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "UNTIL after WHILE use REPEAT");
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
                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    _currentInstructions!.Add(async intr =>
                    {
                        EnsureStack(intr,1,"AWAIT");
                        var obj = intr.PopInternal();
                        if (obj is not Task t) throw new ForthException(ForthErrorCode.CompileError,"AWAIT expects a Task id");
                        await t.ConfigureAwait(false);
                        var tt = t.GetType();
                        if (tt.IsGenericType && !intr.IsResultConsumed(obj))
                        {
                            var resultProp = tt.GetProperty("Result");
                            var resultType = resultProp?.PropertyType;
                            if (resultType?.FullName != "System.Threading.Tasks.VoidTaskResult")
                            {
                                var val = resultProp!.GetValue(t);
                                intr.Push(Normalize(val));
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
                if (TryParseNumber(tok, out var lit)) { CurrentList().Add(intr => { intr.Push(lit); return Task.CompletedTask; }); continue; }
                if (_dict.TryGetValue(tok, out var cw)) { CurrentList().Add(async intr => await cw.ExecuteAsync(intr)); continue; }
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }
        return !_exitRequested;
    }

    private static object Normalize(object? val)
    {
        return val switch
        {
            null => 0L,
            int i => (long)i,
            long l => l,
            short s => (long)s,
            byte b => (long)b,
            char c => (long)c,
            bool bo => bo ? 1L : 0L,
            _ => val!
        };
    }

    private static bool TryParseNumber(string token, out long value) => long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    private void InstallPrimitives()
    {
        CorePrimitives.Install(this, _dict);
    }

    // Helpers used by CorePrimitives
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

    private List<Func<ForthInterpreter, Task>> CurrentList()
    {
        if (_controlStack.Count == 0) return _currentInstructions!;
        return _controlStack.Peek().GetCurrentList();
    }

    internal sealed class Word
    {
        private readonly Func<ForthInterpreter, Task> _run;
        public Word(Action<ForthInterpreter> sync) => _run = intr => { sync(intr); return Task.CompletedTask; };
        public Word(Func<ForthInterpreter, Task> asyncRun) => _run = asyncRun;
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
    private sealed class ExitWordException : Exception {}
}
