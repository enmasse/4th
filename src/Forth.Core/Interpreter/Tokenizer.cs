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

            // Support C-style // line comments: skip to end-of-line
            if (c == '/' && i + 1 < input.Length && input[i + 1] == '/')
            {
                // flush any pending token
                if (current.Count > 0) { list.Add(new string(current.ToArray())); current.Clear(); }
                // advance to end-of-line (\n or \r) or end of input
                i += 2; // past //
                while (i < input.Length && input[i] != '\n' && input[i] != '\r') i++;
                // The for-loop will increment i; adjust so we continue at newline
                i--;
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
                
                // Otherwise, it's a comment
                inComment = true;
                continue;
            }
            if (c == '\\') break; // line comment
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

            if (c == '[' || c == ']' || c == '\'')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add(c.ToString());
                continue;
            }

            current.Add(c);
        }
        if (current.Count > 0)
            list.Add(new string(current.ToArray()));
        return list;
    }
}
