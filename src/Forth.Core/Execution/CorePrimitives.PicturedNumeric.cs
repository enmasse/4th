using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddPicturedNumericEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "<#")] = new(i => { i.PicturedBegin(); return Task.CompletedTask; });
        d[(null, "HOLD")] = new(i => { i.EnsureStack(1, "HOLD"); var n = ToLong(i.PopInternal()); i.PicturedHold((char)(n & 0xFFFF)); return Task.CompletedTask; });
        d[(null, "#")] = new(i => { i.EnsureStack(1, "#"); var n = ToLong(i.PopInternal()); const long b = 10; long u = n < 0 ? -n : n; long rem = u % b; long q = u / b; i.PicturedHoldDigit(rem); i.Push(q); return Task.CompletedTask; });
        d[(null, "#S")] = new(i => { i.EnsureStack(1, "#S"); var n = ToLong(i.PopInternal()); const long b = 10; long u = n < 0 ? -n : n; if (u == 0) { i.PicturedHoldDigit(0); i.Push(0L); return Task.CompletedTask; } while (u > 0) { long rem = u % b; i.PicturedHoldDigit(rem); u /= b; } i.Push(0L); return Task.CompletedTask; });
        d[(null, "SIGN")] = new(i => { i.EnsureStack(1, "SIGN"); var n = ToLong(i.PopInternal()); if (n < 0) i.PicturedHold('-'); return Task.CompletedTask; });
        d[(null, "#>")] = new(i => { var s = i.PicturedEnd(); i.Push(s); return Task.CompletedTask; });
    }
}
