using System.Collections.Generic;

namespace Forth.Core.Interpreter;

// Partial: parsing and tokenization
public partial class ForthInterpreter
{
    // Current source tracking (line and index within it)
    internal string? _currentSource;
    // Refill buffer set by REFILL primitive (takes precedence over current source)
    internal string? _refillSource;
    internal string? CurrentSource => _refillSource ?? _currentSource;

    internal long _currentSourceId;
    internal long SourceId => _currentSourceId;

    /// <summary>
    /// Sets the current source ID.
    /// </summary>
    /// <param name="id">The source ID.</param>
    public void SetSourceId(long id) => _currentSourceId = id;

    // Parse buffer for immediate word body expansion (character parser feature)
    internal Queue<string>? _parseBuffer;

    // Character-based parsing helpers (NEW - replaces token-based parsing)
    /// <summary>
    /// Tries to parse the next word from the current source using character-based parsing.
    /// Returns false if no more words available.
    /// Automatically updates >IN as characters are consumed.
    /// Checks parse buffer first (for immediate word body expansion).
    /// </summary>
    internal bool TryParseNextWord(out string word)
    {
        // Check parse buffer first (for immediate word body expansion)
        if (_parseBuffer != null && _parseBuffer.Count > 0)
        {
            word = _parseBuffer.Dequeue();
            // Parse buffer words don't advance >IN (they're pre-parsed)
            return true;
        }
        
        if (_parser == null)
        {
            word = string.Empty;
            return false;
        }
        
        // IMPORTANT: Don't check IsAtEnd here! The parser might have a buffered token
        // even if it's at the end of the source. Let ParseNext() handle this.

        // Synchronize parser position with >IN before parsing
        // Allow forward skipping (>IN ahead of parser) but prevent backward rewinding
        // This enables >IN ! to skip input but prevents infinite loops from negative >IN
        MemTryGet(_inAddr, out var inVal);
        var inPos = (int)ToLong(inVal);
        var currentPos = _parser.Position;
        
        // Synchronize if:
        // 1. >IN is ahead (forward skip via >IN !)
        // 2. At start of line (currentPos == 0, allow any >IN including negative)
        if (inPos > currentPos || currentPos == 0)
        {
            _parser.SetPosition(Math.Max(0, inPos)); // Clamp negative to 0 for parsing
        }
        // Otherwise keep current parser position (don't rewind during active parse)

        var result = _parser.ParseNext();
        if (result == null)
        {
            word = string.Empty;
            return false;
        }

        // Update >IN to reflect consumed characters
        // This keeps >IN synchronized with parse position for read operations
        _mem[_inAddr] = (long)_parser.Position;
        
        word = result;
        return true;
    }

    /// <summary>
    /// Parses the next word or throws an exception if none available.
    /// </summary>
    internal string ParseNextWordOrThrow(string message)
    {
        if (!TryParseNextWord(out var word) || string.IsNullOrEmpty(word))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return word;
    }

    internal string ReadNextTokenOrThrow(string message)
    {
        if (!TryParseNextWord(out var word) || string.IsNullOrEmpty(word))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return word;
    }

    // Public method for REFILL to set the current source
    /// <summary>
    /// Refills the current input source with the specified line, resetting the input index.
    /// </summary>
    /// <param name="line">The new input line.</param>
    public void RefillSource(string line)
    {
        // Store refill buffer - next EvalAsync will consume it as the parse source
        _refillSource = line;
        _mem[_inAddr] = 0;
    }
}
