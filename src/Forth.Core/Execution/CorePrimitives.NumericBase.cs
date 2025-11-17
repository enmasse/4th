using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddNumericBaseEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, ">NUMBER")] = new(i =>
        {
            i.EnsureStack(3, ">NUMBER");
            var consumed = (int)ToLong(i.PopInternal());
            var start = (int)ToLong(i.PopInternal());
            var obj = i.PopInternal();
            if (obj is not string s)
                throw new ForthException(ForthErrorCode.TypeError, ">NUMBER expects a string and two integers");
            int idx = Math.Clamp(start, 0, s.Length);
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
        });

        d[(null, "BASE")] = new(i => { i.Push(i.BaseAddr); return Task.CompletedTask; });

        d[(null, "DECIMAL")] = new(i => { i.MemSet(i.BaseAddr, 10); return Task.CompletedTask; });

        d[(null, "HEX")] = new(i => { i.MemSet(i.BaseAddr, 16); return Task.CompletedTask; });

        d[(null, "STATE")] = new(i => { i.Push(i.StateAddr); return Task.CompletedTask; });
    }
}
