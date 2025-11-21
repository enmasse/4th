using Forth.Core.Interpreter;
using System.IO;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( str filename -- ) Write string to file (overwrites)")]
    private static Task Prim_WRITEFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "WRITE-FILE");
        var fnameObj = i.PopInternal();
        var strObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects filename string");
        if (strObj is not string s)
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects string to write");

        File.WriteAllText(fname, s);
        return Task.CompletedTask;
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( str filename -- ) Append string to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "APPEND-FILE");
        var fnameObj = i.PopInternal();
        var strObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects filename string");
        if (strObj is not string s)
            throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects string to append");

        File.AppendAllText(fname, s);
        return Task.CompletedTask;
    }

    [Primitive("READ-FILE", HelpString = "READ-FILE ( filename -- str ) Read entire file contents and push as string")]
    private static Task Prim_READFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "READ-FILE");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "READ-FILE expects filename string");

        var text = File.ReadAllText(fname);
        i.Push(text);
        return Task.CompletedTask;
    }

    [Primitive("FILE-EXISTS", HelpString = "FILE-EXISTS ( filename -- flag ) Push -1 if exists, 0 if not")]
    private static Task Prim_FILEEXISTS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-EXISTS");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-EXISTS expects filename string");

        i.Push(File.Exists(fname) ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size ) Push file size in bytes or -1 on missing")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-SIZE");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-SIZE expects filename string");

        if (!File.Exists(fname))
        {
            i.Push(-1L);
            return Task.CompletedTask;
        }

        var fi = new FileInfo(fname);
        i.Push((long)fi.Length);
        return Task.CompletedTask;
    }

    [Primitive("OPEN-FILE", HelpString = "OPEN-FILE ( filename [mode] -- ior fileid ) Open file and push ior (0 ok) and file id (0 on failure); mode: 0=read,1=write,2=append")]
    private static Task Prim_OPENFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "OPEN-FILE");
        long mode = 0;
        object maybeModeOrPath = i.PopInternal();
        try
        {
            if (maybeModeOrPath is long)
            {
                // mode provided, path below
                mode = (long)maybeModeOrPath;
                var pathObj = i.PopInternal();
                if (pathObj is not string path)
                {
                    i.Push(1L);
                    i.Push(0L);
                    return Task.CompletedTask;
                }

                var m = (ForthInterpreter.FileOpenMode)(mode <= 0 ? 0 : (mode == 1 ? 1 : 2));
                var handle = i.OpenFileHandle(path, m);
                i.Push(0L);
                i.Push((long)handle);
                return Task.CompletedTask;
            }
            else
            {
                // Single arg (path)
                if (maybeModeOrPath is not string path)
                {
                    i.Push(1L);
                    i.Push(0L);
                    return Task.CompletedTask;
                }

                var handle = i.OpenFileHandle(path, ForthInterpreter.FileOpenMode.Read);
                i.Push(0L);
                i.Push((long)handle);
                return Task.CompletedTask;
            }
        }
        catch
        {
            // On any failure, push non-zero ior and fileid 0
            i.Push(1L);
            i.Push(0L);
            return Task.CompletedTask;
        }
    }

    [Primitive("CLOSE-FILE", HelpString = "CLOSE-FILE ( fileid -- ior ) Close an open file handle and push ior (0=ok)")]
    private static Task Prim_CLOSEFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "CLOSE-FILE");
        var hObj = i.PopInternal();
        var h = (int)ToLong(hObj);
        var ok = i.TryCloseFileHandle(h);
        // ANS ior: 0 == success, non-zero is error
        i.Push(ok ? 0L : 1L);
        return Task.CompletedTask;
    }

    [Primitive("REPOSITION-FILE", HelpString = "REPOSITION-FILE ( handle offset -- ) Seek to offset in file")]
    private static Task Prim_REPOSITIONFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "REPOSITION-FILE");
        var offset = ToLong(i.PopInternal());
        var h = (int)ToLong(i.PopInternal());
        i.RepositionFileHandle(h, offset);
        return Task.CompletedTask;
    }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> - load and execute a Forth source file")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    {
        var fname = i.ReadNextTokenOrThrow("Expected filename after INCLUDE");
        // Accept quoted filename token as produced by tokenizer
        if (fname.Length >= 2 && fname[0] == '"' && fname[^1] == '"')
        {
            fname = fname[1..^1];
        }

        if (!File.Exists(fname))
            throw new ForthException(ForthErrorCode.CompileError, $"INCLUDE: file not found: {fname}");

        var lines = await File.ReadAllLinesAsync(fname).ConfigureAwait(false);
        foreach (var raw in lines)
        {
            var line = raw?.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            await i.EvalAsync(line).ConfigureAwait(false);
        }
    }

    // Runtime LOAD: takes a filename string on the stack and executes it (non-immediate)
    [Primitive("LOAD", IsAsync = true, HelpString = "LOAD ( filename -- ) - load and execute a Forth source file at runtime")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    {
        i.EnsureStack(1, "LOAD");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "LOAD expects filename string on stack");

        if (!File.Exists(fname))
            throw new ForthException(ForthErrorCode.CompileError, $"LOAD: file not found: {fname}");

        var lines = await File.ReadAllLinesAsync(fname).ConfigureAwait(false);
        foreach (var raw in lines)
        {
            var line = raw?.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            await i.EvalAsync(line).ConfigureAwait(false);
        }
    }

    [Primitive("READ-FILE-BYTES", HelpString = "READ-FILE-BYTES ( handle addr u -- actual ) Read up to u bytes from handle into memory at addr and push actual bytes read")]
    private static Task Prim_READ_FILE_BYTES(ForthInterpreter i)
    {
        i.EnsureStack(3, "READ-FILE-BYTES");
        var u = (int)ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        var h = (int)ToLong(i.PopInternal());
        var read = i.ReadFileIntoMemory(h, addr, u);
        i.Push((long)read);
        return Task.CompletedTask;
    }

    [Primitive("WRITE-FILE-BYTES", HelpString = "WRITE-FILE-BYTES ( handle addr u -- written ) Write u bytes from memory at addr to handle and push actual written")]
    private static Task Prim_WRITE_FILE_BYTES(ForthInterpreter i)
    {
        i.EnsureStack(3, "WRITE-FILE-BYTES");
        var u = (int)ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        var h = (int)ToLong(i.PopInternal());
        var written = i.WriteMemoryToFile(h, addr, u);
        i.Push((long)written);
        return Task.CompletedTask;
    }

