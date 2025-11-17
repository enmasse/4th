using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddConcurrencyEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "TASK?")] = new(i => { i.EnsureStack(1, "TASK?"); var obj = i.PopInternal(); long flag = obj is Task t && t.IsCompleted ? 1L : 0L; i.Push(flag); return Task.CompletedTask; }) { Name = "TASK?" };

        d[(null, "AWAIT")] = new(async i =>
        {
            i.EnsureStack(1, "AWAIT");
            var obj = i.PopInternal();
            switch (obj)
            {
                case Task t:
                    await t.ConfigureAwait(false);
                    var taskType = t.GetType();
                    if (taskType.IsGenericType)
                    {
                        var resultProperty = taskType.GetProperty("Result");
                        if (resultProperty != null && resultProperty.CanRead)
                        {
                            var result = resultProperty.GetValue(t);
                            if (result != null)
                            {
                                var resultType = result.GetType();
                                if (resultType.Name == "VoidTaskResult")
                                    break;
                                switch (result)
                                {
                                    case int iv: i.Push((long)iv); break;
                                    case long lv: i.Push(lv); break;
                                    case short sv: i.Push((long)sv); break;
                                    case byte bv: i.Push((long)bv); break;
                                    case char cv: i.Push((long)cv); break;
                                    case bool bov: i.Push(bov ? 1L : 0L); break;
                                    default: i.Push(result); break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new ForthException(ForthErrorCode.CompileError, "AWAIT expects a Task or ValueTask");
            }
        }) { Name = "AWAIT" };

        d[(null, "JOIN")] = new(async i => {
            i.EnsureStack(1, "JOIN");
            i._dict.TryGetValue((null, "AWAIT"), out var awaitWord);
            if (awaitWord is null)
                throw new ForthException(ForthErrorCode.UndefinedWord, "AWAIT not found for JOIN");
            await awaitWord.ExecuteAsync(i);
        }) { Name = "JOIN" };

        d[(null, "SPAWN")] = new(i => { i.EnsureStack(1, "SPAWN"); var obj = i.PopInternal(); if (obj is not ForthInterpreter.Word xt) throw new ForthException(ForthErrorCode.TypeError, "SPAWN expects an execution token"); var snapshot = i.CreateMarkerSnapshot(); var task = Task.Run(async () => { var child = new ForthInterpreter(snapshot); await xt.ExecuteAsync(child).ConfigureAwait(false); }); i.Push(task); return Task.CompletedTask; }) { Name = "SPAWN" };

        d[(null, "FUTURE")] = new(i =>
        {
            i.EnsureStack(1, "FUTURE");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt)
                throw new ForthException(ForthErrorCode.TypeError, "FUTURE expects an execution token");
            var snapshot = i.CreateMarkerSnapshot();
            var task = Task.Run(async () =>
            {
                var child = new ForthInterpreter(snapshot);
                await xt.ExecuteAsync(child).ConfigureAwait(false);
                return child.Stack.Count > 0 ? child.Pop() : null;
            });
            i.Push(task);
            return Task.CompletedTask;
        }) { Name = "FUTURE" };

        d[(null, "TASK")] = new(i =>
        {
            i.EnsureStack(1, "TASK");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt)
                throw new ForthException(ForthErrorCode.TypeError, "TASK expects an execution token");
            var snapshot = i.CreateMarkerSnapshot();
            var task = Task.Run(async () =>
            {
                var child = new ForthInterpreter(snapshot);
                await xt.ExecuteAsync(child).ConfigureAwait(false);
                return child.Stack.Count > 0 ? child.Pop() : null;
            });
            i.Push(task);
            return Task.CompletedTask;
        }) { Name = "TASK" };
    }
}
