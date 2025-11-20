using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive(".", HelpString = "Print top of stack as a number ( n -- )")]
    private static Task Prim_Dot(ForthInterpreter i) { i.EnsureStack(1, "."); var n = ToLong(i.PopInternal()); i.WriteNumber(n); return Task.CompletedTask; }

    [Primitive(".S", HelpString = "Print stack contents in angle brackets")]
    private static Task Prim_DotS(ForthInterpreter i)
    {
        var items = i.Stack;
        var sb = new StringBuilder();
        sb.Append('<').Append(items.Count).Append("> ");
        for (int idx = 0; idx < items.Count; idx++)
        {
            if (idx > 0) sb.Append(' ');
            var o = items[idx];
            switch (o)
            {
                case long l: sb.Append(l); break;
                case int ii: sb.Append(ii); break;
                case short s: sb.Append((long)s); break;
                case byte b: sb.Append((long)b); break;
                case char ch: sb.Append((int)ch); break;
                case bool bo: sb.Append(bo ? 1 : 0); break;
                default:
                    sb.Append(o?.ToString() ?? "null");
                    break;
            }
        }
        i.WriteText(sb.ToString());
        return Task.CompletedTask;
    }

    [Primitive("CR", HelpString = "Emit newline")]
    private static Task Prim_CR(ForthInterpreter i) { i.NewLine(); return Task.CompletedTask; }

    [Primitive("EMIT", HelpString = "Emit character with given code ( n -- )")]
    private static Task Prim_EMIT(ForthInterpreter i) { i.EnsureStack(1, "EMIT"); var n = ToLong(i.PopInternal()); char ch = (char)(n & 0xFFFF); i.WriteText(ch.ToString()); return Task.CompletedTask; }

    [Primitive("TYPE", HelpString = "TYPE ( c-addr u -- ) - write a counted string to output")]
    private static Task Prim_TYPE(ForthInterpreter i) { i.EnsureStack(1, "TYPE"); var obj = i.PopInternal(); if (obj is string s) { i.WriteText(s); return Task.CompletedTask; } else throw new ForthException(ForthErrorCode.TypeError, "TYPE expects a string"); }

    [Primitive("WORDS", HelpString = "List all available word names")]
    private static Task Prim_WORDS(ForthInterpreter i) { var names = i.GetAllWordNames(); var sb = new StringBuilder(); bool first = true; foreach (var n in names) { if (!first) sb.Append(' '); first = false; sb.Append(n); } i.WriteText(sb.ToString()); return Task.CompletedTask; }

    [Primitive("KEY", HelpString = "KEY ( -- char|-1 ) - read a single key code or -1 for EOF")]
    private static Task Prim_KEY(ForthInterpreter i)
    {
        var kc = i.ReadKey();
        i.Push((long)kc);
        return Task.CompletedTask;
    }

    [Primitive("KEY?", HelpString = "KEY? ( -- flag ) - push -1 if key available else 0")]
    private static Task Prim_KEYQ(ForthInterpreter i)
    {
        var available = i.KeyAvailable();
        i.Push(available ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("ACCEPT", HelpString = "ACCEPT ( c-addr u -- actual ) - read a line into buffer or push string")]
    private static Task Prim_ACCEPT(ForthInterpreter i)
    {
        // Simplified: push the read line as string (ignore c-addr/u semantics)
        i.EnsureStack(2, "ACCEPT");
        var max = ToLong(i.PopInternal());
        var addrObj = i.PopInternal();
        var line = i.ReadLineFromIO() ?? string.Empty;
        if (line.Length > max) line = line.Substring(0, (int)max);
        i.Push(line);
        i.Push((long)line.Length);
        return Task.CompletedTask;
    }
}
