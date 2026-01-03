using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class ControlExecutionPrimitives
{
    [Primitive("EXIT", HelpString = "Exit from current word")]
    private static Task Prim_EXIT(ForthInterpreter i) { i.ThrowExit(); return Task.CompletedTask; }

    [Primitive("UNLOOP", HelpString = "Remove loop from control stack")]
    private static Task Prim_UNLOOP(ForthInterpreter i) { i.Unloop(); return Task.CompletedTask; }

    [Primitive("I", HelpString = "Push current loop index")]
    private static Task Prim_I(ForthInterpreter i) { i.Push(i.CurrentLoopIndex()); return Task.CompletedTask; }

    [Primitive("YIELD", IsAsync = true, HelpString = "Yield execution (async)")]
    private static async Task Prim_YIELD(ForthInterpreter i) { await Task.Yield(); }

    [Primitive("BYE", HelpString = "Request interpreter exit")]
    private static Task Prim_BYE(ForthInterpreter i) { i.RequestExit(); return Task.CompletedTask; }

    [Primitive("QUIT", HelpString = "Request interpreter quit")]
    private static Task Prim_QUIT(ForthInterpreter i) { i.RequestExit(); return Task.CompletedTask; }

    [Primitive("ABORT", HelpString = "Abort execution with error")]
    private static Task Prim_ABORT(ForthInterpreter i)
    {
        if (i.Stack.Count > 0 && i.StackTop() is string s)
        {
            i.PopInternal();
            throw new ForthException(ForthErrorCode.Unknown, s);
        }
        throw new ForthException(ForthErrorCode.Unknown, "ABORT");
    }

    [Primitive("LATEST", HelpString = "Push the latest defined word onto the stack")]
    private static Task Prim_LATEST(ForthInterpreter i) { var last = i._lastDefinedWord; if (last is null) throw new ForthException(ForthErrorCode.UndefinedWord, "No latest word"); i.Push(last); return Task.CompletedTask; }

    [Primitive("EXECUTE", IsAsync = true, HelpString = "Execute a word or execution token on the stack")]
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

    [Primitive("CATCH", IsAsync = true, HelpString = "Execute a word and catch exceptions, returning an error code")]
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
        catch (ForthException ex)
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

    [Primitive("THROW", HelpString = "Throw an exception by error code")]
    private static Task Prim_THROW(ForthInterpreter i) { i.EnsureStack(1, "THROW"); var err = PrimitivesUtil.ToLong(i.PopInternal()); if (err != 0) throw new ForthException((ForthErrorCode)err, $"THROW {err}"); return Task.CompletedTask; }
}
