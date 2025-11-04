namespace Forth;

public static class Tokenizer
{
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
