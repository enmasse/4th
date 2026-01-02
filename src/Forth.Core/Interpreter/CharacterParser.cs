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
    private string? _nextToken; // Buffered token for multi-token constructs (S", .", ABORT")

    public CharacterParser(string source, int startPosition = 0)
    {
        _source = source ?? string.Empty;
        _position = startPosition;
        _nextToken = null;
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
    /// Positions are clamped to valid range [0, source.Length].
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
    /// Skips to end of source (for bracket conditional skip mode).
    /// </summary>
    public void SkipToEnd()
    {
        _position = _source.Length;
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
        // Check if we have a buffered token from a multi-token construct
        if (_nextToken != null)
        {
            var token = _nextToken;
            _nextToken = null;
            return token;
        }

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

            // Return .( as the directive token, and buffer the payload as a quoted token
            // so the `.(` primitive can consume it consistently.
            _nextToken = "\"" + textBuilder.ToString() + "\"";
            return ".(";
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
            // FIXED: Buffer the string token for next ParseNext() call
            _nextToken = textBuilder.ToString();
            return ".\"";
        }

        // Handle S" string literal (case-insensitive)
        if ((ch == 'S' || ch == 's') && _position + 1 < _source.Length && _source[_position + 1] == '"')
        {
            _position += 2; // skip 'S"' or 's"'
            // Preserve all characters after S" as part of the literal, including leading whitespace.
            // ANS Forth treats everything up to the closing quote as the string payload.
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
            _nextToken = textBuilder.ToString();
            return "S\"";
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
                // FIXED: Buffer the string token for next ParseNext() call
                _nextToken = textBuilder.ToString();
                return "ABORT\"";
            }
        }

        // Handle bracket conditionals as composite tokens
        // Supports both compact ([IF]) and separated ([ IF ]) forms
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

            // Try to parse separated forms first: [ IF ], [ ELSE ], [ THEN ]
            // Save position for potential backtrack
            var savedPos = _position;
            _position++; // skip '['
            
            // Skip optional whitespace after '['
            while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
            {
                _position++;
            }
            
            // Check for IF/ELSE/THEN keyword
            string? keyword = null;
            int keywordLength = 0;
            
            if (_position + 2 <= _source.Length)
            {
                var twoChar = _source.Substring(_position, Math.Min(2, _source.Length - _position));
                if (twoChar.Equals("IF", StringComparison.OrdinalIgnoreCase))
                {
                    keyword = "IF";
                    keywordLength = 2;
                }
            }
            
            if (keyword == null && _position + 4 <= _source.Length)
            {
                var fourChar = _source.Substring(_position, Math.Min(4, _source.Length - _position));
                if (fourChar.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
                {
                    keyword = "ELSE";
                    keywordLength = 4;
                }
                else if (fourChar.Equals("THEN", StringComparison.OrdinalIgnoreCase))
                {
                    keyword = "THEN";
                    keywordLength = 4;
                }
            }
            
            // If we found a keyword, check for closing ]
            if (keyword != null)
            {
                var afterKeyword = _position + keywordLength;
                
                // Skip optional whitespace after keyword
                while (afterKeyword < _source.Length && char.IsWhiteSpace(_source[afterKeyword]))
                {
                    afterKeyword++;
                }
                
                // Check for closing ]
                if (afterKeyword < _source.Length && _source[afterKeyword] == ']')
                {
                    // Success! This is a separated form bracket conditional
                    _position = afterKeyword + 1; // consume through ]
                    return $"[{keyword}]";
                }
            }
            
            // Not a separated form, backtrack and try compact forms
            _position = savedPos + 1; // restore position after '['
            
            // [IF] (compact form)
            if (_position + 2 < _source.Length &&
                _source.Substring(_position, 2).Equals("IF", StringComparison.OrdinalIgnoreCase) &&
                _source[_position + 2] == ']')
            {
                _position += 3;
                return "[IF]";
            }

            // [ELSE], [THEN] (compact forms)
            if (_position + 4 < _source.Length)
            {
                var inner = _source.Substring(_position, 4);
                if (inner.Equals("ELSE", StringComparison.OrdinalIgnoreCase) &&
                    _source[_position + 4] == ']')
                {
                    _position += 5;
                    return "[ELSE]";
                }
                if (inner.Equals("THEN", StringComparison.OrdinalIgnoreCase) &&
                    _source[_position + 4] == ']')
                {
                    _position += 5;
                    return "[THEN]";
                }
            }
            
            // Not a recognized bracket conditional, backtrack completely
            _position = savedPos;
            // Fall through to parse '[' as a regular token
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
    public char PeekChar()
    {
        return _position < _source.Length ? _source[_position] : '\0';
    }

    /// <summary>
    /// Peeks at the next word/token without consuming it.
    /// Returns null if no more words available.
    /// This is essential for immediate parsing words like S", .", CREATE, etc.
    /// that need to look ahead at the next token without consuming it.
    /// </summary>
    public string? PeekNext()
    {
        // Save current position
        var savedPosition = _position;
        var savedNextToken = _nextToken;
        
        // Parse next token
        var token = ParseNext();
        
        // Restore position and buffered token
        _position = savedPosition;
        _nextToken = savedNextToken;
        
        return token;
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

    static int FromHexDigit(char c)
    {
        if (c is >= '0' and <= '9') return c - '0';
        if (c is >= 'a' and <= 'f') return 10 + (c - 'a');
        if (c is >= 'A' and <= 'F') return 10 + (c - 'A');
        return -1;
    }
}
