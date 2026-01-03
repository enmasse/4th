using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterModuleManagement
{
    private readonly ForthInterpreter _i;

    public ForthInterpreterModuleManagement(ForthInterpreter i)
    {
        _i = i;
    }

    internal void WithModule(string name, Action action)
    {
        var prev = _i._currentModule;
        _i._currentModule = name;

        try
        {
            action();
        }
        finally
        {
            _i._currentModule = prev;
        }
    }

    internal ImmutableList<string?> GetOrder()
    {
        var list = new List<string?>();
        for (int i = _i._usingModules.Count - 1; i >= 0; i--)
            list.Add(_i._usingModules[i]);
        list.Add(null);
        return list.ToImmutableList();
    }

    internal void SetOrder(IEnumerable<string?> order)
    {
        _i._usingModules.Clear();
        foreach (var m in order)
        {
            if (m is null) break;
            if (!string.IsNullOrWhiteSpace(m) && !_i._usingModules.Contains(m))
                _i._usingModules.Add(m);
        }
    }
}
