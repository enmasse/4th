using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddControlExecutionEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "EXIT")] = new(i => { i.ThrowExit(); return Task.CompletedTask; });
        d[(null, "UNLOOP")] = new(i => { i.Unloop(); return Task.CompletedTask; });
        d[(null, "I")] = new(i => { i.Push(i.CurrentLoopIndex()); return Task.CompletedTask; });
        d[(null, "YIELD")] = new(async i => await Task.Yield());
        d[(null, "BYE")] = new(i => { i.RequestExit(); return Task.CompletedTask; });
        d[(null, "QUIT")] = new(i => { i.RequestExit(); return Task.CompletedTask; });
        d[(null, "ABORT")] = new(i => throw new ForthException(ForthErrorCode.Unknown, "ABORT"));
        d[(null, "LATEST")] = new(i => { var last = i._lastDefinedWord; if (last is null) throw new ForthException(ForthErrorCode.UndefinedWord, "No latest word"); i.Push(last); return Task.CompletedTask; });

        d[(null, "EXECUTE")] = new(async i =>
        {
            i.EnsureStack(1, "EXECUTE");
            var top = i.StackTop();
            if (top is ForthInterpreter.Word wTop)
            {
                i.PopInternal();
                await wTop.ExecuteAsync(i).ConfigureAwait(false);
                return;
            }
            if (i.Stack.Count >= 2 && i.StackNthFromTop(2) is ForthInterpreter.Word wBelow)
            {
                var data = i.PopInternal();
                i.PopInternal();
                await wBelow.ExecuteAsync(i).ConfigureAwait(false);
                i.Push(data);
                return;
            }
            throw new ForthException(ForthErrorCode.TypeError, "EXECUTE expects an execution token");
        });

        d[(null, "CATCH")] = new(async i =>
        {
            i.EnsureStack(1, "CATCH");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt) throw new ForthException(ForthErrorCode.TypeError, "CATCH expects an execution token");
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
        });

        d[(null, "THROW")] = new(i => { i.EnsureStack(1, "THROW"); var err = ToLong(i.PopInternal()); if (err != 0) throw new Forth.Core.ForthException((Forth.Core.ForthErrorCode)err, $"THROW {err}"); return Task.CompletedTask; });
    }
}
