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
        
        var seenPrimitives = new Dictionary<string, (string MethodName, string? Module)>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<PrimitiveAttribute>();
            if (attr is not null)
            {
                var name = attr.Name;
                var module = attr.Module;
                
                // Check for duplicate primitive declarations
                if (seenPrimitives.TryGetValue(name, out var existing))
                {
                    // Allow same name in different modules
                    if (string.Equals(existing.Module, module, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate primitive '{name}' declared in methods '{existing.MethodName}' and '{method.Name}'. " +
                            $"Each primitive name must be unique within a module.");
                    }
                }
                else
                {
                    seenPrimitives[name] = (method.Name, module);
                }
                
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
}
