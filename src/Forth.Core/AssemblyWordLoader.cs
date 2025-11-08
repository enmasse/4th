using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Forth;

/// <summary>
/// Reflection-based loader for word modules from .NET assemblies.
/// </summary>
public static class AssemblyWordLoader
{
    /// <summary>
    /// Load an assembly from a path and register any discovered <see cref="IForthWordModule"/> implementations.
    /// </summary>
    /// <param name="interpreter">Target interpreter.</param>
    /// <param name="path">Assembly file path.</param>
    /// <returns>Count of modules registered.</returns>
    public static int Load(IForthInterpreter interpreter, string path)
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new ForthException(ForthErrorCode.CompileError, $"Assembly not found: {fullPath}");
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        return RegisterFromAssembly(interpreter, asm);
    }

    /// <summary>
    /// Register modules from an already loaded assembly.
    /// </summary>
    public static int RegisterFromAssembly(IForthInterpreter interpreter, Assembly asm)
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentNullException.ThrowIfNull(asm);
        int count = 0;
        var types = asm.GetTypes().Where(t => !t.IsAbstract && typeof(IForthWordModule).IsAssignableFrom(t));
        foreach (var t in types)
        {
            var moduleName = t.GetCustomAttribute<ForthModuleAttribute>()?.Name;
            var instance = (IForthWordModule)Activator.CreateInstance(t)!;
            if (interpreter is ForthInterpreter impl && !string.IsNullOrWhiteSpace(moduleName))
            {
                // Scope registration into a module
                impl.WithModule(moduleName!, () => instance.Register(interpreter));
            }
            else
            {
                instance.Register(interpreter);
            }
            count++;
        }
        return count;
    }
}
