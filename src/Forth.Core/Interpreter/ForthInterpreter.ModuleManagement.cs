using System.Collections.Immutable;

namespace Forth.Core.Interpreter;

// Partial: module management, using modules, current module
public partial class ForthInterpreter
{
    internal readonly List<string> _usingModules = new(); // internal
    internal string? _currentModule; // internal

    internal void WithModule(string name, Action action)
    {
        var prev = _currentModule;
        _currentModule = name;

        try
        {
            action();
        }
        finally
        {
            _currentModule = prev;
        }
    }

    internal ImmutableList<string?> GetOrder()
    {
        var list = new List<string?>();
        for (int i = _usingModules.Count - 1; i >= 0; i--)
            list.Add(_usingModules[i]);
        list.Add(null);
        return list.ToImmutableList();
    }

    internal void SetOrder(IEnumerable<string?> order)
    {
        _usingModules.Clear();
        foreach (var m in order)
        {
            if (m is null) break;
            if (!string.IsNullOrWhiteSpace(m) && !_usingModules.Contains(m))
                _usingModules.Add(m);
        }
    }
}