using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("AND", HelpString = "Bitwise AND of two numbers ( a b -- a&b )")]
    private static Task Prim_AND(ForthInterpreter i) { i.EnsureStack(2, "AND"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a & b); return Task.CompletedTask; }

    [Primitive("OR", HelpString = "Bitwise OR of two numbers ( a b -- a|b )")]
    private static Task Prim_OR(ForthInterpreter i) { i.EnsureStack(2, "OR"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a | b); return Task.CompletedTask; }

    [Primitive("XOR", HelpString = "Bitwise XOR of two numbers ( a b -- a^b )")]
    private static Task Prim_XOR(ForthInterpreter i) { i.EnsureStack(2, "XOR"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a ^ b); return Task.CompletedTask; }

    [Primitive("INVERT", HelpString = "Bitwise NOT of top item ( a -- ~a )")]
    private static Task Prim_INVERT(ForthInterpreter i) { i.EnsureStack(1, "INVERT"); var a = ToLong(i.PopInternal()); i.Push(~a); return Task.CompletedTask; }

    [Primitive("LSHIFT", HelpString = "Left shift a by n bits ( a n -- (a<<n) )")]
    private static Task Prim_LSHIFT(ForthInterpreter i) { i.EnsureStack(2, "LSHIFT"); var u = ToLong(i.PopInternal()); var x = ToLong(i.PopInternal()); i.Push((long)((ulong)x << (int)u)); return Task.CompletedTask; }

    [Primitive("RSHIFT", HelpString = "Logical right shift a by n bits ( a n -- (a>>n) )")]
    private static Task Prim_RSHIFT(ForthInterpreter i) { i.EnsureStack(2, "RSHIFT"); var u = ToLong(i.PopInternal()); var x = ToLong(i.PopInternal()); i.Push((long)((ulong)x >> (int)u)); return Task.CompletedTask; }
}