#if DEBUG
    [Primitive("LAST-WRITE-BYTES", HelpString = "LAST-WRITE-BYTES ( -- handle pos count addr ) Expose diagnostics for last write")]
    private static Task Prim_LAST_WRITE_BYTES(ForthInterpreter i)
    {
        // Provide handle, position-after, count, and address of a temporary buffer copied into memory
        var handle = i._lastWriteHandle;
        var pos = i._lastWritePositionAfter;
        var buf = i._lastWriteBuffer ?? System.Array.Empty<byte>();
        // Allocate memory region
        var addr = i._nextAddr;
        for (int k = 0; k < buf.Length; k++) i.MemSet(addr + k, buf[k]);
        i._nextAddr += buf.Length;
        i.Push((long)handle);
        i.Push(pos);
        i.Push((long)buf.Length);
        i.Push(addr);
        return Task.CompletedTask;
    }

    [Primitive("LAST-READ-BYTES", HelpString = "LAST-READ-BYTES ( -- handle pos count addr ) Expose diagnostics for last read")]
    private static Task Prim_LAST_READ_BYTES(ForthInterpreter i)
    {
        var handle = i._lastReadHandle;
        var pos = i._lastReadPositionAfter;
        var buf = i._lastReadBuffer ?? System.Array.Empty<byte>();
        var addr = i._nextAddr;
        for (int k = 0; k < buf.Length; k++) i.MemSet(addr + k, buf[k]);
        i._nextAddr += buf.Length;
        i.Push((long)handle);
        i.Push(pos);
        i.Push((long)buf.Length);
        i.Push(addr);
        return Task.CompletedTask;
    }
