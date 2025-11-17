using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddIOFormattingEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, ".")] = new(i => { i.EnsureStack(1, "."); var n = ToLong(i.PopInternal()); i.WriteNumber(n); return Task.CompletedTask; });

        d[(null, ".S")] = new(i =>
        {
            var items = i.Stack;
            var sb = new StringBuilder();
            sb.Append('<').Append(items.Count).Append("> ");
            for (int idx = 0; idx < items.Count; idx++)
            {
                if (idx > 0) sb.Append(' ');
                var o = items[idx];
                switch (o)
                {
                    case long l: sb.Append(l); break;
                    case int ii: sb.Append(ii); break;
                    case short s: sb.Append((long)s); break;
                    case byte b: sb.Append((long)b); break;
                    case char ch: sb.Append((int)ch); break;
                    case bool bo: sb.Append(bo ? 1 : 0); break;
                    default:
                        sb.Append(o?.ToString() ?? "null");
                        break;
                }
            }
            i.WriteText(sb.ToString());
            return Task.CompletedTask;
        });

        d[(null, "CR")] = new(i => { i.NewLine(); return Task.CompletedTask; });

        d[(null, "EMIT")] = new(i => { i.EnsureStack(1, "EMIT"); var n = ToLong(i.PopInternal()); char ch = (char)(n & 0xFFFF); i.WriteText(ch.ToString()); return Task.CompletedTask; });

        d[(null, "TYPE")] = new(i =>
        {
            i.EnsureStack(1, "TYPE"); var obj = i.PopInternal(); if (obj is string s) { i.WriteText(s); return Task.CompletedTask; } else throw new ForthException(ForthErrorCode.TypeError, "TYPE expects a string");
        });

        d[(null, "WORDS")] = new(i =>
        {
            var names = i.GetAllWordNames();
            var sb = new StringBuilder();
            bool first = true;
            foreach (var n in names)
            {
                if (!first) sb.Append(' ');
                first = false;
                sb.Append(n);
            }
            i.WriteText(sb.ToString());
            return Task.CompletedTask;
        });
    }
}
