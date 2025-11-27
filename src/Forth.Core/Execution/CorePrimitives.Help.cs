using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("HELP", IsImmediate = true, HelpString = "HELP <name> - show help text for a word if available, or general help if no name given")]
    private static Task Prim_HELP(ForthInterpreter i)
    {
        string? name = null;
        try
        {
            name = i.ReadNextTokenOrThrow("Expected name after HELP");
        }
        catch (ForthException)
        {
            // No token provided, show general help
            i.WriteText("Forth interpreter commands:\n  WORDS - list all words\n  HELP - show this help\n  HELP <word> - show help for specific word\n  BYE - exit\n\nForth syntax: stack-based, postfix notation");
            i.NewLine();
            return Task.CompletedTask;
        }

        var plain = name;
        var cidx = name.IndexOf(':');
        if (cidx > 0) plain = name[(cidx + 1)..];

        if (i.TryResolveWord(name, out var w) && w is not null)
        {
            if (!string.IsNullOrEmpty(w.HelpString))
            {
                i.WriteText(w.HelpString);
            }
            else
            {
                i.WriteText($"No help available for {plain}");
            }

            i.NewLine();
            return Task.CompletedTask;
        }

        i.WriteText($"No help available for {plain}");
        i.NewLine();
        return Task.CompletedTask;
    }
}
