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
    // Helpers used across groups
    internal static long ToLong(object o)
    {
        return o switch
        {
            long l => l,
            int ii => ii,
            short s => s,
            byte b => b,
            double d => (long)d,
            char c => c,
            bool bo => bo ? -1 : 0,
            string str => str.Length > 0 ? (long)str[0] : throw new ForthException(ForthErrorCode.TypeError, "Empty string for number conversion"),
            Word w when w.BodyAddr.HasValue => w.BodyAddr.Value,
            _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {o?.GetType().Name ?? "null"}")
        };
    }

    internal static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        double d => d != 0.0,
        float f => f != 0.0f,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };

    internal static bool IsNumeric(object o) => o is long || o is int || o is short || o is byte || o is double || o is char || o is bool;

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

    private static ImmutableDictionary<(string? Module, string Name), Word> CreateWords()
    {
        var dict = ImmutableDictionary.CreateBuilder<(string? Module, string Name), Word>(new KeyComparer());

        var asm = typeof(CorePrimitives).Assembly;
        var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

        var seenPrimitives = new Dictionary<(string? Module, string Name), (string TypeName, string MethodName)>(new KeyComparer());

        foreach (var type in asm.GetTypes())
        {
            if (!type.IsClass)
            {
                continue;
            }

            // Only consider primitive containers. Today primitives live under the Execution namespace.
            // This keeps scanning tight and avoids accidental pickup from unrelated tooling types.
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

                // Check for duplicate primitive declarations (unique within a module)
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

    [Primitive("ALLOCATE", HelpString = "ALLOCATE ( u -- a-addr ior ) - allocate u bytes of memory")]
    private static Task Prim_Allocate(ForthInterpreter i)
    {
        i.EnsureStack(1, "ALLOCATE");
        var u = ToLong(i.PopInternal());
        if (u < 0)
        {
            i.Push(0L); // undefined addr
            i.Push(-1L); // error
            return Task.CompletedTask;
        }
        var bytes = new byte[u];
        var addr = i._heapPtr;
        i._heapAllocations[addr] = (bytes, u);
        i._heapPtr += u;
        i.Push(addr);
        i.Push(0L);
        return Task.CompletedTask;
    }

    [Primitive("FREE", HelpString = "FREE ( a-addr -- ior ) - deallocate memory at a-addr")]
    private static Task Prim_Free(ForthInterpreter i)
    {
        i.EnsureStack(1, "FREE");
        var addr = ToLong(i.PopInternal());
        if (i._heapAllocations.Remove(addr))
        {
            i.Push(0L);
        }
        else
        {
            i.Push(-1L); // not allocated
        }
        return Task.CompletedTask;
    }

    [Primitive("RESIZE", HelpString = "RESIZE ( a-addr1 u -- a-addr2 ior ) - resize allocated memory")]
    private static Task Prim_Resize(ForthInterpreter i)
    {
        i.EnsureStack(2, "RESIZE");
        var u = ToLong(i.PopInternal());
        var addr1 = ToLong(i.PopInternal());
        if (u < 0)
        {
            i.Push(0L);
            i.Push(-1L);
            return Task.CompletedTask;
        }
        if (!i._heapAllocations.TryGetValue(addr1, out var oldAlloc))
        {
            i.Push(0L);
            i.Push(-1L);
            return Task.CompletedTask;
        }
        var (oldBytes, oldSize) = oldAlloc;
        var newBytes = new byte[u];
        var addr2 = i._heapPtr;
        i._heapAllocations[addr2] = (newBytes, u);
        i._heapPtr += u;
        var copySize = Math.Min(oldSize, u);
        Array.Copy(oldBytes, newBytes, copySize);
        i._heapAllocations.Remove(addr1);
        i.Push(addr2);
        i.Push(0L);
        return Task.CompletedTask;
    }
}
