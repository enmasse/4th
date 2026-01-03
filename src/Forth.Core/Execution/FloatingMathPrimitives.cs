using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System;

namespace Forth.Core.Execution;

internal static class FloatingMathPrimitives
{
    [Primitive("FSIN", HelpString = "FSIN ( r -- r ) - floating-point sine")]
    private static Task Prim_FSIN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FSIN");
        var a = i.PopInternal();
        i.Push(Math.Sin(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FCOS", HelpString = "FCOS ( r -- r ) - floating-point cosine")]
    private static Task Prim_FCOS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FCOS");
        var a = i.PopInternal();
        i.Push(Math.Cos(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FTAN", HelpString = "FTAN ( r -- r ) - floating-point tangent")]
    private static Task Prim_FTAN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FTAN");
        var a = i.PopInternal();
        i.Push(Math.Tan(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FACOS", HelpString = "FACOS ( r -- r ) - floating-point arccosine")]
    private static Task Prim_FACOS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FACOS");
        var a = i.PopInternal();
        i.Push(Math.Acos(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FASIN", HelpString = "FASIN ( r -- r ) - floating-point arcsine")]
    private static Task Prim_FASIN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FASIN");
        var a = i.PopInternal();
        i.Push(Math.Asin(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FATAN2", HelpString = "FATAN2 ( f: y x -- radians ) - arc tangent of y/x")]
    private static Task Prim_FAtan2(ForthInterpreter i)
    {
        i.EnsureStack(2, "FATAN2");
        var x = i.PopInternal();
        var y = i.PopInternal();
        var dx = PrimitivesUtil.ToDoubleFromObj(x);
        var dy = PrimitivesUtil.ToDoubleFromObj(y);
        i.Push(Math.Atan2(dy, dx));
        return Task.CompletedTask;
    }

    [Primitive("FEXP", HelpString = "FEXP ( r -- r ) - floating-point exponential (e^r)")]
    private static Task Prim_FEXP(ForthInterpreter i)
    {
        i.EnsureStack(1, "FEXP");
        var a = i.PopInternal();
        i.Push(Math.Exp(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FLOG", HelpString = "FLOG ( r -- r ) - floating-point natural logarithm")]
    private static Task Prim_FLOG(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOG");
        var a = i.PopInternal();
        i.Push(Math.Log(PrimitivesUtil.ToDoubleFromObj(a)));
        return Task.CompletedTask;
    }

    [Primitive("FLN", HelpString = "FLN ( r -- r ) - floating-point natural logarithm (alias for FLOG)")]
    private static Task Prim_FLN(ForthInterpreter i) => Prim_FLOG(i);
}
