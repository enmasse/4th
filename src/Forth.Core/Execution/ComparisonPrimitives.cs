using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class ComparisonPrimitives
{
    [Primitive("0>", HelpString = "Push -1 if top is greater than zero else 0 ( n -- flag )")]
    private static Task Prim_0Gt(ForthInterpreter i)
    {
        i.EnsureStack(1, "0>");
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a > 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0<", HelpString = "Push -1 if top is less than zero else 0 ( n -- flag )")]
    private static Task Prim_0Lt(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<");
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a < 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<", HelpString = "Compare: push -1 if second < top else 0 ( a b -- flag )")]
    private static Task Prim_Lt(ForthInterpreter i)
    {
        i.EnsureStack(2, "<");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a < b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("=", HelpString = "Compare: push -1 if equal else 0 ( a b -- flag )")]
    private static Task Prim_Eq(ForthInterpreter i)
    {
        i.EnsureStack(2, "=");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a == b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">", HelpString = "Compare: push -1 if second > top else 0 ( a b -- flag )")]
    private static Task Prim_Gt(ForthInterpreter i)
    {
        i.EnsureStack(2, ">");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a > b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0=", HelpString = "Push -1 if top is zero else 0 ( n -- flag )")]
    private static Task Prim_0Eq(ForthInterpreter i)
    {
        i.EnsureStack(1, "0=");
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a == 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0<>", HelpString = "Push -1 if top is non-zero else 0 ( n -- flag )")]
    private static Task Prim_0Ne(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<>");
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a != 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<>", HelpString = "Compare: push -1 if not equal else 0 ( a b -- flag )")]
    private static Task Prim_Ne(ForthInterpreter i)
    {
        i.EnsureStack(2, "<>");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a != b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<=", HelpString = "Compare: push -1 if second <= top else 0 ( a b -- flag )")]
    private static Task Prim_Le(ForthInterpreter i)
    {
        i.EnsureStack(2, "<=");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a <= b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">=", HelpString = "Compare: push -1 if second >= top else 0 ( a b -- flag )")]
    private static Task Prim_Ge(ForthInterpreter i)
    {
        i.EnsureStack(2, ">=");
        var b = CorePrimitives.ToLong(i.PopInternal());
        var a = CorePrimitives.ToLong(i.PopInternal());
        i.Push(a >= b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("U<", HelpString = "Unsigned compare: push -1 if second < top (unsigned) else 0 ( u1 u2 -- flag )")]
    private static Task Prim_ULt(ForthInterpreter i)
    {
        i.EnsureStack(2, "U<");
        var b = (ulong)CorePrimitives.ToLong(i.PopInternal());
        var a = (ulong)CorePrimitives.ToLong(i.PopInternal());
        i.Push(a < b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("COMPARE", HelpString = "COMPARE ( c-addr1 u1 c-addr2 u2 -- n ) - compare two strings lexicographically, n=-1 less, 0 equal, 1 greater")]
    private static Task Prim_COMPARE(ForthInterpreter i)
    {
        i.EnsureStack(4, "COMPARE");
        var u2 = CorePrimitives.ToLong(i.PopInternal());
        var addr2 = i.PopInternal();
        var u1 = CorePrimitives.ToLong(i.PopInternal());
        var addr1 = i.PopInternal();
        if (u1 < 0 || u2 < 0) throw new ForthException(ForthErrorCode.TypeError, "COMPARE negative length");

        string str1, str2;
        if (addr1 is string s1)
        {
            str1 = u1 <= s1.Length ? s1.Substring(0, (int)u1) : s1;
        }
        else
        {
            var a1 = CorePrimitives.ToLong(addr1);
            str1 = i.ReadMemoryString(a1, u1);
        }

        if (addr2 is string s2)
        {
            str2 = u2 <= s2.Length ? s2.Substring(0, (int)u2) : s2;
        }
        else
        {
            var a2 = CorePrimitives.ToLong(addr2);
            str2 = i.ReadMemoryString(a2, u2);
        }

        var cmp = string.CompareOrdinal(str1, str2);
        var result = cmp < 0 ? -1 : cmp > 0 ? 1 : 0;
        i.Push((long)result);
        return Task.CompletedTask;
    }
}
