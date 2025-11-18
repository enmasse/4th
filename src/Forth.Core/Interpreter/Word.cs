using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

internal class Word
{
    private readonly Func<ForthInterpreter, Task> _run;
    public bool IsAsync { get; }
    public bool IsImmediate { get; set; }
    public List<string>? BodyTokens { get; set; }
    public string? Name { get; set; }
    public string? Module { get; set; }
    public bool IsHidden { get; set; }
    public string? HelpString { get; set; }

    public Word(Action<ForthInterpreter> sync)
    {
        _run = intr =>
        {
            sync(intr);
            return Task.CompletedTask;
        };

        IsAsync = false;
    }

    public Word(Func<ForthInterpreter, Task> asyncRun)
    {
        _run = asyncRun;
        IsAsync = true;
    }

    public Task ExecuteAsync(ForthInterpreter intr = null!) =>
        _run(intr);
}
