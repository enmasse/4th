using System;

namespace Forth.Core.Interpreter;

// Partial: file IO
public partial class ForthInterpreter
{
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
}