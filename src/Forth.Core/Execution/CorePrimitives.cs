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
internal static class CorePrimitives
{
    private static readonly Lazy<ImmutableDictionary<(string? Module, string Name), Word>> _words =
        new(CreateWords);

    public static ImmutableDictionary<(string? Module, string Name), Word> Words =>
        _words.Value;

    private static ImmutableDictionary<(string? Module, string Name), Word> CreateWords()
    {
        var dict = ImmutableDictionary.CreateBuilder<(string? Module, string Name), Word>(new PrimitivesUtil.KeyComparer());

        var asm = typeof(CorePrimitives).Assembly;
        var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

        var seenPrimitives = new Dictionary<(string? Module, string Name), (string TypeName, string MethodName)>(new PrimitivesUtil.KeyComparer());

        foreach (var type in asm.GetTypes())
        {
            if (!type.IsClass)
            {
                continue;
            }

            if (!string.Equals(type.Namespace, typeof(CorePrimitives).Namespace, StringComparison.Ordinal))
            {
                continue;
            }

            var methods = type.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<PrimitiveAttribute>();
                if (attr is null)
                {
                    continue;
                }

                var name = attr.Name;
                var module = attr.Module;
                var key = (module, name);

                if (seenPrimitives.TryGetValue(key, out var existing))
                {
                    throw new InvalidOperationException(
                        $"Duplicate primitive '{name}' declared in '{existing.TypeName}.{existing.MethodName}' and '{type.Name}.{method.Name}'. " +
                        "Each primitive name must be unique within a module.");
                }

                seenPrimitives[key] = (type.Name, method.Name);

                var func = (Func<ForthInterpreter, Task>)Delegate.CreateDelegate(typeof(Func<ForthInterpreter, Task>), method);
                var word = new Word(func)
                {
                    Name = name,
                    Module = module,
                    IsImmediate = attr.IsImmediate,
                    HelpString = attr.HelpString,
                    Category = attr.Category
                };

                dict[key] = word;
            }
        }

        return dict.ToImmutable();
    }
}
