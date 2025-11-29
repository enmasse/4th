// See https://aka.ms/new-console-template for more information
using Forth.Core;
using Forth.Core.Interpreter;
using System.IO;
using System.Text;
using System.Linq;

Console.WriteLine("Forth REPL - type BYE to exit");
var interp = new ForthInterpreter();
var reader = new LineReader();

// Add REPL-specific words
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
            // Show stack on error
            var stack = interp.Stack;
            if (stack.Count > 0)
            {
                Console.Write("Stack: ");
                Console.WriteLine(string.Join(" ", stack.Select(o => o?.ToString() ?? "null")));
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
        // Show stack on error
        var stack = interp.Stack;
        if (stack.Count > 0)
        {
            Console.Write("Stack: ");
            Console.WriteLine(string.Join(" ", stack.Select(o => o?.ToString() ?? "null")));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

class LineReader
{
    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private readonly StringBuilder _currentLine = new();
    private int _cursorPosition = 0;

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
        // Write current content
        Console.Write(_currentLine.ToString());
        // Position cursor
        if (_cursorPosition < _currentLine.Length)
        {
            Console.Write(new string('\b', _currentLine.Length - _cursorPosition));
        }
    }
}
