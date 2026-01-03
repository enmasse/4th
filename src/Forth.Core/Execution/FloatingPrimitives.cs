using System;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Globalization;

namespace Forth.Core.Execution;

internal static class FloatingPrimitives
{
    [Primitive("FCONSTANT", IsImmediate = true, HelpString = "FCONSTANT <name> - define a floating constant from top of stack")]
    private static Task Prim_FCONSTANT(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after FCONSTANT");
        if (i.Stack.Count == 0)
        {
            if (i.TryResolveWord(name, out var existing) && existing is not null)
            {
                return Task.CompletedTask;
            }
            throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow in FCONSTANT");
        }

        i.EnsureStack(1, "FCONSTANT");
        var val = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(val);
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
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var d = System.BitConverter.Int64BitsToDouble(bits);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("F!", HelpString = "F! ( addr f -- ) - store double at address")]
    private static Task Prim_FBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "F!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(val);
        var bits = System.BitConverter.DoubleToInt64Bits(d);
        i.MemSet(addr, bits);
        return Task.CompletedTask;
    }

    [Primitive("FLOATS", HelpString = "FLOATS ( n -- n' ) - scale by float-cell size (8 bytes for double precision)")]
    private static Task Prim_FLOATS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FLOATS");
        var n = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(n * 8L);
        return Task.CompletedTask;
    }

    [Primitive("F>S", HelpString = "F>S ( r -- n ) - convert floating-point number to single-cell integer")]
    private static Task Prim_FToS(ForthInterpreter i)
    {
        i.EnsureStack(1, "F>S");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        i.Push((long)d);
        return Task.CompletedTask;
    }

    [Primitive("S>F", HelpString = "S>F ( n -- r ) - convert single-cell integer to floating-point number")]
    private static Task Prim_SToF(ForthInterpreter i)
    {
        i.EnsureStack(1, "S>F");
        var n = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(n);
        i.Push(d);
        return Task.CompletedTask;
    }

    [Primitive("D>F", HelpString = "D>F ( d -- r ) - convert double-cell integer to floating-point number")]
    private static Task Prim_DToF(ForthInterpreter i)
    {
        i.EnsureStack(2, "D>F");
        _ = PrimitivesUtil.ToLong(i.PopInternal());
        var lo = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push((double)lo);
        return Task.CompletedTask;
    }

    [Primitive("F>D", HelpString = "F>D ( r -- d ) - convert floating-point number to double-cell integer")]
    private static Task Prim_FToD(ForthInterpreter i)
    {
        i.EnsureStack(1, "F>D");
        var r = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(r);
        var intVal = (long)d;
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
        long u = PrimitivesUtil.ToLong(uObj);
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
            throw new ForthException(ForthErrorCode.TypeError, $">FLOAT received unexpected type: {addrObj?.GetType().Name ?? "null"}");
        }

        var style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

        if (slice.Length > 0 && (char.IsWhiteSpace(slice[0]) || char.IsWhiteSpace(slice[^1])))
        {
            i.Push(addrObj);
            i.Push(uObj);
            i.Push(0L);
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(slice))
        {
            i.Push(0.0);
            i.Push(-1L);
            return Task.CompletedTask;
        }

        if (double.TryParse(slice, style, CultureInfo.InvariantCulture, out var result) && slice.Trim() != ".")
        {
            i.Push(result);
            i.Push(-1L);
        }
        else
        {
            i.Push(addrObj);
            i.Push(uObj);
            i.Push(0L);
        }

        return Task.CompletedTask;
    }

    [Primitive("SF!", HelpString = "SF! ( r addr -- ) - store single-precision float (32-bit) at address")]
    private static Task Prim_SFBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "SF!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(val);
        var bits = System.BitConverter.SingleToInt32Bits((float)d);
        i.MemSet(addr, bits);
        return Task.CompletedTask;
    }

    [Primitive("SF@", HelpString = "SF@ ( addr -- r ) - fetch single-precision float (32-bit) from address")]
    private static Task Prim_SFAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "SF@");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var f = System.BitConverter.Int32BitsToSingle((int)bits);
        i.Push((double)f);
        return Task.CompletedTask;
    }

    [Primitive("DF!", HelpString = "DF! ( r addr -- ) - store double-precision float (64-bit) at address")]
    private static Task Prim_DFBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "DF!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var val = i.PopInternal();
        var d = PrimitivesUtil.ToDoubleFromObj(val);
        var bits = System.BitConverter.DoubleToInt64Bits(d);
        i.MemSet(addr, bits);
        return Task.CompletedTask;
    }

    [Primitive("DF@", HelpString = "DF@ ( addr -- r ) - fetch double-precision float (64-bit) from address")]
    private static Task Prim_DFAt(ForthInterpreter i)
    {
        i.EnsureStack(1, "DF@");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var bits);
        var d = System.BitConverter.Int64BitsToDouble(bits);
        i.Push(d);
        return Task.CompletedTask;
    }
}
