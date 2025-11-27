using Forth.Core.Interpreter;
using VT = Forth.Core.Interpreter.ValueType;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
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

    [Primitive("0<", HelpString = "0< ( n -- flag ) true if n < 0")]
    private static Task Prim_ZeroLess(ForthInterpreter i)
    {
        i.EnsureStack(1, "0<");
        var n = ToLong(i.PopInternal());
        i.Push(n < 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("0>", HelpString = "0> ( n -- flag ) true if n > 0")]
    private static Task Prim_ZeroGreater(ForthInterpreter i)
    {
        i.EnsureStack(1, "0>");
        var n = ToLong(i.PopInternal());
        i.Push(n > 0 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("1+", HelpString = "1+ ( n -- n+1 ) increment by 1")]
    private static Task Prim_OnePlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "1+");
        var n = ToLong(i.PopInternal());
        i.Push(n + 1);
        return Task.CompletedTask;
    }

    [Primitive("1-", HelpString = "1- ( n -- n-1 ) decrement by 1")]
    private static Task Prim_OneMinus(ForthInterpreter i)
    {
        i.EnsureStack(1, "1-");
        var n = ToLong(i.PopInternal());
        i.Push(n - 1);
        return Task.CompletedTask;
    }

    [Primitive("ABS", HelpString = "ABS ( n -- |n| ) absolute value")]
    private static Task Prim_Abs(ForthInterpreter i)
    {
        i.EnsureStack(1, "ABS");
        var n = ToLong(i.PopInternal());
        i.Push(n < 0 ? -n : n);
        return Task.CompletedTask;
    }

    [Primitive("2*", HelpString = "2* ( n -- n*2 ) arithmetic left shift by 1")]
    private static Task Prim_2Star(ForthInterpreter i)
    {
        i.EnsureStack(1, "2*");
        var n = ToLong(i.PopInternal());
        i.Push(n << 1);
        return Task.CompletedTask;
    }

    [Primitive("2/", HelpString = "2/ ( n -- n/2 ) arithmetic right shift by 1")]
    private static Task Prim_2Slash(ForthInterpreter i)
    {
        i.EnsureStack(1, "2/");
        var n = ToLong(i.PopInternal());
        i.Push(n >> 1);
        return Task.CompletedTask;
    }

    [Primitive("U<", HelpString = "U< ( u1 u2 -- flag ) unsigned less than")]
    private static Task Prim_ULess(ForthInterpreter i)
    {
        i.EnsureStack(2, "U<");
        var u2 = ToLong(i.PopInternal());
        var u1 = ToLong(i.PopInternal());
        i.Push((ulong)u1 < (ulong)u2 ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("UM*", HelpString = "UM* ( u1 u2 -- ud ) unsigned multiply producing double-cell")]
    private static Task Prim_UMStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "UM*");
        var u2 = (ulong)ToLong(i.PopInternal());
        var u1 = (ulong)ToLong(i.PopInternal());
        var prod = u1 * u2;
        var low = (long)(prod & 0xFFFFFFFFFFFFFFFFUL);
        var high = (long)(prod >> 64);
        i.Push(low);
        i.Push(high);
        return Task.CompletedTask;
    }

    [Primitive("UM/MOD", HelpString = "UM/MOD ( ud u -- rem quot ) unsigned divide double-cell by single-cell")]
    private static Task Prim_UMSlashMod(ForthInterpreter i)
    {
        i.EnsureStack(3, "UM/MOD");
        var u = (ulong)ToLong(i.PopInternal());
        var ud_high = (ulong)ToLong(i.PopInternal());
        var ud_low = (ulong)ToLong(i.PopInternal());
        var dividend = (ud_high << 64) | ud_low;
        var quot = dividend / u;
        var rem = dividend % u;
        i.Push((long)rem);
        i.Push((long)quot);
        return Task.CompletedTask;
    }
}
