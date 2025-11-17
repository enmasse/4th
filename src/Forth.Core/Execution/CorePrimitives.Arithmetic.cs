using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("+")]
    private static Task Prim_Plus(ForthInterpreter i)
    {
        i.EnsureStack(2, "+");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a + b);
        return Task.CompletedTask;
    }

    [Primitive("-")]
    private static Task Prim_Minus(ForthInterpreter i)
    {
        i.EnsureStack(2, "-");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a - b);
        return Task.CompletedTask;
    }

    [Primitive("*")]
    private static Task Prim_Star(ForthInterpreter i)
    {
        i.EnsureStack(2, "*");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a * b);
        return Task.CompletedTask;
    }

    [Primitive("/")]
    private static Task Prim_Slash(ForthInterpreter i)
    {
        i.EnsureStack(2, "/");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i.Push(a / b);
        return Task.CompletedTask;
    }

    [Primitive("/MOD")]
    private static Task Prim_SlashMod(ForthInterpreter i)
    {
        i.EnsureStack(2, "/MOD");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        var quot = a / b;
        var rem = a % b;
        i.Push(rem);
        i.Push(quot);
        return Task.CompletedTask;
    }

    [Primitive("MOD")]
    private static Task Prim_Mod(ForthInterpreter i)
    {
        i.EnsureStack(2, "MOD");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i.Push(a % b);
        return Task.CompletedTask;
    }

    [Primitive("MIN")]
    private static Task Prim_MIN(ForthInterpreter i)
    {
        i.EnsureStack(2, "MIN");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a < b ? a : b);
        return Task.CompletedTask;
    }

    [Primitive("MAX")]
    private static Task Prim_MAX(ForthInterpreter i)
    {
        i.EnsureStack(2, "MAX");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a > b ? a : b);
        return Task.CompletedTask;
    }

    [Primitive("NEGATE")]
    private static Task Prim_NEGATE(ForthInterpreter i)
    {
        i.EnsureStack(1, "NEGATE");
        var a = ToLong(i.PopInternal());
        i.Push(-a);
        return Task.CompletedTask;
    }

    [Primitive("*/")]
    private static Task Prim_StarSlash(ForthInterpreter i)
    {
        i.EnsureStack(3, "*/");
        var dval = ToLong(i.PopInternal());
        var n2 = ToLong(i.PopInternal());
        var n1 = ToLong(i.PopInternal());
        if (dval == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        var prod = n1 * n2;
        i.Push(prod / dval);
        return Task.CompletedTask;
    }
}
