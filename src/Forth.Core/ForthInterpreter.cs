using System.Globalization;

namespace Forth;

public class ForthInterpreter : IForthInterpreter
{
    private readonly List<long> _stack = new();
    private readonly Dictionary<string, Word> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly IForthIO _io;

    // Compile state
    private bool _isCompiling;
    private string? _currentDefName;
    private List<Action<ForthInterpreter>>? _currentInstructions;
    private readonly Stack<CompileFrame> _controlStack = new();

    // Simple variable storage (addresses mapped to values)
    private readonly Dictionary<long, long> _mem = new();
    private long _nextAddr = 1;

    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        InstallPrimitives();
    }

    public IReadOnlyList<long> Stack => _stack;

    public bool Interpret(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        var tokens = Tokenizer.Tokenize(line);
        var i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i++];
            if (tok.Length == 0) continue;

            if (!_isCompiling)
            {
                if (tok == ":")
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected word name after ':'");
                    _currentDefName = tokens[i++];
                    if (string.IsNullOrWhiteSpace(_currentDefName)) throw new ForthException(ForthErrorCode.CompileError, "Invalid word name");
                    _currentInstructions = new List<Action<ForthInterpreter>>();
                    _controlStack.Clear();
                    _isCompiling = true;
                    continue;
                }

                if (tok.Equals("[", StringComparison.Ordinal))
                {
                    _isCompiling = false;
                    continue;
                }
                if (tok.Equals("]", StringComparison.Ordinal))
                {
                    _isCompiling = true;
                    continue;
                }

                if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after VARIABLE");
                    var name = tokens[i++];
                    var addr = _nextAddr++;
                    _mem[addr] = 0;
                    _dict[name] = new Word(interp => interp._stack.Add(addr));
                    continue;
                }
                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CONSTANT");
                    var name = tokens[i++];
                    EnsureStack(this, 1, "CONSTANT");
                    var value = Pop();
                    _dict[name] = new Word(interp => interp._stack.Add(value));
                    continue;
                }
                if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected character after CHAR");
                    var word = tokens[i++];
                    _stack.Add(word.Length > 0 ? word[0] : 0);
                    continue;
                }

                if (TryParseNumber(tok, out var num))
                {
                    _stack.Add(num);
                    continue;
                }

                if (_dict.TryGetValue(tok, out var w))
                {
                    w.Execute(this);
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                if (tok == ";")
                {
                    if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
                        throw new ForthException(ForthErrorCode.CompileError, "No open definition to end");
                    if (_controlStack.Count != 0)
                        throw new ForthException(ForthErrorCode.CompileError, "Unmatched control structure in definition");

                    var compiledBody = _currentInstructions;
                    var compiled = new Word(run: interp =>
                    {
                        try
                        {
                            foreach (var instr in compiledBody)
                                instr(interp);
                        }
                        catch (ExitWordException)
                        {
                            // early exit from word
                        }
                    });
                    _dict[_currentDefName] = compiled;
                    _isCompiling = false;
                    _currentDefName = null;
                    _currentInstructions = null;
                    continue;
                }

                if (tok.Equals("[", StringComparison.Ordinal))
                {
                    _isCompiling = false;
                    continue;
                }
                if (tok.Equals("]", StringComparison.Ordinal))
                {
                    _isCompiling = true;
                    continue;
                }

                if (tok.Equals("IMMEDIATE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ForthException(ForthErrorCode.CompileError, "IMMEDIATE not supported");
                }

                if (tok.Equals("LITERAL", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureStack(this, 1, "LITERAL");
                    var value = Pop();
                    CurrentList().Add(interp => interp._stack.Add(value));
                    continue;
                }
                if (tok.Equals("[CHAR]", StringComparison.OrdinalIgnoreCase))
                {
                    if (i >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Expected character after [CHAR]");
                    var word = tokens[i++];
                    var ch = word.Length > 0 ? word[0] : (char)0;
                    CurrentList().Add(interp => interp._stack.Add(ch));
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
                    ifr.ElsePart = new List<Action<ForthInterpreter>>();
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
                    Action<ForthInterpreter> compiledIf = interp =>
                    {
                        EnsureStack(interp, 1, "IF");
                        var flag = interp.Pop();
                        if (flag != 0)
                        {
                            foreach (var a in thenPart)
                                a(interp);
                        }
                        else if (elsePart is not null)
                        {
                            foreach (var a in elsePart)
                                a(interp);
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
                    if (bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "Multiple WHILE in loop");
                    bf.InWhile = true;
                    continue;
                }
                if (tok.Equals("REPEAT", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT without BEGIN");
                    if (!bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT requires WHILE");
                    _controlStack.Pop();
                    var pre = bf.PrePart;
                    var mid = bf.MidPart;
                    Action<ForthInterpreter> compiledLoop = interp =>
                    {
                        while (true)
                        {
                            foreach (var a in pre)
                                a(interp);
                            EnsureStack(interp, 1, "WHILE");
                            var flag = interp.Pop();
                            if (flag == 0) break;
                            foreach (var b in mid)
                                b(interp);
                        }
                    };
                    CurrentList().Add(compiledLoop);
                    continue;
                }
                if (tok.Equals("UNTIL", StringComparison.OrdinalIgnoreCase))
                {
                    if (_controlStack.Count == 0 || _controlStack.Peek() is not BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL without BEGIN");
                    if (bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL not allowed after WHILE; use REPEAT");
                    _controlStack.Pop();
                    var body = bf.PrePart;
                    Action<ForthInterpreter> compiledUntil = interp =>
                    {
                        while (true)
                        {
                            foreach (var a in body)
                                a(interp);
                            EnsureStack(interp, 1, "UNTIL");
                            var flag = interp.Pop();
                            if (flag != 0) break;
                        }
                    };
                    CurrentList().Add(compiledUntil);
                    continue;
                }

                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase) || tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ForthException(ForthErrorCode.CompileError, $"{tok} not supported inside definitions");
                }

                if (TryParseNumber(tok, out var lit))
                {
                    CurrentList().Add(interp => interp._stack.Add(lit));
                    continue;
                }

                if (_dict.TryGetValue(tok, out var compiledWord))
                {
                    CurrentList().Add(interp => compiledWord.Execute(interp));
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }

        return true;
    }

    private List<Action<ForthInterpreter>> CurrentList()
    {
        if (_controlStack.Count == 0)
            return _currentInstructions!;
        return _controlStack.Peek().GetCurrentList();
    }

    private static bool TryParseNumber(string token, out long value)
    {
        return long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private void InstallPrimitives()
    {
        _dict["+"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "+");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a + b);
        });
        _dict["-"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "-");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a - b);
        });
        _dict["*"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "*");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a * b);
        });
        _dict["/"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "/");
            var b = interp.Pop();
            var a = interp.Pop();
            if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
            interp._stack.Add(a / b);
        });

        _dict["<"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "<");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a < b ? 1 : 0);
        });
        _dict["="] = new Word(interp =>
        {
            EnsureStack(interp, 2, "=");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a == b ? 1 : 0);
        });
        _dict[">"] = new Word(interp =>
        {
            EnsureStack(interp, 2, ">");
            var b = interp.Pop();
            var a = interp.Pop();
            interp._stack.Add(a > b ? 1 : 0);
        });

        _dict["@"] = new Word(interp =>
        {
            EnsureStack(interp, 1, "@");
            var addr = interp.Pop();
            interp._mem.TryGetValue(addr, out var val);
            interp._stack.Add(val);
        });
        _dict["!"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "!");
            var addr = interp.Pop();
            var val = interp.Pop();
            interp._mem[addr] = val;
        });

        _dict["DUP"] = new Word(interp =>
        {
            EnsureStack(interp, 1, "DUP");
            interp._stack.Add(interp._stack[^1]);
        });
        _dict["DROP"] = new Word(interp =>
        {
            EnsureStack(interp, 1, "DROP");
            interp._stack.RemoveAt(interp._stack.Count - 1);
        });
        _dict["SWAP"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "SWAP");
            var last = interp._stack.Count - 1;
            ( interp._stack[last - 1], interp._stack[last] ) = ( interp._stack[last], interp._stack[last - 1] );
        });
        _dict["OVER"] = new Word(interp =>
        {
            EnsureStack(interp, 2, "OVER");
            interp._stack.Add(interp._stack[^2]);
        });
        _dict["ROT"] = new Word(interp =>
        {
            EnsureStack(interp, 3, "ROT");
            var last = interp._stack.Count - 1;
            var a = interp._stack[last - 2];
            var b = interp._stack[last - 1];
            var c = interp._stack[last];
            interp._stack[last - 2] = b;
            interp._stack[last - 1] = c;
            interp._stack[last] = a;
        });
        _dict["-ROT"] = new Word(interp =>
        {
            EnsureStack(interp, 3, "-ROT");
            var last = interp._stack.Count - 1;
            var a = interp._stack[last - 2];
            var b = interp._stack[last - 1];
            var c = interp._stack[last];
            interp._stack[last - 2] = c;
            interp._stack[last - 1] = a;
            interp._stack[last] = b;
        });

        _dict["EXIT"] = new Word(interp =>
        {
            throw new ExitWordException();
        });

        _dict["."] = new Word(interp =>
        {
            EnsureStack(interp, 1, ".");
            var n = interp.Pop();
            interp._io.PrintNumber(n);
        });
        _dict["CR"] = new Word(interp =>
        {
            interp._io.NewLine();
        });
        _dict["EMIT"] = new Word(interp =>
        {
            EnsureStack(interp, 1, "EMIT");
            var n = interp.Pop();
            char ch = (char)(n & 0xFFFF);
            interp._io.Print(ch.ToString());
        });
    }

    private static void EnsureStack(ForthInterpreter interp, int required, string word)
    {
        if (interp._stack.Count < required)
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
    }

    private long Pop()
    {
        var idx = _stack.Count - 1;
        if (idx < 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _stack[idx];
        _stack.RemoveAt(idx);
        return v;
    }

    private sealed class Word
    {
        private readonly Action<ForthInterpreter> _run;
        public Word(Action<ForthInterpreter> run)
        {
            _run = run;
        }
        public void Execute(ForthInterpreter interp) => _run(interp);
    }

    private abstract class CompileFrame
    {
        public abstract List<Action<ForthInterpreter>> GetCurrentList();
    }

    private sealed class IfFrame : CompileFrame
    {
        public List<Action<ForthInterpreter>> ThenPart { get; } = new();
        public List<Action<ForthInterpreter>>? ElsePart { get; set; }
        public bool InElse { get; set; }
        public override List<Action<ForthInterpreter>> GetCurrentList() => InElse ? (ElsePart ??= new()) : ThenPart;
    }

    private sealed class BeginFrame : CompileFrame
    {
        public List<Action<ForthInterpreter>> PrePart { get; } = new();
        public List<Action<ForthInterpreter>> MidPart { get; } = new();
        public bool InWhile { get; set; }
        public override List<Action<ForthInterpreter>> GetCurrentList() => InWhile ? MidPart : PrePart;
    }

    private sealed class ExitWordException : Exception { }
}
