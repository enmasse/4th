using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class MemoryCellPrimitives
{
    [Primitive("@", HelpString = "@ ( addr -- value ) - fetch cell at address")]
    private static Task Prim_At(ForthInterpreter i)
    {
        i.EnsureStack(1, "@");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGetObject(addr, out object v);
        i.Push(v);
        return Task.CompletedTask;
    }

    [Primitive("!", HelpString = "! ( x addr -- ) - store x at address")]
    private static Task Prim_Bang(ForthInterpreter i)
    {
        i.EnsureStack(2, "!");
        var addrObj = i.PopInternal();
        var valObj = i.PopInternal();

        long addr = addrObj switch
        {
            Word w when w.BodyAddr.HasValue => w.BodyAddr.Value,
            _ => PrimitivesUtil.ToLong(addrObj)
        };

        if (valObj is Word)
        {
            i.MemSet(addr, valObj);
            i._lastStoreAddr = addr;
            i._lastStoreValue = 0;
        }
        else
        {
            long valueToStore = PrimitivesUtil.ToLong(valObj);
            i.MemSet(addr, valueToStore);
            i._lastStoreAddr = addr;
            i._lastStoreValue = valueToStore;
        }

        return Task.CompletedTask;
    }

    [Primitive("+!", HelpString = "+! ( x addr -- ) - add x to cell at addr")]
    private static Task Prim_PlusBang(ForthInterpreter i)
    {
        i.EnsureStack(2, "+!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var add = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var cur);
        i.MemSet(addr, PrimitivesUtil.ToLong(cur) + add);
        return Task.CompletedTask;
    }

    [Primitive(",", HelpString = ", ( x -- ) - append cell to dictionary and advance here")]
    private static Task Prim_Comma(ForthInterpreter i)
    {
        i.EnsureStack(1, ",");
        var v = PrimitivesUtil.ToLong(i.PopInternal());
        i._mem[i._nextAddr++] = v;
        return Task.CompletedTask;
    }

    [Primitive("2!", HelpString = "2! ( x1 x2 addr -- ) - store two cells at address")]
    private static Task Prim_2Bang(ForthInterpreter i)
    {
        i.EnsureStack(3, "2!");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        var x2 = PrimitivesUtil.ToLong(i.PopInternal());
        var x1 = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemSet(addr, x1);
        i.MemSet(addr + 1, x2);
        return Task.CompletedTask;
    }

    [Primitive("2@", HelpString = "2@ ( addr -- x1 x2 ) - fetch two cells from address")]
    private static Task Prim_2At(ForthInterpreter i)
    {
        i.EnsureStack(1, "2@");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(addr, out var x1);
        i.MemTryGet(addr + 1, out var x2);
        i.Push(x1);
        i.Push(x2);
        return Task.CompletedTask;
    }

#if DEBUG
    [Primitive("LAST-STORE", HelpString = "LAST-STORE ( -- addr val ) - diagnostics: last ! store")]
    private static Task Prim_LAST_STORE(ForthInterpreter i)
    {
        i.Push(i._lastStoreAddr);
        i.Push(i._lastStoreValue);
        return Task.CompletedTask;
    }
#endif
}
