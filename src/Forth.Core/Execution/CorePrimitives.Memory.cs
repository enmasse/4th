using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddMemoryEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "@")] = new(i => { i.EnsureStack(1, "@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); return Task.CompletedTask; });
        d[(null, "!")] = new(i => { i.EnsureStack(2, "!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); i.MemSet(addr, val); return Task.CompletedTask; });
        d[(null, "+!")] = new(i => { i.EnsureStack(2, "+!"); var addr = ToLong(i.PopInternal()); var add = ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, cur + add); return Task.CompletedTask; });
        d[(null, "C!")] = new(i => { i.EnsureStack(2, "C!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); var b = (long)((byte)val); i.MemSet(addr, b); return Task.CompletedTask; });
        d[(null, "C@")] = new(i => { i.EnsureStack(1, "C@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push((long)((byte)v)); return Task.CompletedTask; });

        d[(null, "MOVE")] = new(i =>
        {
            i.EnsureStack(3, "MOVE");
            var u = ToLong(i.PopInternal());
            var dst = ToLong(i.PopInternal());
            var src = ToLong(i.PopInternal());
            if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative MOVE length");
            if (u == 0) return Task.CompletedTask;
            if (src < dst && src + u > dst)
            {
                for (long k = u - 1; k >= 0; k--)
                {
                    i.MemTryGet(src + k, out var v);
                    i.MemSet(dst + k, (long)((byte)v));
                }
                return Task.CompletedTask;
            }
            for (long k = 0; k < u; k++)
            {
                i.MemTryGet(src + k, out var v);
                i.MemSet(dst + k, (long)((byte)v));
            }
            return Task.CompletedTask;
        });

        d[(null, "FILL")] = new(i => { i.EnsureStack(3, "FILL"); var ch = ToLong(i.PopInternal()); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length"); var b = (long)((byte)ch); for (long k = 0; k < u; k++) i.MemSet(addr + k, b); return Task.CompletedTask; });

        d[(null, "ERASE")] = new(i => { i.EnsureStack(2, "ERASE"); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length"); for (long k = 0; k < u; k++) i.MemSet(addr + k, 0); return Task.CompletedTask; });

        d[(null, "DUMP")] = new(i =>
        {
            i.EnsureStack(2, "DUMP");
            var u = ToLong(i.PopInternal());
            var addr = ToLong(i.PopInternal());
            if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative DUMP length");
            var sb = new StringBuilder();
            for (long k = 0; k < u; k++)
            {
                if (k > 0) sb.Append(' ');
                i.MemTryGet(addr + k, out var v);
                var b = (byte)v;
                sb.Append(b.ToString("X2"));
            }
            i.WriteText(sb.ToString());
            return Task.CompletedTask;
        });

        d[(null, "COUNT")] = new(i =>
        {
            i.EnsureStack(1, "COUNT");
            var obj = i.PopInternal();
            switch (obj)
            {
                case string s:
                    i.Push(s);
                    i.Push((long)s.Length);
                    return Task.CompletedTask;
                case long addr:
                    i.MemTryGet(addr, out var v);
                    var len = (long)((byte)v);
                    i.Push(addr + 1);
                    i.Push(len);
                    return Task.CompletedTask;
                default:
                    throw new ForthException(ForthErrorCode.TypeError, "COUNT expects a string or address");
            }
        });

        d[(null, ",")] = new(i => { i.EnsureStack(1, ","); var v = ToLong(i.PopInternal()); i._mem[i._nextAddr++] = v; return Task.CompletedTask; });

        d[(null, "ALLOT")] = new(i => { i.EnsureStack(1, "ALLOT"); var cells = ToLong(i.PopInternal()); if (cells < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size"); for (long k = 0; k < cells; k++) i._mem[i._nextAddr++] = 0; return Task.CompletedTask; });
    }
}
