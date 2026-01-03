using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class FloatingStackPrimitives
{
    [Primitive("FOVER", HelpString = "FOVER ( r1 r2 -- r1 r2 r1 ) - copy second floating item to top")]
    private static Task Prim_FOVER(ForthInterpreter i)
    {
        i.EnsureStack(2, "FOVER");
        var r2 = i.PopInternal();
        var r1 = i.PopInternal();
        i.Push(r1);
        i.Push(r2);
        i.Push(PrimitivesUtil.ToDoubleFromObj(r1));
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
        var d = PrimitivesUtil.ToDoubleFromObj(r);
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
}
