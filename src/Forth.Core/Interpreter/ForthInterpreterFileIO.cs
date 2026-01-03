using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterFileIO
{
    private readonly ForthInterpreter _i;

    private int _nextFileHandle = 1;

    public ForthInterpreterFileIO(ForthInterpreter i)
    {
        _i = i;
        _nextFileHandle = i._nextFileHandle;
    }

    internal int OpenFileHandle(string path, ForthInterpreter.FileOpenMode mode = ForthInterpreter.FileOpenMode.Read, bool create = false)
    {
        FileMode fm;
        FileAccess fa;
        switch (mode)
        {
            case ForthInterpreter.FileOpenMode.Read:
                fm = create ? FileMode.Create : FileMode.Open;
                fa = FileAccess.Read;
                break;
            case ForthInterpreter.FileOpenMode.Write:
                fm = FileMode.Create;
                fa = FileAccess.ReadWrite;
                break;
            case ForthInterpreter.FileOpenMode.Append:
                fm = create ? FileMode.Create : FileMode.OpenOrCreate;
                fa = FileAccess.ReadWrite;
                break;
            default:
                fm = create ? FileMode.Create : FileMode.Open;
                fa = FileAccess.Read;
                break;
        }

        var fs = new FileStream(path, fm, fa, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
        if (mode == ForthInterpreter.FileOpenMode.Append)
            fs.Seek(0, SeekOrigin.End);

        var h = _nextFileHandle++;
        _i._nextFileHandle = _nextFileHandle;
        _i._openFiles[h] = fs;
        return h;
    }

    internal int OpenFileHandle(string filename, long fam, bool truncate = false)
    {
        FileMode fmode;
        FileAccess faccess;
        if ((fam & 1) != 0)
        {
            fmode = truncate ? FileMode.Create : FileMode.OpenOrCreate;
            faccess = FileAccess.Write;
        }
        else if ((fam & 2) != 0)
        {
            fmode = FileMode.Open;
            faccess = FileAccess.ReadWrite;
        }
        else
        {
            fmode = FileMode.Open;
            faccess = FileAccess.Read;
        }

        var fs = new FileStream(filename, fmode, faccess);
        var handle = ++_nextFileHandle;
        _i._nextFileHandle = _nextFileHandle;
        _i._openFiles[handle] = fs;
        return handle;
    }

    internal void CloseFileHandle(int handle)
    {
        if (!TryCloseFileHandle(handle))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
    }

    internal bool TryCloseFileHandle(int handle)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            return false;
        try { fs.Close(); } catch { }
        _i._openFiles.Remove(handle);
        return true;
    }

    internal void RepositionFileHandle(int handle, long offset)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (offset < 0 || offset > fs.Length)
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file offset");
        fs.Seek(offset, SeekOrigin.Begin);
    }

    internal long GetFilePosition(int handle)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        return fs.Position;
    }

    internal int ReadFileIntoMemory(int handle, long addr, int count)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanRead) throw new ForthException(ForthErrorCode.CompileError, "File not open for reading");
        if (count <= 0) return 0;

        var buffer = new byte[count];
        int read = fs.Read(buffer, 0, count);
        for (int i = 0; i < read; i++)
            _i._mem[addr + i] = buffer[i];

        _i._lastReadHandle = handle;
        _i._lastReadBuffer = buffer.Take(read).ToArray();
        _i._lastReadPositionAfter = fs.Position;
        return read;
    }

    internal int WriteMemoryToFile(int handle, long addr, int count)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanWrite) throw new ForthException(ForthErrorCode.CompileError, "File not open for writing");
        if (count <= 0) return 0;

        var buffer = new byte[count];
        for (int i = 0; i < count; i++)
        {
            _i.MemTryGet(addr + i, out var v);
            buffer[i] = (byte)v;
        }

        fs.Write(buffer, 0, count);
        try { fs.Flush(true); } catch { fs.Flush(); }

        _i._lastWriteHandle = handle;
        _i._lastWriteBuffer = buffer;
        _i._lastWritePositionAfter = fs.Position;
        return count;
    }

    internal void WriteStringToFile(int handle, string s)
    {
        if (!_i._openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanWrite) throw new ForthException(ForthErrorCode.CompileError, "File not open for writing");

        var bytes = Encoding.UTF8.GetBytes(s);
        fs.Write(bytes, 0, bytes.Length);
        try { fs.Flush(true); } catch { fs.Flush(); }

        _i._lastWriteHandle = handle;
        _i._lastWriteBuffer = bytes;
        _i._lastWritePositionAfter = fs.Position;
    }
}
