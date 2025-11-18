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
}
