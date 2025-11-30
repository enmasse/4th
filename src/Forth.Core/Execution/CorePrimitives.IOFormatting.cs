using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive(".", Category = "Arithmetic", HelpString = ". ( n -- ) - print top of stack as a number")]
    private static Task Prim_Dot(ForthInterpreter i) { i.EnsureStack(1, "."); var n = ToLong(i.PopInternal()); i.WriteNumber(n); return Task.CompletedTask; }

    [Primitive("D.", Category = "Arithmetic", HelpString = "D. ( d -- ) - print double-cell number")]
    private static Task Prim_DDot(ForthInterpreter i)
    {
        i.EnsureStack(2, "D.");
        var high = ToLong(i.PopInternal());
        var low = ToLong(i.PopInternal());
        var d = (new System.Numerics.BigInteger(high) << 64) | new System.Numerics.BigInteger((ulong)low);
        i.WriteText(d.ToString());
        return Task.CompletedTask;
    }

    [Primitive(".S", HelpString = "Print stack contents in angle brackets")]
    private static Task Prim_DotS(ForthInterpreter i)
    { var items = i.Stack; var sb = new StringBuilder(); sb.Append('<').Append(items.Count).Append("> "); for (int idx = 0; idx < items.Count; idx++) { if (idx > 0) sb.Append(' '); var o = items[idx]; switch (o) { case long l: sb.Append(l); break; case int ii: sb.Append(ii); break; case short s: sb.Append((long)s); break; case byte b: sb.Append((long)b); break; case char ch: sb.Append((int)ch); break; case bool bo: sb.Append(bo ? -1 : 0); break; default: sb.Append(o?.ToString() ?? "null"); break; } } i.WriteText(sb.ToString()); return Task.CompletedTask; }

    [Primitive(".[S]", HelpString = "Print stack contents in square brackets")]
    private static Task Prim_DotBracketS(ForthInterpreter i)
    { var items = i.Stack; var sb = new StringBuilder(); sb.Append('[').Append(items.Count).Append("] "); for (int idx = 0; idx < items.Count; idx++) { if (idx > 0) sb.Append(' '); var o = items[idx]; switch (o) { case long l: sb.Append(l); break; case int ii: sb.Append(ii); break; case short s: sb.Append((long)s); break; case byte b: sb.Append((long)b); break; case char ch: sb.Append((int)ch); break; case bool bo: sb.Append(bo ? -1 : 0); break; default: sb.Append(o?.ToString() ?? "null"); break; } } i.WriteText(sb.ToString()); return Task.CompletedTask; }

    [Primitive("CR", HelpString = "Emit newline")]
    private static Task Prim_CR(ForthInterpreter i) { i.NewLine(); return Task.CompletedTask; }

    [Primitive("BL", HelpString = "BL ( -- 32 ) ASCII space character")]
    private static Task Prim_BL(ForthInterpreter i) { i.Push(32L); return Task.CompletedTask; }

    [Primitive("EMIT", HelpString = "Emit character with given code ( n -- )")]
    private static Task Prim_EMIT(ForthInterpreter i) { i.EnsureStack(1, "EMIT"); var n = ToLong(i.PopInternal()); char ch = (char)(n & 0xFFFF); i.WriteText(ch.ToString()); return Task.CompletedTask; }

    [Primitive("TYPE", HelpString = "TYPE ( c-addr u | string -- ) - write string data to output")]
    private static Task Prim_TYPE(ForthInterpreter i)
    {
        if (i.Stack.Count == 0)
            throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow in TYPE");
            
        if (i.Stack.Count >= 2 && IsNumeric(i.Stack[^1]) && (IsNumeric(i.Stack[^2]) || i.Stack[^2] is string || i.Stack[^2] is Word))
        {
            var u = ToLong(i.PopInternal());
            var addrOrStr = i.PopInternal();
            if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "TYPE negative length");
            if (addrOrStr is string sstr)
            { var sliceLen = (int)u <= sstr.Length ? (int)u : sstr.Length; i.WriteText(sliceLen == sstr.Length ? sstr : sstr.Substring(0, sliceLen)); return Task.CompletedTask; }
            if (IsNumeric(addrOrStr))
            { var a = ToLong(addrOrStr); i.WriteText(i.ReadMemoryString(a, u)); return Task.CompletedTask; }
            if (addrOrStr is Word w)
            {
                // If the word has a known body address, use it directly
                if (w.BodyAddr.HasValue)
                {
                    i.WriteText(i.ReadMemoryString(w.BodyAddr.Value, u));
                    return Task.CompletedTask;
                }
                // Otherwise, execute the word to obtain address or (addr u) pair
                int before = i.Stack.Count;
                w.ExecuteAsync(i).GetAwaiter().GetResult();
                int added = i.Stack.Count - before;
                if (added <= 0)
                    throw new ForthException(ForthErrorCode.TypeError, "TYPE address word did not push a result");
                if (added >= 2 && IsNumeric(i.Stack[^1]) && IsNumeric(i.Stack[^2]))
                {
                    var u2 = ToLong(i.PopInternal());
                    var a2 = ToLong(i.PopInternal());
                    // Prefer the length from the pair just produced
                    i.WriteText(i.ReadMemoryString(a2, u2));
                    return Task.CompletedTask;
                }
                // Single value: treat as address
                var addrVal = i.PopInternal();
                var a = ToLong(addrVal);
                i.WriteText(i.ReadMemoryString(a, u));
                return Task.CompletedTask;
            }
            throw new ForthException(ForthErrorCode.TypeError, "TYPE address/string expected before length");
        }
        // Single string on stack
        var obj = i.PopInternal();
        if (obj is string s) { i.WriteText(s); return Task.CompletedTask; }
        throw new ForthException(ForthErrorCode.TypeError, "TYPE expects string or (addr len)");
    }

    internal static bool IsNumeric(object o) => o is long || o is int || o is short || o is byte || o is double || o is char || o is bool;

    private static long ResolveWordAddress(ForthInterpreter i, Word w)
    {
        int before = i.Stack.Count;
        w.ExecuteAsync(i).GetAwaiter().GetResult();
        if (i.Stack.Count <= before)
            throw new ForthException(ForthErrorCode.TypeError, "Address word did not push a result");
        var addrVal = i.PopInternal();
        return ForthInterpreter.ToLong(addrVal);
    }

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

    [Primitive("-TRAILING", HelpString = "-TRAILING ( c-addr u1 -- c-addr u2 ) - remove trailing spaces from string")]
    private static Task Prim_Trailing(ForthInterpreter i)
    {
        object addr;
        long u1;
        if (i.Stack.Count >= 2 && IsNumeric(i.Stack[^1]) && (IsNumeric(i.Stack[^2]) || i.Stack[^2] is string))
        {
            u1 = ToLong(i.PopInternal());
            addr = i.PopInternal();
        }
        else if (i.Stack.Count >= 1 && i.Stack[^1] is string s)
        {
            addr = i.PopInternal();
            u1 = s.Length;
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, "-TRAILING expects string or (addr len)");
        }
        if (u1 < 0) throw new ForthException(ForthErrorCode.TypeError, "-TRAILING negative length");
        string str;
        if (addr is string sstr)
        {
            str = u1 <= sstr.Length ? sstr.Substring(0, (int)u1) : sstr;
        }
        else if (IsNumeric(addr))
        {
            var a = ToLong(addr);
            str = i.ReadMemoryString(a, u1);
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, "-TRAILING expects string or (addr len)");
        }
        // Trim trailing spaces
        var trimmedLen = str.Length;
        while (trimmedLen > 0 && str[trimmedLen - 1] == ' ') trimmedLen--;
        i.Push(addr);
        i.Push((long)trimmedLen);
        return Task.CompletedTask;
    }

    [Primitive("SEARCH", HelpString = "SEARCH ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 flag ) - search for substring")]
    private static Task Prim_Search(ForthInterpreter i)
    {
        i.EnsureStack(4, "SEARCH");
        var u2 = ToLong(i.PopInternal());
        var addr2 = ToLong(i.PopInternal());
        var u1 = ToLong(i.PopInternal());
        var addr1 = ToLong(i.PopInternal());
        var str1 = i.ReadMemoryString(addr1, u1);
        var str2 = i.ReadMemoryString(addr2, u2);
        var index = str1.IndexOf(str2);
        if (index >= 0)
        {
            var addr3 = addr1 + index;
            var u3 = u1 - index;
            i.Push(addr3);
            i.Push(u3);
            i.Push(-1L); // true
        }
        else
        {
            i.Push(addr1);
            i.Push(u1);
            i.Push(0L); // false
        }
        return Task.CompletedTask;
    }

    [Primitive("REFILL", HelpString = "REFILL ( -- flag ) - refill the input buffer from the input source")]
    private static Task Prim_REFILL(ForthInterpreter i)
    {
        var line = i.ReadLineFromIO();
        if (line == null)
        {
            i.Push(0L);
        }
        else
        {
            i.RefillSource(line);
            i.Push(-1L);
        }
        return Task.CompletedTask;
    }

    [Primitive("/STRING", HelpString = "/STRING ( c-addr1 u1 n -- c-addr2 u2 ) - adjust string by n characters")]
    private static Task Prim_SlashString(ForthInterpreter i)
    {
        i.EnsureStack(3, "/STRING");
        var n = ToLong(i.PopInternal());
        var u1 = ToLong(i.PopInternal());
        var addr1 = ToLong(i.PopInternal());
        i.Push(addr1 + n);
        i.Push(u1 - n);
        return Task.CompletedTask;
    }

    [Primitive("PAGE", HelpString = "PAGE ( -- ) - clear the screen")]
    private static Task Prim_PAGE(ForthInterpreter i) { try { Console.Clear(); } catch { } return Task.CompletedTask; }

    [Primitive("TIME&DATE", HelpString = "TIME&DATE ( -- sec min hour day month year ) - return the current time and date")]
    private static Task Prim_TIME_DATE(ForthInterpreter i)
    {
        var now = DateTime.Now;
        i.Push((long)now.Second);
        i.Push((long)now.Minute);
        i.Push((long)now.Hour);
        i.Push((long)now.Day);
        i.Push((long)now.Month);
        i.Push((long)now.Year);
        return Task.CompletedTask;
    }

    [Primitive("AT-XY", HelpString = "AT-XY ( col row -- ) - position cursor at column and row")]
    private static Task Prim_AT_XY(ForthInterpreter i)
    {
        i.EnsureStack(2, "AT-XY");
        var row = (int)ToLong(i.PopInternal());
        var col = (int)ToLong(i.PopInternal());
        try { Console.SetCursorPosition(col, row); } catch { }
        return Task.CompletedTask;
    }

    [Primitive("MS", HelpString = "MS ( u -- ) - wait u milliseconds")]
    private static async Task Prim_MS(ForthInterpreter i)
    {
        i.EnsureStack(1, "MS");
        var u = (int)ToLong(i.PopInternal());
        await Task.Delay(u);
    }

    [Primitive("SOURCE-ID", HelpString = "SOURCE-ID ( -- 0 | -1 | fileid ) - return the input source identifier")]
    private static Task Prim_SOURCE_ID(ForthInterpreter i)
    {
        i.Push(i.SourceId);
        return Task.CompletedTask;
    }

    [Primitive("SLITERAL", IsImmediate = true, HelpString = "SLITERAL ( c-addr1 u -- ) ( compiling: -- c-addr2 u ) - compile string literal")]
    private static Task Prim_SLITERAL(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "SLITERAL can only be used in definitions");
        i.EnsureStack(2, "SLITERAL");
        var u = ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        var str = i.ReadMemoryString(addr, u);
        i.CurrentList().Add(ii =>
        {
            var a = ii.AllocateCountedString(str);
            ii.Push(a + 1);
            ii.Push((long)str.Length);
            return Task.CompletedTask;
        });
        return Task.CompletedTask;
    }

    [Primitive("SAVE-INPUT", HelpString = "SAVE-INPUT ( -- xn ... x1 n ) - save current input source state")]
    private static Task Prim_SAVE_INPUT(ForthInterpreter i)
    {
        i.MemTryGet(i.InAddr, out var inVal);
        i.Push(i.SourceId);
        i.Push(inVal);
        i.Push((long)i._tokenIndex);
        i.Push(i._currentSource ?? "");
        i.Push(4L);
        return Task.CompletedTask;
    }

    [Primitive("RESTORE-INPUT", HelpString = "RESTORE-INPUT ( xn ... x1 n -- flag ) - restore input source state")]
    private static Task Prim_RESTORE_INPUT(ForthInterpreter i)
    {
        i.EnsureStack(1, "RESTORE-INPUT");
        var n = ToLong(i.PopInternal());
        if (n != 4)
        {
            i.Push(-1L);
            return Task.CompletedTask;
        }
        var source = (string)i.PopInternal();
        var index = (int)ToLong(i.PopInternal());
        var inVal = i.PopInternal();
        var id = ToLong(i.PopInternal());
        if (id != i.SourceId)
        {
            i.Push(-1L);
            return Task.CompletedTask;
        }
        i._currentSource = source;
        i._tokenIndex = index;
        i._tokens = Tokenizer.Tokenize(source);
        i._mem[i.InAddr] = ToLong(inVal);
        i.Push(0L);
        return Task.CompletedTask;
    }
}
