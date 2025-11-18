using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("<#", HelpString = "Begin pictured numeric conversion")]
    private static Task Prim_Pic_Begin(ForthInterpreter i) { i.PicturedBegin(); return Task.CompletedTask; }

    [Primitive("HOLD", HelpString = "Hold a character for pictured numeric output")]
    private static Task Prim_Pic_Hold(ForthInterpreter i) { i.EnsureStack(1, "HOLD"); var n = ToLong(i.PopInternal()); i.PicturedHold((char)(n & 0xFFFF)); return Task.CompletedTask; }

    [Primitive("#", HelpString = "Divide by base and hold digit ( n -- n/base )")]
    private static Task Prim_Pic_Hash(ForthInterpreter i) { i.EnsureStack(1, "#"); var n = ToLong(i.PopInternal()); const long b = 10; long u = n < 0 ? -n : n; long rem = u % b; long q = u / b; i.PicturedHoldDigit(rem); i.Push(q); return Task.CompletedTask; }

    [Primitive("#S", HelpString = "Produce digits until 0 for pictured numeric conversion")]
    private static Task Prim_Pic_HashS(ForthInterpreter i) { i.EnsureStack(1, "#S"); var n = ToLong(i.PopInternal()); const long b = 10; long u = n < 0 ? -n : n; if (u == 0) { i.PicturedHoldDigit(0); i.Push(0L); return Task.CompletedTask; } while (u > 0) { long rem = u % b; i.PicturedHoldDigit(rem); u /= b; } i.Push(0L); return Task.CompletedTask; }

    [Primitive("SIGN", HelpString = "Add sign for pictured numeric conversion if negative")]
    private static Task Prim_Pic_SIGN(ForthInterpreter i) { i.EnsureStack(1, "SIGN"); var n = ToLong(i.PopInternal()); if (n < 0) i.PicturedHold('-'); return Task.CompletedTask; }

    [Primitive("#>", HelpString = "End pictured numeric conversion and push resulting string")]
    private static Task Prim_Pic_End(ForthInterpreter i) { var s = i.PicturedEnd(); i.Push(s); return Task.CompletedTask; }
}
