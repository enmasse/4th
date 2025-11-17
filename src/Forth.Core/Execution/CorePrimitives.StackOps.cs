using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddStackOpsEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "DUP")] = new(i => { i.EnsureStack(1, "DUP"); i.Push(i.StackTop()); return Task.CompletedTask; });
        d[(null, "2DUP")] = new(i => { i.EnsureStack(2, "2DUP"); var a = i.StackNthFromTop(2); var b = i.StackNthFromTop(1); i.Push(a); i.Push(b); return Task.CompletedTask; });
        d[(null, "DROP")] = new(i => { i.EnsureStack(1, "DROP"); i.DropTop(); return Task.CompletedTask; });
        d[(null, "SWAP")] = new(i => { i.EnsureStack(2, "SWAP"); i.SwapTop2(); return Task.CompletedTask; });
        d[(null, "2SWAP")] = new(i => { i.EnsureStack(4, "2SWAP"); var d2 = i.PopInternal(); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(d2); i.Push(a); i.Push(b); return Task.CompletedTask; });
        d[(null, "OVER")] = new(i => { i.EnsureStack(2, "OVER"); i.Push(i.StackNthFromTop(2)); return Task.CompletedTask; });
        d[(null, "ROT")] = new(i => { i.EnsureStack(3, "ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); return Task.CompletedTask; });
        d[(null, "-ROT")] = new(i => { i.EnsureStack(3, "-ROT"); var c = i.PopInternal(); var b = i.PopInternal(); var a = i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); return Task.CompletedTask; });
        d[(null, "PICK")] = new(i => { i.EnsureStack(1, "PICK"); var n = ToLong(i.PopInternal()); if (n < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: negative index {n}"); if (n >= i.Stack.Count) throw new ForthException(ForthErrorCode.StackUnderflow, $"PICK: index {n} exceeds stack depth {i.Stack.Count}"); i.Push(i.StackNthFromTop((int)n + 1)); return Task.CompletedTask; });
        d[(null, "DEPTH")] = new(i => { i.Push((long)i.Stack.Count); return Task.CompletedTask; });
    }
}
