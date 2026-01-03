using System.Collections.Generic;

namespace Forth.Core.Interpreter;

// Partial: parsing and tokenization
public partial class ForthInterpreter
{
    internal string? _currentSource;
    internal string? _refillSource;
    internal string? CurrentSource => _refillSource ?? _currentSource;

    internal long _currentSourceId;
    internal long SourceId => _currentSourceId;

    /// <summary>
    /// Sets the current source ID.
    /// </summary>
    /// <param name="id">The source ID.</param>
    public void SetSourceId(long id) => _currentSourceId = id;

    internal Queue<string>? _parseBuffer;
}
