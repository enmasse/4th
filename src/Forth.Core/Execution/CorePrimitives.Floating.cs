using Forth.Core.Interpreter;
using System.Globalization;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    private static double ToDoubleFromObj(object o)
    {
        return o switch
        {
            double d => d,
            float f => (double)f,
            long l => (double)l,
            int ii => (double)ii,
            short s => (double)s,
            byte b => (double)b,
            char c => (double)c,
            bool bo => bo ? 1.0 : 0.0,
            _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {o?.GetType().Name ?? "null"}")
        };
    }

    [Primitive("F+", HelpString = "Floating add ( f1 f2 -- f1+f2 )")]
    private static Task Prim_FPlus(ForthInterpreter i)
    {
        i.EnsureStack(2, "F+");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = ToDoubleFromObj(a) + ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F-", HelpString = "Floating subtract ( f1 f2 -- f1-f2 )")]
    private static Task Prim_FMinus(ForthInterpreter i)
    {
        i.EnsureStack(2, "F-");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = ToDoubleFromObj(a) - ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F*", HelpString = "Floating multiply ( f1 f2 -- f1*f2 )")]
    private static Task Prim_FStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "F*");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var res = ToDoubleFromObj(a) * ToDoubleFromObj(b);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F/", HelpString = "Floating divide ( f1 f2 -- f1/f2 )")]
    private static Task Prim_FSlash(ForthInterpreter i)
    {
        i.EnsureStack(2, "F/");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var dv = ToDoubleFromObj(b);
        if (dv == 0.0) throw new ForthException(ForthErrorCode.DivideByZero, "Floating divide by zero");
        var res = ToDoubleFromObj(a) / dv;
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FNEGATE", HelpString = "Negate floating ( f -- -f )")]
    private static Task Prim_FNegate(ForthInterpreter i)
    {
        i.EnsureStack(1, "FNEGATE");
        var a = i.PopInternal();
        var res = -ToDoubleFromObj(a);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F.", HelpString = "Print floating number")]
    private static Task Prim_FDot(ForthInterpreter i)
    {
        i.EnsureStack(1, "F.");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.WriteText(d.ToString(CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    [Primitive("FCONSTANT", IsImmediate = true, HelpString = "FCONSTANT <name> - define a floating constant from top of stack")]
    private static Task Prim_FCONSTANT(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after FCONSTANT");
        i.EnsureStack(1, "FCONSTANT");
        var val = i.PopInternal();
        var d = ToDoubleFromObj(val);
        var createdConst = new Word(ii => { ii.Push(d); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), createdConst);
        i.RegisterDefinition(name);
        i._lastDefinedWord = createdConst;
        return Task.CompletedTask;
    }

    [Primitive("FVARIABLE", IsImmediate = true, HelpString = "FVARIABLE <name> - define a floating variable (initialized to 0.0)")]
    private static Task Prim_FVARIABLE(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after FVARIABLE");
        var addr = i._nextAddr++;
        // Store double bits into memory
        long bits = System.BitConverter.DoubleToInt64Bits(0.0);
        i._mem[addr] = bits;
        var createdVar = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), createdVar);
        i.RegisterDefinition(name);
        i._lastDefinedWord = createdVar;
        return Task.CompletedTask;
    }

    [Primitive("F@", HelpString = "F@ ( addr -- f ) - fetch double stored at address")]
    private static Task Prim_FAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "F@");
        var addr = ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var d = System.BitConverter.Int64BitsToDouble(bits);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("F!", HelpString = "F! ( addr f -- ) - store double at address")]
    private static Task Prim_FBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "F!");
        var addr = ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = ToDoubleFromObj(val);
        var bits = System.BitConverter.DoubleToInt64Bits(d);
        i.MemSet(addr, bits);
        return Task.CompletedTask;
    }

    [Primitive("F0=", HelpString = "F0= ( r -- flag ) true if r is zero")]
    private static Task Prim_FZeroEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0=");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.Push(d == 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0<", HelpString = "F0< ( r -- flag ) true if r < 0")]
    private static Task Prim_FZeroLess(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0<");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.Push(d < 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F<", HelpString = "F< ( r1 r2 -- flag ) true if r1 < r2")]
    private static Task Prim_FLess(ForthInterpreter i)
    {
        i.EnsureStack(2, "F<");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 < d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F=", HelpString = "F= ( r1 r2 -- flag ) true if r1 == r2")]
    private static Task Prim_FEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 == d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F>S", HelpString = "F>S ( r -- n ) - convert floating-point number to single-cell integer")]
    private static Task Prim_FToS(ForthInterpreter i)
    {
        i.EnsureStack(1, "F>S");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        var n = (long)d;
        i.Push(n);
        return Task.CompletedTask;
    }

    [Primitive("S>F", HelpString = "S>F ( n -- r ) - convert single-cell integer to floating-point number")]
    private static Task Prim_SToF(ForthInterpreter i)
    {
        i.EnsureStack(1, "S>F");
        var n = i.PopInternal();
        var d = ToDoubleFromObj(n);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("FABS", HelpString = "FABS ( r -- |r| ) - floating-point absolute value")]
    private static Task Prim_FABS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FABS");
        var a = i.PopInternal();
        var res = Math.Abs(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FLOOR", HelpString = "FLOOR ( r -- n ) - convert floating-point number to integer by flooring")]
    private static Task Prim_FLOOR(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOOR");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        var n = (long)Math.Floor(d);
        i.Push(n);
        return Task.CompletedTask;
    }

    [Primitive("FROUND", HelpString = "FROUND ( r -- n ) - convert floating-point number to integer by rounding")]
    private static Task Prim_FROUND(ForthInterpreter i)
    {
        i.EnsureStack(1, "FROUND");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        var n = (long)Math.Round(d);
        i.Push(n);
        return Task.CompletedTask;
    }

    [Primitive("FSIN", HelpString = "FSIN ( r -- r ) - floating-point sine")]
    private static Task Prim_FSIN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FSIN");
        var a = i.PopInternal();
        var res = Math.Sin(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FTAN", HelpString = "FTAN ( r -- r ) - floating-point tangent")]
    private static Task Prim_FTAN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FTAN");
        var a = i.PopInternal();
        var res = Math.Tan(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FCOS", HelpString = "FCOS ( r -- r ) - floating-point cosine")]
    private static Task Prim_FCOS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FCOS");
        var a = i.PopInternal();
        var res = Math.Cos(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FEXP", HelpString = "FEXP ( r -- r ) - floating-point exponential (e^r)")]
    private static Task Prim_FEXP(ForthInterpreter i)
    {
        i.EnsureStack(1, "FEXP");
        var a = i.PopInternal();
        var res = Math.Exp(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FLOG", HelpString = "FLOG ( r -- r ) - floating-point natural logarithm")]
    private static Task Prim_FLOG(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOG");
        var a = i.PopInternal();
        var res = Math.Log(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FACOS", HelpString = "FACOS ( r -- r ) - floating-point arccosine")]
    private static Task Prim_FACOS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FACOS");
        var a = i.PopInternal();
        var res = Math.Acos(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FASIN", HelpString = "FASIN ( r -- r ) - floating-point arcsine")]
    private static Task Prim_FASIN(ForthInterpreter i)
    {
        i.EnsureStack(1, "FASIN");
        var a = i.PopInternal();
        var res = Math.Asin(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FATAN2", HelpString = "FATAN2 ( y x -- r ) - floating-point arctangent of y/x")]
    private static Task Prim_FATAN2(ForthInterpreter i)
    {
        i.EnsureStack(2, "FATAN2");
        var x = i.PopInternal();
        var y = i.PopInternal();
        var res = Math.Atan2(ToDoubleFromObj(y), ToDoubleFromObj(x));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F~", HelpString = "F~ ( r1 r2 r3 -- flag ) - true if |r1 - r2| < r3")]
    private static Task Prim_FTilde(ForthInterpreter i)
    {
        i.EnsureStack(3, "F~");
        var r3 = ToDoubleFromObj(i.PopInternal());
        var r2 = ToDoubleFromObj(i.PopInternal());
        var r1 = ToDoubleFromObj(i.PopInternal());
        var diff = Math.Abs(r1 - r2);
        i.Push(diff < r3 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("FMIN", HelpString = "FMIN ( r1 r2 -- r3 ) - return the minimum of r1 and r2")]
    private static Task Prim_FMIN(ForthInterpreter i)
    {
        i.EnsureStack(2, "FMIN");
        var r2 = ToDoubleFromObj(i.PopInternal());
        var r1 = ToDoubleFromObj(i.PopInternal());
        var min = Math.Min(r1, r2);
        i.Push(min);
        return Task.CompletedTask;
    }

    [Primitive("FMAX", HelpString = "FMAX ( r1 r2 -- r3 ) - return the maximum of r1 and r2")]
    private static Task Prim_FMAX(ForthInterpreter i)
    {
        i.EnsureStack(2, "FMAX");
        var r2 = ToDoubleFromObj(i.PopInternal());
        var r1 = ToDoubleFromObj(i.PopInternal());
        var max = Math.Max(r1, r2);
        i.Push(max);
        return Task.CompletedTask;
    }

    [Primitive("FSQRT", HelpString = "FSQRT ( r -- r ) - floating-point square root")]
    private static Task Prim_FSQRT(ForthInterpreter i)
    {
        i.EnsureStack(1, "FSQRT");
        var a = i.PopInternal();
        var res = Math.Sqrt(ToDoubleFromObj(a));
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("FTRUNC", HelpString = "FTRUNC ( r -- n ) - convert floating-point number to integer by truncating towards zero")]
    private static Task Prim_FTRUNC(ForthInterpreter i)
    {
        i.EnsureStack(1, "FTRUNC");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        var n = (long)Math.Truncate(d);
        i.Push(n);
        return Task.CompletedTask;
    }
}
