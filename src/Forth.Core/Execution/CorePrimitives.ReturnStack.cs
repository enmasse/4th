using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive(">R")]
    private static Task Prim_ToR(ForthInterpreter i) { i.EnsureStack(1, ">R"); var a = i.PopInternal(); i.RPush(a); return Task.CompletedTask; }

    [Primitive("R>")]
    private static Task Prim_RFrom(ForthInterpreter i) { if (i.RCount == 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Return stack underflow in R>"); var a = i.RPop(); i.Push(a); return Task.CompletedTask; }

    [Primitive("2>R")]
    private static Task Prim_2ToR(ForthInterpreter i) { i.EnsureStack(2, "2>R"); var b = i.PopInternal(); var a = i.PopInternal(); i.RPush(a); i.RPush(b); return Task.CompletedTask; }

    [Primitive("2R>")]
    private static Task Prim_2RFrom(ForthInterpreter i) { if (i.RCount < 2) throw new ForthException(ForthErrorCode.StackUnderflow, "Return stack underflow in 2R>"); var b = i.RPop(); var a = i.RPop(); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("RP@")]
    private static Task Prim_RPAt(ForthInterpreter i) { i.Push((long)i.RCount); return Task.CompletedTask; }
}
