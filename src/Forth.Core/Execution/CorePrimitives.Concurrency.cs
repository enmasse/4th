using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("TASK?")]
    private static Task Prim_TASKQ(ForthInterpreter i) { i.EnsureStack(1, "TASK?"); var obj = i.PopInternal(); long flag = obj is Task t && t.IsCompleted ? 1L : 0L; i.Push(flag); return Task.CompletedTask; }

    [Primitive("AWAIT", IsAsync = true)]
    private static Task Prim_AWAIT(ForthInterpreter i) => AwaitImpl(i);

    private static async Task AwaitImpl(ForthInterpreter i)
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
    }

    [Primitive("JOIN", IsAsync = true)]
    private static Task Prim_JOIN(ForthInterpreter i) => JoinImpl(i);

    private static async Task JoinImpl(ForthInterpreter i)
    {
        i.EnsureStack(1, "JOIN");
        i._dict.TryGetValue((null, "AWAIT"), out var awaitWord);
        if (awaitWord is null)
            throw new ForthException(ForthErrorCode.UndefinedWord, "AWAIT not found for JOIN");
        await awaitWord.ExecuteAsync(i);
    }

    [Primitive("SPAWN")]
    private static Task Prim_SPAWN(ForthInterpreter i)
    {
        i.EnsureStack(1, "SPAWN");
        var obj = i.PopInternal();
        if (obj is not ForthInterpreter.Word xt) throw new ForthException(ForthErrorCode.TypeError, "SPAWN expects an execution token");

        var snapshot = i.CreateMarkerSnapshot();

        var task = Task.Run(async () =>
        {
            var child = new ForthInterpreter(snapshot);
            await xt.ExecuteAsync(child).ConfigureAwait(false);
        });
        i.Push(task);
        return Task.CompletedTask;
    }

    [Primitive("FUTURE")]
    private static Task Prim_FUTURE(ForthInterpreter i)
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
    }

    [Primitive("TASK")]
    private static Task Prim_TASK(ForthInterpreter i)
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
    }
}
