using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace Forth;

internal static class ClrBinder
{
    public static ForthInterpreter.Word CreateBoundWord(string typeName, string methodName, int argCount)
    {
        var type = ResolveType(typeName) ?? throw new ForthException(ForthErrorCode.UndefinedWord, $"Type not found: {typeName}");
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        // Pick first matching by name and parameter count.
        var target = methods.FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argCount)
            ?? throw new ForthException(ForthErrorCode.UndefinedWord, $"No such method {methodName} with {argCount} args on {typeName}");
        bool isStatic = target.IsStatic;
        var pars = target.GetParameters();
        var returnType = target.ReturnType;
        bool isTask = typeof(Task).IsAssignableFrom(returnType);
        bool isGenericTask = isTask && returnType.IsGenericType;
        bool isValueTask = returnType == typeof(ValueTask);
        bool isGenericValueTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>);
        return new ForthInterpreter.Word(async interp =>
        {
            // If instance method expect object reference on stack first (after args)
            int totalPop = argCount + (isStatic ? 0 : 1);
            ForthInterpreter.EnsureStack(interp, totalPop, methodName);
            // Pop values; stack is LIFO so collect in reverse
            object? instance = null;
            var argVals = new object?[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                var raw = interp.Pop();
                argVals[i] = Coerce(raw, pars[i].ParameterType);
            }
            if (!isStatic)
            {
                var rawInstance = interp.Pop();
                instance = CoerceObject(interp, rawInstance, type);
            }
            object? result = target.Invoke(instance, argVals);
            if (isTask)
            {
                var task = (Task)result!;
                await task.ConfigureAwait(false);
                if (isGenericTask)
                {
                    var val = task.GetType().GetProperty("Result")!.GetValue(task);
                    PushValueOrStoreObject(interp, val);
                }
            }
            else if (isValueTask)
            {
                // Convert to Task and await
                var asTask = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
                await asTask.ConfigureAwait(false);
            }
            else if (isGenericValueTask)
            {
                var asTask = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!; // Task<T>
                await asTask.ConfigureAwait(false);
                var val = asTask.GetType().GetProperty("Result")!.GetValue(asTask);
                PushValueOrStoreObject(interp, val);
            }
            else
            {
                PushValueOrStoreObject(interp, result);
            }
        });
    }

    public static ForthInterpreter.Word CreateBoundTaskWord(string typeName, string methodName, int argCount)
    {
        var type = ResolveType(typeName) ?? throw new ForthException(ForthErrorCode.UndefinedWord, $"Type not found: {typeName}");
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        var target = methods.FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argCount)
            ?? throw new ForthException(ForthErrorCode.UndefinedWord, $"No such method {methodName} with {argCount} args on {typeName}");
        bool isStatic = target.IsStatic;
        var pars = target.GetParameters();
        var returnType = target.ReturnType;
        bool returnsTaskLike = typeof(Task).IsAssignableFrom(returnType) || returnType == typeof(ValueTask) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));
        if (!returnsTaskLike)
            throw new ForthException(ForthErrorCode.CompileError, $"{typeName}.{methodName} does not return Task/ValueTask");
        return new ForthInterpreter.Word(interp =>
        {
            int totalPop = argCount + (isStatic ? 0 : 1);
            ForthInterpreter.EnsureStack(interp, totalPop, methodName);
            object? instance = null;
            var argVals = new object?[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                var raw = interp.Pop();
                argVals[i] = Coerce(raw, pars[i].ParameterType);
            }
            if (!isStatic)
            {
                var rawInstance = interp.Pop();
                instance = CoerceObject(interp, rawInstance, type);
            }
            object? result = target.Invoke(instance, argVals);
            object toStore;
            if (result is Task t)
            {
                toStore = t;
            }
            else
            {
                // ValueTask or ValueTask<T> -> convert to Task via AsTask
                toStore = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
            }
            var id = interp.StoreObject(toStore);
            interp.Push(id);
            return Task.CompletedTask;
        });
    }

    private static Type? ResolveType(string name)
    {
        var t = Type.GetType(name, throwOnError: false, ignoreCase: false);
        if (t != null) return t;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(name, throwOnError: false, ignoreCase: false);
            if (t != null) return t;
        }
        return null;
    }

    private static object? Coerce(long value, Type target)
    {
        if (target == typeof(long) || target == typeof(object)) return value;
        if (target == typeof(int)) return (int)value;
        if (target == typeof(short)) return (short)value;
        if (target == typeof(byte)) return (byte)value;
        if (target == typeof(char)) return (char)value;
        if (target == typeof(bool)) return value != 0;
        if (target.IsEnum) return Enum.ToObject(target, value);
        // For now only primitive numeric. Instance binding expects passing an address to object map: not implemented.
        throw new ForthException(ForthErrorCode.CompileError, $"Cannot coerce {value} to {target.Name}");
    }

    private static object? CoerceObject(ForthInterpreter interp, long id, Type expected)
    {
        var obj = interp.GetObject(id);
        if (obj is null) return null;
        if (!expected.IsInstanceOfType(obj))
            throw new ForthException(ForthErrorCode.CompileError, $"Object {id} is not {expected.Name}");
        return obj;
    }

    internal static void PushValueOrStoreObject(ForthInterpreter interp, object? value)
    {
        if (value is null) return;
        switch (value)
        {
            case int i: interp.Push(i); break;
            case long l: interp.Push(l); break;
            case short s: interp.Push(s); break;
            case byte b: interp.Push(b); break;
            case char c: interp.Push(c); break;
            case bool bo: interp.Push(bo ? 1 : 0); break;
            case Enum e: interp.Push(Convert.ToInt64(e)); break;
            default:
                // store object and push its id
                var id = interp.StoreObject(value);
                interp.Push(id);
                break;
        }
    }
}
