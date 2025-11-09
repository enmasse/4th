using System;
using System.Reflection;

namespace Forth;

public partial class ForthInterpreter // split to add dynamic module helpers
{
    internal void WithModule(string name, Action action)
    {
        var previous = _currentModule;
        _currentModule = name;
        try { action(); }
        finally { _currentModule = previous; }
    }

    /// <summary>
    /// Load words from a loaded assembly (C# API) using reflection module discovery.
    /// Returns number of modules registered.
    /// </summary>
    public int LoadAssemblyWords(Assembly asm) => AssemblyWordLoader.RegisterFromAssembly(this, asm);

    /// <summary>
    /// Temporarily begin a module scope for programmatic word registration.
    /// </summary>
    public void BeginModuleScope(string moduleName, Action register)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentNullException.ThrowIfNull(register);
        WithModule(moduleName, register);
    }
}
