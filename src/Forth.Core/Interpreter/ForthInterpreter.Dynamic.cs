using System;
using System.Reflection;
using Forth.Core.Binding;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter // dynamic module helpers
{
    internal void WithModule(string name, Action action)
    {
        var previous = _currentModule;
        _currentModule = name;
        try { action(); }
        finally { _currentModule = previous; }
    }

    public int LoadAssemblyWords(Assembly asm) => AssemblyWordLoader.RegisterFromAssembly(this, asm);

    public void BeginModuleScope(string moduleName, Action register)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentNullException.ThrowIfNull(register);
        WithModule(moduleName, register);
    }
}
