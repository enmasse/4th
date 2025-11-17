using Forth.Core.Binding;
using Forth.Core.Interpreter;
using System.Collections.Immutable;
using System.Collections.Generic;

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

    // Aggregate all group-specific dictionaries into a single immutable dictionary
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

    // Each partial file will populate this with its own entries
    private static IReadOnlyDictionary<(string? Module, string Name), ForthInterpreter.Word> GroupedWords { get; } = CreateGroupedWords();

    private static IReadOnlyDictionary<(string? Module, string Name), ForthInterpreter.Word> CreateGroupedWords()
    {
        var d = new Dictionary<(string? Module, string Name), ForthInterpreter.Word>(new KeyComparer());
        // call partial methods implemented by group files
        AddArithmeticEntries(d);
        AddComparisonEntries(d);
        AddStackOpsEntries(d);
        AddReturnStackEntries(d);
        AddMemoryEntries(d);
        AddIOFormattingEntries(d);
        AddPicturedNumericEntries(d);
        AddNumericBaseEntries(d);
        AddBitwiseEntries(d);
        AddControlExecutionEntries(d);
        AddCompilationEntries(d);
        AddDictionaryVocabEntries(d);
        AddConcurrencyEntries(d);
        return d;
    }

    // Partial methods implemented in other files to add group-specific entries
    static partial void AddArithmeticEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddComparisonEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddStackOpsEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddReturnStackEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddMemoryEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddIOFormattingEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddPicturedNumericEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddNumericBaseEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddBitwiseEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddControlExecutionEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddCompilationEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddDictionaryVocabEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);
    static partial void AddConcurrencyEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d);

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