#endif

    [Primitive("BLOCK", HelpString = "BLOCK ( n -- c-addr u ) - load block n into memory and push c-addr and count")]
    private static Task Prim_BLOCK(ForthInterpreter i)
    {
        i.EnsureStack(1, "BLOCK");
        var n = (int)ToLong(i.PopInternal());
        // Ensure on-disk file has space if configured
        i.EnsureBlockExistsOnDisk(n);
        var addr = i.GetOrAllocateBlockAddr(n);
        i.LoadBlockFromBacking(n, addr);
        i.SetCurrentBlockNumber(n);
        i.Push((long)addr);
        i.Push((long)ForthInterpreter.BlockSize);
        return Task.CompletedTask;
    }

    [Primitive("SAVE", HelpString = "SAVE ( c-addr u n -- ) - save u bytes from c-addr into block n")]
    private static Task Prim_SAVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "SAVE");
        var n = (int)ToLong(i.PopInternal());
        var u = (int)ToLong(i.PopInternal());
        var addrObj = i.PopInternal();
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "SAVE expects non-negative u");
        if (u > ForthInterpreter.BlockSize) u = ForthInterpreter.BlockSize;

        long addr;
        if (addrObj is string s)
        {
            var content = s.Length > u ? s.Substring(0, u) : s.PadRight(u, ' ');
            i.SetBlockBuffer(n, content);
            return Task.CompletedTask;
        }

        if (addrObj is long a)
        {
            addr = a;
            i.SaveBlockToBacking(n, addr, u);
            return Task.CompletedTask;
        }

        throw new ForthException(ForthErrorCode.TypeError, "SAVE expects c-addr (address) or string");
    }

    [Primitive("OPEN-BLOCK-FILE", HelpString = "OPEN-BLOCK-FILE <path> - open or create a block-file backing store")]
    private static Task Prim_OPENBLOCKFILE(ForthInterpreter i)
    {
        string path;
        // Accept path as a pushed string on the stack (S"..."), or as following token
        if (i.Stack.Count > 0 && i.Stack[^1] is string s)
        {
            // consume stack value
            var _ = i.PopInternal();
            path = s;
        }
        else
        {
            path = i.ReadNextTokenOrThrow("Expected path after OPEN-BLOCK-FILE");
        }

        i.OpenBlockFile(path);
        return Task.CompletedTask;
    }

    [Primitive("OPEN-BLOCK-DIR", HelpString = "OPEN-BLOCK-DIR <path> - open or create a directory of per-block files for block storage")]
    private static Task Prim_OPENBLOCKDIR(ForthInterpreter i)
    {
        string path;
        if (i.Stack.Count > 0 && i.Stack[^1] is string s)
        {
            var _ = i.PopInternal();
            path = s;
        }
        else
        {
            path = i.ReadNextTokenOrThrow("Expected path after OPEN-BLOCK-DIR");
        }

        i.OpenBlockFile(path, perBlock: true);
        return Task.CompletedTask;
    }

    [Primitive("FLUSH-BLOCK-FILE", HelpString = "FLUSH-BLOCK-FILE - flush cached MMF accessors and file stream to disk")]
    private static Task Prim_FLUSHBLOCKFILE(ForthInterpreter i)
    {
        i.FlushBlockFile();
        return Task.CompletedTask;
    }

    [Primitive("CLOSE-BLOCK-FILE", HelpString = "CLOSE-BLOCK-FILE - close any open block-file backing store and dispose accessors")]
    private static Task Prim_CLOSEBLOCKFILE(ForthInterpreter i)
    {
        i.CloseBlockFile();
        return Task.CompletedTask;
    }
}
