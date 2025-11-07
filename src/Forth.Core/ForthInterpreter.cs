using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Forth;

public class ForthInterpreter : IForthInterpreter
{
    private readonly List<long> _stack = new();
    private readonly Dictionary<string, Word> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly IForthIO _io;
    private readonly FiberScheduler _scheduler = new();
    // Object heap for future instance support
    private readonly Dictionary<long, object?> _objHeap = new();
    private long _nextObjId = 1;

    // Compile state
    private bool _isCompiling;
    private string? _currentDefName;
    private List<Func<ForthInterpreter, Task>>? _currentInstructions;
    private readonly Stack<CompileFrame> _controlStack = new();

    // Simple variable storage (addresses mapped to values)
    private readonly Dictionary<long, long> _mem = new();
    private long _nextAddr = 1;

    // REPL exit signal
    private bool _exitRequested;

    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        InstallPrimitives();
    }

    public IReadOnlyList<long> Stack => _stack;

    internal long StoreObject(object? obj)
    {
        var id = _nextObjId++;
        _objHeap[id] = obj;
        return id;
    }
    internal object? GetObject(long id)
    {
        _objHeap.TryGetValue(id, out var o);
        return o;
    }
    internal void Push(long v) => _stack.Add(v);
    internal long Pop() => PopInternal();
    internal static void EnsureStack(ForthInterpreter interp, int required, string word)
    {
        if (interp._stack.Count < required)
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
    }

    public bool Interpret(string line) => InterpretAsync(line).GetAwaiter().GetResult();

    public async Task<bool> InterpretAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        var tokens = Tokenizer.Tokenize(line);
        var fiber = _scheduler.CreateFiber();
        var i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i++];
            if (tok.Length == 0) continue;

            // Compilation state handled synchronously (definitions created immediately)
            if (!_isCompiling)
            {
                if (tok == ":")
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected word name after ':'");
                    _currentDefName = tokens[i++];
                    if (string.IsNullOrWhiteSpace(_currentDefName)) throw new ForthException(ForthErrorCode.CompileError, "Invalid word name");
                    _currentInstructions = new List<Func<ForthInterpreter, Task>>();
                    _controlStack.Clear();
                    _isCompiling = true;
                    continue;
                }

                if (tok.Equals("BIND", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 3 >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "BIND requires: type method argCount name");
                    var typeName = tokens[i++];
                    var methodName = tokens[i++];
                    if (!int.TryParse(tokens[i++], NumberStyles.Integer, CultureInfo.InvariantCulture, out var argCount))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid argCount for BIND");
                    var forthName = tokens[i++];
                    var word = ClrBinder.CreateBoundWord(typeName, methodName, argCount);
                    _dict[forthName] = word;
                    continue;
                }
                if (tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 3 >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "BINDASYNC requires: type method argCount name");
                    var typeName = tokens[i++];
                    var methodName = tokens[i++];
                    if (!int.TryParse(tokens[i++], NumberStyles.Integer, CultureInfo.InvariantCulture, out var argCount))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid argCount for BINDASYNC");
                    var forthName = tokens[i++];
                    var word = ClrBinder.CreateBoundTaskWord(typeName, methodName, argCount);
                    _dict[forthName] = word;
                    continue;
                }

                if (tok.Equals("[", StringComparison.Ordinal)) { _isCompiling = false; continue; }
                if (tok.Equals("]", StringComparison.Ordinal)) { _isCompiling = true; continue; }

                if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after VARIABLE");
                    var name = tokens[i++];
                    var addr = _nextAddr++;
                    _mem[addr] = 0;
                    _dict[name] = new Word(interp => { interp.Push(addr); });
                    continue;
                }
                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CONSTANT");
                    var name = tokens[i++];
                    EnsureStack(this, 1, "CONSTANT");
                    var value = PopInternal();
                    _dict[name] = new Word(interp => { interp.Push(value); });
                    continue;
                }
                if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected character after CHAR");
                    var word = tokens[i++];
                    var ch = word.Length > 0 ? word[0] : 0;
                    Push(ch);
                    continue;
                }

                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    // Fiber based await remains
                    Func<ForthInterpreter, Fiber, Task>? instr = null;
                    instr = async (interp,f) =>
                    {
                        EnsureStack(interp, 1, "AWAIT");
                        var id = interp.PopInternal();
                        var obj = interp.GetObject(id) ?? throw new ForthException(ForthErrorCode.MemoryFault, $"No object for id {id}");
                        if (obj is not Task task) throw new ForthException(ForthErrorCode.CompileError, "AWAIT expects a Task id");
                        if (!task.IsCompleted)
                        {
                            // push result on completion then resume fiber with next instruction
                            task.ContinueWith(t =>
                            {
                                if (t.GetType().IsGenericType)
                                {
                                    var val = t.GetType().GetProperty("Result")!.GetValue(t);
                                    ClrBinder.PushValueOrStoreObject(interp, val);
                                }
                            }, TaskScheduler.Default);
                            f.WaitOn(task, _scheduler);
                            return;
                        }
                        if (task.GetType().IsGenericType)
                        {
                            var val = task.GetType().GetProperty("Result")!.GetValue(task);
                            ClrBinder.PushValueOrStoreObject(interp, val);
                        }
                    };
                    fiber.Instructions.Enqueue(instr);
                    continue;
                }
                if (tok.Equals("TASK?", StringComparison.OrdinalIgnoreCase))
                {
                    fiber.Instructions.Enqueue((interp,f) =>
                    {
                        EnsureStack(interp, 1, "TASK?");
                        var id = interp.PopInternal();
                        var obj = interp.GetObject(id);
                        var t = obj as Task;
                        interp.Push(t != null && t.IsCompleted ? 1 : 0);
                        return Task.CompletedTask;
                    });
                    continue;
                }
                if (tok.Equals("YIELD", StringComparison.OrdinalIgnoreCase))
                {
                    fiber.Instructions.Enqueue(async (interp,f) => { await Task.Yield(); });
                    continue;
                }
                if (tok.Equals("SPAWN", StringComparison.OrdinalIgnoreCase))
                {
                    fiber.Instructions.Enqueue((interp,f) =>
                    {
                        EnsureStack(interp, 1, "SPAWN");
                        var id = interp.PopInternal();
                        var obj = interp.GetObject(id) ?? throw new ForthException(ForthErrorCode.MemoryFault, $"No object for id {id}");
                        if (obj is not Task original) throw new ForthException(ForthErrorCode.CompileError, "SPAWN expects a Task id");
                        var spawned = Task.Run(async () => await original.ConfigureAwait(false));
                        var newId = interp.StoreObject(spawned);
                        interp.Push(newId);
                        return Task.CompletedTask;
                    });
                    continue;
                }

                if (TryParseNumber(tok, out var num))
                {
                    Push(num);
                    continue;
                }

                if (_dict.TryGetValue(tok, out var w))
                {
                    await w.ExecuteAsync(this);
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                // Compilation mode (still synchronous building of word body)
                if (tok == ";")
                {
                    if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
                        throw new ForthException(ForthErrorCode.CompileError, "No open definition to end");
                    if (_controlStack.Count != 0)
                        throw new ForthException(ForthErrorCode.CompileError, "Unmatched control structure in definition");
                    var compiledBody = _currentInstructions;
                    var compiled = new Word(async interp =>
                    {
                        try
                        {
                            foreach (var instr in compiledBody)
                                await instr(interp);
                        }
                        catch (ExitWordException) { }
                    });
                    _dict[_currentDefName] = compiled;
                    _isCompiling = false;
                    _currentDefName = null;
                    _currentInstructions = null;
                    continue;
                }
                if (tok.Equals("IF", StringComparison.OrdinalIgnoreCase))
                {
                    _controlStack.Push(new IfFrame());
                    continue;
                }
                if (tok.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not IfFrame ifr)
                        throw new ForthException(ForthErrorCode.CompileError, "ELSE without IF");
                    if (ifr.ElsePart is not null)
                        throw new ForthException(ForthErrorCode.CompileError, "Multiple ELSE in IF");
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
                    Func<ForthInterpreter, Task> compiledIf = async interp =>
                    {
                        EnsureStack(interp, 1, "IF");
                        var flag = interp.PopInternal();
                        if (flag != 0)
                        {
                            foreach (var a in thenPart) await a(interp);
                        }
                        else if (elsePart is not null)
                        {
                            foreach (var a in elsePart) await a(interp);
                        }
                    };
                    CurrentList().Add(compiledIf);
                    continue;
                }
                if (tok.Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
                {
                    _controlStack.Push(new BeginFrame());
                    continue;
                }
                if (tok.Equals("WHILE", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "WHILE without BEGIN");
                    if (bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "Multiple WHILE in loop");
                    bf.InWhile = true;
                    continue;
                }
                if (tok.Equals("REPEAT", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT without BEGIN");
                    if (!bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "REPEAT requires WHILE");
                    _controlStack.Pop();
                    var pre = bf.PrePart;
                    var mid = bf.MidPart;
                    Func<ForthInterpreter, Task> compiledLoop = async interp =>
                    {
                        while (true)
                        {
                            foreach (var a in pre) await a(interp);
                            EnsureStack(interp, 1, "WHILE");
                            var flag = interp.PopInternal();
                            if (flag == 0) break;
                            foreach (var b in mid) await b(interp);
                        }
                    };
                    CurrentList().Add(compiledLoop);
                    continue;
                }
                if (tok.Equals("UNTIL", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL without BEGIN");
                    if (bf.InWhile) throw new ForthException(ForthErrorCode.CompileError, "UNTIL not allowed after WHILE; use REPEAT");
                    _controlStack.Pop();
                    var body = bf.PrePart;
                    Func<ForthInterpreter, Task> compiledUntil = async interp =>
                    {
                        while (true)
                        {
                            foreach (var a in body) await a(interp);
                            EnsureStack(interp, 1, "UNTIL");
                            var flag = interp.PopInternal();
                            if (flag != 0) break;
                        }
                    };
                    CurrentList().Add(compiledUntil);
                    continue;
                }
                if (tok.Equals("AWAIT", StringComparison.OrdinalIgnoreCase))
                {
                    // inside definitions treat as immediate await (blocking) for now
                    _currentInstructions!.Add(async interp =>
                    {
                        EnsureStack(interp,1,"AWAIT");
                        var id = interp.PopInternal();
                        var obj = interp.GetObject(id) ?? throw new ForthException(ForthErrorCode.MemoryFault,$"No object for id {id}");
                        if (obj is not Task task) throw new ForthException(ForthErrorCode.CompileError,"AWAIT expects a Task id");
                        await task.ConfigureAwait(false);
                        if (task.GetType().IsGenericType)
                        {
                            var val = task.GetType().GetProperty("Result")!.GetValue(task);
                            ClrBinder.PushValueOrStoreObject(interp,val);
                        }
                    });
                    continue;
                }
                // Other compile tokens keep previous synchronous logic (numbers, words, control flow etc.)
                if (tok.Equals("LITERAL", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this,1,"LITERAL");
                    var value = PopInternal();
                    _currentInstructions!.Add(interp => { interp.Push(value); return Task.CompletedTask; });
                    continue;
                }
                if (TryParseNumber(tok,out var lit))
                {
                    CurrentList().Add(interp => { interp.Push(lit); return Task.CompletedTask; });
                    continue;
                }
                if (_dict.TryGetValue(tok,out var compiledWord))
                {
                    CurrentList().Add(async interp => await compiledWord.ExecuteAsync(interp));
                    continue;
                }
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }
        // Run scheduled fiber instructions non-blocking
        await _scheduler.RunAsync(this);
        return !_exitRequested;
    }

    private static bool TryParseNumber(string token, out long value)
    {
        return long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private void InstallPrimitives()
    {
        _dict["+"] = new Word(interp => { EnsureStack(interp,2,"+"); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a+b); });
        _dict["-"] = new Word(interp => { EnsureStack(interp,2,"-"); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a-b); });
        _dict["*"] = new Word(interp => { EnsureStack(interp,2,"*"); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a*b); });
        _dict["/"] = new Word(interp => { EnsureStack(interp,2,"/"); var b=interp.PopInternal(); var a=interp.PopInternal(); if(b==0) throw new ForthException(ForthErrorCode.DivideByZero,"Divide by zero"); interp.Push(a/b); });
        _dict["<"] = new Word(interp => { EnsureStack(interp,2,"<"); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a < b ? 1 : 0); });
        _dict["="] = new Word(interp => { EnsureStack(interp,2,"="); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a == b ? 1 : 0); });
        _dict[">"] = new Word(interp => { EnsureStack(interp,2,">"); var b=interp.PopInternal(); var a=interp.PopInternal(); interp.Push(a > b ? 1 : 0); });
        _dict["@"]= new Word(interp => { EnsureStack(interp,1,"@"); var addr=interp.PopInternal(); interp._mem.TryGetValue(addr, out var val); interp.Push(val); });
        _dict["!"]= new Word(interp => { EnsureStack(interp,2,"!"); var addr=interp.PopInternal(); var val=interp.PopInternal(); interp._mem[addr]=val; });
        _dict["ROT"]= new Word(interp => { EnsureStack(interp,3,"ROT"); var last=interp._stack.Count-1; var a=interp._stack[last-2]; var b=interp._stack[last-1]; var c=interp._stack[last]; interp._stack[last-2]=b; interp._stack[last-1]=c; interp._stack[last]=a; });
        _dict["-ROT"]= new Word(interp => { EnsureStack(interp,3,"-ROT"); var last=interp._stack.Count-1; var a=interp._stack[last-2]; var b=interp._stack[last-1]; var c=interp._stack[last]; interp._stack[last-2]=c; interp._stack[last-1]=a; interp._stack[last]=b; });
        _dict["DUP"] = new Word(interp => { EnsureStack(interp,1,"DUP"); interp.Push(interp._stack[^1]); });
        _dict["DROP"] = new Word(interp => { EnsureStack(interp,1,"DROP"); interp._stack.RemoveAt(interp._stack.Count-1); });
        _dict["SWAP"] = new Word(interp => { EnsureStack(interp,2,"SWAP"); var last=interp._stack.Count-1; (interp._stack[last-1],interp._stack[last])=(interp._stack[last],interp._stack[last-1]); });
        _dict["OVER"] = new Word(interp => { EnsureStack(interp,2,"OVER"); interp.Push(interp._stack[^2]); });
        _dict["EXIT"] = new Word(interp => { throw new ExitWordException(); });
        _dict["BYE"] = new Word(interp => { interp._exitRequested = true; });
        _dict["QUIT"] = new Word(interp => { interp._exitRequested = true; });
        _dict["."] = new Word(interp => { EnsureStack(interp,1,"."); var n=interp.PopInternal(); interp._io.PrintNumber(n); });
        _dict["CR"] = new Word(interp => { interp._io.NewLine(); });
        _dict["EMIT"] = new Word(interp => { EnsureStack(interp,1,"EMIT"); var n=interp.PopInternal(); char ch=(char)(n & 0xFFFF); interp._io.Print(ch.ToString()); });
    }

    private long PopInternal()
    {
        var idx = _stack.Count - 1;
        if (idx < 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _stack[idx];
        _stack.RemoveAt(idx);
        return v;
    }

    private List<Func<ForthInterpreter, Task>> CurrentList()
    {
        if (_controlStack.Count == 0) return _currentInstructions!;
        return _controlStack.Peek().GetCurrentList();
    }

    internal sealed class Word
    {
        private readonly Func<ForthInterpreter, Task> _run;
        public Word(Action<ForthInterpreter> run) => _run = interp => { run(interp); return Task.CompletedTask; };
        public Word(Func<ForthInterpreter, Task> run) => _run = run;
        public Task ExecuteAsync(ForthInterpreter interp) => _run(interp);
    }

    private abstract class CompileFrame
    {
        public abstract List<Func<ForthInterpreter, Task>> GetCurrentList();
    }
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
    private sealed class ExitWordException : Exception { }
}
