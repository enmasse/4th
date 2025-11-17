using Forth.Core.Binding;
using Forth.Core.Interpreter;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

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

    // Aggregate all primitives discovered via reflection into a single immutable dictionary
    public static ImmutableDictionary<(string? Module, string Name), ForthInterpreter.Word> Words
    {
        get
        {
            var dict = new Dictionary<(string? Module, string Name), ForthInterpreter.Word>(new KeyComparer());
            foreach (var kv in GroupedWords)
                dict[kv.Key] = kv.Value;
            return dict.ToImmutableDictionary(new KeyComparer());
        }
    }

    private static IReadOnlyDictionary<(string? Module, string Name), ForthInterpreter.Word> GroupedWords { get; } = CreateGroupedWords();

    private static IReadOnlyDictionary<(string? Module, string Name), ForthInterpreter.Word> CreateGroupedWords()
    {
        var d = new Dictionary<(string? Module, string Name), ForthInterpreter.Word>(new KeyComparer());

        // Find all private static methods on this type decorated with PrimitiveAttribute
        var methods = typeof(CorePrimitives).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var m in methods)
        {
            var pa = m.GetCustomAttribute<PrimitiveAttribute>(false);
            if (pa is null) continue;

            // Create a delegate Func<ForthInterpreter, Task>
            var del = (Func<ForthInterpreter, Task>)Delegate.CreateDelegate(typeof(Func<ForthInterpreter, Task>), m);

            // Build Word
            var w = new ForthInterpreter.Word(del)
            {
                Name = pa.Name,
                IsImmediate = pa.IsImmediate
            };

            d[(pa.Module, pa.Name)] = w;
        }

        return d;
    }

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
