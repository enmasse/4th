using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive(">NUMBER", HelpString = ">NUMBER ( string start consumed | counted-addr start consumed | addr u start consumed -- value remainder consumed ) parse digits using BASE")]
    private static Task Prim_GTN(ForthInterpreter i)
    {
        if (i.Stack.Count < 3)
            throw new ForthException(ForthErrorCode.StackUnderflow, ">NUMBER requires at least three parameters");

        string s;
        int start;
        int consumed;

        // Detect addr u start consumed form: need 4 numeric values
        bool formAddrU = i.Stack.Count >= 4 && IsNum(i.Stack[^1]) && IsNum(i.Stack[^2]) && IsNum(i.Stack[^3]) && IsNum(i.Stack[^4]);
        if (formAddrU)
        {
            consumed = (int)ToLong(i.PopInternal());
            start = (int)ToLong(i.PopInternal());
            long u = ToLong(i.PopInternal());
            long addr = ToLong(i.PopInternal());
            var sb = new System.Text.StringBuilder();
            for (long k = 0; k < u; k++) { i.MemTryGet(addr + k, out var v); char ch = (char)(ToLong(v) & 0xFF); sb.Append(ch); }
            s = sb.ToString();
        }
        else
        {
            // Common forms: string start consumed OR counted-addr start consumed
            consumed = (int)ToLong(i.PopInternal());
            start = (int)ToLong(i.PopInternal());
            var obj = i.PopInternal();
            if (obj is string sObj)
            {
                s = sObj;
            }
            else if (IsNum(obj))
            {
                long addr = ToLong(obj);
                i.MemTryGet(addr, out var lenCell);
                long u = ToLong(lenCell);
                var sb = new System.Text.StringBuilder();
                for (long k = 0; k < u; k++) { i.MemTryGet(addr + 1 + k, out var v); char ch = (char)(ToLong(v) & 0xFF); sb.Append(ch); }
                s = sb.ToString();
            }
            else
            {
                throw new ForthException(ForthErrorCode.TypeError, ">NUMBER expects string, counted address or addr u start consumed");
            }
        }

        int idx = System.Math.Clamp(start, 0, s.Length);
        while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
        i.MemTryGet(i.BaseAddr, out var baseVal);
        int b = baseVal <= 0 ? 10 : (int)baseVal;
        long value = 0;
        int digits = 0;
        while (idx < s.Length)
        {
            int d;
            char ch = s[idx];
            if (ch >= '0' && ch <= '9') d = ch - '0';
            else if (ch >= 'A' && ch <= 'Z') d = ch - 'A' + 10;
            else if (ch >= 'a' && ch <= 'z') d = ch - 'a' + 10;
            else break;
            if (d >= b) break;
            value = value * b + d;
            idx++;
            digits++;
        }
        int remainder = s.Length - idx;
        i.Push(value);
        i.Push((long)remainder);
        i.Push((long)(consumed + digits));
        return Task.CompletedTask;
    }

    private static bool IsNum(object o) => o is long || o is int || o is short || o is byte;

    [Primitive("BASE", HelpString = "Push address of BASE variable")]
    private static Task Prim_BASE(ForthInterpreter i) { i.Push(i.BaseAddr); return Task.CompletedTask; }

    [Primitive("DECIMAL", HelpString = "Set number base to decimal")]
    private static Task Prim_DECIMAL(ForthInterpreter i) { i.MemSet(i.BaseAddr, 10); return Task.CompletedTask; }

    [Primitive("HEX", HelpString = "Set number base to hexadecimal")]
    private static Task Prim_HEX(ForthInterpreter i) { i.MemSet(i.BaseAddr, 16); return Task.CompletedTask; }

    [Primitive("STATE", HelpString = "Push address of STATE variable")]
    private static Task Prim_STATE(ForthInterpreter i) { i.Push(i.StateAddr); return Task.CompletedTask; }
}
