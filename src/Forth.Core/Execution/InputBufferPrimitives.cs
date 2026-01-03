using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class InputBufferPrimitives
{
    [Primitive("SOURCE", HelpString = "SOURCE ( -- addr u ) - return address and length of current input buffer")]
    private static Task Prim_SOURCE(ForthInterpreter i)
    {
        var src = i.CurrentSource ?? string.Empty;

        i.MemTryGet(i.InAddr, out var savedIn);
        var addr = i.AllocateSourceString(src);
        i._mem[i.InAddr] = savedIn;

        i.Push(addr);
        i.Push((long)src.Length);
        return Task.CompletedTask;
    }

    [Primitive(">IN", HelpString = ">IN ( -- addr ) - address of >IN index cell")]
    private static Task Prim_IN(ForthInterpreter i)
    {
        i.Push(i.InAddr);
        return Task.CompletedTask;
    }
}
