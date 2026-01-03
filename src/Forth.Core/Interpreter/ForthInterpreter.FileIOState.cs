using System.IO;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter
{
    // File IO state kept on the interpreter for diagnostics and for tests that use reflection.
    internal readonly System.Collections.Generic.Dictionary<int, FileStream> _openFiles = new();
    internal int _nextFileHandle = 1;
}
