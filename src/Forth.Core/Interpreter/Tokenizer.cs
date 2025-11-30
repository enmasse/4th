using System;
using System.Collections.Generic;

namespace Forth.Core.Interpreter;

/// <summary>
/// Provides tokenization logic for Forth source lines, handling string literals, comments, and special forms.
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// Splits the input string into Forth tokens, handling quoted strings, comments, and special forms.
    /// </summary>
    /// <param name="input">The source line to tokenize.</param>
    /// <returns>List of tokens as strings.</returns>
    public static List<string> Tokenize(string input)
    {
        var list = new List<string>();
        var current = new List<char>();
        bool inComment = false;
        bool inString = false;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (inString)
            {
                if (c == '"')
                {
                    current.Add('"');
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                    inString = false;
                }
                else
                {
                    current.Add(c);
                }
                continue;
            }
            if (inComment)
            {
                if (c == ')') inComment = false;
                continue;
            }

            // Handle S" ..." string literal
            if (c == 'S' && i + 1 < input.Length && input[i + 1] == '"')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add("S\"");
                i += 2; // move past S"
                // Skip at most one leading space (but preserve newlines or tabs) per Forth convention
                if (i < input.Length && input[i] == ' ') i++;
                var lit = new List<char>();
                while (i < input.Length && input[i] != '"')
                {
                    lit.Add(input[i]);
                    i++;
                }
                if (i >= input.Length)
                    throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "S\" missing closing quote");
                var token = '"' + new string(lit.ToArray()) + '"';
                list.Add(token);
                continue;
            }

            if (c == '[' && i + 2 < input.Length && input[i + 1] == '\'' && input[i + 2] == ']')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add("[']");
                i += 2;
                continue;
            }

            if (c == '.' && i + 1 < input.Length && input[i + 1] == '"')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add(".\"");
                i++; // skip opening '"'
                while (i + 1 < input.Length && char.IsWhiteSpace(input[i + 1])) i++;
                inString = true;
                current.Add('"');
                continue;
            }
            if (c == '.' && i + 3 < input.Length && input[i + 1] == '[' && input[i + 2] == 'S' && input[i + 3] == ']')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add(".[S]");
                i += 3;
                continue;
            }

            if (c == '(')
            {
                // Special case: (LOCAL) as a single token
                if (i + 6 < input.Length && 
                    input.Substring(i, 7).Equals("(LOCAL)", StringComparison.OrdinalIgnoreCase))
                {
                    if (current.Count > 0)
                    {
                        list.Add(new string(current.ToArray()));
                        current.Clear();
                    }
                    list.Add("(LOCAL)");
                    i += 6; // skip past "(LOCAL)"
                    continue;
                }
                
                // Otherwise, it's a comment - save current token and skip to )
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                inComment = true;
                continue;
            }
            if (c == '\\')
            {
                // Backslash comment - rest of line is ignored
                // Save current token first
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                // Skip to end of line or end of input
                while (i + 1 < input.Length && input[i + 1] != '\n' && input[i + 1] != '\r')
                {
                    i++;
                }
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                continue;
            }
            if (c == ';')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add(";");
                continue;
            }
            if (c == '"')
            {
                if (current.Count > 0)
                {
                    var pending = new string(current.ToArray());
                    // Special-case ABORT" to form a single token like ." and S"
                    if (string.Equals(pending, "ABORT", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add("ABORT\"");
                        current.Clear();
                        // begin string accumulation for the following quoted text
                        current.Add('"');
                        // Skip any leading whitespace after the opening quote
                        while (i + 1 < input.Length && char.IsWhiteSpace(input[i + 1])) i++;
                        inString = true;
                        continue;
                    }
                    list.Add(pending);
                    current.Clear();
                }
                current.Add('"');
                inString = true;
                continue;
            }

            current.Add(c);
        }
        if (current.Count > 0)
            list.Add(new string(current.ToArray()));
        
        // Note: We don't need to check if inComment is still true at end of line,
        // because according to ANS Forth, parenthetical comments that don't close
        // on the same line simply remain open until the closing ) is found in
        // subsequent input. However, when tokenizing single lines in isolation,
        // an unclosed comment will simply consume the rest of the line, which is
        // the correct behavior for line-by-line evaluation.
        
        return list;
    }
}
