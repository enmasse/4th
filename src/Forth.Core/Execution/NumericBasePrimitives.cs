using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class NumericBasePrimitives
{
    [Primitive("BASE", HelpString = "Push address of BASE variable")]
    private static Task Prim_BASE(ForthInterpreter i) { i.Push(i.BaseAddr); return Task.CompletedTask; }

    [Primitive("DECIMAL", HelpString = "Set number base to decimal")]
    private static Task Prim_DECIMAL(ForthInterpreter i) { i.MemSet(i.BaseAddr, 10); return Task.CompletedTask; }

    [Primitive("HEX", HelpString = "Set number base to hexadecimal")]
    private static Task Prim_HEX(ForthInterpreter i) { i.MemSet(i.BaseAddr, 16); return Task.CompletedTask; }

    [Primitive("STATE", HelpString = "Push address of STATE variable")]
    private static Task Prim_STATE(ForthInterpreter i) { i.Push(i.StateAddr); return Task.CompletedTask; }
}
