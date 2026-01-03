using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Reflection;

namespace Forth.Core.Execution;

internal static class ConcurrencyPrimitives
{
    [Primitive("TASK?", HelpString = "TASK? ( task -- flag ) - push -1 if Task/ValueTask is completed else 0")]
    private static Task Prim_TASKQ(ForthInterpreter i)
    {
        i.EnsureStack(1, "TASK?");
        var obj = i.PopInternal();
        Task? t = null;

        if (obj is Task tt) t = tt;
        else if (obj is ValueTask vt) t = vt.AsTask();
        else if (AwaitableHelper.IsAwaitable(obj))
        {
            // If it's a pattern-based awaitable that can report completion quickly, use helper
            if (AwaitableHelper.IsCompletedAwaitable(obj))
            {
                i.Push(-1L);
                return Task.CompletedTask;
            }

            // Try ValueTask<T>.AsTask() via reflection when available
            var ot = obj?.GetType();
            if (ot is not null && ot.IsGenericType && ot.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var asTask = ot.GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
                if (asTask is not null)
                {
                    try
                    {
                        var invoked = asTask.Invoke(obj, null) as Task;
                        if (invoked is not null) t = invoked;
                    }
                    catch
                    {
                        // ignore reflection failures and treat as not completed
                    }
                }
            }
        }

        long flag = (t != null && t.IsCompleted) ? -1L : 0L;
        i.Push(flag);
        return Task.CompletedTask;
    }

    [Primitive("AWAIT", IsAsync = true, HelpString = "AWAIT ( task -- ) - await a Task or ValueTask and push its result if any")]
    private static Task Prim_AWAIT(ForthInterpreter i) => AwaitImpl(i);

    private static async Task AwaitImpl(ForthInterpreter i)
    {
        i.EnsureStack(1, "AWAIT");
        var obj = i.PopInternal();
        if (obj is null)
            throw new ForthException(ForthErrorCode.TypeError, "AWAIT expects a Task or awaitable");

        // Fast paths for Task and ValueTask
        if (obj is Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ForthException(ForthErrorCode.Unknown, ex.Message);
            }

            var taskType = task.GetType();
            if (taskType.IsGenericType)
            {
                var resultProperty = taskType.GetProperty("Result");
                if (resultProperty != null && resultProperty.CanRead)
                {
                    var result = resultProperty.GetValue(task);
                    if (result != null)
                    {
                        if (result.GetType().Name == "VoidTaskResult")
                            return;
                        switch (result)
                        {
                            case int iv: i.Push((long)iv); break;
                            case long lv: i.Push(lv); break;
                            case short sv: i.Push((long)sv); break;
                            case byte bv: i.Push((long)bv); break;
                            case char cv: i.Push((long)cv); break;
                            case bool bov: i.Push(bov ? -1L : 0L); break;
                            default: i.Push(result); break;
                        }
                    }
                }
            }
            return;
        }

        if (obj is ValueTask vtask)
        {
            try
            {
                await vtask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ForthException(ForthErrorCode.Unknown, ex.Message);
            }
            return;
        }

        // ValueTask<T> pattern via AsTask
        var ot = obj?.GetType();
        if (ot is not null && ot.IsGenericType && ot.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var asTask = ot.GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
            if (asTask != null)
            {
                var taskObj = asTask.Invoke(obj, null) as Task;
                if (taskObj != null)
                {
                    try
                    {
                        await taskObj.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new ForthException(ForthErrorCode.Unknown, ex.Message);
                    }

                    var tt = taskObj.GetType();
                    if (tt.IsGenericType)
                    {
                        var prop = tt.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            var res = prop.GetValue(taskObj);
                            if (res != null)
                            {
                                switch (res)
                                {
                                    case int iv: i.Push((long)iv); break;
                                    case long lv: i.Push(lv); break;
                                    case short sv: i.Push((long)sv); break;
                                    case byte bv: i.Push((long)bv); break;
                                    case char cv: i.Push((long)cv); break;
                                    case bool bov: i.Push(bov ? -1L : 0L); break;
                                    default: i.Push(res); break;
                                }
                            }
                        }
                    }
                    return;
                }
            }
        }

        // Pattern-based awaitable: has GetAwaiter()
        var getAwaiterMethod = obj?.GetType().GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
        if (getAwaiterMethod is not null)
        {
            var (isFaulted, result) = await AwaitableHelper.AwaitAndUnwrap(obj).ConfigureAwait(false);
            if (isFaulted)
            {
                if (result is Exception ex) throw new ForthException(ForthErrorCode.Unknown, ex.Message);
                throw new ForthException(ForthErrorCode.Unknown, "Awaitable faulted");
            }

            if (result is null) return;

            switch (result)
            {
                case int iv: i.Push((long)iv); break;
                case long lv: i.Push(lv); break;
                case short sv: i.Push((long)sv); break;
                case byte bv: i.Push((long)bv); break;
                case char cv: i.Push((long)cv); break;
                case bool bo: i.Push(bo ? -1L : 0L); break;
                default: i.Push(result); break;
            }
            return;
        }

        throw new ForthException(ForthErrorCode.CompileError, "AWAIT expects a Task or ValueTask");
    }

    [Primitive("JOIN", IsAsync = true, HelpString = "JOIN - synonym for awaiting a spawned task (uses AWAIT)")]
    private static Task Prim_JOIN(ForthInterpreter i) => JoinImpl(i);

    private static async Task JoinImpl(ForthInterpreter i)
    {
        i.EnsureStack(1, "JOIN");
        i._dict.TryGetValue((null, "AWAIT"), out var awaitWord);
        if (awaitWord is null)
            throw new ForthException(ForthErrorCode.UndefinedWord, "AWAIT not found for JOIN");
        await awaitWord.ExecuteAsync(i);
    }

    [Primitive("SPAWN", HelpString = "SPAWN ( xt -- task ) - run word in background and return Task")]
    private static Task Prim_SPAWN(ForthInterpreter i)
    {
        i.EnsureStack(1, "SPAWN");
        var obj = i.PopInternal();
        if (obj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "SPAWN expects an execution token");

        var snapshot = i.CreateMarkerSnapshot();

        var task = Task.Run(async () =>
        {
            var child = new ForthInterpreter(snapshot);
            await xt.ExecuteAsync(child).ConfigureAwait(false);
        });
        i.Push(task);
        return Task.CompletedTask;
    }

    [Primitive("FUTURE", HelpString = "FUTURE ( xt -- task ) - run word asynchronously and return Task with its result")]
    private static Task Prim_FUTURE(ForthInterpreter i)
    {
        i.EnsureStack(1, "FUTURE");
        var obj = i.PopInternal();
        if (obj is not Word xt)
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

    [Primitive("TASK", HelpString = "TASK ( xt -- task ) - run word asynchronously and return Task with its result")]
    private static Task Prim_TASK(ForthInterpreter i)
    {
        i.EnsureStack(1, "TASK");
        var obj = i.PopInternal();
        if (obj is not Word xt)
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

    [Primitive("RUN-NEXT", IsAsync = true, HelpString = "RUN-NEXT ( -- ) - read next token, run it in isolated child (SPAWN/JOIN)")]
    private static async Task Prim_RUN_NEXT(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "RUN-NEXT expects a following word");
        if (!i.TryResolveWord(name, out var xt) || xt is null)
            throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
        var snap = i.CreateMarkerSnapshot();
        var task = Task.Run(async () => { var child = new ForthInterpreter(snap); await xt.ExecuteAsync(child).ConfigureAwait(false); });
        await task.ConfigureAwait(false);
    }
}
