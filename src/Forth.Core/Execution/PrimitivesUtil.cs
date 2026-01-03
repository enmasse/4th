using System.Collections.Generic;
using System;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static class PrimitivesUtil
{
    internal static long ToLong(object o)
    {
        return o switch
        {
            long l => l,
            int ii => ii,
            short s => s,
            byte b => b,
            double d => (long)d,
            char c => c,
            bool bo => bo ? -1 : 0,
            string str => str.Length > 0 ? (long)str[0] : throw new ForthException(ForthErrorCode.TypeError, "Empty string for number conversion"),
            Word w when w.BodyAddr.HasValue => w.BodyAddr.Value,
            _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {o?.GetType().Name ?? "null"}")
        };
    }

    internal static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        double d => d != 0.0,
        float f => f != 0.0f,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };

    internal static bool IsNumeric(object o) => o is long || o is int || o is short || o is byte || o is double || o is char || o is bool;

    internal sealed class KeyComparer : IEqualityComparer<(string? Module, string Name)>
    {
        public bool Equals((string? Module, string Name) x, (string? Module, string Name) y)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            return scomp.Equals(x.Module, y.Module) && scomp.Equals(x.Name, y.Name);
        }

        public int GetHashCode((string? Module, string Name) obj)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            int h1 = obj.Module is null ? 0 : scomp.GetHashCode(obj.Module);
            int h2 = scomp.GetHashCode(obj.Name);
            return (h1 * 397) ^ h2;
        }
    }
}
