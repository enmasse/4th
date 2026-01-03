using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace Forth.Core.Execution;

internal static class FloatingFormatPrimitives
{
    [Primitive("F.", HelpString = "Print floating number")]
    private static Task Prim_FDot(ForthInterpreter i)
    {
        i.EnsureStack(1, "F.");
        var a = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(a);
        i.WriteText(d.ToString(CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    [Primitive("FS.", HelpString = "FS. ( r -- ) - display floating-point number in scientific notation")]
    private static Task Prim_FSDot(ForthInterpreter i)
    {
        i.EnsureStack(1, "FS.");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        i.WriteText(d.ToString("E", CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    [Primitive("FLOOR", HelpString = "FLOOR ( r1 -- r2 ) - round r1 to integral value not greater than r1 (ANS Forth returns float)")]
    private static Task Prim_FLOOR(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOOR");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        i.Push(Math.Floor(d));
        return Task.CompletedTask;
    }

    [Primitive("FROUND", HelpString = "FROUND ( r -- n ) - convert floating-point number to integer by rounding")]
    private static Task Prim_FROUND(ForthInterpreter i)
    {
        i.EnsureStack(1, "FROUND");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        i.Push((long)Math.Round(d));
        return Task.CompletedTask;
    }

    [Primitive("FTRUNC", HelpString = "FTRUNC ( r -- n ) - convert floating-point number to integer by truncating towards zero")]
    private static Task Prim_FTRUNC(ForthInterpreter i)
    {
        i.EnsureStack(1, "FTRUNC");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        i.Push((long)Math.Truncate(d));
        return Task.CompletedTask;
    }
}
