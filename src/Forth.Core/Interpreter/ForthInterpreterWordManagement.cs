using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterWordManagement
{
    private readonly ForthInterpreter _i;

    public ForthInterpreterWordManagement(ForthInterpreter i)
    {
        _i = i;
    }

    internal void RegisterDefinition(string name) =>
        _i._definitions.Add(new ForthInterpreter.DefinitionRecord(name, _i._currentModule));

    internal void ForgetWord(string token)
    {
        string? mod = null;
        string name = token;

        var cidx = token.IndexOf(':');
        if (cidx > 0)
        {
            mod = token[..cidx];
            name = token[(cidx + 1)..];
        }

        for (int idx = _i._definitions.Count - 1; idx >= 0; idx--)
        {
            var def = _i._definitions[idx];
            if (!name.Equals(def.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            if (mod != null && !string.Equals(mod, def.Module, StringComparison.OrdinalIgnoreCase))
                continue;

            if (idx < _i._baselineCount)
                throw new ForthException(ForthErrorCode.CompileError, "Cannot FORGET core word");

            for (int j = _i._definitions.Count - 1; j >= idx; j--)
            {
                var d = _i._definitions[j];
                var key = (d.Module, d.Name);
                if (_i._dict.ContainsKey(key))
                {
                    _i._dict = _i._dict.Remove(key);
                }

                _i._decompile.Remove(d.Name);
                _i._deferred.Remove(d.Name);
                _i._values.Remove(d.Name);
            }

            _i._definitions.RemoveRange(idx, _i._definitions.Count - idx);
            _i._lastDefinedWord = null;
            return;
        }

        throw new ForthException(ForthErrorCode.UndefinedWord, $"FORGET target not found: {token}");
    }

    internal void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _i._currentModule };
        _i._dict = _i._dict.SetItem((_i._currentModule, name), w);
        RegisterDefinition(name);
    }

    internal void AddWordAsync(string name, Func<IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _i._currentModule };
        _i._dict = _i._dict.SetItem((_i._currentModule, name), w);
        RegisterDefinition(name);
    }

    internal bool TryResolveWord(string token, out Word? word)
    {
        var idx = token.IndexOf(':');
        if (idx > 0)
        {
            var mod = token[..idx];
            var wname = token[(idx + 1)..];
            if (_i._dict.TryGetValue((mod, wname), out var wq))
            {
                word = wq;
                return true;
            }

            word = null;
            return false;
        }

        if (token.StartsWith("VOCAB", StringComparison.OrdinalIgnoreCase)
            && _i._dict.TryGetValue((null, token), out var wv))
        {
            word = wv;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(_i._currentModule)
            && _i._dict.TryGetValue((_i._currentModule, token), out var wc))
        {
            word = wc;
            return true;
        }

        for (int i = _i._usingModules.Count - 1; i >= 0; i--)
        {
            var mn = _i._usingModules[i];
            if (_i._dict.TryGetValue((mn, token), out var wu))
            {
                word = wu;
                return true;
            }
        }

        if (_i._dict.TryGetValue((null, token), out var wdef))
        {
            word = wdef;
            return true;
        }

        word = null;
        return false;
    }

    internal IEnumerable<string> GetAllWordNames()
    {
        var names = new List<string>();
        foreach (var kv in _i._dict)
        {
            var w = kv.Value;

            if (w is null)
                continue;

            if (w.IsHidden)
                continue;

            var n = w.Name ?? kv.Key.Name;

            if (string.IsNullOrWhiteSpace(kv.Key.Module))
            {
                names.Add(n);
            }
            else
            {
                names.Add($"{kv.Key.Module}:{n}");
            }
        }

        names.Sort(StringComparer.OrdinalIgnoreCase);
        return names;
    }
}
