using System.Collections.Immutable;

namespace Forth.Core.Interpreter;

// Partial: module management, using modules, current module
public partial class ForthInterpreter
{
    internal readonly List<string> _usingModules = new();
    internal string? _currentModule;
}