using System;
using System.IO;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static class FileIoDecoder
{
    // (rollback) do not pin a stable base directory; tests and scripts rely on CurrentDirectory.

    internal static string ResolvePath(string filename)
    {
        // NOTE: `filename` here can originate from tokenizer tokens or interpreter memory.
        // Trace before/after normalization when tracing is enabled.
        // (We don't have interpreter instance here; caller should prefer overloads that have it when tracing.)
        filename = NormalizeFileName(filename);
        if (!Path.IsPathRooted(filename))
        {
            filename = Path.GetFullPath(filename, Directory.GetCurrentDirectory());
        }
        return filename;
    }

    internal static string PopFilenameOrNextWord(ForthInterpreter i, string opname)
    {
        string token;
        if (i.Stack.Count > 0 && i.Stack[^1] is string)
        {
            token = (string)i.PopInternal();
        }
        else
        {
            if (!i.TryParseNextWord(out token))
            {
                throw new ForthException(ForthErrorCode.CompileError, $"Expected filename after {opname}");
            }
        }

        return ResolvePath(token);
    }

    internal static string DecodeCStringFromTwoLongs(ForthInterpreter i, long v1, long v2)
    {
        if (v1 > 0)
        {
            long maybeLen = 0;
            i.MemTryGet(v1 - 1, out maybeLen);
            if (CorePrimitives.ToLong(maybeLen) == v2)
            {
                return i.ReadMemoryString(v1, v2);
            }
        }

        return i.ReadMemoryString(v2, v1);
    }

    internal static string PopTextData(ForthInterpreter i)
    {
        if (i.Stack.Count >= 2 && i.Stack[^1] is long && i.Stack[^2] is long)
        {
            var v1 = CorePrimitives.ToLong(i.PopInternal());
            var v2 = CorePrimitives.ToLong(i.PopInternal());
            return DecodeCStringFromTwoLongs(i, v1, v2);
        }

        var data = i.PopInternal();
        return data switch
        {
            string s => s,
            long addr => i.ReadCountedString(addr),
            null => string.Empty,
            _ => data.ToString() ?? string.Empty,
        };
    }

    internal static string PopFilenameFromStackOrCString(ForthInterpreter i, string opname)
    {
        if (i.Stack.Count > 0 && i.Stack[^1] is string)
        {
            return ResolvePath((string)i.PopInternal());
        }

        i.EnsureStack(2, opname);
        var u = CorePrimitives.ToLong(i.PopInternal());
        var addr = CorePrimitives.ToLong(i.PopInternal());
        return ResolvePath(i.ReadMemoryString(addr, u));
    }

    internal static string DecodeFilenameFromCStringPair(ForthInterpreter i, long v1, long v2)
    {
        var raw = DecodeCStringFromTwoLongs(i, v1, v2);
        return ResolvePath(raw);
    }

    internal static string PopFilenameFromCStringPair(ForthInterpreter i, string opname)
    {
        i.EnsureStack(2, opname);
        var v2 = CorePrimitives.ToLong(i.PopInternal());
        var v1 = CorePrimitives.ToLong(i.PopInternal());
        return DecodeFilenameFromCStringPair(i, v1, v2);
    }

    internal static string NormalizeFileName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        // Keep normalization minimal: trim and remove surrounding quotes.
        // Control characters inside paths should not be silently removed here because that can collapse segments
        // (e.g. turning `Debug\net9.0\tmp` into `Debuget9.0mp`).
        s = s.Trim();

        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
        {
            s = s[1..^1].Trim();
        }

        return s;
    }
}
