using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Forth.Core;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Forth.Core.Execution;

internal static class FileIoPrimitives
{
    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( c-addr u filename | string string | counted-addr string -- ) - write string data to file")]
    private static async Task Prim_WRITEFILE(ForthInterpreter i)
    {
        // Most common forms in tests:
        // - S"..." "path" WRITE-FILE          (c-addr u filename)
        // - "content" "path" WRITE-FILE      (string string)
        // - counted-addr "path" WRITE-FILE    (counted-addr string)

        if (i.Stack.Count >= 1 && i.Stack[^1] is string)
        {
            var filename = FileIoDecoder.ResolvePath((string)i.PopInternal());
            var content = FileIoDecoder.PopTextData(i);
            await File.WriteAllTextAsync(filename, content).ConfigureAwait(false);
            return;
        }

        // Standard form: (c-addr u filename) where filename must be a string object on the stack.
        i.EnsureStack(3, "WRITE-FILE");
        var filenameObj = i.PopInternal();
        var u = CorePrimitives.ToLong(i.PopInternal());
        var addr = CorePrimitives.ToLong(i.PopInternal());

        if (filenameObj is string fname)
        {
            var filename = FileIoDecoder.ResolvePath(fname);
            var content = i.ReadMemoryString(addr, u);
            await File.WriteAllTextAsync(filename, content).ConfigureAwait(false);
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects string filename");
        }
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( data filename -- ) - append data to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        var filename = FileIoDecoder.PopFilenameOrNextWord(i, "APPEND-FILE");

        // payload can be (addr u), counted addr, or string
        var text = FileIoDecoder.PopTextData(i);

        try
        {
            File.AppendAllText(filename, text, Encoding.UTF8);
            i._lastWriteHandle = 0;
            i._lastWriteBuffer = Encoding.UTF8.GetBytes(text);
            i._lastWritePositionAfter = new FileInfo(filename).Length;
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
        var filename = FileIoDecoder.PopFilenameOrNextWord(i, "READ-FILE");
        try
        {
            var text = File.ReadAllText(filename, Encoding.UTF8);
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
        var filename = FileIoDecoder.PopFilenameOrNextWord(i, "FILE-EXISTS");
        i.Push(File.Exists(filename) ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("FILE-STATUS", HelpString = "FILE-STATUS ( c-addr u -- x ior ) - get file status")]
    private static Task Prim_FILESTATUS(ForthInterpreter i)
    {
        i.EnsureStack(2, "FILE-STATUS");
        var v1 = CorePrimitives.ToLong(i.PopInternal());
        var v2 = CorePrimitives.ToLong(i.PopInternal());

        var filename = FileIoDecoder.DecodeFilenameFromCStringPair(i, v1, v2);

        try
        {
            var exists = File.Exists(filename);
            i.Push(exists ? 0L : -1L);
            i.Push(0L);
        }
        catch (Exception)
        {
            i.Push(0L);
            i.Push(-1L);
        }
        return Task.CompletedTask;
    }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size | -1 ) - file size in bytes or -1 if error")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    {
        var filename = FileIoDecoder.PopFilenameOrNextWord(i, "FILE-SIZE");
        try
        {
            var fi = new FileInfo(filename);
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
        long mode = 0;
        if (i.Stack.Count > 0 && i.Stack[^1] is long)
        {
            mode = (long)i.PopInternal();
        }

        var filename = FileIoDecoder.PopFilenameOrNextWord(i, "OPEN-FILE");

        try
        {
            var h = i.OpenFileHandle(filename, (ForthInterpreter.FileOpenMode)mode);
            i.Push(0L);
            i.Push((long)h);
        }
        catch (Exception)
        {
            i.Push(-1L);
            i.Push(0L);
        }

        return Task.CompletedTask;
    }

    [Primitive("CREATE-FILE", HelpString = "CREATE-FILE ( c-addr u fam -- fileid ior ) - create file")]
    private static Task Prim_CREATEFILE(ForthInterpreter i)
    {
        i.EnsureStack(3, "CREATE-FILE");
        var fam = CorePrimitives.ToLong(i.PopInternal());
        var u = CorePrimitives.ToLong(i.PopInternal());
        var addr = CorePrimitives.ToLong(i.PopInternal());
        var filename = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr, u);
        try
        {
            var h = i.OpenFileHandle(filename, fam, truncate: true);
            i.Push((long)h);
            i.Push(0L);
        }
        catch (Exception)
        {
            i.Push(0L);
            i.Push(-1L);
        }

        return Task.CompletedTask;
    }

    [Primitive("DELETE-FILE", HelpString = "DELETE-FILE ( c-addr u -- ior ) - delete file")]
    private static Task Prim_DELETEFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "DELETE-FILE");
        var u = (int)CorePrimitives.ToLong(i.PopInternal());
        var addr = CorePrimitives.ToLong(i.PopInternal());

