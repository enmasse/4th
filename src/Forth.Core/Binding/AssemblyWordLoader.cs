using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Forth.Core.Modules;
using Forth.Core.Interpreter;

namespace Forth.Core.Binding;

public static class AssemblyWordLoader
{
    public static int Load(Forth.Core.IForthInterpreter interpreter, string path)
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, $"Assembly not found: {fullPath}");
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        return RegisterFromAssembly(interpreter, asm);
    }

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
