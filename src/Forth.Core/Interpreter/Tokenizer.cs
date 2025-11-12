using System;
using System.Collections.Generic;

namespace Forth.Core.Interpreter;

/// <summary>
/// Tokenizes a single line of Forth source into whitespace-delimited tokens, supporting
/// inline comments with "( ... )" and line comments starting with backslash ("\\").
/// Also supports quoted string tokens using double quotes ("...").
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// Split input into tokens. Semicolons are returned as standalone tokens to simplify
    /// end-of-definition handling.
    /// </summary>
    /// <param name="input">A single line of Forth source (no trailing newline required).</param>
    /// <returns>List of token strings in order (may be empty).</returns>
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
            // Special-case ." as a single token
            if (c == '.' && i + 1 < input.Length && input[i + 1] == '"')
            {
                if (current.Count > 0)
                {
                    list.Add(new string(current.ToArray()));
                    current.Clear();
                }
                list.Add(".\"");
                i++; // skip the opening '"'
                // enter string mode but skip any whitespace immediately after the quote
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
            current.Add(c);
        }
        if (current.Count > 0)
            list.Add(new string(current.ToArray()));
        return list;
    }
}