        var filename = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr, u);

        if (!File.Exists(filename))
        {
            i.Push(-1L);
        }
        else
        {
            try
            {
                File.Delete(filename);
                i.Push(0L);
            }
            catch (Exception)
            {
                i.Push(-1L);
            }
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
        i.Push(fam | 4);
        return Task.CompletedTask;
    }

    [Primitive("CLOSE-FILE", HelpString = "CLOSE-FILE ( fid -- ior ) - close file handle")]
    private static Task Prim_CLOSEFILE(ForthInterpreter i)
    {
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
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
        var offset = CorePrimitives.ToLong(i.PopInternal());
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
        i.RepositionFileHandle(fid, offset);
        return Task.CompletedTask;
    }

    [Primitive("FILE-POSITION", HelpString = "FILE-POSITION ( fileid -- ud ior ) - get current file position")]
    private static Task Prim_FILEPOSITION(ForthInterpreter i)
    {
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
        try
        {
            var pos = i.GetFilePosition(fid);
            i.Push(pos);
            i.Push(0L);
            i.Push(0L);
        }
        catch (Exception)
        {
            i.Push(0L);
            i.Push(0L);
            i.Push(-1L);
        }
        return Task.CompletedTask;
    }

    [Primitive("READ-FILE-BYTES", HelpString = "READ-FILE-BYTES ( fid addr u -- cnt ) - read up to u bytes into addr from file")]
    private static Task Prim_READFILEBYTES(ForthInterpreter i)
    {
        var uLong = CorePrimitives.ToLong(i.PopInternal());
        var u = (int)uLong;
        var addr = CorePrimitives.ToLong(i.PopInternal());
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative length");
        var read = i.ReadFileIntoMemory(fid, addr, u);
        i.Push((long)read);
        return Task.CompletedTask;
    }

    [Primitive("WRITE-FILE-BYTES", HelpString = "WRITE-FILE-BYTES ( fid addr u -- cnt ) - write u bytes from addr to file")]
    private static Task Prim_WRITEFILEBYTES(ForthInterpreter i)
    {
        var uLong = CorePrimitives.ToLong(i.PopInternal());
        var u = (int)uLong;
        var addr = CorePrimitives.ToLong(i.PopInternal());
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative length");
        var written = i.WriteMemoryToFile(fid, addr, u);
        i.Push((long)written);
        return Task.CompletedTask;
    }

    [Primitive("WRITE-LINE", HelpString = "WRITE-LINE ( c-addr u fileid -- ) - write u chars from c-addr to file, followed by newline")]
    private static Task Prim_WRITELINE(ForthInterpreter i)
    {
        var fid = (int)CorePrimitives.ToLong(i.PopInternal());
        var u = (int)CorePrimitives.ToLong(i.PopInternal());
        var addr = CorePrimitives.ToLong(i.PopInternal());
        var content = i.ReadMemoryString(addr, u);
        i.WriteStringToFile(fid, content + "\n");
        return Task.CompletedTask;
    }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> interpret file contents")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    {
        var path = FileIoDecoder.PopFilenameOrNextWord(i, "INCLUDE");
        var text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("LOAD-FILE", IsAsync = true, HelpString = "LOAD-FILE ( filename -- ) interpret file at runtime")]
    private static async Task Prim_LOADFILE(ForthInterpreter i)
    {
        var path = FileIoDecoder.PopFilenameOrNextWord(i, "LOAD-FILE");
        var text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("INCLUDE-FILE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE-FILE ( i*x c-addr u -- j*x ) - interpret file")]
    private static async Task Prim_INCLUDEFILE(ForthInterpreter i)
    {
        var filename = FileIoDecoder.PopFilenameFromStackOrCString(i, "INCLUDE-FILE");
        var text = await File.ReadAllTextAsync(filename).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("INCLUDED", IsImmediate = true, IsAsync = true, HelpString = "INCLUDED ( i*x c-addr u | string -- j*x ) - interpret file")]
    private static async Task Prim_INCLUDED(ForthInterpreter i)
    {
        var filename = FileIoDecoder.PopFilenameFromStackOrCString(i, "INCLUDED");
        var text = await File.ReadAllTextAsync(filename).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("RENAME-FILE", HelpString = "RENAME-FILE ( c-addr1 u1 c-addr2 u2 -- ior ) - rename file")]
    private static Task Prim_RENAMEFILE(ForthInterpreter i)
    {
        i.EnsureStack(4, "RENAME-FILE");

        var u2 = CorePrimitives.ToLong(i.PopInternal());
        var addr2 = CorePrimitives.ToLong(i.PopInternal());
        var u1 = CorePrimitives.ToLong(i.PopInternal());
        var addr1 = CorePrimitives.ToLong(i.PopInternal());

        var oldName = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr1, u1);
        var newName = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr2, u2);

        try
        {
            const int attempts = 10;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    File.Move(oldName, newName, overwrite: true);

                    // Ensure the destination is observable before reporting success.
                    for (int spin = 0; spin < 20; spin++)
                    {
                        if (File.Exists(newName) && !File.Exists(oldName))
                        {
                            i.Push(0L);
                            return Task.CompletedTask;
                        }
                        Thread.Sleep(5);
                    }

                    // If state is not yet observable, retry.
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // fall through to retry/backoff
                }

                if (attempt + 1 < attempts)
                {
                    Thread.Sleep(10 + (attempt * 5));
                }
            }

            i.Push(-1L);
        }
        catch (Exception)
        {
            i.Push(-1L);
        }
        return Task.CompletedTask;
    }

    [Primitive("COPY-FILE", HelpString = "COPY-FILE ( c-addr1 u1 c-addr2 u2 -- ior ) - copy file")]
    private static Task Prim_COPYFILE(ForthInterpreter i)
    {
        i.EnsureStack(4, "COPY-FILE");
        var u2 = CorePrimitives.ToLong(i.PopInternal());
        var addr2 = CorePrimitives.ToLong(i.PopInternal());
        var u1 = CorePrimitives.ToLong(i.PopInternal());
        var addr1 = CorePrimitives.ToLong(i.PopInternal());

        var src = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr1, u1);
        var dst = FileIoDecoder.DecodeFilenameFromCStringPair(i, addr2, u2);

        try
        {
            const int attempts = 10;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    File.Copy(src, dst, overwrite: true);

                    // On some systems (AV/indexer), the file can be created but not immediately observable.
                    for (int spin = 0; spin < 20; spin++)
                    {
                        if (File.Exists(dst))
                        {
                            i.Push(0L);
                            return Task.CompletedTask;
                        }
                        Thread.Sleep(5);
                    }

                    // If still not visible, fall through and retry.
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // fall through to retry/backoff
                }

                if (attempt + 1 < attempts)
                {
                    Thread.Sleep(10 + (attempt * 5));
                }
            }

            i.Push(-1L);
        }
        catch (Exception)
        {
            i.Push(-1L);
        }
        return Task.CompletedTask;
    }
}
