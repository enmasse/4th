namespace Forth;

/// <summary>
/// Tokenizes a single line of Forth source into whitespace-delimited tokens, supporting
/// inline comments with "( ... )" and line comments starting with backslash ("\\").
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// Split input into tokens. Semicolons are returned as standalone tokens to simplify
    /// end-of-definition handling.
    /// </summary>
    /// <param name="input">Single line of Forth source.</param>
    /// <returns>List of token strings in order.</returns>
    public static List<string> Tokenize(string input)
    {
        var list = new List<string>();
        var current = new List<char>();
        bool inComment = false;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
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
            current.Add(c);
        }
        if (current.Count > 0)
            list.Add(new string(current.ToArray()));
        return list;
    }
}
