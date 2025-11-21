using Forth.Core.Interpreter;
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
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a + b);
        return Task.CompletedTask;
    }

    [Primitive("-", HelpString = "Subtract top from second ( a b -- difference )")]
    private static Task Prim_Minus(ForthInterpreter i)
    {
        i.EnsureStack(2, "-");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a - b);
        return Task.CompletedTask;
    }

    [Primitive("*", HelpString = "Multiply two numbers ( a b -- product )")]
    private static Task Prim_Star(ForthInterpreter i)
    {
        i.EnsureStack(2, "*");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a * b);
        return Task.CompletedTask;
    }

    [Primitive("/", HelpString = "Divide second by top ( a b -- quotient )")]
    private static Task Prim_Slash(ForthInterpreter i)
    {
        i.EnsureStack(2, "/");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i.Push(a / b);
        return Task.CompletedTask;
    }

    [Primitive("/MOD", HelpString = "Divide and return remainder and quotient ( a b -- rem quot )")]
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

    [Primitive("MOD", HelpString = "Remainder of division ( a b -- rem )")]
    private static Task Prim_Mod(ForthInterpreter i)
    {
        i.EnsureStack(2, "MOD");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        if (b == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
        i.Push(a % b);
        return Task.CompletedTask;
    }

    [Primitive("MIN", HelpString = "Return smaller of two numbers ( a b -- min )")]
    private static Task Prim_MIN(ForthInterpreter i)
    {
        i.EnsureStack(2, "MIN");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a < b ? a : b);
        return Task.CompletedTask;
    }

    [Primitive("MAX", HelpString = "Return larger of two numbers ( a b -- max )")]
    private static Task Prim_MAX(ForthInterpreter i)
    {
        i.EnsureStack(2, "MAX");
        var b = ToLong(i.PopInternal());
        var a = ToLong(i.PopInternal());
        i.Push(a > b ? a : b);
        return Task.CompletedTask;
    }

    [Primitive("NEGATE", HelpString = "Negate number ( n -- -n )")]
    private static Task Prim_NEGATE(ForthInterpreter i)
    {
        i.EnsureStack(1, "NEGATE");
        var a = ToLong(i.PopInternal());
        i.Push(-a);
        return Task.CompletedTask;
    }

    [Primitive("*/", HelpString = "Multiply two numbers then divide by third ( n1 n2 d -- (n1*n2)/d )")]
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

    [Primitive("*/MOD", HelpString = "Multiply n1*n2 then divide by d returning remainder and quotient ( n1 n2 d -- rem quot )")]
    private static Task Prim_StarSlashMod(ForthInterpreter i)
    {
        i.EnsureStack(3, "*/MOD");
        var d = ToLong(i.PopInternal());
        var n2 = ToLong(i.PopInternal());
        var n1 = ToLong(i.PopInternal());
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
        i.Push(rem);
        i.Push(quot);
        return Task.CompletedTask;
    }

    // Double-cell addition: D+ (d1_lo d1_hi d2_lo d2_hi -- sum_lo sum_hi)
    [Primitive("D+", HelpString = "D+ ( d1 d2 -- d3 ) - add two double-cell numbers (low then high)")]
    private static Task Prim_DPlus(ForthInterpreter i)
    {
        i.EnsureStack(4, "D+");
        var d2_hi = ToLong(i.PopInternal());
        var d2_lo = ToLong(i.PopInternal());
        var d1_hi = ToLong(i.PopInternal());
        var d1_lo = ToLong(i.PopInternal());

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

            i.Push((long)low);
            i.Push((long)high);
        }

        return Task.CompletedTask;
    }

    // Double-cell subtraction: D- (d1 d2 -- d3) where d3 = d1 - d2
    [Primitive("D-", HelpString = "D- ( d1 d2 -- d3 ) - subtract two double-cell numbers (low then high)")]
    private static Task Prim_DMinus(ForthInterpreter i)
    {
        i.EnsureStack(4, "D-");
        var d2_hi = ToLong(i.PopInternal());
        var d2_lo = ToLong(i.PopInternal());
        var d1_hi = ToLong(i.PopInternal());
        var d1_lo = ToLong(i.PopInternal());

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

            i.Push((long)low);
            i.Push((long)high);
        }

        return Task.CompletedTask;
    }

    // M* ( n1 n2 -- d ) multiply two single-cell numbers producing double-cell product
    [Primitive("M*", HelpString = "M* ( n1 n2 -- d ) - multiply two cells producing a double-cell result (low then high)")]
    private static Task Prim_MStar(ForthInterpreter i)
    {
        i.EnsureStack(2, "M*");
        var n2 = ToLong(i.PopInternal());
        var n1 = ToLong(i.PopInternal());

        var prod = new BigInteger(n1) * new BigInteger(n2);
        var mask = (BigInteger.One << 64) - 1;
        var lowBig = prod & mask;
        var highBig = prod >> 64; // arithmetic shift preserves sign for high cell

        long low = (long)(ulong)lowBig;
        long high = (long)highBig;

        i.Push(low);
        i.Push(high);
        return Task.CompletedTask;
    }
}
