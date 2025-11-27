using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("0>", HelpString = "Push -1 if top is greater than zero else 0 ( n -- flag )")]
    private static Task Prim_0Gt(ForthInterpreter i)
    {
        i.EnsureStack(1, "0>");
        var a = ToLong(i.PopInternal());
        i.Push(a > 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0<", HelpString = "Push -1 if top is less than zero else 0 ( n -- flag )")]
    private static Task Prim_0Lt(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<");
        var a = ToLong(i.PopInternal());
        i.Push(a < 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<", HelpString = "Compare: push 1 if second < top else 0 ( a b -- flag )")]
    private static Task Prim_Lt(ForthInterpreter i)
    {
        i.EnsureStack(2, "<");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a < b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("=", HelpString = "Compare: push 1 if equal else 0 ( a b -- flag )")]
    private static Task Prim_Eq(ForthInterpreter i)
    {
        i.EnsureStack(2, "=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a == b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">", HelpString = "Compare: push 1 if second > top else 0 ( a b -- flag )")]
    private static Task Prim_Gt(ForthInterpreter i)
    {
        i.EnsureStack(2, ">");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a > b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0=", HelpString = "Push 1 if top is zero else 0 ( n -- flag )")]
    private static Task Prim_0Eq(ForthInterpreter i)
    {
        i.EnsureStack(1, "0=");
        var a = ToLong(i.PopInternal());
        i.Push(a == 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0<>", HelpString = "Push 1 if top is non-zero else 0 ( n -- flag )")]
    private static Task Prim_0Ne(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<>");
        var a = ToLong(i.PopInternal());
        i.Push(a != 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<>", HelpString = "Compare: push 1 if not equal else 0 ( a b -- flag )")]
    private static Task Prim_Ne(ForthInterpreter i)
    {
        i.EnsureStack(2, "<>");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a != b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<=", HelpString = "Compare: push 1 if second <= top else 0 ( a b -- flag )")]
    private static Task Prim_Le(ForthInterpreter i)
    {
        i.EnsureStack(2, "<=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a <= b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">=", HelpString = "Compare: push 1 if second >= top else 0 ( a b -- flag )")]
    private static Task Prim_Ge(ForthInterpreter i)
    {
        i.EnsureStack(2, ">=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a >= b ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("U<", HelpString = "Unsigned compare: push -1 if second < top (unsigned) else 0 ( u1 u2 -- flag )")]
    private static Task Prim_ULt(ForthInterpreter i)
    {
        i.EnsureStack(2, "U<");
        var b = (ulong)ToLong(i.PopInternal());
        var a = (ulong)ToLong(i.PopInternal());
        i.Push(a < b ? -1L : 0L);
        return Task.CompletedTask;
    }
}
