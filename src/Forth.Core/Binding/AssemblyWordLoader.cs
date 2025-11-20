using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Forth.Core.Modules;
using Forth.Core.Interpreter;

namespace Forth.Core.Binding;

/// <summary>
/// Helper to load and register Forth word modules from CLR assemblies.
/// Scans an assembly for types implementing <see cref="Forth.Core.Modules.IForthWordModule"/> and
/// invokes their <c>Register</c> method to add words to the interpreter.
/// </summary>
public static class AssemblyWordLoader
{
    /// <summary>
    /// Load an assembly from disk and register any found Forth modules with the interpreter.
    /// </summary>
    /// <param name="interpreter">Interpreter to register words into.</param>
    /// <param name="path">Path to the assembly to load.</param>
    /// <returns>The number of modules registered from the assembly.</returns>
    public static int Load(Forth.Core.IForthInterpreter interpreter, string path)
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, $"Assembly not found: {fullPath}");
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        return RegisterFromAssembly(interpreter, asm);
    }

    /// <summary>
    /// Register all <see cref="Forth.Core.Modules.IForthWordModule"/> implementations found in the provided assembly.
    /// </summary>
    /// <param name="interpreter">Interpreter to register words into.</param>
    /// <param name="asm">Assembly to scan for modules.</param>
    /// <returns>The number of modules registered.</returns>
    public static int RegisterFromAssembly(Forth.Core.IForthInterpreter interpreter, Assembly asm)
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
