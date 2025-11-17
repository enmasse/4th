using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddComparisonEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "<")] = new(i => { i.EnsureStack(2, "<"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a < b ? 1L : 0L); return Task.CompletedTask; });
        d[(null, "=")] = new(i => { i.EnsureStack(2, "="); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a == b ? 1L : 0L); return Task.CompletedTask; });
        d[(null, ">")] = new(i => { i.EnsureStack(2, ">"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a > b ? 1L : 0L); return Task.CompletedTask; });
        d[(null, "0=")] = new(i => { i.EnsureStack(1, "0="); var a = ToLong(i.PopInternal()); i.Push(a == 0 ? 1L : 0L); return Task.CompletedTask; });
        d[(null, "0<>")] = new(i => { i.EnsureStack(1, "0<>"); var a = ToLong(i.PopInternal()); i.Push(a != 0 ? 1L : 0L); return Task.CompletedTask; });
        d[(null, "<>")] = new(i => { i.EnsureStack(2, "<>"); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a != b ? 1L : 0L); return Task.CompletedTask; });
        d[(null, "<=")] = new(i => { i.EnsureStack(2, "<="); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a <= b ? 1L : 0L); return Task.CompletedTask; });
        d[(null, ">=")] = new(i => { i.EnsureStack(2, ">="); var b = ToLong(i.PopInternal()); var a = ToLong(i.PopInternal()); i.Push(a >= b ? 1L : 0L); return Task.CompletedTask; });
    }
}
