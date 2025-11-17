using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddBitwiseEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "AND")] = new(i => { i.EnsureStack(2, "AND"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a & b); return Task.CompletedTask; });
        d[(null, "OR")] = new(i => { i.EnsureStack(2, "OR"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a | b); return Task.CompletedTask; });
        d[(null, "XOR")] = new(i => { i.EnsureStack(2, "XOR"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a ^ b); return Task.CompletedTask; });
        d[(null, "INVERT")] = new(i => { i.EnsureStack(1, "INVERT"); var a = ToLong(i.PopInternal()); i.Push(~a); return Task.CompletedTask; });
        d[(null, "LSHIFT")] = new(i => { i.EnsureStack(2, "LSHIFT"); var u = ToLong(i.PopInternal()); var x = ToLong(i.PopInternal()); i.Push((long)((ulong)x << (int)u)); return Task.CompletedTask; });
        d[(null, "RSHIFT")] = new(i => { i.EnsureStack(2, "RSHIFT"); var u = ToLong(i.PopInternal()); var x = ToLong(i.PopInternal()); i.Push((long)((ulong)x >> (int)u)); return Task.CompletedTask; });
    }
}
