using Forth.Core.Interpreter;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( str filename | addr u filename | counted-addr filename -- ) overwrite file with data")]
    private static Task Prim_WRITEFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "WRITE-FILE");
        var fnameObj = i.PopInternal(); // filename
        var second = i.PopInternal();   // content descriptor (string OR u OR counted addr)
        if (fnameObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects filename string");
        if (second is string s) { File.WriteAllText(fname, s); return Task.CompletedTask; }
        if ((second is long || second is int || second is short || second is byte) && i.Stack.Count >= 1)
        {
            long u = ToLong(second);
            var addrObj = i.PopInternal();
            long addr = ToLong(addrObj);
            if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE negative length");
            File.WriteAllText(fname, ReadMemString(i, addr, u));
            return Task.CompletedTask;
        }
        if (second is long addrLong)
        {
            var (str, _) = ReadCounted(i, addrLong);
            File.WriteAllText(fname, str);
            return Task.CompletedTask;
        }
        throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects (str filename) or (addr u filename) or (counted-addr filename)");
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( str filename | addr u filename | counted-addr filename -- ) append data to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "APPEND-FILE");
        var fnameObj = i.PopInternal();
        var second = i.PopInternal();
        if (fnameObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects filename string");
        if (second is string s) { File.AppendAllText(fname, s); return Task.CompletedTask; }
        if ((second is long || second is int || second is short || second is byte) && i.Stack.Count >= 1)
        {
            long u = ToLong(second);
            var addrObj = i.PopInternal();
            long addr = ToLong(addrObj);
            if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE negative length");
            File.AppendAllText(fname, ReadMemString(i, addr, u));
            return Task.CompletedTask;
        }
        if (second is long addrLong)
        {
            var (str, _) = ReadCounted(i, addrLong);
            File.AppendAllText(fname, str);
            return Task.CompletedTask;
        }
        throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects (str filename) or (addr u filename) or (counted-addr filename)");
    }

    private static string ReadMemString(ForthInterpreter i, long addr, long u)
    {
        var sb = new StringBuilder();
        for (long k = 0; k < u; k++) { i.MemTryGet(addr + k, out var v); char ch = (char)(ToLong(v) & 0xFF); sb.Append(ch); }
        return sb.ToString();
    }
    private static (string Text, long Length) ReadCounted(ForthInterpreter i, long addr)
    {
        i.MemTryGet(addr, out var lenCell);
        long u = ToLong(lenCell);
        var sb = new StringBuilder();
        for (long k = 0; k < u; k++) { i.MemTryGet(addr + 1 + k, out var v); char ch = (char)(ToLong(v) & 0xFF); sb.Append(ch); }
        return (sb.ToString(), u);
    }

    [Primitive("READ-FILE", HelpString = "READ-FILE ( filename -- str ) read whole file")]
    private static Task Prim_READFILE(ForthInterpreter i)
    { i.EnsureStack(1, "READ-FILE"); var fObj = i.PopInternal(); if (fObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "READ-FILE expects filename string"); if (!File.Exists(fname)) throw new ForthException(ForthErrorCode.CompileError, "File not found"); i.Push(File.ReadAllText(fname)); return Task.CompletedTask; }

    [Primitive("FILE-EXISTS", HelpString = "FILE-EXISTS ( filename -- flag ) -1 if exists else 0")]
    private static Task Prim_FILEEXISTS(ForthInterpreter i)
    { i.EnsureStack(1, "FILE-EXISTS"); var fObj = i.PopInternal(); if (fObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "FILE-EXISTS expects filename string"); i.Push(File.Exists(fname) ? -1L : 0L); return Task.CompletedTask; }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size|-1 ) size or -1 if missing")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    { i.EnsureStack(1, "FILE-SIZE"); var fObj = i.PopInternal(); if (fObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "FILE-SIZE expects filename string"); i.Push(File.Exists(fname) ? (long)new FileInfo(fname).Length : -1L); return Task.CompletedTask; }

    [Primitive("OPEN-FILE", HelpString = "OPEN-FILE ( filename [mode] -- ior fileid ) mode 0 read 1 write 2 append")]
    private static Task Prim_OPENFILE(ForthInterpreter i)
    { if (i.Stack.Count >= 2 && i.StackTop() is long modeObj && i.StackNthFromTop(2) is string path) { int mode = (int)modeObj; i.PopInternal(); i.PopInternal(); var fm = (ForthInterpreter.FileOpenMode)(mode <= 0 ? 0 : mode == 1 ? 1 : 2); try { var h = i.OpenFileHandle(path, fm); i.Push(0L); i.Push((long)h); } catch { i.Push(1L); i.Push(0L); } return Task.CompletedTask; } i.EnsureStack(1, "OPEN-FILE"); var fObj = i.PopInternal(); if (fObj is not string fname) { i.Push(1L); i.Push(0L); return Task.CompletedTask; } try { var h2 = i.OpenFileHandle(fname, ForthInterpreter.FileOpenMode.Read); i.Push(0L); i.Push((long)h2); } catch { i.Push(1L); i.Push(0L); } return Task.CompletedTask; }

    [Primitive("CLOSE-FILE", HelpString = "CLOSE-FILE ( fileid -- ior ) close handle")]
    private static Task Prim_CLOSEFILE(ForthInterpreter i)
    { i.EnsureStack(1, "CLOSE-FILE"); var h = (int)ToLong(i.PopInternal()); var ok = i.TryCloseFileHandle(h); i.Push(ok ? 0L : 1L); return Task.CompletedTask; }

    [Primitive("REPOSITION-FILE", HelpString = "REPOSITION-FILE ( fileid pos -- ) seek to absolute position")]
    private static Task Prim_REPOSITIONFILE(ForthInterpreter i)
    { i.EnsureStack(2, "REPOSITION-FILE"); var pos = ToLong(i.PopInternal()); var h = (int)ToLong(i.PopInternal()); i.RepositionFileHandle(h, pos); return Task.CompletedTask; }

    [Primitive("READ-FILE-BYTES", HelpString = "READ-FILE-BYTES ( fileid addr u -- actual ) read bytes into memory")]
    private static Task Prim_READ_FILE_BYTES(ForthInterpreter i)
    { i.EnsureStack(3, "READ-FILE-BYTES"); var u = (int)ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); var fileid = (int)ToLong(i.PopInternal()); var read = i.ReadFileIntoMemory(fileid, addr, u); i.Push((long)read); return Task.CompletedTask; }

    [Primitive("WRITE-FILE-BYTES", HelpString = "WRITE-FILE-BYTES ( fileid addr u -- written ) write bytes from memory")]
    private static Task Prim_WRITE_FILE_BYTES(ForthInterpreter i)
    { i.EnsureStack(3, "WRITE-FILE-BYTES"); var u = (int)ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); var fileid = (int)ToLong(i.PopInternal()); var written = i.WriteMemoryToFile(fileid, addr, u); i.Push((long)written); return Task.CompletedTask; }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> interpret file contents")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    { var fname = i.ReadNextTokenOrThrow("Expected filename after INCLUDE"); if (fname.Length >= 2 && fname[0] == '"' && fname[^1] == '"') fname = fname[1..^1]; if (!File.Exists(fname)) throw new ForthException(ForthErrorCode.CompileError, $"INCLUDE: file not found: {fname}"); foreach (var raw in await File.ReadAllLinesAsync(fname).ConfigureAwait(false)) { var line = raw?.Trim(); if (string.IsNullOrEmpty(line)) continue; await i.EvalAsync(line).ConfigureAwait(false); } }

    [Primitive("LOAD", IsAsync = true, HelpString = "LOAD ( filename -- ) interpret file at runtime")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    { i.EnsureStack(1, "LOAD"); var fObj = i.PopInternal(); if (fObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "LOAD expects filename string"); if (!File.Exists(fname)) throw new ForthException(ForthErrorCode.CompileError, $"LOAD: file not found: {fname}"); foreach (var raw in await File.ReadAllLinesAsync(fname).ConfigureAwait(false)) { var line = raw?.Trim(); if (string.IsNullOrEmpty(line)) continue; await i.EvalAsync(line).ConfigureAwait(false); } }
}
