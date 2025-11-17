using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("<")]
    private static Task Prim_Lt(ForthInterpreter i)
    {
        i.EnsureStack(2, "<");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a < b ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("=")]
    private static Task Prim_Eq(ForthInterpreter i)
    {
        i.EnsureStack(2, "=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a == b ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">")]
    private static Task Prim_Gt(ForthInterpreter i)
    {
        i.EnsureStack(2, ">");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a > b ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0=")]
    private static Task Prim_0Eq(ForthInterpreter i)
    {
        i.EnsureStack(1, "0=");
        var a = ToLong(i.PopInternal());
        i.Push(a == 0 ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0<>")]
    private static Task Prim_0Ne(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<>");
        var a = ToLong(i.PopInternal());
        i.Push(a != 0 ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<>")]
    private static Task Prim_Ne(ForthInterpreter i)
    {
        i.EnsureStack(2, "<>");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a != b ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("<=")]
    private static Task Prim_Le(ForthInterpreter i)
    {
        i.EnsureStack(2, "<=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a <= b ? 1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive(">=")]
    private static Task Prim_Ge(ForthInterpreter i)
    {
        i.EnsureStack(2, ">=");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a >= b ? 1L : 0L);
        return Task.CompletedTask;
    }
}
