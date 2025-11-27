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
    { var items = i.Stack; var sb = new StringBuilder(); sb.Append('<').Append(items.Count).Append("> "); for (int idx = 0; idx < items.Count; idx++) { if (idx > 0) sb.Append(' '); var o = items[idx]; switch (o) { case long l: sb.Append(l); break; case int ii: sb.Append(ii); break; case short s: sb.Append((long)s); break; case byte b: sb.Append((long)b); break; case char ch: sb.Append((int)ch); break; case bool bo: sb.Append(bo ? -1 : 0); break; default: sb.Append(o?.ToString() ?? "null"); break; } } i.WriteText(sb.ToString()); return Task.CompletedTask; }

    [Primitive("CR", HelpString = "Emit newline")]
    private static Task Prim_CR(ForthInterpreter i) { i.NewLine(); return Task.CompletedTask; }

    [Primitive("BL", HelpString = "BL ( -- 32 ) ASCII space character")]
    private static Task Prim_BL(ForthInterpreter i) { i.Push(32L); return Task.CompletedTask; }

    [Primitive("EMIT", HelpString = "Emit character with given code ( n -- )")]
    private static Task Prim_EMIT(ForthInterpreter i) { i.EnsureStack(1, "EMIT"); var n = ToLong(i.PopInternal()); char ch = (char)(n & 0xFFFF); i.WriteText(ch.ToString()); return Task.CompletedTask; }

    [Primitive("TYPE", HelpString = "TYPE ( c-addr u | string -- ) - write string data to output")]
    private static Task Prim_TYPE(ForthInterpreter i)
    {
        if (i.Stack.Count >= 2 && IsNumeric(i.Stack[^1]) && (IsNumeric(i.Stack[^2]) || i.Stack[^2] is string))
        {
            var u = ToLong(i.PopInternal());
            var addrOrStr = i.PopInternal();
            if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "TYPE negative length");
            if (addrOrStr is string sstr)
            { var sliceLen = (int)u <= sstr.Length ? (int)u : sstr.Length; i.WriteText(sliceLen == sstr.Length ? sstr : sstr.Substring(0, sliceLen)); return Task.CompletedTask; }
            if (IsNumeric(addrOrStr))
            { var a = ToLong(addrOrStr); i.WriteText(i.ReadMemoryString(a, u)); return Task.CompletedTask; }
            throw new ForthException(ForthErrorCode.TypeError, "TYPE address/string expected before length");
        }
        i.EnsureStack(1, "TYPE");
        var obj = i.PopInternal();
        if (obj is string s) { i.WriteText(s); return Task.CompletedTask; }
        throw new ForthException(ForthErrorCode.TypeError, "TYPE expects string or (addr len)");
    }

    internal static bool IsNumeric(object o) => o is long || o is int || o is short || o is byte;

    [Primitive("WORDS", HelpString = "List all available word names")]
    private static Task Prim_WORDS(ForthInterpreter i) { var names = i.GetAllWordNames(); var sb = new StringBuilder(); bool first = true; foreach (var n in names) { if (!first) sb.Append(' '); first = false; sb.Append(n); } i.WriteText(sb.ToString()); return Task.CompletedTask; }

    [Primitive("KEY", HelpString = "KEY ( -- char|-1 ) - read a single key code or -1 for EOF")]
    private static Task Prim_KEY(ForthInterpreter i) { var kc = i.ReadKey(); i.Push((long)kc); return Task.CompletedTask; }

    [Primitive("KEY?", HelpString = "KEY? ( -- flag ) - push -1 if key available else 0")]
    private static Task Prim_KEYQ(ForthInterpreter i) { var available = i.KeyAvailable(); i.Push(available ? -1L : 0L); return Task.CompletedTask; }

    // ANS-style READ-LINE: exclude CR/LF terminators
    [Primitive("READ-LINE", HelpString = "READ-LINE ( c-addr u -- actual ) - read a line excluding CR/LF terminators")]
    private static Task Prim_READLINE(ForthInterpreter i)
    {
        i.EnsureStack(2, "READ-LINE");
        var u = (int)ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        int len = 0;
        while (len < u)
        {
            int ch = i.ReadKey();
            if (ch == -1 || ch == '\r' || ch == '\n') break;
            i.MemSet(addr + len, (long)ch);
            len++;
        }
        i.Push((long)len);
        return Task.CompletedTask;
    }

    // ANS-style ACCEPT: exclude CR/LF terminators
    [Primitive("ACCEPT", HelpString = "ACCEPT ( addr u -- u ) - read line excluding CR/LF terminators")]
    private static Task Prim_ACCEPT(ForthInterpreter i)
    {
        i.EnsureStack(2, "ACCEPT");
        var u = (int)ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        int len = 0;
        while (len < u)
        {
            int ch = i.ReadKey();
            if (ch == -1 || ch == '\r' || ch == '\n') break;
            i.MemSet(addr + len, (long)ch);
            len++;
        }
        i.Push((long)len);
        return Task.CompletedTask;
    }

    // EXPECT alias of ACCEPT
    [Primitive("EXPECT", HelpString = "EXPECT ( addr u -- u ) - alias of ACCEPT")]
    private static Task Prim_EXPECT(ForthInterpreter i) => Prim_ACCEPT(i);

    [Primitive("WORD", HelpString = "WORD ( char \"<chars>ccc<char>\" -- c-addr ) - parse word delimited by char")]
    private static Task Prim_WORD(ForthInterpreter i)
    {
        i.EnsureStack(1, "WORD");
        var delim = (char)ToLong(i.PopInternal());
        var source = i.CurrentSource ?? string.Empty;
        i.MemTryGet(i.InAddr, out var inVal);
        var inIndex = (int)(long)inVal;
        // Skip leading delimiters
        while (inIndex < source.Length && source[inIndex] == delim) inIndex++;
        var start = inIndex;
        // Collect until delimiter or end
        while (inIndex < source.Length && source[inIndex] != delim) inIndex++;
        var word = source.Substring(start, inIndex - start);
        // Advance >IN past the delimiter if present
        var newIn = inIndex < source.Length ? inIndex + 1 : inIndex;
        i.MemSet(i.InAddr, (long)newIn);
        // Store as counted string at HERE
        var addr = i.AllocateCountedString(word);
        i.Push(addr);
        return Task.CompletedTask;
    }
}
