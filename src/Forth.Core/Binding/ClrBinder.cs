using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Forth.Core.Interpreter;

namespace Forth.Core.Binding;

internal static class ClrBinder
{
    public static ForthInterpreter.Word CreateBoundWord(string typeName, string methodName, int argCount)
    {
        var type = ResolveType(typeName) ?? throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Type not found: {typeName}");
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        var target = methods.FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argCount)
            ?? throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"No such method {methodName} with {argCount} args on {typeName}");
        bool isStatic = target.IsStatic;
        var pars = target.GetParameters();
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
                var rawInst = interp.Pop();
                instance = rawInst;
            }
            object? result;
            try
            {
                result = target.Invoke(instance, argVals);

                switch (result)
                {
                    case null:
                        break;
                    case ValueTask vt:
                        ForthInterpreterPush(interp, vt.AsTask());
                        break;
                    default:
                        // Check if it's ValueTask<T> using reflection
                        var resultType = result.GetType();
                        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                        {
                            // Convert ValueTask<T> to Task<T>
                            var asTaskMethod = resultType.GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
                            if (asTaskMethod != null)
                            {
                                var task = asTaskMethod.Invoke(result, null);
                                ForthInterpreterPush(interp, task);
                                break;
                            }
                        }
                        ForthInterpreterPush(interp, result);
                        break;
                }
            }
            catch (TargetInvocationException tie)
            {
                throw WrapInterop(tie.InnerException ?? tie); // propagate
            }
            catch (Exception ex)
            {
                throw WrapInterop(ex);
            }
        });
    }

    private static Forth.Core.ForthException WrapInterop(Exception ex)
    {
        return ex is Forth.Core.ForthException fe ? fe : new Forth.Core.ForthException(Forth.Core.ForthErrorCode.Unknown, ex.Message);
    }

    private static object? Coerce(object raw, Type target)
    {
        if (raw is null) return null;
        if (target.IsInstanceOfType(raw)) return raw;
        try
        {
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
