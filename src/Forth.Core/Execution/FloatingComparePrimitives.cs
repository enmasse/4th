using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System;

namespace Forth.Core.Execution;

internal static class FloatingComparePrimitives
{
    [Primitive("F0=", HelpString = "F0= ( r -- flag ) true if r is zero")]
    private static Task Prim_FZeroEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0=");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(d == 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0<", HelpString = "F0< ( r -- flag ) true if r < 0")]
    private static Task Prim_FZeroLess(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0<");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(d < 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0>", HelpString = "F0> ( r -- flag ) true if r > 0")]
    private static Task Prim_FZeroGreater(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0>");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(d > 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0<=", HelpString = "F0<= ( r -- flag ) true if r <= 0")]
    private static Task Prim_FZeroLessEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0<=");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(d <= 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0>=", HelpString = "F0>= ( r -- flag ) true if r >= 0")]
    private static Task Prim_FZeroGreaterEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0>=");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(d >= 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F<", HelpString = "F< ( r1 r2 -- flag ) true if r1 < r2")]
    private static Task Prim_FLess(ForthInterpreter i)
    {
        i.EnsureStack(2, "F<");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 < d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F>", HelpString = "F> ( r1 r2 -- flag ) true if r1 > r2")]
    private static Task Prim_FGreater(ForthInterpreter i)
    {
        i.EnsureStack(2, "F>");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 > d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F=", HelpString = "F= ( r1 r2 -- flag ) true if r1 == r2")]
    private static Task Prim_FEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 == d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F!=", HelpString = "F!= ( r1 r2 -- flag ) true if r1 != r2")]
    private static Task Prim_FNotEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F!=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 != d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F<=", HelpString = "F<= ( r1 r2 -- flag ) true if r1 <= r2")]
    private static Task Prim_FLessEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F<=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 <= d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F>=", HelpString = "F>= ( r1 r2 -- flag ) true if r1 >= r2")]
    private static Task Prim_FGreaterEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F>=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(a);
        var d2 = PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(d1 >= d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F~", HelpString = "Floating point proximity test ( r1 r2 r3 -- flag )")]
    private static Task Prim_FTilde(ForthInterpreter i)
    {
        i.EnsureStack(3, "F~");
        var r3 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        var r2 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        var r1 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        var diff = Math.Abs(r1 - r2);

        double tol;
        if (r3 >= 0)
        {
            tol = Math.Abs(r3);
        }
        else
        {
            var absTolerance = Math.Abs(r3);
            var absR1 = Math.Abs(r1);
            var absR2 = Math.Abs(r2);
            tol = absTolerance * ((absR1 + absR2) / 2.0);
        }

        var flag = diff <= tol || (r1 == 0.0 && r2 == 0.0);
        i.Push(flag ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("SET-NEAR", HelpString = "SET-NEAR - enable approximate FP equality (compatibility no-op)")]
    private static Task Prim_SET_NEAR(ForthInterpreter i) => Task.CompletedTask;

    [Primitive("SET-EXACT", HelpString = "SET-EXACT - enable exact FP equality (compatibility no-op)")]
    private static Task Prim_SET_EXACT(ForthInterpreter i) => Task.CompletedTask;
}
