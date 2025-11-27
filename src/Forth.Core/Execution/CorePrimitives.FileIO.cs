using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Forth.Core;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Linq;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( c-addr u filename | string string | counted-addr string -- ) - write string data to file")]
    private static async Task Prim_WRITEFILE(ForthInterpreter i)
    {
        // If top is filename string and there are two numeric values below, accept either
        // (addr u filename) or (u addr filename) produced by different string literals.
        if (i.Stack.Count >= 3 && i.Stack[^1] is string && i.Stack[^2] is long && i.Stack[^3] is long)
        {
            var filename = (string)i.PopInternal();
            var v1 = ToLong(i.PopInternal()); // popped top
            var v2 = ToLong(i.PopInternal()); // next

            // Disambiguate forms. If v1 points to the first char of a counted string
            // then the length cell will be at (v1 - 1) and equal to v2 -> prefer that.
            string content;
            // If v1 points to first char of a counted string, the length cell is at v1-1.
            // Since MemTryGet has no boolean return, call it separately and compare.
            if (v1 > 0)
            {
                long maybeLen = 0;
                i.MemTryGet(v1 - 1, out maybeLen);
                if (ToLong(maybeLen) == v2)
                {
                    content = i.ReadMemoryString(v1, v2);
                }
                else
                {
                    content = i.ReadMemoryString(v2, v1);
                }
            }
            else
            {
                content = i.ReadMemoryString(v2, v1);
            }

            await File.WriteAllTextAsync(filename, content);
            return;
        }

        // Check for counted-addr string form (single counted-addr value then filename)
        if (i.Stack.Count >= 2 && i.Stack[^1] is string && i.Stack[^2] is long)
        {
            var filename = (string)i.PopInternal();
            var countedAddr = (long)i.PopInternal();
            var content = i.ReadCountedString(countedAddr);
            await File.WriteAllTextAsync(filename, content);
            return;
        }

        // Check for string string form (content filename)
        if (i.Stack.Count >= 2 && i.Stack[^1] is string && i.Stack[^2] is string)
        {
            var filename = (string)i.PopInternal();
            var content = (string)i.PopInternal();
            await File.WriteAllTextAsync(filename, content);
            return;
        }

        // Standard form: c-addr u filename
        i.EnsureStack(3, "WRITE-FILE");
        var filenameObj = i.PopInternal();
        var u = ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());

        if (filenameObj is string fname)
        {
            var content = i.ReadMemoryString(addr, u);
            await File.WriteAllTextAsync(fname, content);
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects string filename");
        }
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( data filename -- ) - append data to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        var filenameToken = (i.Stack.Count > 0 && i.Stack[^1] is string sfn) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected filename after APPEND-FILE");
        // support (addr u filename) and (u addr filename) forms as written by S" variations
        object data;
        if (i.Stack.Count >= 2 && i.Stack[^1] is long && i.Stack[^2] is long)
        {
            var v1 = ToLong(i.PopInternal());
            var v2 = ToLong(i.PopInternal());
            // If v1 points to first char of a counted string, the length cell is at v1-1.
            // Since MemTryGet has no boolean return, call it separately and compare.
            if (v1 > 0)
            {
                long maybeLen = 0;
                i.MemTryGet(v1 - 1, out maybeLen);
                if (ToLong(maybeLen) == v2)
                {
                    data = i.ReadMemoryString(v1, v2);
                }
                else
                {
                    data = i.ReadMemoryString(v2, v1);
                }
            }
            else
            {
                data = i.ReadMemoryString(v2, v1);
            }
        }
        else
        {
            data = i.PopInternal();
        }
        try
        {
            string text = data is string sd ? sd : data is long addr ? i.ReadCountedString(addr) : data?.ToString() ?? string.Empty;
            System.IO.File.AppendAllText(filenameToken, text, Encoding.UTF8);
            i._lastWriteHandle = 0;
            i._lastWriteBuffer = Encoding.UTF8.GetBytes(text);
            i._lastWritePositionAfter = new FileInfo(filenameToken).Length;
        }
        catch (Exception)
        {
            throw new ForthException(ForthErrorCode.Unknown, "APPEND-FILE failed");
        }

        return Task.CompletedTask;
    }

    [Primitive("READ-FILE", HelpString = "READ-FILE ( filename -- str ) - read entire file as string")]
    private static Task Prim_READFILE(ForthInterpreter i)
    {
        var filenameToken = (i.Stack.Count > 0 && i.Stack[^1] is string sfn) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected filename after READ-FILE");
        try
        {
            var text = System.IO.File.ReadAllText(filenameToken, Encoding.UTF8);
            i.Push(text);
        }
        catch (Exception)
        {
            throw new ForthException(ForthErrorCode.Unknown, "READ-FILE failed");
        }

        return Task.CompletedTask;
    }

    [Primitive("FILE-EXISTS", HelpString = "FILE-EXISTS ( filename -- flag ) - true if file exists")]
    private static Task Prim_FILEEXISTS(ForthInterpreter i)
    {
        var filenameToken = (i.Stack.Count > 0 && i.Stack[^1] is string sfn) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected filename after FILE-EXISTS");
        i.Push(System.IO.File.Exists(filenameToken) ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size | -1 ) - file size in bytes or -1 if error")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    {
        var filenameToken = (i.Stack.Count > 0 && i.Stack[^1] is string sfn) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected filename after FILE-SIZE");
        try
        {
            var fi = new FileInfo(filenameToken);
            i.Push(fi.Exists ? fi.Length : -1L);
        }
        catch (Exception)
        {
            i.Push(-1L);
        }

        return Task.CompletedTask;
    }

    [Primitive("OPEN-FILE", HelpString = "OPEN-FILE ( filename mode -- ior fid ) - open file, mode 0=read 1=write 2=append")]
    private static Task Prim_OPENFILE(ForthInterpreter i)
    {
        // mode may be on stack; default to read
        long mode = 0;
        if (i.Stack.Count > 0 && i.Stack[^1] is long) mode = (long)i.PopInternal();
        var filenameToken = (i.Stack.Count > 0 && i.Stack[^1] is string sfn) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected filename after OPEN-FILE");
        try
        {
            var h = i.OpenFileHandle(filenameToken, (ForthInterpreter.FileOpenMode)mode);
            i.Push(0L); // ior success
            i.Push((long)h);
        }
        catch (Exception)
        {
            i.Push(-1L);
            i.Push(0L);
        }

        return Task.CompletedTask;
    }

    [Primitive("R/O", HelpString = "R/O ( -- fam ) read-only file access method")]
    private static Task Prim_RO(ForthInterpreter i) { i.Push(0L); return Task.CompletedTask; }

    [Primitive("W/O", HelpString = "W/O ( -- fam ) write-only file access method")]
    private static Task Prim_WO(ForthInterpreter i) { i.Push(1L); return Task.CompletedTask; }

    [Primitive("R/W", HelpString = "R/W ( -- fam ) read-write file access method")]
    private static Task Prim_RW(ForthInterpreter i) { i.Push(2L); return Task.CompletedTask; }

    [Primitive("BIN", HelpString = "BIN ( fam -- fam' ) modify fam for binary mode")]
    private static Task Prim_BIN(ForthInterpreter i)
    {
        i.EnsureStack(1, "BIN");
        var fam = (long)i.PopInternal();
        // For now, just return fam, since binary is default
        i.Push(fam);
        return Task.CompletedTask;
    }

    [Primitive("CLOSE-FILE", HelpString = "CLOSE-FILE ( fid -- ior ) - close file handle")]
    private static Task Prim_CLOSEFILE(ForthInterpreter i)
    {
        var fid = (int)ToLong(i.PopInternal());
        try
        {
            i.CloseFileHandle(fid);
            i.Push(0L);
        }
        catch (Exception)
        {
            i.Push(-1L);
        }

        return Task.CompletedTask;
    }

    [Primitive("REPOSITION-FILE", HelpString = "REPOSITION-FILE ( fid offset -- ) - seek to offset in file")]
    private static Task Prim_REPOSITIONFILE(ForthInterpreter i)
    {
        var offset = ToLong(i.PopInternal());
        var fid = (int)ToLong(i.PopInternal());
        i.RepositionFileHandle(fid, offset);
        return Task.CompletedTask;
    }

    [Primitive("READ-FILE-BYTES", HelpString = "READ-FILE-BYTES ( fid addr u -- cnt ) - read up to u bytes into addr from file")]
    private static Task Prim_READFILEBYTES(ForthInterpreter i)
    {
        var uLong = ToLong(i.PopInternal());
        var u = (int)uLong;
        var addr = ToLong(i.PopInternal());
        var fid = (int)ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative length");
        var read = i.ReadFileIntoMemory(fid, addr, u);
        i.Push((long)read);
        return Task.CompletedTask;
    }

    [Primitive("WRITE-FILE-BYTES", HelpString = "WRITE-FILE-BYTES ( fid addr u -- cnt ) - write u bytes from addr to file")]
    private static Task Prim_WRITEFILEBYTES(ForthInterpreter i)
    {
        var uLong = ToLong(i.PopInternal());
        var u = (int)uLong;
        var addr = ToLong(i.PopInternal());
        var fid = (int)ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative length");
        var written = i.WriteMemoryToFile(fid, addr, u);
        i.Push((long)written);
        return Task.CompletedTask;
    }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> interpret file contents")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    {
        string pathToken;
        if (i.Stack.Count > 0 && i.Stack[^1] is string s)
        {
            pathToken = (string)i.PopInternal();
        }
        else
        {
            pathToken = i.ReadNextTokenOrThrow("Expected path after INCLUDE");
        }

        if (pathToken.Length >= 2 && pathToken[0] == '"' && pathToken[^1] == '"')
            pathToken = pathToken[1..^1];
        // Resolve relative paths against the current working directory rather than
        // perform repository/test-specific searches. Let callers control which
        // working directory is active when invoking INCLUDE (tests or REPL can set
        // the current directory as needed).
        if (!System.IO.Path.IsPathRooted(pathToken))
        {
            pathToken = System.IO.Path.GetFullPath(pathToken, System.IO.Directory.GetCurrentDirectory());
        }

        var text = await System.IO.File.ReadAllTextAsync(pathToken).ConfigureAwait(false);
        // Evaluate the entire file content in one go so multi-line constructs are preserved
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("LOAD", IsAsync = true, HelpString = "LOAD ( filename -- ) interpret file at runtime")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    {
        string pathToken;
        if (i.Stack.Count > 0 && i.Stack[^1] is string s)
        {
            pathToken = (string)i.PopInternal();
        }
        else
        {
            pathToken = i.ReadNextTokenOrThrow("Expected path after LOAD");
        }
        if (pathToken.Length >= 2 && pathToken[0] == '"' && pathToken[^1] == '"')
            pathToken = pathToken[1..^1];

        var text = await System.IO.File.ReadAllTextAsync(pathToken).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }
}
