using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Forth.Core.Execution
{
    internal static class AwaitableHelper
    {
        public static bool IsAwaitable(object? obj)
        {
            if (obj == null) return false;
            if (obj is Task) return true;
            if (obj is ValueTask) return true;
            var t = obj.GetType();
            // ValueTask<T>
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ValueTask<>)) return true;
            // Pattern-based: has public instance GetAwaiter()
            var m = t.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
            return m != null;
        }

        public static bool IsCompletedAwaitable(object? obj)
        {
            if (!IsAwaitable(obj)) return false;
            if (obj is Task tt) return tt.IsCompleted;
            if (obj is ValueTask vt) return vt.IsCompleted;
            var awaiter = GetAwaiterInstance(obj);
            if (awaiter == null) return false;
            var prop = awaiter.GetType().GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return false;
            var v = prop.GetValue(awaiter);
            return v is bool b && b;
        }

        private static object? GetAwaiterInstance(object? obj)
        {
            if (obj == null) return null;
            try
            {
                var getAwaiter = obj.GetType().GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
                return getAwaiter?.Invoke(obj, null);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool isFaulted, object? result)> AwaitAndUnwrap(object? awaitable)
        {
            if (awaitable == null) return (false, null);

            try
            {
                if (awaitable is Task task)
                {
                    try
                    {
                        await task.ConfigureAwait(false);
                    }
                    catch (Exception ex) { return (true, ex); }
                    var taskType = task.GetType();
                    if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var prop = taskType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                        return (false, prop?.GetValue(task));
                    }
                    return (false, null);
                }

                if (awaitable is ValueTask vt)
                {
                    try
                    {
                        await vt.ConfigureAwait(false);
                    }
                    catch (Exception ex) { return (true, ex); }
                    return (false, null);
                }

                var aType = awaitable.GetType();
                if (aType.IsGenericType && aType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    // call AsTask via reflection
                    var asTask = aType.GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
                    if (asTask != null)
                    {
                        var taskObj = asTask.Invoke(awaitable, null) as Task;
                        if (taskObj != null)
                        {
                            try
                            {
                                await taskObj.ConfigureAwait(false);
                            }
                            catch (Exception ex) { return (true, ex); }
                            var tt = taskObj.GetType();
                            if (tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(Task<>))
                            {
                                var prop = tt.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                                return (false, prop?.GetValue(taskObj));
                            }
                            return (false, null);
                        }
                    }
                }

                // Pattern-based awaitable
                var awaiter = GetAwaiterInstance(awaitable);
                if (awaiter == null) return (false, null);
                var isCompletedProp = awaiter.GetType().GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance);
                var getResult = awaiter.GetType().GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);
                var onCompleted = awaiter.GetType().GetMethod("OnCompleted", BindingFlags.Public | BindingFlags.Instance) ?? awaiter.GetType().GetMethod("UnsafeOnCompleted", BindingFlags.Public | BindingFlags.Instance);

                // If completed synchronously
                if (isCompletedProp != null)
                {
                    var ic = isCompletedProp.GetValue(awaiter);
                    if (ic is bool b && b)
                    {
                        if (getResult != null)
                        {
                            try
                            {
                                var res = getResult.Invoke(awaiter, null);
                                return (false, res);
                            }
                            catch (TargetInvocationException tie) { return (true, tie.InnerException ?? tie); }
                            catch (Exception ex) { return (true, ex); }
                        }
                        return (false, null);
                    }
                }

                if (getResult != null && onCompleted != null)
                {
                    var tcs = new TaskCompletionSource<object?>();
                    Action cont = () =>
                    {
                        try
                        {
                            var res = getResult.Invoke(awaiter, null);
                            tcs.SetResult(res);
                        }
                        catch (TargetInvocationException tie) { tcs.SetException(tie.InnerException ?? tie); }
                        catch (Exception ex) { tcs.SetException(ex); }
                    };
                    onCompleted.Invoke(awaiter, new object[] { cont });
                    try
                    {
                        var final = await tcs.Task.ConfigureAwait(false);
                        return (false, final);
                    }
                    catch (Exception ex) { return (true, ex); }
                }

                return (false, null);
            }
            catch (TargetInvocationException tie)
            {
                return (true, tie.InnerException ?? tie);
            }
            catch (Exception ex)
            {
                return (true, ex);
            }
        }
    }
}
