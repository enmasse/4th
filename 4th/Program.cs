// See https://aka.ms/new-console-template for more information
using Forth.Core;
using Forth.Core.Interpreter;
using Forth.Core.Execution;
using System.IO;
using System.Text;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Forth REPL - type BYE to exit");
        var interp = new ForthInterpreter();
        var completions = new Dictionary<string, string>();
        foreach (var name in interp.GetAllWordNames())
        {
            completions[name] = GetCategory(name, interp);
        }
        var reader = new LineReader(completions);

        interp.AddWord("STACK", i => {
            var stack = i.Stack;
            if (stack.Count == 0)
                Console.WriteLine("<empty>");
            else
                Console.WriteLine(string.Join(" ", stack.Select(o => o?.ToString() ?? "null")));
        });

        if (args.Length > 0)
        {
            var path = args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine($"Script not found: {path}");
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            foreach (var rawLine in lines)
            {
                var line = rawLine?.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    interp.SetSourceId(0); // user input
                    var keepGoing = await interp.EvalAsync(line);
                    Console.WriteLine(" ok");
                    if (!keepGoing) break;
                }
                catch (ForthException ex)
                {
                    Console.WriteLine($"Error: {ex.Code}: {ex.Message}");
                    Console.WriteLine($"Line: {line}");
                    // Show stack on error
                    var stack = interp.Stack;
                    if (stack.Count > 0)
                    {
                        Console.Write("Stack: ");
                        Console.WriteLine(string.Join(" ", stack.Select(o => o?.ToString() ?? "null")));
                    }
                    // Suggest if unknown word
                    if (ex.Message.Contains("unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        var suggestions = GetSuggestions(line, completions);
                        if (suggestions.Any())
                        {
                            Console.WriteLine($"Did you mean: {string.Join(", ", suggestions)}");
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }
            }

            return;
        }

        while (true)
        {
            Console.Write("> ");
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                interp.SetSourceId(0); // user input
                var keepGoing = await interp.EvalAsync(line);
                Console.WriteLine(" ok");
                if (!keepGoing) break;
            }
            catch (ForthException ex)
            {
                Console.WriteLine($"Error: {ex.Code}: {ex.Message}");
                Console.WriteLine($"Line: {line}");
                // Show stack on error
                var stack = interp.Stack;
                if (stack.Count > 0)
                {
                    Console.Write("Stack: ");
                    Console.WriteLine(string.Join(" ", stack.Select(o => o?.ToString() ?? "null")));
                }
                // Suggest if unknown word
                if (ex.Message.Contains("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    var suggestions = GetSuggestions(line, completions);
                    if (suggestions.Any())
                    {
                        Console.WriteLine($"Did you mean: {string.Join(", ", suggestions)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    class LineReader
    {
        private readonly List<string> _history = new();
        private int _historyIndex = -1;
        private readonly StringBuilder _currentLine = new();
        private int _cursorPosition = 0;
        private readonly IReadOnlyDictionary<string, string> _completions;

        public LineReader(IReadOnlyDictionary<string, string> completions)
        {
            _completions = completions;
            LoadHistory();
        }

        public string ReadLine()
        {
            _currentLine.Clear();
            _cursorPosition = 0;
            _historyIndex = -1;

            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        var line = _currentLine.ToString();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _history.Add(line);
                            SaveHistory();
                        }
                        return line;

                    case ConsoleKey.Backspace:
                        if (_cursorPosition > 0)
                        {
                            _currentLine.Remove(_cursorPosition - 1, 1);
                            _cursorPosition--;
                            RedrawLine();
                        }
                        break;

                    case ConsoleKey.UpArrow:
                        if (_history.Count > 0)
                        {
                            if (_historyIndex == -1)
                            {
                                _historyIndex = _history.Count - 1;
                            }
                            else if (_historyIndex > 0)
                            {
                                _historyIndex--;
                            }
                            _currentLine.Clear();
                            _currentLine.Append(_history[_historyIndex]);
                            _cursorPosition = _currentLine.Length;
                            RedrawLine();
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (_historyIndex >= 0)
                        {
                            _historyIndex++;
                            if (_historyIndex >= _history.Count)
                            {
                                _historyIndex = -1;
                                _currentLine.Clear();
                                _cursorPosition = 0;
                            }
                            else
                            {
                                _currentLine.Clear();
                                _currentLine.Append(_history[_historyIndex]);
                                _cursorPosition = _currentLine.Length;
                            }
                            RedrawLine();
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (_cursorPosition > 0)
                        {
                            _cursorPosition--;
                            Console.Write("\b");
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (_cursorPosition < _currentLine.Length)
                        {
                            _cursorPosition++;
                            Console.Write(_currentLine[_cursorPosition - 1]);
                        }
                        break;

                    case ConsoleKey.Tab:
                        HandleTabCompletion();
                        break;

                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            _currentLine.Insert(_cursorPosition, key.KeyChar);
                            _cursorPosition++;
                            RedrawLine();
                        }
                        break;
                }
            }
        }

        private void RedrawLine()
        {
            // Clear current line
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r> ");
            // Write current content with highlighting
            WriteHighlightedLine(_currentLine.ToString());
            // Position cursor
            if (_cursorPosition < _currentLine.Length)
            {
                Console.Write(new string('\b', _currentLine.Length - _cursorPosition));
            }
        }

        private void WriteHighlightedLine(string line)
        {
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int pos = 0;
            foreach (var token in tokens)
            {
                // Skip spaces
                while (pos < line.Length && char.IsWhiteSpace(line[pos])) { Console.Write(line[pos]); pos++; }
                if (pos >= line.Length) break;

                string color = "\e[0m"; // default
                if (_completions.TryGetValue(token, out var cat))
                {
                    color = cat switch
                    {
                        "Arithmetic" => "\e[36m", // cyan
                        "Stack" => "\e[33m", // yellow
                        "Control" => "\e[35m", // magenta
                        "IO" => "\e[32m", // green
                        _ => "\e[34m" // blue for other keywords
                    };
                }
                else if (int.TryParse(token, out _) || double.TryParse(token, out _))
                    color = "\e[32m"; // green for numbers
                else if (token.StartsWith('"') || token.EndsWith('"'))
                    color = "\e[31m"; // red for strings

                Console.Write(color + token + "\e[0m");
                pos += token.Length;
            }
            // Write remaining spaces
            while (pos < line.Length) { Console.Write(line[pos]); pos++; }
        }

        private void HandleTabCompletion()
        {
            // Find the start of the current word
            int wordStart = _cursorPosition;
            while (wordStart > 0 && char.IsWhiteSpace(_currentLine[wordStart - 1]))
            {
                wordStart--;
            }
            var currentWord = _currentLine.ToString(wordStart, _cursorPosition - wordStart);
            if (string.IsNullOrEmpty(currentWord)) return;

            var matches = _completions.Keys.Where(c => c.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0) return;

            // Find common prefix
            var commonPrefix = matches[0];
            for (int i = 1; i < matches.Count; i++)
            {
                commonPrefix = GetCommonPrefix(commonPrefix, matches[i]);
                if (string.IsNullOrEmpty(commonPrefix)) break;
            }

            if (commonPrefix.Length > currentWord.Length)
            {
                // Complete to common prefix
                _currentLine.Remove(wordStart, _cursorPosition - wordStart);
                _currentLine.Insert(wordStart, commonPrefix);
                _cursorPosition = wordStart + commonPrefix.Length;
                RedrawLine();
            }
            else if (matches.Count == 1)
            {
                // Exact match, add space
                _currentLine.Append(' ');
                _cursorPosition++;
                RedrawLine();
            }
            // If multiple and no more common, do nothing or cycle (for now, nothing)
        }

        private static string GetCommonPrefix(string a, string b)
        {
            int minLen = Math.Min(a.Length, b.Length);
            for (int i = 0; i < minLen; i++)
            {
                if (a[i] != b[i]) return a.Substring(0, i);
            }
            return a.Substring(0, minLen);
        }

        private void SaveHistory()
        {
            try
            {
                File.WriteAllLines(".forth_history", _history);
            }
            catch { } // ignore
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(".forth_history"))
                {
                    _history.AddRange(File.ReadAllLines(".forth_history"));
                }
            }
            catch { } // ignore
        }
    }

    private static List<string> GetSuggestions(string line, IReadOnlyDictionary<string, string> completions)
    {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var suggestions = new List<string>();
        foreach (var word in words)
        {
            if (!completions.ContainsKey(word))
            {
                var closest = completions.Keys
                    .Select(c => (word: c, dist: LevenshteinDistance(word, c)))
                    .Where(x => x.dist <= 2) // max 2 edits
                    .OrderBy(x => x.dist)
                    .ThenBy(x => x.word)
                    .Take(3)
                    .Select(x => x.word)
                    .ToList();
                suggestions.AddRange(closest);
            }
        }
        return suggestions.Distinct().ToList();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        int lenA = a.Length;
        int lenB = b.Length;
        var matrix = new int[lenA + 1, lenB + 1];

        for (int i = 0; i <= lenA; i++) matrix[i, 0] = i;
        for (int j = 0; j <= lenB; j++) matrix[0, j] = j;

        for (int i = 1; i <= lenA; i++)
        {
            for (int j = 1; j <= lenB; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }
        return matrix[lenA, lenB];
    }

    private static string GetCategory(string name, ForthInterpreter interp)
    {
        // For now, return "Other" since categories not set
        return "Other";
    }
}
