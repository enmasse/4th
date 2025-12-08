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

    // DEPRECATED: Token-based parsing (being replaced by character-based parsing)
    // These fields are kept temporarily for backward compatibility during migration
    internal List<string>? _tokens; // internal current token stream
    internal int _tokenIndex;       // internal current token index
    internal List<int>? _tokenCharPositions; // character position of each token in source

    // Character-based parsing helpers (NEW - replaces token-based parsing)
    /// <summary>
    /// Tries to parse the next word from the current source using character-based parsing.
    /// Returns false if no more words available.
    /// Automatically updates >IN as characters are consumed.
    /// </summary>
    internal bool TryParseNextWord(out string word)
    {
        if (_parser == null || _parser.IsAtEnd)
        {
            word = string.Empty;
            return false;
        }

        // Synchronize parser position with >IN (in case >IN was modified externally)
        MemTryGet(_inAddr, out var inVal);
        var inPos = (int)ToLong(inVal);
        if (_parser.Position != inPos)
        {
            _parser.SetPosition(inPos);
        }

        var result = _parser.ParseNext();
        if (result == null)
        {
            word = string.Empty;
            return false;
        }

        // Update >IN to reflect consumed characters
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

    // DEPRECATED: Token-based parsing (kept for backward compatibility during migration)
    // Token reading helpers used by primitives and source-level operations
    internal bool TryReadNextToken(out string token)
    {
        // Check if >IN has been modified externally (e.g., by >IN ! to skip input)
        MemTryGet(_inAddr, out var inVal);
        var inPos = (int)ToLong(inVal);
        
        // If >IN points to or past the end of source, no more tokens
        if (_currentSource != null && inPos >= _currentSource.Length)
        {
            token = string.Empty;
            return false;
        }
        
        if (_tokens is null || _tokenIndex >= _tokens.Count)
        {
            token = string.Empty;
            return false;
        }
        
        // Skip tokens that start before the current >IN position
        // This handles both WORD-based character parsing and >IN ! manipulation
        while (_tokenCharPositions != null && _tokenIndex < _tokenCharPositions.Count)
        {
            var tokenStartPos = _tokenCharPositions[_tokenIndex];
            if (tokenStartPos >= inPos)
            {
                // This token starts at or after >IN, so it's valid
                break;
            }
            // This token was consumed or skipped, advance past it
            _tokenIndex++;
            if (_tokenIndex >= _tokens.Count)
            {
                token = string.Empty;
                return false;
            }
        }
        
        // Get the token
        token = _tokens[_tokenIndex++];
        
        // NOTE: We do NOT advance >IN here during normal token-based parsing
        // >IN is only meaningful for character-based parsing (like WORD primitive)
        // When WORD is called, it will synchronize >IN with its character-level parsing
        // and update _tokenIndex to match. This keeps the two parsing modes in sync
        // without breaking immediate words like S" that expect tokens to be available.
        
        return true;
    }

    internal string ReadNextTokenOrThrow(string message)
    {
        if (!TryReadNextToken(out var t) || string.IsNullOrEmpty(t))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return t;
    }

    /// <summary>
    /// Compute character positions of tokens in source line for synchronization with WORD primitive.
    /// </summary>
    private List<int>? ComputeTokenPositions(string source, List<string>? tokens)
    {
        if (tokens == null || tokens.Count == 0)
            return null;
        
        var positions = new List<int>();
        int searchStart = 0;
        
        foreach (var token in tokens)
        {
            // Find this token in the source starting from searchStart
            int pos = source.IndexOf(token, searchStart, StringComparison.Ordinal);
            if (pos >= 0)
            {
                positions.Add(pos);
                searchStart = pos + token.Length;
            }
            else
            {
                // Token not found (shouldn't happen), use approximate position
                positions.Add(searchStart);
            }
        }
        
        return positions;
    }

    // Public method for REFILL to set the current source
    /// <summary>
    /// Refills the current input source with the specified line, resetting the input index.
    /// </summary>
    /// <param name="line">The new input line.</param>
    public void RefillSource(string line)
    {
        // Store refill buffer separately so subsequent EvalAsync calls do not
        // overwrite the refill source when they set _currentSource for their
        // own command text. >IN is reset to 0 for the new refill buffer.
        _refillSource = line;
        _mem[_inAddr] = 0;
    }
}
