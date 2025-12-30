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
        // IEEE 754 floating-point division by zero produces Infinity or NaN, not an exception
        // This matches ANS Forth floating-point semantics and allows tests to verify behavior
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

    [Primitive("FS.", HelpString = "FS. ( r -- ) - display floating-point number in scientific notation")]
    private static Task Prim_FSDot(ForthInterpreter i)
    {
        i.EnsureStack(1, "FS.");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        i.WriteText(d.ToString("E", CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    [Primitive("FCONSTANT", IsImmediate = true, HelpString = "FCONSTANT <name> - define a floating constant from top of stack")]
    private static Task Prim_FCONSTANT(ForthInterpreter i)
    {
            var name = i.ReadNextTokenOrThrow("Expected name after FCONSTANT");
            if (i.Stack.Count == 0)
            {
                if (i.TryResolveWord(name, out var existing) && existing is not null)
                {
                    // Already defined; nothing to do
                    return Task.CompletedTask;
                }
                throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow in FCONSTANT");
            }
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

    [Primitive("F0>", HelpString = "F0> ( r -- flag ) true if r > 0")]
    private static Task Prim_FZeroGreater(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0>");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.Push(d > 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0<=", HelpString = "F0<= ( r -- flag ) true if r <= 0")]
    private static Task Prim_FZeroLessEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0<=");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.Push(d <= 0.0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F0>=", HelpString = "F0>= ( r -- flag ) true if r >= 0")]
    private static Task Prim_FZeroGreaterEqual(ForthInterpreter i)
    {
        i.EnsureStack(1, "F0>=");
        var a = i.PopInternal();
        var d = ToDoubleFromObj(a);
        i.Push(d >= 0.0 ? -1L : 0L);
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

    [Primitive("F>", HelpString = "F> ( r1 r2 -- flag ) true if r1 > r2")]
    private static Task Prim_FGreater(ForthInterpreter i)
    {
        i.EnsureStack(2, "F>");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 > d2 ? -1L : 0L);
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

    [Primitive("F!=", HelpString = "F!= ( r1 r2 -- flag ) true if r1 != r2")]
    private static Task Prim_FNotEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F!=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 != d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F<=", HelpString = "F<= ( r1 r2 -- flag ) true if r1 <= r2")]
    private static Task Prim_FLessEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F<=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 <= d2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("F>=", HelpString = "F>= ( r1 r2 -- flag ) true if r1 >= r2")]
    private static Task Prim_FGreaterEqual(ForthInterpreter i)
    {
        i.EnsureStack(2, "F>=");
        var b = i.PopInternal();
        var a = i.PopInternal();
        var d1 = ToDoubleFromObj(a);
        var d2 = ToDoubleFromObj(b);
        i.Push(d1 >= d2 ? -1L : 0L);
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

    [Primitive("FLOOR", HelpString = "FLOOR ( r1 -- r2 ) - round r1 to integral value not greater than r1 (ANS Forth returns float)")]
    private static Task Prim_FLOOR(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOOR");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        // ANS Forth FLOOR returns a floating-point result, not an integer
        var result = Math.Floor(d);
        i.Push(result);
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

    [Primitive("FLN", HelpString = "FLN ( r -- r ) - floating-point natural logarithm (alias for FLOG)")]
    private static Task Prim_FLN(ForthInterpreter i)
    {
        // FLN is a common alias for FLOG in many Forth systems
        return Prim_FLOG(i);
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

    [Primitive("FATAN2", HelpString = "FATAN2 ( f: y x -- radians ) - arc tangent of y/x")]
    private static Task Prim_FAtan2(ForthInterpreter i)
    {
        i.EnsureStack(2, "FATAN2");
        var x = i.PopInternal();
        var y = i.PopInternal();
        var dx = ToDoubleFromObj(x);
        var dy = ToDoubleFromObj(y);
        var res = Math.Atan2(dy, dx);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("F~", HelpString = "Floating point proximity test ( r1 r2 r3 -- flag )")]
    private static Task Prim_FTilde(ForthInterpreter i)
    {
        i.EnsureStack(3, "F~");
        var r3 = ToDoubleFromObj(i.PopInternal());
        var r2 = ToDoubleFromObj(i.PopInternal());
        var r1 = ToDoubleFromObj(i.PopInternal());
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

    [Primitive("FOVER", HelpString = "FOVER ( r1 r2 -- r1 r2 r1 ) - copy second floating item to top")]
    private static Task Prim_FOVER(ForthInterpreter i)
    {
        i.EnsureStack(2, "FOVER");
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        // Restore original order and then push copy of r1
        i.Push(r1);
        i.Push(r2);
        i.Push(ToDoubleFromObj(r1));
        return Task.CompletedTask;
    }

    [Primitive("FDROP", HelpString = "FDROP ( r -- ) - drop top floating item")]
    private static Task Prim_FDROP(ForthInterpreter i)
    {
        i.EnsureStack(1, "FDROP");
        _ = i.PopInternal();
        return Task.CompletedTask;
    }

    [Primitive("FDUP", HelpString = "FDUP ( r -- r r ) - duplicate top floating item")]
    private static Task Prim_FDUP(ForthInterpreter i)
    {
        i.EnsureStack(1, "FDUP");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        i.Push(d);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("FSWAP", HelpString = "FSWAP ( r1 r2 -- r2 r1 ) - swap top two floating items")]
    private static Task Prim_FSWAP(ForthInterpreter i)
    {
        i.EnsureStack(2, "FSWAP");
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        i.Push(r2);
        i.Push(r1);
        return Task.CompletedTask;
    }

    [Primitive("FROT", HelpString = "FROT ( r1 r2 r3 -- r2 r3 r1 ) - rotate top three floating items")]
    private static Task Prim_FROT(ForthInterpreter i)
    {
        i.EnsureStack(3, "FROT");
        var r3 = i.PopInternal();
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        i.Push(r2);
        i.Push(r3);
        i.Push(r1);
        return Task.CompletedTask;
    }

    [Primitive("FLOATS", HelpString = "FLOATS ( n -- n' ) - scale by float-cell size (8 bytes for double precision)")]
    private static Task Prim_FLOATS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOATS");
        var n = ForthInterpreter.ToLong(i.PopInternal());
        // Double precision floats are 8 bytes each
        // This matches ANS Forth expectation that 1 FLOATS returns float-cell size
        i.Push(n * 8L);
        return Task.CompletedTask;
    }

    [Primitive("FDEPTH", HelpString = "FDEPTH ( -- n ) - return number of floating items on stack")]
    private static Task Prim_FDEPTH(ForthInterpreter i)
    {
        int count = 0;
        for (int idx = 0; idx < i.Stack.Count; idx++)
        {
            if (i.Stack[idx] is double) count++;
        }
        i.Push((long)count);
        return Task.CompletedTask;
    }

    [Primitive("D>F", HelpString = "D>F ( d -- r ) - convert double-cell integer to floating-point number")]
    private static Task Prim_DToF(ForthInterpreter i)
    {
        i.EnsureStack(2, "D>F");
        var hi = ForthInterpreter.ToLong(i.PopInternal());
        var lo = ForthInterpreter.ToLong(i.PopInternal());
        // In this 64-bit cell implementation, a double-cell number is represented
        // as lo (least significant) and hi (most significant/sign extension).
        // For most values that fit in 64-bit, hi is 0 or -1 (sign extension).
        // Convert to floating-point using just the lo cell since double can represent ~53 bits precision.
        var result = (double)lo;
        i.Push(result);
        return Task.CompletedTask;
    }

    [Primitive("F>D", HelpString = "F>D ( r -- d ) - convert floating-point number to double-cell integer")]
    private static Task Prim_FToD(ForthInterpreter i)
    {
        i.EnsureStack(1, "F>D");
        var r = i.PopInternal();
        var d = ToDoubleFromObj(r);
        // Convert to 64-bit signed integer
        var intVal = (long)d;
        // In this 64-bit cell implementation, push as double-cell (lo, hi)
        // where lo contains the value and hi is sign extension
        var lo = intVal;
        var hi = intVal < 0 ? -1L : 0L;
        i.Push(lo);
        i.Push(hi);
        return Task.CompletedTask;
    }

    [Primitive(">FLOAT", HelpString = ">FLOAT ( c-addr u -- r true | c-addr u false ) - attempt to convert string to floating-point number")]
    private static Task Prim_ToFloat(ForthInterpreter i)
    {
        i.EnsureStack(2, ">FLOAT");
        var uObj = i.PopInternal();
        var addrObj = i.PopInternal();
        long u = ForthInterpreter.ToLong(uObj);
        if (u < 0) throw new ForthException(ForthErrorCode.TypeError, ">FLOAT negative length");

        string slice;
        if (addrObj is string s)
        {
            slice = u <= s.Length ? s.Substring(0, (int)u) : s;
        }
        else if (addrObj is long addr)
        {
            slice = i.ReadMemoryString(addr, (int)u);
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, 
                $">FLOAT received unexpected type: {addrObj?.GetType().Name ?? "null"}");
        }

        var style = System.Globalization.NumberStyles.AllowLeadingSign |
                    System.Globalization.NumberStyles.AllowDecimalPoint |
                    System.Globalization.NumberStyles.AllowExponent;

        // Forth >FLOAT does not allow leading/trailing whitespace, but TryParse does.
        if (slice.Length > 0 && (char.IsWhiteSpace(slice[0]) || char.IsWhiteSpace(slice[^1])))
        {
            i.Push(addrObj);
            i.Push(uObj);
            i.Push(0L); // false
            return Task.CompletedTask;
        }

        // An empty string is a valid floating point number (0).
        if (string.IsNullOrWhiteSpace(slice))
        {
            i.Push(0.0);
            i.Push(-1L); // true
            return Task.CompletedTask;
        }

        if (double.TryParse(slice, style, System.Globalization.CultureInfo.InvariantCulture, out var result) && slice.Trim() != ".")
        {
            i.Push(result);
            i.Push(-1L); // true
        }
        else
        {
            i.Push(addrObj);
            i.Push(uObj);
            i.Push(0L); // false
        }
        return Task.CompletedTask;
    }

    [Primitive("SF!", HelpString = "SF! ( r addr -- ) - store single-precision float (32-bit) at address")]
    private static Task Prim_SFBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "SF!");
        var addr = ForthInterpreter.ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = ToDoubleFromObj(val);
        var f = (float)d;  // convert to single precision
        var bits = System.BitConverter.SingleToInt32Bits(f);
        i.MemSet(addr, (long)bits);
        return Task.CompletedTask;
    }

    [Primitive("SF@", HelpString = "SF@ ( addr -- r ) - fetch single-precision float (32-bit) from address")]
    private static Task Prim_SFAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "SF@");
        var addr = ForthInterpreter.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var f = System.BitConverter.Int32BitsToSingle((int)bits);
        i.Push((double)f);  // promote to double for stack
        return Task.CompletedTask;
    }

    [Primitive("DF!", HelpString = "DF! ( r addr -- ) - store double-precision float (64-bit) at address")]
    private static Task Prim_DFBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "DF!");
        var addr = ForthInterpreter.ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = ToDoubleFromObj(val);
        var bits = System.BitConverter.DoubleToInt64Bits(d);
        i.MemSet(addr, bits);
        return Task.CompletedTask;
    }

    [Primitive("DF@", HelpString = "DF@ ( addr -- r ) - fetch double-precision float (64-bit) from address")]
    private static Task Prim_DFAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "DF@");
        var addr = ForthInterpreter.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var d = System.BitConverter.Int64BitsToDouble(bits);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("F**", HelpString = "F** ( r1 r2 -- r1^r2 ) - floating-point exponentiation")]
    private static Task Prim_FPow(ForthInterpreter i)
    {
        i.EnsureStack(2, "F**");
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        var d1 = ToDoubleFromObj(r1);
        var d2 = ToDoubleFromObj(r2);
        var res = Math.Pow(d1, d2);
        i.Push(res);
        return Task.CompletedTask;
    }

    [Primitive("SET-NEAR", HelpString = "SET-NEAR - enable approximate FP equality (compatibility no-op)")]
    private static Task Prim_SET_NEAR(ForthInterpreter i) => Task.CompletedTask;

    [Primitive("SET-EXACT", HelpString = "SET-EXACT - enable exact FP equality (compatibility no-op)")]
    private static Task Prim_SET_EXACT(ForthInterpreter i) => Task.CompletedTask;
}
