using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("HELP", IsImmediate = true)]
    private static Task Prim_HELP(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after HELP");
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
