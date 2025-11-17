using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("@")]
    private static Task Prim_At(ForthInterpreter i) { i.EnsureStack(1, "@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); return Task.CompletedTask; }

    [Primitive("!")]
    private static Task Prim_Bang(ForthInterpreter i) { i.EnsureStack(2, "!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); i.MemSet(addr, val); return Task.CompletedTask; }

    [Primitive("+!")]
    private static Task Prim_PlusBang(ForthInterpreter i) { i.EnsureStack(2, "+!"); var addr = ToLong(i.PopInternal()); var add = ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, cur + add); return Task.CompletedTask; }

    [Primitive("C!")]
    private static Task Prim_CBang(ForthInterpreter i) { i.EnsureStack(2, "C!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); var b = (long)((byte)val); i.MemSet(addr, b); return Task.CompletedTask; }

    [Primitive("C@")]
    private static Task Prim_CAt(ForthInterpreter i) { i.EnsureStack(1, "C@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push((long)((byte)v)); return Task.CompletedTask; }

    [Primitive("MOVE")]
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

    [Primitive("FILL")]
    private static Task Prim_FILL(ForthInterpreter i) { i.EnsureStack(3, "FILL"); var ch = ToLong(i.PopInternal()); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length"); var b = (long)((byte)ch); for (long k = 0; k < u; k++) i.MemSet(addr + k, b); return Task.CompletedTask; }

    [Primitive("ERASE")]
    private static Task Prim_ERASE(ForthInterpreter i) { i.EnsureStack(2, "ERASE"); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length"); for (long k = 0; k < u; k++) i.MemSet(addr + k, 0); return Task.CompletedTask; }

    [Primitive("DUMP")]
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

    [Primitive("COUNT")]
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

    [Primitive(",")]
    private static Task Prim_Comma(ForthInterpreter i) { i.EnsureStack(1, ","); var v = ToLong(i.PopInternal()); i._mem[i._nextAddr++] = v; return Task.CompletedTask; }

    [Primitive("ALLOT")]
    private static Task Prim_ALLOT(ForthInterpreter i) { i.EnsureStack(1, "ALLOT"); var cells = ToLong(i.PopInternal()); if (cells < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size"); for (long k = 0; k < cells; k++) i._mem[i._nextAddr++] = 0; return Task.CompletedTask; }
}
