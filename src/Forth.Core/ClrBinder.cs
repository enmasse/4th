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
                var rawInst = interp.Pop();
                instance = rawInst; // allow instance object directly
            }
            object? result = target.Invoke(instance, argVals);
            if (isTask)
            {
                var task = (Task)result!;
                await task.ConfigureAwait(false);
                if (isGenericTask)
                {
                    var val = task.GetType().GetProperty("Result")!.GetValue(task);
                    ForthInterpreterPush(interp, val);
                }
            }
            else if (isValueTask)
            {
                var asTask = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
                await asTask.ConfigureAwait(false);
            }
            else if (isGenericValueTask)
            {
                var asTask = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
                await asTask.ConfigureAwait(false);
                var val = asTask.GetType().GetProperty("Result")!.GetValue(asTask);
                ForthInterpreterPush(interp, val);
            }
            else
            {
                ForthInterpreterPush(interp, result);
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
                instance = interp.Pop();
            }
            object? result = target.Invoke(instance, argVals);
            Task toPush;
            if (result is Task t)
            {
                toPush = t;
            }
            else
            {
                toPush = (Task)result!.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
            }
            interp.Push(toPush);
            return Task.CompletedTask;
        });
    }

    private static object? Coerce(object raw, Type target)
    {
        if (raw is null) return null;
        if (target.IsInstanceOfType(raw)) return raw;
        try
        {
            // Handle numeric coercions from boxed longs/ints
            if (raw is long l)
            {
                if (target == typeof(int)) return (int)l;
                if (target == typeof(short)) return (short)l;
                if (target == typeof(byte)) return (byte)l;
                if (target == typeof(char)) return (char)l;
                if (target == typeof(bool)) return l != 0;
                if (target == typeof(long)) return l;
            }
            if (raw is int i)
            {
                if (target == typeof(long)) return (long)i;
                if (target == typeof(short)) return (short)i;
                if (target == typeof(byte)) return (byte)i;
                if (target == typeof(char)) return (char)i;
                if (target == typeof(bool)) return i != 0;
                if (target == typeof(int)) return i;
            }
        }
        catch { }
        return raw;
    }

    private static void ForthInterpreterPush(ForthInterpreter interp, object? value)
    {
        if (value is null) return;
        switch (value)
        {
            case int i: interp.Push((long)i); break;
            case long l: interp.Push(l); break;
            case short s: interp.Push((long)s); break;
            case byte b: interp.Push((long)b); break;
            case char c: interp.Push((long)c); break;
            case bool bo: interp.Push(bo ? 1L : 0L); break;
            default: interp.Push(value); break;
        }
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
}
