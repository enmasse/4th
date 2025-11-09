using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static class IlScriptCompiler
{
    public static bool TryCompile(IReadOnlyList<string> tokens, out Func<ForthInterpreter, Task> runner)
    {
        runner = null!;
        foreach (var t in tokens)
        {
            if (t.Length == 0) return false;
            if (t == "+" || t == "-" || t == "*" || t == "/") continue;
            if (!long.TryParse(t, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _))
                return false;
        }
        var dm = new DynamicMethod("ForthIL", typeof(Task), new[] { typeof(ForthInterpreter) }, typeof(IlScriptCompiler).Module, skipVisibility: true);
        var il = dm.GetILGenerator();
        foreach (var t in tokens)
        {
            if (t == "+" || t == "-" || t == "*" || t == "/")
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, t);
                il.Emit(OpCodes.Call, typeof(IlScriptCompiler).GetMethod(nameof(ApplyOp), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!);
            }
            else
            {
                long val = long.Parse(t, System.Globalization.CultureInfo.InvariantCulture);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I8, val);
                il.Emit(OpCodes.Box, typeof(long));
                il.Emit(OpCodes.Call, typeof(ForthInterpreter).GetMethod("Push", new[] { typeof(object) })!);
            }
        }
        il.Emit(OpCodes.Call, typeof(Task).GetProperty(nameof(Task.CompletedTask))!.GetMethod!);
        il.Emit(OpCodes.Ret);
        var del = (Func<ForthInterpreter, Task>)dm.CreateDelegate(typeof(Func<ForthInterpreter, Task>));
        runner = del;
        return true;
    }

    private static void ApplyOp(ForthInterpreter i, string op)
    {
        ForthInterpreter.EnsureStack(i, 2, op);
        var b = ForthInterpreter.ToLongPublic(i.Pop());
        var a = ForthInterpreter.ToLongPublic(i.Pop());
        long r = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b == 0 ? throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero, "Divide by zero") : a / b,
            _ => throw new InvalidOperationException()
        };
        i.Push(r);
    }
}
