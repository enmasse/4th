namespace Forth;

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
                    // end string
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
            if (c == '(')
            {
                inComment = true;
                continue;
            }
            if (c == '\\') // line comment
            {
                // stop tokenizing rest of the line unless we are in a quoted string
                break;
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
                    // flush previous token then start string
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
