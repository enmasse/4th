using System.IO;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Forth.Core.Interpreter;

// Partial: file IO and related diagnostics
public partial class ForthInterpreter
{
    /// <summary>
    /// Table of currently open file handles mapped to their <see cref="FileStream"/> plus related write/read diagnostics state.
    /// </summary>
    private readonly Dictionary<int, FileStream> _openFiles = new();
    private int _nextFileHandle = 1;
    internal int _lastWriteHandle;
    internal byte[]? _lastWriteBuffer;
    internal long _lastWritePositionAfter;
    internal int _lastReadHandle;
    internal byte[]? _lastReadBuffer;
    internal long _lastReadPositionAfter;

    /// <summary>
    /// Modes that a file may be opened with from Forth code.
    /// </summary>
    public enum FileOpenMode
    {
        /// <summary>Open an existing file for reading.</summary>
        Read = 0,
        /// <summary>Create/overwrite a file for writing (position reset to beginning).</summary>
        Write = 1,
        /// <summary>Open or create a file and seek to end for append writes.</summary>
        Append = 2
    }

    /// <summary>
    /// Opens a file at the specified path using the provided mode and returns a numeric handle used by Forth words.
    /// </summary>
    /// <param name="path">File system path.</param>
    /// <param name="mode">Open mode (defaults to <see cref="FileOpenMode.Read"/>).</param>
    /// <param name="create">If true, creates the file (truncating if exists); otherwise opens existing.</param>
    /// <returns>Allocated file handle.</returns>
    /// <exception cref="IOException">Propagated if the underlying file cannot be opened.</exception>
    internal int OpenFileHandle(string path, FileOpenMode mode = FileOpenMode.Read, bool create = false)
    {
        FileStream fs;
        FileMode fm;
        FileAccess fa;
        switch (mode)
        {
            case FileOpenMode.Read:
                fm = create ? FileMode.Create : FileMode.Open;
                fa = FileAccess.Read;
                break;
            case FileOpenMode.Write:
                fm = FileMode.Create;
                fa = FileAccess.ReadWrite;
                break;
            case FileOpenMode.Append:
                fm = create ? FileMode.Create : FileMode.OpenOrCreate;
                fa = FileAccess.ReadWrite;
                break;
            default:
                fm = create ? FileMode.Create : FileMode.Open;
                fa = FileAccess.Read;
                break;
        }
        fs = new FileStream(path, fm, fa, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
        if (mode == FileOpenMode.Append)
            fs.Seek(0, SeekOrigin.End);
        var h = _nextFileHandle++;
        _openFiles[h] = fs;
        return h;
    }

    /// <summary>
    /// Closes a file handle or throws a Forth exception if the handle is invalid.
    /// </summary>
    /// <param name="handle">File handle to close.</param>
    internal void CloseFileHandle(int handle)
    {
        if (!TryCloseFileHandle(handle))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
    }

    /// <summary>
    /// Attempts to close a file handle without throwing.
    /// </summary>
    /// <param name="handle">File handle.</param>
    /// <returns><c>true</c> if the handle was found and closed; otherwise <c>false</c>.</returns>
    internal bool TryCloseFileHandle(int handle)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            return false;
        try { fs.Close(); } catch { }
        _openFiles.Remove(handle);
        return true;
    }

    /// <summary>
    /// Repositions an open file stream to an absolute offset.
    /// </summary>
    /// <param name="handle">File handle.</param>
    /// <param name="offset">Absolute byte offset within the file.</param>
    /// <exception cref="ForthException">Thrown if handle invalid or offset out of range.</exception>
    internal void RepositionFileHandle(int handle, long offset)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (offset < 0 || offset > fs.Length)
            throw new ForthException(ForthErrorCode.CompileError, "Invalid file offset");
        fs.Seek(offset, SeekOrigin.Begin);
    }

    /// <summary>
    /// Gets the current position in an open file stream.
    /// </summary>
    /// <param name="handle">File handle.</param>
    /// <returns>Current position in bytes.</returns>
    /// <exception cref="ForthException">Thrown if handle invalid.</exception>
    internal long GetFilePosition(int handle)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        return fs.Position;
    }

    /// <summary>
    /// Reads up to <paramref name="count"/> bytes from the file into interpreter linear memory starting at <paramref name="addr"/>.
    /// Diagnostics for last read are stored for tooling.
    /// </summary>
    /// <param name="handle">File handle referenced.</param>
    /// <param name="addr">Target memory address.</param>
    /// <param name="count">Maximum number of bytes to read.</param>
    /// <returns>Number of bytes actually read.</returns>
    /// <exception cref="ForthException">Thrown for invalid handle or non-readable file.</exception>
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

    /// <summary>
    /// Writes <paramref name="count"/> bytes from interpreter memory starting at <paramref name="addr"/> into a file.
    /// Diagnostics for last write are captured.
    /// </summary>
    /// <param name="handle">File handle.</param>
    /// <param name="addr">Source memory address.</param>
    /// <param name="count">Number of bytes to write.</param>
    /// <returns>Number of bytes written (same as <paramref name="count"/> when successful).</returns>
    /// <exception cref="ForthException">Thrown for invalid handle or non-writable file.</exception>
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

    /// <summary>
    /// Writes a string to an open file handle.
    /// Diagnostics for last write are captured.
    /// </summary>
    /// <param name="handle">File handle.</param>
    /// <param name="s">String to write.</param>
    /// <exception cref="ForthException">Thrown for invalid handle or non-writable file.</exception>
    internal void WriteStringToFile(int handle, string s)
    {
        if (!_openFiles.TryGetValue(handle, out var fs))
            throw new ForthException(ForthErrorCode.CompileError, $"Invalid file handle: {handle}");
        if (!fs.CanWrite) throw new ForthException(ForthErrorCode.CompileError, "File not open for writing");
        var bytes = Encoding.UTF8.GetBytes(s);
        fs.Write(bytes, 0, bytes.Length);
        try { fs.Flush(true); } catch { fs.Flush(); }
        _lastWriteHandle = handle;
        _lastWriteBuffer = bytes;
        _lastWritePositionAfter = fs.Position;
    }

    /// <summary>
    /// Opens a file at the specified path using the provided file access method bits and returns a numeric handle used by Forth words.
    /// </summary>
    /// <param name="filename">File system path.</param>
    /// <param name="fam">File access method bits.</param>
    /// <param name="truncate">If true, truncates the file if it exists.</param>
    /// <returns>Allocated file handle.</returns>
    /// <exception cref="IOException">Propagated if the underlying file cannot be opened.</exception>
    internal int OpenFileHandle(string filename, long fam, bool truncate = false)
    {
        FileStream fs;
        // fam
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
        fs = new FileStream(filename, fmode, faccess);
        var handle = ++_nextFileHandle;
        _openFiles[handle] = fs;
        return handle;
    }
}