namespace Forth.Core.Interpreter;

// Facade: keep `ForthInterpreter` FileIO helpers available for existing call sites,
// but delegate implementation to `ForthInterpreterFileIO`.
public partial class ForthInterpreter
{
    internal int OpenFileHandle(string path, FileOpenMode mode = FileOpenMode.Read, bool create = false) =>
        _fileIo.OpenFileHandle(path, mode, create);

    internal int OpenFileHandle(string filename, long fam, bool truncate = false) =>
        _fileIo.OpenFileHandle(filename, fam, truncate);

    internal void CloseFileHandle(int handle) =>
        _fileIo.CloseFileHandle(handle);

    internal bool TryCloseFileHandle(int handle) =>
        _fileIo.TryCloseFileHandle(handle);

    internal void RepositionFileHandle(int handle, long offset) =>
        _fileIo.RepositionFileHandle(handle, offset);

    internal long GetFilePosition(int handle) =>
        _fileIo.GetFilePosition(handle);

    internal int ReadFileIntoMemory(int handle, long addr, int count) =>
        _fileIo.ReadFileIntoMemory(handle, addr, count);

    internal int WriteMemoryToFile(int handle, long addr, int count) =>
        _fileIo.WriteMemoryToFile(handle, addr, count);

    internal void WriteStringToFile(int handle, string s) =>
        _fileIo.WriteStringToFile(handle, s);
}
