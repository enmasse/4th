using System;
using System.Text;
using Forth.Core.Execution;

namespace Forth.Core.Interpreter;

/// <summary>
/// Provides character-level parsing for ANS Forth compliance.
/// Replaces the token-based parser to eliminate synchronization issues.
/// </summary>
internal class CharacterParser
{
    private readonly string _source;
    private int _position;

    public CharacterParser(string source, int startPosition = 0)
    {
        _source = source ?? string.Empty;
        _position = startPosition;
    }

    /// <summary>
    /// Gets the current parse position.
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Gets the source string.
    /// </summary>
    public string Source => _source;

    /// <summary>
    /// Returns true if at end of source.
    /// </summary>
    public bool IsAtEnd => _position >= _source.Length;

    /// <summary>
    /// Sets the parse position.
    /// </summary>
    public void SetPosition(int position)
    {
        _position = Math.Max(0, Math.Min(position, _source.Length));
    }

    /// <summary>
    /// Skips whitespace characters.
    /// </summary>
    public void SkipWhitespace()
    {
        while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
        {
            _position++;
        }
    }

    /// <summary>
    /// Skips to end of line.
    /// </summary>
    public void SkipToEndOfLine()
    {
        while (_position < _source.Length && _source[_position] != '\n' && _source[_position] != '\r')
        {
            _position++;
        }
    }

    /// <summary>
    /// Skips a parenthetical comment: ( ... )
    /// Advances past the closing ')'.
    /// </summary>
    public void SkipParenComment()
    {
        if (_position >= _source.Length || _source[_position] != '(')
            return;

        _position++; // skip '('
        while (_position < _source.Length && _source[_position] != ')')
        {
            _position++;
        }
        if (_position < _source.Length && _source[_position] == ')')
        {
            _position++; // skip ')'
        }
    }

    /// <summary>
    /// Parses a word delimited by the specified delimiter character.
    /// Used by the WORD primitive.
    /// Returns the parsed word and updates position.
    /// </summary>
    public string ParseWord(char delimiter)
    {
        // Skip leading delimiters
        while (_position < _source.Length && _source[_position] == delimiter)
        {
            _position++;
        }

        var start = _position;

        // Collect characters until delimiter or end
        while (_position < _source.Length && _source[_position] != delimiter)
        {
            _position++;
        }

        var word = _source.Substring(start, _position - start);

        // Advance past the delimiter if present
        if (_position < _source.Length && _source[_position] == delimiter)
        {
            _position++;
        }

        return word;
    }

