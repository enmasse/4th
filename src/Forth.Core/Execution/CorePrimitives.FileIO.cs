using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    private static readonly Dictionary<int, FileStream> _openFiles = new();
    private static int _fileHandleCounter = 0;

    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( data filename -- ) - write data to file")]
    private static Task Prim_WRITEFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "WRITE-FILE");
        var filenameFv = i._stack.PopValue();
        var dataFv = i._stack.PopValue();
        if (dataFv.Type == Forth.Core.Interpreter.ValueType.String && filenameFv.Type == Forth.Core.Interpreter.ValueType.String)
        {
            var str = dataFv.AsString;
            var fname = filenameFv.AsString;
            using var sw = new StreamWriter(fname, false, Encoding.UTF8);
            sw.Write(str);
        }
        else if (dataFv.Type == Forth.Core.Interpreter.ValueType.Long && filenameFv.Type == Forth.Core.Interpreter.ValueType.String)
        {
            var addr = dataFv.AsLong;
            var fname = filenameFv.AsString;
            // read length
            i.MemTryGet(addr, out var lenObj);
            var len = (int)ToLong(lenObj);
            var sb = new StringBuilder();
            for (int k = 0; k < len; k++)
            {
                i.MemTryGet(addr + 1 + k, out var chObj);
                sb.Append((char)ToLong(chObj));
            }
            using var sw = new StreamWriter(fname, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "WRITE-FILE expects (str filename) or (addr u filename) or (counted-addr filename)");
        }
        return Task.CompletedTask;
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( data filename -- ) - append data to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "APPEND-FILE");
        var filenameFv = i._stack.PopValue();
        var dataFv = i._stack.PopValue();
        if (dataFv.Type == Forth.Core.Interpreter.ValueType.String && filenameFv.Type == Forth.Core.Interpreter.ValueType.String)
        {
            var str = dataFv.AsString;
            var fname = filenameFv.AsString;
            using var sw = new StreamWriter(fname, true, Encoding.UTF8);
            sw.Write(str);
        }
        else if (dataFv.Type == Forth.Core.Interpreter.ValueType.Long && filenameFv.Type == Forth.Core.Interpreter.ValueType.String)
        {
            var addr = dataFv.AsLong;
            var fname = filenameFv.AsString;
            // read length
            i.MemTryGet(addr, out var lenObj);
            var len = (int)ToLong(lenObj);
            var sb = new StringBuilder();
            for (int k = 0; k < len; k++)
            {
                i.MemTryGet(addr + 1 + k, out var chObj);
                sb.Append((char)ToLong(chObj));
            }
            using var sw = new StreamWriter(fname, true, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "APPEND-FILE expects (str filename) or (addr u filename) or (counted-addr filename)");
        }
        return Task.CompletedTask;
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

    [Primitive("READ-FILE", HelpString = "READ-FILE ( filename -- str ) - read entire file as string")]
    private static Task Prim_READFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "READ-FILE");
        var filenameFv = i._stack.PopValue();
        if (filenameFv.Type != ValueType.String)
            throw new ForthException(ForthErrorCode.TypeError, "READ-FILE expects string filename");
        var fname = filenameFv.AsString;
        if (!File.Exists(fname))
            throw new ForthException(ForthErrorCode.CompileError, "File not found");
        var content = File.ReadAllText(fname);
        i._stack.Push(ForthValue.FromString(content));
        return Task.CompletedTask;
    }

    [Primitive("FILE-EXISTS", HelpString = "FILE-EXISTS ( filename -- flag ) - true if file exists")]
    private static Task Prim_FILEEXISTS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-EXISTS");
        var filenameFv = i._stack.PopValue();
        if (filenameFv.Type != ValueType.String)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-EXISTS expects string filename");
        var fname = filenameFv.AsString;
        i._stack.Push(ForthValue.FromLong(File.Exists(fname) ? -1L : 0L));
        return Task.CompletedTask;
    }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size | -1 ) - file size in bytes or -1 if error")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-SIZE");
        var filenameFv = i._stack.PopValue();
        if (filenameFv.Type != ValueType.String)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-SIZE expects string filename");
        var fname = filenameFv.AsString;
        try
        {
            var fi = new FileInfo(fname);
            i._stack.Push(ForthValue.FromLong(fi.Length));
        }
        catch
        {
            i._stack.Push(ForthValue.FromLong(-1L));
        }
        return Task.CompletedTask;
    }

    [Primitive("OPEN-FILE", HelpString = "OPEN-FILE ( filename mode -- ior fid ) - open file, mode 0=read 1=write 2=create 3=readwrite")]
    private static Task Prim_OPENFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "OPEN-FILE");
        var modeFv = i._stack.PopValue();
        var filenameFv = i._stack.PopValue();
        if (filenameFv.Type != ValueType.String || modeFv.Type != ValueType.Long)
            throw new ForthException(ForthErrorCode.TypeError, "OPEN-FILE expects (filename mode -- ior fid)");
        var fname = filenameFv.AsString;
        var mode = modeFv.AsLong;
        FileMode fm;
        FileAccess fa;
        if (mode == 0) { fm = FileMode.Open; fa = FileAccess.Read; }
        else if (mode == 1) { fm = FileMode.OpenOrCreate; fa = FileAccess.Write; }
        else if (mode == 2) { fm = FileMode.Create; fa = FileAccess.Write; }
        else if (mode == 3) { fm = FileMode.OpenOrCreate; fa = FileAccess.ReadWrite; }
        else throw new ForthException(ForthErrorCode.CompileError, "Invalid file mode");
        try
        {
            var fs = new FileStream(fname, fm, fa, FileShare.ReadWrite);
            var handle = ++_fileHandleCounter;
            _openFiles[(int)handle] = fs;
            i._stack.Push(ForthValue.FromLong(0L)); // ior
            i._stack.Push(ForthValue.FromLong(handle));
        }
        catch (Exception ex)
        {
            i._stack.Push(ForthValue.FromLong(-1L)); // ior
            i._stack.Push(ForthValue.FromLong(0L)); // fid
        }
        return Task.CompletedTask;
    }

    [Primitive("CLOSE-FILE", HelpString = "CLOSE-FILE ( fid -- ior ) - close file handle")]
    private static Task Prim_CLOSEFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "CLOSE-FILE");
        var handleFv = i._stack.PopValue();
        if (handleFv.Type != ValueType.Long)
            throw new ForthException(ForthErrorCode.TypeError, "CLOSE-FILE expects handle");
        var handle = handleFv.AsLong;
        if (_openFiles.TryGetValue((int)handle, out var fs))
        {
            fs.Close();
            _openFiles.Remove((int)handle);
            i._stack.Push(ForthValue.FromLong(0L));
        }
        else
        {
            i._stack.Push(ForthValue.FromLong(-1L));
        }
        return Task.CompletedTask;
    }

    [Primitive("REPOSITION-FILE", HelpString = "REPOSITION-FILE ( fid offset -- ) - seek to offset in file")]
    private static Task Prim_REPOSITIONFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "REPOSITION-FILE");
        var offsetFv = i._stack.PopValue();
        var handleFv = i._stack.PopValue();
        if (offsetFv.Type != ValueType.Long || handleFv.Type != ValueType.Long)
            throw new ForthException(ForthErrorCode.TypeError, "REPOSITION-FILE expects (handle offset -- )");
        var offset = offsetFv.AsLong;
        var handle = handleFv.AsLong;
        if (_openFiles.TryGetValue((int)handle, out var fs))
        {
            fs.Seek(offset, SeekOrigin.Begin);
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file handle");
        }
        return Task.CompletedTask;
    }

    [Primitive("READ-FILE-BYTES", HelpString = "READ-FILE-BYTES ( fid addr u -- ) - read up to u bytes into addr from file")]
    private static Task Prim_READFILEBYTES(ForthInterpreter i)
    {
        i.EnsureStack(3, "READ-FILE-BYTES");
        var uFv = i._stack.PopValue();
        var addrFv = i._stack.PopValue();
        var handleFv = i._stack.PopValue();
        if (uFv.Type != ValueType.Long || addrFv.Type != ValueType.Long || handleFv.Type != ValueType.Long)
            throw new ForthException(ForthErrorCode.TypeError, "READ-FILE-BYTES expects (handle addr u -- )");
        var u = uFv.AsLong;
        var addr = addrFv.AsLong;
        var handle = handleFv.AsLong;
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative READ-FILE-BYTES length");
        if (_openFiles.TryGetValue((int)handle, out var fs))
        {
            var bytes = new byte[u];
            var read = fs.Read(bytes, 0, (int)u);
            for (long k = 0; k < read; k++)
            {
                i.MemSet(addr + k, bytes[k]);
            }
            i._stack.Push(ForthValue.FromLong(read));
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file handle");
        }
        return Task.CompletedTask;
    }

    [Primitive("WRITE-FILE-BYTES", HelpString = "WRITE-FILE-BYTES ( fid addr u -- ) - write u bytes from addr to file")]
    private static Task Prim_WRITEFILEBYTES(ForthInterpreter i)
    {
        i.EnsureStack(3, "WRITE-FILE-BYTES");
        var uFv = i._stack.PopValue();
        var addrFv = i._stack.PopValue();
        var handleFv = i._stack.PopValue();
        if (uFv.Type != ValueType.Long || addrFv.Type != ValueType.Long || handleFv.Type != ValueType.Long)
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE-BYTES expects (handle addr u -- )");
        var u = uFv.AsLong;
        var addr = addrFv.AsLong;
        var handle = handleFv.AsLong;
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative WRITE-FILE-BYTES length");
        if (_openFiles.TryGetValue((int)handle, out var fs))
        {
            var bytes = new byte[u];
            for (long k = 0; k < u; k++)
            {
                i.MemTryGet(addr + k, out var v);
                bytes[k] = (byte)v;
            }
            fs.Write(bytes, 0, (int)u);
            i._stack.Push(ForthValue.FromLong(u));
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file handle");
        }
        return Task.CompletedTask;
    }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> interpret file contents")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    {
        var fname = i.ReadNextTokenOrThrow("Expected filename after INCLUDE");
        if (fname.Length >= 2 && fname[0] == '"' && fname[^1] == '"') fname = fname[1..^1];
        if (!File.Exists(fname)) throw new ForthException(ForthErrorCode.CompileError, $"INCLUDE: file not found: {fname}");
        // Read entire file and evaluate as a single block so bracketed conditionals spanning lines are handled
        var text = await File.ReadAllTextAsync(fname).ConfigureAwait(false);
        // Split into non-empty trimmed lines and evaluate as a single combined line preserving spacing/newlines
        // Use a single EvalAsync call so tokenization can see brackets across original line boundaries
        await i.EvalAsync(text).ConfigureAwait(false);
    }

    [Primitive("LOAD", IsAsync = true, HelpString = "LOAD ( filename -- ) interpret file at runtime")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    {
        i.EnsureStack(1, "LOAD");
        var fObj = i.PopInternal();
        if (fObj is not string fname) throw new ForthException(ForthErrorCode.TypeError, "LOAD expects filename string");
        if (!File.Exists(fname)) throw new ForthException(ForthErrorCode.CompileError, $"LOAD: file not found: {fname}");
        var text = await File.ReadAllTextAsync(fname).ConfigureAwait(false);
        await i.EvalAsync(text).ConfigureAwait(false);
    }
}
