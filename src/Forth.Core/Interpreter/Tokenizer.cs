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

            // Handle S" ..." string literal: emit S" then a quoted token with content up to next '"'
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
            if (c == '(')
            {
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
                    list.Add(new string(current.ToArray()));
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
