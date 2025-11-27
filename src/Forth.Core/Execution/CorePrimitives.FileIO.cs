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
        // Check for counted-addr string form
        if (i.Stack.Count >= 2 && i.Stack[^1] is string && i.Stack[^2] is long)
        {
            var filename = (string)i.PopInternal();
            var countedAddr = (long)i.PopInternal();
            var content = i.ReadCountedString(countedAddr);
            await File.WriteAllTextAsync(filename, content);
            return;
        }

        // Check for string string form
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
        var data = i.PopInternal();
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
