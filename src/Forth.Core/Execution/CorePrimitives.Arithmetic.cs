using Forth.Core.Interpreter;
using VT = Forth.Core.Interpreter.ValueType;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("1+", HelpString = "Increment ( n -- n+1 )")]
    private static Task Prim_OnePlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "1+");
        var v = i._stack.PopValue();
        if (v.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        i._stack.Push(ForthValue.FromLong(v.LongValue + 1));
        return Task.CompletedTask;
    }

    // M/MOD - Symmetric division producing remainder and quotient (remainder has sign of dividend)
    // Stack effect per /MOD: ( n1 n2 -- rem quot )
    [Primitive("M/MOD", HelpString = "M/MOD ( n1 n2 -- rem quot ) - symmetric division; remainder sign of dividend")]
    private static Task Prim_M_Slash_MOD(ForthInterpreter i)
    {
        i.EnsureStack(2, "M/MOD");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        long b = bFv.LongValue;
        long a = aFv.LongValue;
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");

        long q = a / b; // truncates toward zero (symmetric quotient)
        long r = a % b; // remainder has sign of dividend
        i._stack.Push(ForthValue.FromLong(r));
        i._stack.Push(ForthValue.FromLong(q));
        return Task.CompletedTask;
    }

    [Primitive("1-", HelpString = "Decrement ( n -- n-1 )")]
    private static Task Prim_OneMinus(ForthInterpreter i)
    {
        i.EnsureStack(1, "1-");
        var v = i._stack.PopValue();
        if (v.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        i._stack.Push(ForthValue.FromLong(v.LongValue - 1));
        return Task.CompletedTask;
    }

    [Primitive("2*", HelpString = "Multiply by two ( n -- 2n )")]
    private static Task Prim_TwoStar(ForthInterpreter i)
    {
        i.EnsureStack(1, "2*");
        var v = i._stack.PopValue();
        if (v.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        i._stack.Push(ForthValue.FromLong(v.LongValue * 2));
        return Task.CompletedTask;
    }

    [Primitive("2/", HelpString = "Divide by two ( n -- n/2 )")]
    private static Task Prim_TwoSlash(ForthInterpreter i)
    {
        i.EnsureStack(1, "2/");
        var v = i._stack.PopValue();
        if (v.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        if (v.LongValue == long.MinValue) { i._stack.Push(ForthValue.FromLong(v.LongValue / 2)); return Task.CompletedTask; }
        i._stack.Push(ForthValue.FromLong(v.LongValue / 2));
        return Task.CompletedTask;
    }

    [Primitive("ABS", HelpString = "Absolute value ( n -- |n| )")]
    private static Task Prim_ABS(ForthInterpreter i)
    {
        i.EnsureStack(1, "ABS");
        var v = i._stack.PopValue();
        if (v.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        var n = v.LongValue;
        i._stack.Push(ForthValue.FromLong(n < 0 ? -n : n));
        return Task.CompletedTask;
    }
    [Primitive("+", HelpString = "Add two numbers ( a b -- sum )")]
    private static Task Prim_Plus(ForthInterpreter i)
    {
        i.EnsureStack(2, "+");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(a + b));
        return Task.CompletedTask;
    }

    [Primitive("-", HelpString = "Subtract top from second ( a b -- difference )")]
    private static Task Prim_Minus(ForthInterpreter i)
    {
        i.EnsureStack(2, "-");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(a - b));
        return Task.CompletedTask;
    }

    [Primitive("*", HelpString = "Multiply two numbers ( a b -- product )")]
    private static Task Prim_Star(ForthInterpreter i)
    {
        i.EnsureStack(2, "*");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(a * b));
        return Task.CompletedTask;
    }

    [Primitive("/", HelpString = "Divide second by top ( a b -- quotient )")]
    private static Task Prim_Slash(ForthInterpreter i)
    {
        i.EnsureStack(2, "/");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i._stack.Push(ForthValue.FromLong(a / b));
        return Task.CompletedTask;
    }

    [Primitive("/MOD", HelpString = "Divide and return remainder and quotient ( a b -- rem quot )")]
    private static Task Prim_SlashMod(ForthInterpreter i)
    {
        i.EnsureStack(2, "/MOD");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        var quot = a / b;
        var rem = a % b;
        i._stack.Push(ForthValue.FromLong(rem));
        i._stack.Push(ForthValue.FromLong(quot));
        return Task.CompletedTask;
    }

    [Primitive("MOD", HelpString = "Remainder of division ( a b -- rem )")]
    private static Task Prim_Mod(ForthInterpreter i)
    {
        i.EnsureStack(2, "MOD");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i._stack.Push(ForthValue.FromLong(a % b));
        return Task.CompletedTask;
    }

    // FM/MOD - Floored division and modulus for single-cell numbers
    // Returns remainder then quotient per /MOD convention
    [Primitive("FM/MOD", HelpString = "Floored division ( n1 n2 -- rem quot ) with remainder having sign of divisor")]
    private static Task Prim_FM_Slash_MOD(ForthInterpreter i)
    {
        i.EnsureStack(2, "FM/MOD");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        long b = bFv.LongValue;
        long a = aFv.LongValue;
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");

        long q = a / b; // truncates toward zero
        long r = a % b; // remainder with sign of dividend
        if (r != 0 && ((a ^ b) < 0)) // signs differ -> need floor adjustment
        {
            q -= 1;
            r += b; // remainder sign follows divisor (floored semantics)
        }

        i._stack.Push(ForthValue.FromLong(r));
        i._stack.Push(ForthValue.FromLong(q));
        return Task.CompletedTask;
    }

    [Primitive("MIN", HelpString = "Return smaller of two numbers ( a b -- min )")]
    private static Task Prim_MIN(ForthInterpreter i)
    {
        i.EnsureStack(2, "MIN");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(a < b ? a : b));
        return Task.CompletedTask;
    }

    [Primitive("MAX", HelpString = "Return larger of two numbers ( a b -- max )")]
    private static Task Prim_MAX(ForthInterpreter i)
    {
        i.EnsureStack(2, "MAX");
        var bFv = i._stack.PopValue();
        var aFv = i._stack.PopValue();
        if (bFv.Type != VT.Long || aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var b = bFv.LongValue;
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(a > b ? a : b));
        return Task.CompletedTask;
    }

    [Primitive("NEGATE", HelpString = "Negate number ( n -- -n )")]
    private static Task Prim_NEGATE(ForthInterpreter i)
    {
        i.EnsureStack(1, "NEGATE");
        var aFv = i._stack.PopValue();
        if (aFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        var a = aFv.LongValue;
        i._stack.Push(ForthValue.FromLong(-a));
        return Task.CompletedTask;
    }

    [Primitive("*/", HelpString = "Multiply two numbers then divide by third ( n1 n2 d -- (n1*n2)/d )")]
    private static Task Prim_StarSlash(ForthInterpreter i)
    {
        i.EnsureStack(3, "*/");
        var dFv = i._stack.PopValue();
        var n2Fv = i._stack.PopValue();
        var n1Fv = i._stack.PopValue();
        if (dFv.Type != VT.Long || n2Fv.Type != VT.Long || n1Fv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var dval = dFv.LongValue;
        var n2 = n2Fv.LongValue;
        var n1 = n1Fv.LongValue;
        if (dval == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        var prod = n1 * n2;
        i._stack.Push(ForthValue.FromLong(prod / dval));
        return Task.CompletedTask;
    }

    [Primitive("*/MOD", HelpString = "Multiply n1*n2 then divide by d returning remainder and quotient ( n1 n2 d -- rem quot )")]
    private static Task Prim_StarSlashMod(ForthInterpreter i)
    {
        i.EnsureStack(3, "*/MOD");
        var dFv = i._stack.PopValue();
        var n2Fv = i._stack.PopValue();
        var n1Fv = i._stack.PopValue();
        if (dFv.Type != VT.Long || n2Fv.Type != VT.Long || n1Fv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var d = dFv.LongValue;
        var n2 = n2Fv.LongValue;
        var n1 = n1Fv.LongValue;
        if (d == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");

        var prod = new BigInteger(n1) * new BigInteger(n2);
        var bigD = new BigInteger(d);
        var quotBig = BigInteger.Divide(prod, bigD);
        var remBig = BigInteger.Remainder(prod, bigD);

        if (quotBig < long.MinValue || quotBig > long.MaxValue || remBig < long.MinValue || remBig > long.MaxValue)
            throw new ForthException(ForthErrorCode.Unknown, "*/MOD result out of range");

        var quot = (long)quotBig;
        var rem = (long)remBig;
        // Push remainder then quotient as per /MOD convention
        i._stack.Push(ForthValue.FromLong(rem));
        i._stack.Push(ForthValue.FromLong(quot));
        return Task.CompletedTask;
    }

    // Double-cell addition: D+ (d1_lo d1_hi d2_lo d2_hi -- sum_lo sum_hi)
    [Primitive("D+", HelpString = "D+ ( d1 d2 -- d3 ) - add two double-cell numbers (low then high)")]
    private static Task Prim_DPlus(ForthInterpreter i)
    {
        i.EnsureStack(4, "D+");
        var d2_hiFv = i._stack.PopValue();
        var d2_loFv = i._stack.PopValue();
        var d1_hiFv = i._stack.PopValue();
        var d1_loFv = i._stack.PopValue();
        if (d2_hiFv.Type != VT.Long || d2_loFv.Type != VT.Long || d1_hiFv.Type != VT.Long || d1_loFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var d2_hi = d2_hiFv.LongValue;
        var d2_lo = d2_loFv.LongValue;
        var d1_hi = d1_hiFv.LongValue;
        var d1_lo = d1_loFv.LongValue;

        unchecked
        {
            ulong u1 = (ulong)d1_lo;
            ulong u2 = (ulong)d2_lo;
            ulong low = u1 + u2;
            // carry if overflow
            ulong carry = (low < u1) ? 1UL : 0UL;

            ulong uh1 = (ulong)d1_hi;
            ulong uh2 = (ulong)d2_hi;
            ulong high = uh1 + uh2 + carry;

            i._stack.Push(ForthValue.FromLong((long)low));
            i._stack.Push(ForthValue.FromLong((long)high));
        }

        return Task.CompletedTask;
    }

    // Double-cell subtraction: D- (d1 d2 -- d3) where d3 = d1 - d2
    [Primitive("D-", HelpString = "D- ( d1 d2 -- d3 ) - subtract two double-cell numbers (low then high)")]
    private static Task Prim_DMinus(ForthInterpreter i)
    {
        i.EnsureStack(4, "D-");
        var d2_hiFv = i._stack.PopValue();
        var d2_loFv = i._stack.PopValue();
        var d1_hiFv = i._stack.PopValue();
        var d1_loFv = i._stack.PopValue();
        if (d2_hiFv.Type != VT.Long || d2_loFv.Type != VT.Long || d1_hiFv.Type != VT.Long || d1_loFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var d2_hi = d2_hiFv.LongValue;
        var d2_lo = d2_loFv.LongValue;
        var d1_hi = d1_hiFv.LongValue;
        var d1_lo = d1_loFv.LongValue;

        unchecked
        {
            ulong u1 = (ulong)d1_lo;
            ulong u2 = (ulong)d2_lo;
            ulong low = u1 - u2;
            // borrow if underflow
            ulong borrow = (u1 < u2) ? 1UL : 0UL;

            ulong uh1 = (ulong)d1_hi;
            ulong uh2 = (ulong)d2_hi;
            ulong high = uh1 - uh2 - borrow;

            i._stack.Push(ForthValue.FromLong((long)low));
            i._stack.Push(ForthValue.FromLong((long)high));
        }

        return Task.CompletedTask;
    }

    // M* ( n1 n2 -- d ) multiply two single-cell numbers producing double-cell product
    [Primitive("M*", HelpString = "M* ( n1 n2 -- d ) - multiply two cells producing a double-cell result (low then high)")]
    private static Task Prim_MStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "M*");
        var n2Fv = i._stack.PopValue();
        var n1Fv = i._stack.PopValue();
        if (n2Fv.Type != VT.Long || n1Fv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var n2 = n2Fv.LongValue;
        var n1 = n1Fv.LongValue;

        var prod = new BigInteger(n1) * new BigInteger(n2);
        var mask = (BigInteger.One << 64) - 1;
        var lowBig = prod & mask;
        var highBig = prod >> 64; // arithmetic shift preserves sign for high cell

        long low = (long)(ulong)lowBig;
        long high = (long)highBig;

        i._stack.Push(ForthValue.FromLong(low));
        i._stack.Push(ForthValue.FromLong(high));
        return Task.CompletedTask;
    }

    // UM* ( u1 u2 -- ud ) unsigned multiply producing double-cell result (low then high)
    [Primitive("UM*", HelpString = "UM* ( u1 u2 -- ud ) - unsigned multiply to double-cell (low then high)")]
    private static Task Prim_UMStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "UM*");
        var u2Fv = i._stack.PopValue();
        var u1Fv = i._stack.PopValue();
        if (u2Fv.Type != VT.Long || u1Fv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        ulong u2 = (ulong)u2Fv.LongValue;
        ulong u1 = (ulong)u1Fv.LongValue;

        var prod = new BigInteger(u1) * new BigInteger(u2);
        var mask = (BigInteger.One << 64) - 1;
        var lowBig = prod & mask;
        var highBig = prod >> 64;

        long low = (long)(ulong)lowBig;
        long high = (long)(ulong)highBig;

        i._stack.Push(ForthValue.FromLong(low));
        i._stack.Push(ForthValue.FromLong(high));
        return Task.CompletedTask;
    }

    // UM/MOD ( ud u -- urem uquot ) unsigned division returning remainder then quotient
    [Primitive("UM/MOD", HelpString = "UM/MOD ( ud u -- urem uquot ) - unsigned division of double-cell by single-cell")]
    private static Task Prim_UMSlashMod(ForthInterpreter i)
    {
        i.EnsureStack(3, "UM/MOD");
        var uFv = i._stack.PopValue();
        var d_hiFv = i._stack.PopValue();
        var d_loFv = i._stack.PopValue();
        if (uFv.Type != VT.Long || d_hiFv.Type != VT.Long || d_loFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        ulong u = (ulong)uFv.LongValue;
        ulong hi = (ulong)d_hiFv.LongValue;
        ulong lo = (ulong)d_loFv.LongValue;
        if (u == 0UL) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");

        var dividend = (new BigInteger(hi) << 64) + new BigInteger(lo);
        var divisor = new BigInteger(u);
        var quot = BigInteger.Divide(dividend, divisor);
        var rem = BigInteger.Remainder(dividend, divisor);

        i._stack.Push(ForthValue.FromLong((long)(ulong)rem));
        i._stack.Push(ForthValue.FromLong((long)(ulong)quot));
        return Task.CompletedTask;
    }

    // SM/REM - Divide double-cell dividend by single-cell divisor using floored semantics
    // Stack: ( d_low d_high n -- rem quot )
    [Primitive("SM/REM", HelpString = "SM/REM ( d n -- rem quot ) - floored division of double-cell by single-cell")]
    private static Task Prim_SM_Slash_REM(ForthInterpreter i)
    {
        i.EnsureStack(3, "SM/REM");
        var nFv = i._stack.PopValue();
        var d_hiFv = i._stack.PopValue();
        var d_loFv = i._stack.PopValue();
        if (nFv.Type != VT.Long || d_hiFv.Type != VT.Long || d_loFv.Type != VT.Long)
            throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        long n = nFv.LongValue;
        long d_hi = d_hiFv.LongValue;
        long d_lo = d_loFv.LongValue;
        if (n == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");

        var dividend = (new BigInteger(d_hi) << 64) + new BigInteger((ulong)d_lo);
        var divisor = new BigInteger(n);

        var q0 = BigInteger.Divide(dividend, divisor);
        var r0 = BigInteger.Remainder(dividend, divisor);

        if (r0 != 0 && ((divisor.Sign > 0 && r0.Sign < 0) || (divisor.Sign < 0 && r0.Sign > 0)))
        {
            q0 -= BigInteger.One;
            r0 += divisor;
        }

        if (q0 < long.MinValue || q0 > long.MaxValue || r0 < long.MinValue || r0 > long.MaxValue)
            throw new ForthException(ForthErrorCode.Unknown, "SM/REM result out of range");

        long quot = (long)q0;
        long rem = (long)r0;
        i._stack.Push(ForthValue.FromLong(rem));
        i._stack.Push(ForthValue.FromLong(quot));
        return Task.CompletedTask;
    }

    [Primitive("S>D", HelpString = "S>D ( n -- d ) - convert single-cell to double-cell with sign extension")]
    private static Task Prim_SToD(ForthInterpreter i)
    {
        i.EnsureStack(1, "S>D");
        var nFv = i._stack.PopValue();
        if (nFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected long");
        var n = nFv.LongValue;
        long high = n < 0 ? -1L : 0L;
        i._stack.Push(ForthValue.FromLong(n));
        i._stack.Push(ForthValue.FromLong(high));
        return Task.CompletedTask;
    }

    [Primitive("M+", HelpString = "M+ ( d n -- d' ) - add single-cell n to double-cell d")]
    private static Task Prim_MPlus(ForthInterpreter i)
    {
        i.EnsureStack(3, "M+");
        var nFv = i._stack.PopValue();
        var d_highFv = i._stack.PopValue();
        var d_lowFv = i._stack.PopValue();
        if (nFv.Type != VT.Long || d_highFv.Type != VT.Long || d_lowFv.Type != VT.Long) throw new ForthException(ForthErrorCode.TypeError, "Expected longs");
        var n = nFv.LongValue;
        var d_high = d_highFv.LongValue;
        var d_low = d_lowFv.LongValue;

        long n_high = n < 0 ? -1L : 0L;

        unchecked
        {
            ulong u_d = (ulong)d_low;
            ulong u_n = (ulong)n;
            ulong low = u_d + u_n;
            ulong carry = (low < u_d) ? 1UL : 0UL;

            ulong uh_d = (ulong)d_high;
            ulong uh_n = (ulong)n_high;
            ulong high = uh_d + uh_n + carry;

            i._stack.Push(ForthValue.FromLong((long)low));
            i._stack.Push(ForthValue.FromLong((long)high));
        }

        return Task.CompletedTask;
    }
}
