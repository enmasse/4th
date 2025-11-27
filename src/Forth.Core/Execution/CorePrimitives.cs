using Forth.Core.Interpreter;
using System.Collections.Immutable;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Forth.Core.Execution;

[SuppressMessage(
    category: "Style",
    checkId: "IDE0051:Remove unused private members",
    Justification = "Primitives are discovered and invoked via reflection by CreateWords() scanning [Primitive] attributes.")]
[UnconditionalSuppressMessage(
    category: "Style",
    checkId: "IDE0051:Remove unused private members",
    Justification = "Primitives are discovered and invoked via reflection by CreateWords() scanning [Primitive] attributes.")]
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

    private static readonly Lazy<ImmutableDictionary<(string? Module, string Name), Word>> _words =
        new(CreateWords);

    public static ImmutableDictionary<(string? Module, string Name), Word> Words =>
        _words.Value;

    // Helpers used across groups
    private static long ToLong(object v) => ForthInterpreter.ToLong(v);

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

    private static ImmutableDictionary<(string? Module, string Name), Word> CreateWords()
    {
        var dict = ImmutableDictionary.CreateBuilder<(string? Module, string Name), Word>(new KeyComparer());
        var primType = typeof(CorePrimitives);
        var methods = primType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<PrimitiveAttribute>();
            if (attr is not null)
            {
                var name = attr.Name;
                var module = attr.Module;
                var func = (Func<ForthInterpreter, Task>)Delegate.CreateDelegate(typeof(Func<ForthInterpreter, Task>), method);
                var word = new Word(func);
                word.Name = name;
                word.Module = module;
                word.IsImmediate = attr.IsImmediate;
                word.HelpString = attr.HelpString;
                dict[(module, name)] = word;
            }
        }
        return dict.ToImmutable();
    }
}
