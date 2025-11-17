using Forth.Core.Interpreter;
using System.Collections.Immutable;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    private sealed class KeyComparer : IEqualityComparer<(string? Module, string Name)>
    {
        public bool Equals((string? Module, string Name) x, (string? Module, string Name) y)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            return scomp.Equals(x.Module, y.Module) && scomp.Equals(x.Name, y.Name);
        }

        public int GetHashCode((string? Module, string Name) obj)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            int h1 = obj.Module is null ? 0 : scomp.GetHashCode(obj.Module);
            int h2 = scomp.GetHashCode(obj.Name);
            return (h1 * 397) ^ h2;
        }
    }

    private static readonly Lazy<ImmutableDictionary<(string? Module, string Name), ForthInterpreter.Word>> _words =
        new(CreateWords);

    public static ImmutableDictionary<(string? Module, string Name), ForthInterpreter.Word> Words =>
        _words.Value;

    // Helpers used across groups
    private static long ToLong(object v) => ForthInterpreter.ToLongPublic(v);

    private static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };
}
