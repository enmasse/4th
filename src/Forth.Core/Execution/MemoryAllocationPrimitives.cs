using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class MemoryAllocationPrimitives
{
    [Primitive("ALLOCATE", HelpString = "ALLOCATE ( u -- a-addr ior ) - allocate u bytes of memory")]
    private static Task Prim_Allocate(ForthInterpreter i)
    {
        i.EnsureStack(1, "ALLOCATE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0)
        {
            i.Push(0L);
            i.Push(-1L);
            return Task.CompletedTask;
        }
        var bytes = new byte[u];
        var addr = i._heapPtr;
        i._heapAllocations[addr] = (bytes, u);
        i._heapPtr += u;
        i.Push(addr);
        i.Push(0L);
        return Task.CompletedTask;
    }

    [Primitive("FREE", HelpString = "FREE ( a-addr -- ior ) - deallocate memory at a-addr")]
    private static Task Prim_Free(ForthInterpreter i)
    {
        i.EnsureStack(1, "FREE");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(i._heapAllocations.Remove(addr) ? 0L : -1L);
        return Task.CompletedTask;
    }

    [Primitive("RESIZE", HelpString = "RESIZE ( a-addr1 u -- a-addr2 ior ) - resize allocated memory")]
    private static Task Prim_Resize(ForthInterpreter i)
    {
        i.EnsureStack(2, "RESIZE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr1 = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0)
        {
            i.Push(0L);
            i.Push(-1L);
            return Task.CompletedTask;
        }
        if (!i._heapAllocations.TryGetValue(addr1, out var oldAlloc))
        {
            i.Push(0L);
            i.Push(-1L);
            return Task.CompletedTask;
        }

        var bytes = new byte[u];
        var copyLen = (int)System.Math.Min(u, oldAlloc.size);
        if (copyLen > 0)
        {
            System.Array.Copy(oldAlloc.bytes, 0, bytes, 0, copyLen);
        }

        i._heapAllocations.Remove(addr1);
        var addr2 = i._heapPtr;
        i._heapAllocations[addr2] = (bytes, u);
        i._heapPtr += u;

        i.Push(addr2);
        i.Push(0L);
        return Task.CompletedTask;
    }
}
