using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("DUP")]
    private static Task Prim_DUP(ForthInterpreter i) { i.EnsureStack(1, "DUP"); i.Push(i.StackTop()); return Task.CompletedTask; }

    [Primitive("2DUP")]
    private static Task Prim_2DUP(ForthInterpreter i) { i.EnsureStack(2, "2DUP"); var a = i.StackNthFromTop(2); var b = i.StackNthFromTop(1); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("DROP")]
    private static Task Prim_DROP(ForthInterpreter i) { i.EnsureStack(1, "DROP"); i.DropTop(); return Task.CompletedTask; }

    [Primitive("SWAP")]
    private static Task Prim_SWAP(ForthInterpreter i) { i.EnsureStack(2, "SWAP"); i.SwapTop2(); return Task.CompletedTask; }

    [Primitive("2SWAP")]
    private static Task Prim_2SWAP(ForthInterpreter i) { i.EnsureStack(4, "2SWAP"); var d2 = i.PopInternal(); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(d2); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("OVER")]
    private static Task Prim_OVER(ForthInterpreter i) { i.EnsureStack(2, "OVER"); i.Push(i.StackNthFromTop(2)); return Task.CompletedTask; }

    [Primitive("ROT")]
    private static Task Prim_ROT(ForthInterpreter i) { i.EnsureStack(3, "ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); return Task.CompletedTask; }

    [Primitive("-ROT")]
    private static Task Prim_NEGROT(ForthInterpreter i) { i.EnsureStack(3, "-ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("PICK")]
    private static Task Prim_PICK(ForthInterpreter i) { i.EnsureStack(1, "PICK"); var n = ToLong(i.PopInternal()); if (n < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: negative index {n}"); if (n >= i.Stack.Count) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: index {n} exceeds stack depth {i.Stack.Count}"); i.Push(i.StackNthFromTop((int)n + 1)); return Task.CompletedTask; }

    [Primitive("DEPTH")]
    private static Task Prim_DEPTH(ForthInterpreter i) { i.Push((long)i.Stack.Count); return Task.CompletedTask; }
}
