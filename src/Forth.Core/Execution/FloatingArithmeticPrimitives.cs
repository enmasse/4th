using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System;

namespace Forth.Core.Execution;

internal static class FloatingArithmeticPrimitives
{
    [Primitive("F+", HelpString = "Floating add ( f1 f2 -- f1+f2 )")]
    private static Task Prim_FPlus(ForthInterpreter i)
    {
        i.EnsureStack(2, "F+");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = PrimitivesUtil.ToDoubleFromObj(a) + PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F-", HelpString = "Floating subtract ( f1 f2 -- f1-f2 )")]
    private static Task Prim_FMinus(ForthInterpreter i)
    {
        i.EnsureStack(2, "F-");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = PrimitivesUtil.ToDoubleFromObj(a) - PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F*", HelpString = "Floating multiply ( f1 f2 -- f1*f2 )")]
    private static Task Prim_FStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "F*");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = PrimitivesUtil.ToDoubleFromObj(a) * PrimitivesUtil.ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F/", HelpString = "Floating divide ( f1 f2 -- f1/f2 )")]
    private static Task Prim_FSlash(ForthInterpreter i)
    {
        i.EnsureStack(2, "F/");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var dv = PrimitivesUtil.ToDoubleFromObj(b);
        var res = PrimitivesUtil.ToDoubleFromObj(a) / dv;
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FNEGATE", HelpString = "Negate floating ( f -- -f )")]
    private static Task Prim_FNegate(ForthInterpreter i)
    {
        i.EnsureStack(1, "FNEGATE");
        var a = i.PopInternal();
        var res = -PrimitivesUtil.ToDoubleFromObj(a);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FABS", HelpString = "FABS ( r -- |r| ) - floating-point absolute value")]
    private static Task Prim_FABS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FABS");
        var a = i.PopInternal();
        var res = Math.Abs(PrimitivesUtil.ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FMIN", HelpString = "FMIN ( r1 r2 -- r3 ) - return the minimum of r1 and r2")]
    private static Task Prim_FMIN(ForthInterpreter i)
    {
        i.EnsureStack(2, "FMIN");
        var r2 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        var r1 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        i.Push(Math.Min(r1, r2));
        return Task.CompletedTask;
    }

    [Primitive("FMAX", HelpString = "FMAX ( r1 r2 -- r3 ) - return the maximum of r1 and r2")]
    private static Task Prim_FMAX(ForthInterpreter i)
    {
        i.EnsureStack(2, "FMAX");
        var r2 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        var r1 = PrimitivesUtil.ToDoubleFromObj(i.PopInternal());
        i.Push(Math.Max(r1, r2));
        return Task.CompletedTask;
    }

    [Primitive("FSQRT", HelpString = "FSQRT ( r -- r ) - floating-point square root")]
    private static Task Prim_FSQRT(ForthInterpreter i)
    {
        i.EnsureStack(1, "FSQRT");
        var a = i.PopInternal();
        i.Push(Math.Sqrt(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("F**", HelpString = "F** ( r1 r2 -- r1^r2 ) - floating-point exponentiation")]
    private static Task Prim_FPow(ForthInterpreter i)
    {
        i.EnsureStack(2, "F**");
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        var d1 = PrimitivesUtil.ToDoubleFromObj(r1);
        var d2 = PrimitivesUtil.ToDoubleFromObj(r2);
        i.Push(Math.Pow(d1, d2));
        return Task.CompletedTask;
    }
}
