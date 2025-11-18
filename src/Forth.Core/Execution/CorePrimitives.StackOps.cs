using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("DUP", HelpString = "Duplicate top stack item ( x -- x x )")]
    private static Task Prim_DUP(ForthInterpreter i) { i.EnsureStack(1, "DUP"); i.Push(i.StackTop()); return Task.CompletedTask; }

    [Primitive("2DUP", HelpString = "Duplicate top two stack items ( x y -- x y x y )")]
    private static Task Prim_2DUP(ForthInterpreter i) { i.EnsureStack(2, "2DUP"); var a = i.StackNthFromTop(2); var b = i.StackNthFromTop(1); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("DROP", HelpString = "Drop top stack item ( x -- )")]
    private static Task Prim_DROP(ForthInterpreter i) { i.EnsureStack(1, "DROP"); i.DropTop(); return Task.CompletedTask; }

    [Primitive("SWAP", HelpString = "Swap top two items ( a b -- b a )")]
    private static Task Prim_SWAP(ForthInterpreter i) { i.EnsureStack(2, "SWAP"); i.SwapTop2(); return Task.CompletedTask; }

    [Primitive("2SWAP", HelpString = "Swap two pairs on the stack ( a b c d -- c d a b )")]
    private static Task Prim_2SWAP(ForthInterpreter i) { i.EnsureStack(4, "2SWAP"); var d2 = i.PopInternal(); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(d2); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("OVER", HelpString = "Copy second item to top ( a b -- a b a )")]
    private static Task Prim_OVER(ForthInterpreter i) { i.EnsureStack(2, "OVER"); i.Push(i.StackNthFromTop(2)); return Task.CompletedTask; }

    [Primitive("ROT", HelpString = "Rotate third to top ( a b c -- b c a )")]
    private static Task Prim_ROT(ForthInterpreter i) { i.EnsureStack(3, "ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); return Task.CompletedTask; }

    [Primitive("-ROT", HelpString = "Reverse rotate ( a b c -- c a b )")]
    private static Task Prim_NEGROT(ForthInterpreter i) { i.EnsureStack(3, "-ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); return Task.CompletedTask; }

    [Primitive("PICK", HelpString = "Copy Nth item from top ( n -- ) \nPICK expects index n and pushes the item at that depth")]
    private static Task Prim_PICK(ForthInterpreter i) { i.EnsureStack(1, "PICK"); var n = ToLong(i.PopInternal()); if (n < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: negative index {n}"); if (n >= i.Stack.Count) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: index {n} exceeds stack depth {i.Stack.Count}"); i.Push(i.StackNthFromTop((int)n + 1)); return Task.CompletedTask; }

    [Primitive("DEPTH", HelpString = "Return current stack depth ( -- n )")]
    private static Task Prim_DEPTH(ForthInterpreter i) { i.Push((long)i.Stack.Count); return Task.CompletedTask; }

    [Primitive("2OVER", HelpString = "2OVER ( a b c d -- a b c d a b ) - copy pair two down into top")]
    private static Task Prim_2OVER(ForthInterpreter i)
    {
        i.EnsureStack(4, "2OVER");
        var a = i.StackNthFromTop(4);
        var b = i.StackNthFromTop(3);
        i.Push(a);
        i.Push(b);
        return Task.CompletedTask;
    }
}
