using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class MemorySpacePrimitives
{
    [Primitive("CELL+", HelpString = "CELL+ ( addr -- addr+1 ) - add cell size to address")]
    private static Task Prim_CellPlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "CELL+");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(addr + 1);
        return Task.CompletedTask;
    }

    [Primitive("CELLS", HelpString = "CELLS ( n -- n*cellsize ) - multiply by cell size")]
    private static Task Prim_Cells(ForthInterpreter i)
    {
        i.EnsureStack(1, "CELLS");
        var n = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(n * 1);
        return Task.CompletedTask;
    }

    [Primitive("CHAR+", HelpString = "CHAR+ ( c-addr -- c-addr+1 ) - add char size to address")]
    private static Task Prim_CharPlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "CHAR+");
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(addr + 1);
        return Task.CompletedTask;
    }

    [Primitive("CHARS", HelpString = "CHARS ( n -- n*charsize ) - multiply by char size")]
    private static Task Prim_Chars(ForthInterpreter i)
    {
        i.EnsureStack(1, "CHARS");
        var n = PrimitivesUtil.ToLong(i.PopInternal());
        i.Push(n * 1);
        return Task.CompletedTask;
    }

    [Primitive("ALIGN", HelpString = "ALIGN ( -- ) - align dictionary pointer to cell boundary")]
    private static Task Prim_Align(ForthInterpreter i) => Task.CompletedTask;

    [Primitive("ALLOT", HelpString = "ALLOT ( u -- ) - reserve u cells in dictionary")]
    private static Task Prim_ALLOT(ForthInterpreter i)
    {
        i.EnsureStack(1, "ALLOT");
        var cells = PrimitivesUtil.ToLong(i.PopInternal());
        if (cells < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size");
        for (long k = 0; k < cells; k++) i._mem[i._nextAddr++] = 0;
        return Task.CompletedTask;
    }

    [Primitive("HERE", HelpString = "HERE ( -- addr ) - push current dictionary allocation pointer")]
    private static Task Prim_HERE(ForthInterpreter i)
    {
        i.Push(i._nextAddr);
        return Task.CompletedTask;
    }

    [Primitive("UNUSED", HelpString = "UNUSED ( -- u ) - return the number of cells remaining in the data space")]
    private static Task Prim_UNUSED(ForthInterpreter i)
    {
        i.Push(1000000L - i._nextAddr);
        return Task.CompletedTask;
    }

    [Primitive("PAD", HelpString = "PAD ( -- addr ) - push address of scratch pad buffer")]
    private static Task Prim_PAD(ForthInterpreter i)
    {
        i.Push(i.PadAddr);
        return Task.CompletedTask;
    }
}
