using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddReturnStackEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, ">R")] = new(i => { i.EnsureStack(1, ">R"); var a = i.PopInternal(); i.RPush(a); return Task.CompletedTask; });
        d[(null, "R>")] = new(i => { if (i.RCount == 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Return stack underflow in R>"); var a = i.RPop(); i.Push(a); return Task.CompletedTask; });
        d[(null, "2>R")] = new(i => { i.EnsureStack(2, "2>R"); var b = i.PopInternal(); var a = i.PopInternal(); i.RPush(a); i.RPush(b); return Task.CompletedTask; });
        d[(null, "2R>")] = new(i => { if (i.RCount < 2) throw new ForthException(ForthErrorCode.StackUnderflow, "Return stack underflow in 2R>"); var b = i.RPop(); var a = i.RPop(); i.Push(a); i.Push(b); return Task.CompletedTask; });
        d[(null, "RP@")] = new(i => { i.Push((long)i.RCount); return Task.CompletedTask; });
    }
}
