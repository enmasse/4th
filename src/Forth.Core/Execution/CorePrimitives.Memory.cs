using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("@", HelpString = "@ ( addr -- value ) - fetch cell at address")]
    private static Task Prim_At(ForthInterpreter i) { i.EnsureStack(1, "@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); return Task.CompletedTask; }

    [Primitive("!", HelpString = "! ( x addr -- ) - store x at address")]
    private static Task Prim_Bang(ForthInterpreter i) { i.EnsureStack(2, "!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); i.MemSet(addr, val); i._lastStoreAddr = addr; i._lastStoreValue = val; return Task.CompletedTask; }

    [Primitive("+!", HelpString = "+! ( x addr -- ) - add x to cell at addr")]
    private static Task Prim_PlusBang(ForthInterpreter i) { i.EnsureStack(2, "+!"); var addr = ToLong(i.PopInternal()); var add = ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, cur + add); return Task.CompletedTask; }

    [Primitive("C!", HelpString = "C! ( x addr -- ) - store low byte of x at addr")]
    private static Task Prim_CBang(ForthInterpreter i) { i.EnsureStack(2, "C!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); var b = (long)((byte)val); i.MemSet(addr, b); return Task.CompletedTask; }

    [Primitive("C@", HelpString = "C@ ( addr -- byte ) - fetch low byte at address")]
    private static Task Prim_CAt(ForthInterpreter i) { i.EnsureStack(1, "C@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push((long)((byte)v)); return Task.CompletedTask; }

    [Primitive("MOVE", HelpString = "MOVE ( src dst u -- ) - copy u bytes from src to dst")]
    private static Task Prim_MOVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "MOVE");
        var u = ToLong(i.PopInternal());
        var dst = ToLong(i.PopInternal());
        var src = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative MOVE length");
        if (u == 0) return Task.CompletedTask;
        if (src < dst && src + u > dst)
        {
            for (long k = u - 1; k >= 0; k--)
            {
                i.MemTryGet(src + k, out var v);
                i.MemSet(dst + k, (long)((byte)v));
            }
            return Task.CompletedTask;
        }
        for (long k = 0; k < u; k++)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)((byte)v));
        }
        return Task.CompletedTask;
    }

    [Primitive("FILL", HelpString = "FILL ( addr u ch -- ) - fill u bytes at addr with ch")]
    private static Task Prim_FILL(ForthInterpreter i) { i.EnsureStack(3, "FILL"); var ch = ToLong(i.PopInternal()); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length"); var b = (long)((byte)ch); for (long k = 0; k < u; k++) i.MemSet(addr + k, b); return Task.CompletedTask; }

    [Primitive("ERASE", HelpString = "ERASE ( addr u -- ) - set u bytes at addr to zero")]
    private static Task Prim_ERASE(ForthInterpreter i) { i.EnsureStack(2, "ERASE"); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length"); for (long k = 0; k < u; k++) i.MemSet(addr + k, 0); return Task.CompletedTask; }

    [Primitive("DUMP", HelpString = "DUMP ( addr u -- ) - print u bytes from addr in hex")]
    private static Task Prim_DUMP(ForthInterpreter i)
    {
        i.EnsureStack(2, "DUMP");
        var u = ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative DUMP length");
        var sb = new StringBuilder();
        for (long k = 0; k < u; k++)
        {
            if (k > 0) sb.Append(' ');
            i.MemTryGet(addr + k, out var v);
            var b = (byte)v;
            sb.Append(b.ToString("X2"));
        }
        i.WriteText(sb.ToString());
        return Task.CompletedTask;
    }

    [Primitive("COUNT", HelpString = "COUNT ( c-addr -- c-addr u ) - return counted string address and length")]
    private static Task Prim_COUNT(ForthInterpreter i)
    {
        i.EnsureStack(1, "COUNT");
        var obj = i.PopInternal();
        switch (obj)
        {
            case string s:
                i.Push(s);
                i.Push((long)s.Length);
                return Task.CompletedTask;
            case long addr:
                i.MemTryGet(addr, out var v);
                var len = (long)((byte)v);
                i.Push(addr + 1);
                i.Push(len);
                return Task.CompletedTask;
            default:
                throw new ForthException(ForthErrorCode.TypeError, "COUNT expects a string or address");
        }
    }

    [Primitive(",", HelpString = ", ( x -- ) - append cell to dictionary and advance here")]
    private static Task Prim_Comma(ForthInterpreter i) { i.EnsureStack(1, ","); var v = ToLong(i.PopInternal()); i._mem[i._nextAddr++] = v; return Task.CompletedTask; }

    [Primitive("ALLOT", HelpString = "ALLOT ( u -- ) - reserve u cells in dictionary")]
    private static Task Prim_ALLOT(ForthInterpreter i) { i.EnsureStack(1, "ALLOT"); var cells = ToLong(i.PopInternal()); if (cells < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size"); for (long k = 0; k < cells; k++) i._mem[i._nextAddr++] = 0; return Task.CompletedTask; }

    // HERE: push the current dictionary allocation pointer (address of next free cell)
    [Primitive("HERE", HelpString = "HERE ( -- addr ) - push current dictionary allocation pointer")]
    private static Task Prim_HERE(ForthInterpreter i) { i.Push(i._nextAddr); return Task.CompletedTask; }

    [Primitive("SOURCE", HelpString = "SOURCE ( -- addr u ) - return address and length of current input buffer")]
    private static Task Prim_SOURCE(ForthInterpreter i)
    {
        var src = i.CurrentSource ?? string.Empty;
        var addr = i.AllocateCountedString(src);
        i.Push(ForthValue.FromLong(addr));
        i.Push(ForthValue.FromLong(src.Length));
        return Task.CompletedTask;
    }

    [Primitive(">IN", HelpString = ">IN ( -- addr ) - address of >IN index cell")]
    private static Task Prim_IN(ForthInterpreter i)
    {
        i.Push(i.InAddr);
        return Task.CompletedTask;
    }

#if DEBUG
        [Primitive("LAST-STORE", HelpString = "LAST-STORE ( -- addr val ) - diagnostics: last ! store")]
        private static Task Prim_LAST_STORE(ForthInterpreter i)
        {
            i.Push(i._lastStoreAddr);
            i.Push(i._lastStoreValue);
            return Task.CompletedTask;
        }
#endif
}
