using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("EXIT")]
    private static Task Prim_EXIT(ForthInterpreter i) { i.ThrowExit(); return Task.CompletedTask; }

    [Primitive("UNLOOP")]
    private static Task Prim_UNLOOP(ForthInterpreter i) { i.Unloop(); return Task.CompletedTask; }

    [Primitive("I")]
    private static Task Prim_I(ForthInterpreter i) { i.Push(i.CurrentLoopIndex()); return Task.CompletedTask; }

    [Primitive("YIELD", IsAsync = true)]
    private static async Task Prim_YIELD(ForthInterpreter i) { await Task.Yield(); }

    [Primitive("BYE")]
    private static Task Prim_BYE(ForthInterpreter i) { i.RequestExit(); return Task.CompletedTask; }

    [Primitive("QUIT")]
    private static Task Prim_QUIT(ForthInterpreter i) { i.RequestExit(); return Task.CompletedTask; }

    [Primitive("ABORT")]
    private static Task Prim_ABORT(ForthInterpreter i) => throw new ForthException(ForthErrorCode.Unknown, "ABORT");

    [Primitive("LATEST")]
    private static Task Prim_LATEST(ForthInterpreter i) { var last = i._lastDefinedWord; if (last is null) throw new ForthException(ForthErrorCode.UndefinedWord, "No latest word"); i.Push(last); return Task.CompletedTask; }

    [Primitive("EXECUTE", IsAsync = true)]
    private static Task Prim_EXECUTE(ForthInterpreter i) => ExecuteImpl(i);

    private static async Task ExecuteImpl(ForthInterpreter i)
    {
        i.EnsureStack(1, "EXECUTE");
        var top = i.StackTop();
        if (top is Word wTop)
        {
            i.PopInternal();
            await wTop.ExecuteAsync(i).ConfigureAwait(false);
            return;
        }
        if (i.Stack.Count >= 2 && i.StackNthFromTop(2) is Word wBelow)
        {
            var data = i.PopInternal();
            i.PopInternal();
            await wBelow.ExecuteAsync(i).ConfigureAwait(false);
            i.Push(data);
            return;
        }
        throw new ForthException(ForthErrorCode.TypeError, "EXECUTE expects an execution token");
    }

    [Primitive("CATCH", IsAsync = true)]
    private static Task Prim_CATCH(ForthInterpreter i) => CatchImpl(i);

    private static async Task CatchImpl(ForthInterpreter i)
    {
        i.EnsureStack(1, "CATCH");
        var obj = i.PopInternal();
        if (obj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "CATCH expects an execution token");
        try
        {
            await xt.ExecuteAsync(i).ConfigureAwait(false);
            i.Push(0L);
        }
        catch (Forth.Core.ForthException ex)
        {
            var codeVal = (long)ex.Code;
            if (codeVal == 0) codeVal = 1;
            i.Push(codeVal);
        }
        catch (System.Exception)
        {
            i.Push(1L);
        }
    }

    [Primitive("THROW")]
    private static Task Prim_THROW(ForthInterpreter i) { i.EnsureStack(1, "THROW"); var err = ToLong(i.PopInternal()); if (err != 0) throw new Forth.Core.ForthException((Forth.Core.ForthErrorCode)err, $"THROW {err}"); return Task.CompletedTask; }
}
