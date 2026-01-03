using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Forth.Core.Execution;

internal static class MemoryBytePrimitives
{
    [Primitive("C!", HelpString = "C! ( x addr -- ) - store low byte of x at addr")]
    private static Task Prim_CBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "C!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var val = PrimitivesUtil.ToLong(i.PopInternal());
        var b = (long)(byte)val;
        i.MemSet(addr, b);
        return Task.CompletedTask;
    }

    [Primitive("C@", HelpString = "C@ ( addr -- byte ) - fetch low byte at address")]
    private static Task Prim_CAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "C@");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var v);
        i.Push((long)(byte)v);
        return Task.CompletedTask;
    }

    [Primitive("C,", HelpString = "C, ( ch -- ) - append byte to dictionary and advance here")]
    private static Task Prim_CComma(ForthInterpreter i)
    {
        i.EnsureStack(1, "C,");
        var v = PrimitivesUtil.ToLong(i.PopInternal());
        i._mem[i._nextAddr++] = (long)(byte)v;
        return Task.CompletedTask;
    }

    [Primitive("MOVE", HelpString = "MOVE ( src dst u -- ) - copy u bytes from src to dst")]
    private static Task Prim_MOVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "MOVE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var dst = PrimitivesUtil.ToLong(i.PopInternal());
        var src = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative MOVE length");
        if (u == 0) return Task.CompletedTask;

        if (src < dst && src + u > dst)
        {
            for (long k = u - 1; k >= 0; k--)
            {
                i.MemTryGet(src + k, out var v);
                i.MemSet(dst + k, (long)(byte)v);
            }
            return Task.CompletedTask;
        }

        for (long k = 0; k < u; k++)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)(byte)v);
        }

        return Task.CompletedTask;
    }

    [Primitive("CMOVE", HelpString = "CMOVE ( c-addr1 c-addr2 u -- ) - copy u bytes from c-addr1 to c-addr2")]
    private static Task Prim_CMOVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "CMOVE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var dst = PrimitivesUtil.ToLong(i.PopInternal());
        var src = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative CMOVE length");

        for (long k = 0; k < u; k++)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)(byte)v);
        }
        return Task.CompletedTask;
    }

    [Primitive("CMOVE>", HelpString = "CMOVE> ( c-addr1 c-addr2 u -- ) - copy u bytes from c-addr1 to c-addr2, proceeding from high to low")]
    private static Task Prim_CMOVE_Greater(ForthInterpreter i)
    {
        i.EnsureStack(3, "CMOVE>");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var dst = PrimitivesUtil.ToLong(i.PopInternal());
        var src = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative CMOVE> length");

        for (long k = u - 1; k >= 0; k--)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)(byte)v);
        }
        return Task.CompletedTask;
    }

    [Primitive("FILL", HelpString = "FILL ( addr u ch -- ) - fill u bytes at addr with ch")]
    private static Task Prim_FILL(ForthInterpreter i)
    {
        i.EnsureStack(3, "FILL");
        var ch = PrimitivesUtil.ToLong(i.PopInternal());
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length");
        var b = (long)(byte)ch;
        for (long k = 0; k < u; k++) i.MemSet(addr + k, b);
        return Task.CompletedTask;
    }

    [Primitive("BLANK", HelpString = "BLANK ( addr u -- ) - fill u bytes at addr with space")]
    private static Task Prim_BLANK(ForthInterpreter i)
    {
        i.EnsureStack(2, "BLANK");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative BLANK length");
        for (long k = 0; k < u; k++) i.MemSet(addr + k, 32);
        return Task.CompletedTask;
    }

    [Primitive("ERASE", HelpString = "ERASE ( addr u -- ) - set u bytes at addr to zero")]
    private static Task Prim_ERASE(ForthInterpreter i)
    {
        i.EnsureStack(2, "ERASE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length");
        for (long k = 0; k < u; k++) i.MemSet(addr + k, 0);
        return Task.CompletedTask;
    }

    [Primitive("DUMP", HelpString = "DUMP ( addr u -- ) - print u bytes from addr in hex")]
    private static Task Prim_DUMP(ForthInterpreter i)
    {
        i.EnsureStack(2, "DUMP");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative DUMP length");

        var sb = new StringBuilder();
        for (long k = 0; k < u; k++)
        {
            if (k > 0) sb.Append(' ');
            i.MemTryGet(addr + k, out var v);
            sb.Append(((byte)v).ToString("X2"));
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
                var len = (long)(byte)v;
                i.Push(addr + 1);
                i.Push(len);
                return Task.CompletedTask;

            default:
                throw new ForthException(ForthErrorCode.TypeError, "COUNT expects a string or address");
        }
    }
}
