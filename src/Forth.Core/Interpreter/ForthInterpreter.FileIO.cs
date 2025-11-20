using System.IO;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System;

namespace Forth.Core.Interpreter;

// Partial: file IO and related diagnostics
public partial class ForthInterpreter
{
    // File handle table and diagnostics (moved from main file)
    private readonly Dictionary<int, FileStream> _openFiles = new();
    private int _nextFileHandle = 1;
    internal int _lastWriteHandle;
    internal byte[]? _lastWriteBuffer;
    internal long _lastWritePositionAfter;
    internal int _lastReadHandle;
    internal byte[]? _lastReadBuffer;
    internal long _lastReadPositionAfter;

    public enum FileOpenMode
    {
        Read = 0,
        Write = 1,
        Append = 2
    }

    internal int OpenFileHandle(string path, FileOpenMode mode = FileOpenMode.Read)
    {
        FileStream fs;
        switch (mode)
        {
            case FileOpenMode.Read:
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                break;
            case FileOpenMode.Write:
                fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                fs.Seek(0, SeekOrigin.Begin);
                break;
            case FileOpenMode.Append:
                fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                fs.Seek(0, SeekOrigin.End);
                break;
            default:
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                break;
        }
        var h = _nextFileHandle++;
        _openFiles[h] = fs;
        return h;
    }

    internal void CloseFileHandle(int handle)
    {
        if (!TryCloseFileHandle(handle))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
    }

    internal bool TryCloseFileHandle(int handle)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            return false;
        try { fs.Flush(); fs.Dispose(); } catch { }
        _openFiles.Remove(handle);
        return true;
    }

    internal void RepositionFileHandle(int handle, long offset)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (offset < 0 || offset > fs.Length)
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file offset");
        fs.Seek(offset, SeekOrigin.Begin);
    }

    internal int ReadFileIntoMemory(int handle, long addr, int count)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanRead) throw new ForthException(ForthErrorCode.CompileError, "File not open for reading");
        if (count <= 0) return 0;
        var buffer = new byte[count];
        int read = fs.Read(buffer, 0, count);
        for (int i = 0; i < read; i++)
            _mem[addr + i] = buffer[i];
        _lastReadHandle = handle;
        _lastReadBuffer = buffer.Take(read).ToArray();
        _lastReadPositionAfter = fs.Position;
        return read;
    }

    internal int WriteMemoryToFile(int handle, long addr, int count)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanWrite) throw new ForthException(ForthErrorCode.CompileError, "File not open for writing");
        if (count <= 0) return 0;
        var buffer = new byte[count];
        for (int i = 0; i < count; i++)
        {
            MemTryGet(addr + i, out var v);
            buffer[i] = (byte)v;
        }
        fs.Write(buffer, 0, count);
        try { fs.Flush(true); } catch { fs.Flush(); }
        _lastWriteHandle = handle;
        _lastWriteBuffer = buffer;
        _lastWritePositionAfter = fs.Position;
        return count;
    }
}