    /// <summary>
    /// Parses the next whitespace-delimited word.
    /// Handles comments and special tokens.
    /// Returns null if no more words.
    /// </summary>
    public string? ParseNext()
    {
        SkipWhitespace();

        if (IsAtEnd)
            return null;

        var start = _position;
        var ch = _source[_position];

        // Handle backslash comment
        if (ch == '\\')
        {
            SkipToEndOfLine();
            return ParseNext(); // Recursively get next token
        }

        // Handle C-style // comment (for IL blocks)
        if (ch == '/' && _position + 1 < _source.Length && _source[_position + 1] == '/')
        {
            SkipToEndOfLine();
            return ParseNext();
        }

        // Handle parenthetical comment
        if (ch == '(')
        {
            // Check for special (LOCAL) token
            if (_position + 7 <= _source.Length &&
                _source.Substring(_position, 7).Equals("(LOCAL)", StringComparison.OrdinalIgnoreCase))
            {
                _position += 7;
                return "(LOCAL)";
            }

            SkipParenComment();
            return ParseNext();
        }

        // Handle .( ... ) immediate print token
        if (ch == '.' && _position + 1 < _source.Length && _source[_position + 1] == '(')
        {
            _position += 2; // skip ".("
            var textBuilder = new StringBuilder();
            while (_position < _source.Length && _source[_position] != ')')
            {
                textBuilder.Append(_source[_position]);
                _position++;
            }
            if (_position < _source.Length && _source[_position] == ')')
            {
                _position++; // skip ')'
            }
            return ".(" + textBuilder.ToString() + ")";
        }

        // Handle ." string literal
        if (ch == '.' && _position + 1 < _source.Length && _source[_position + 1] == '"')
        {
            _position += 2; // skip '."'
            // Skip optional leading whitespace
            while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
            {
                _position++;
            }
            var textBuilder = new StringBuilder();
            textBuilder.Append('"');
            while (_position < _source.Length && _source[_position] != '"')
            {
                textBuilder.Append(_source[_position]);
                _position++;
            }
            if (_position < _source.Length && _source[_position] == '"')
            {
                textBuilder.Append('"');
                _position++; // skip closing '"'
            }
            return ".\"";  // Return the prefix, string will be consumed by primitive
        }

        // Handle S" string literal (case-insensitive)
        if ((ch == 'S' || ch == 's') && _position + 1 < _source.Length && _source[_position + 1] == '"')
        {
            _position += 2; // skip 'S"' or 's"'
            // Skip at most one leading space
            if (_position < _source.Length && _source[_position] == ' ')
            {
                _position++;
            }
            var textBuilder = new StringBuilder();
            textBuilder.Append('"');
            while (_position < _source.Length && _source[_position] != '"')
            {
                textBuilder.Append(_source[_position]);
                _position++;
            }
            if (_position < _source.Length && _source[_position] == '"')
            {
                textBuilder.Append('"');
                _position++; // skip closing '"'
            }
            return "S\"";  // Return normalized uppercase, string will be consumed by primitive
        }

        // Handle ABORT" (case-insensitive)
        if (_position + 6 <= _source.Length)
        {
            var prefix = _source.Substring(_position, 5);
            if (prefix.Equals("ABORT", StringComparison.OrdinalIgnoreCase) &&
                _source[_position + 5] == '"')
            {
                _position += 6; // skip 'ABORT"'
                // Skip optional leading whitespace
                while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
                {
                    _position++;
                }
                var textBuilder = new StringBuilder();
                textBuilder.Append('"');
                while (_position < _source.Length && _source[_position] != '"')
                {
                    textBuilder.Append(_source[_position]);
                    _position++;
                }
                if (_position < _source.Length && _source[_position] == '"')
                {
                    textBuilder.Append('"');
                    _position++; // skip closing '"'
                }
                return "ABORT\"";
            }
        }

        // Handle bracket conditionals as composite tokens
        if (ch == '[')
        {
            // [']
            if (_position + 2 < _source.Length &&
                _source[_position + 1] == '\'' &&
                _source[_position + 2] == ']')
            {
                _position += 3;
                return "[']";
            }

            // [IF]
            if (_position + 3 < _source.Length &&
                _source.Substring(_position + 1, 2).Equals("IF", StringComparison.OrdinalIgnoreCase) &&
                _source[_position + 3] == ']')
            {
                _position += 4;
                return "[IF]";
            }

            // [ELSE], [THEN]
            if (_position + 5 < _source.Length)
            {
                var inner = _source.Substring(_position + 1, 4);
                if (inner.Equals("ELSE", StringComparison.OrdinalIgnoreCase) &&
                    _source[_position + 5] == ']')
                {
                    _position += 6;
                    return "[ELSE]";
                }
                if (inner.Equals("THEN", StringComparison.OrdinalIgnoreCase) &&
                    _source[_position + 5] == ']')
                {
                    _position += 6;
                    return "[THEN]";
                }
            }
        }

        // Handle quoted strings
        if (ch == '"')
        {
            var textBuilder = new StringBuilder();
            textBuilder.Append('"');
            _position++; // skip opening '"'
            while (_position < _source.Length && _source[_position] != '"')
            {
                textBuilder.Append(_source[_position]);
                _position++;
            }
            if (_position < _source.Length && _source[_position] == '"')
            {
                textBuilder.Append('"');
                _position++; // skip closing '"'
            }
            return textBuilder.ToString();
        }

        // Handle semicolon as separate token
        if (ch == ';')
        {
            _position++;
            return ";";
        }

        // Parse regular word (non-whitespace sequence)
        while (_position < _source.Length && !char.IsWhiteSpace(_source[_position]))
        {
            _position++;
        }

        return _source.Substring(start, _position - start);
    }

    /// <summary>
    /// Peeks at the next character without advancing position.
    /// Returns '\0' if at end.
    /// </summary>
    public char Peek()
    {
        return _position < _source.Length ? _source[_position] : '\0';
    }

    /// <summary>
    /// Reads and returns the next character, advancing position.
    /// Returns '\0' if at end.
    /// </summary>
    public char ReadChar()
    {
        if (_position >= _source.Length)
            return '\0';
        return _source[_position++];
    }

    /// <summary>
    /// Reads characters until the specified terminator is found.
    /// Returns the substring and advances past the terminator.
    /// </summary>
    public string ReadUntil(char terminator)
    {
        var start = _position;
        while (_position < _source.Length && _source[_position] != terminator)
        {
            _position++;
        }
        var result = _source.Substring(start, _position - start);
        if (_position < _source.Length && _source[_position] == terminator)
        {
            _position++; // advance past terminator
        }
        return result;
    }
}
