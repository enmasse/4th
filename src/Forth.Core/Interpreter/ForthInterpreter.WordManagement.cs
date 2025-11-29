using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: word management, dictionary operations
public partial class ForthInterpreter
{
    // definition order tracking for classic FORGET
    private readonly List<DefinitionRecord> _definitions = new();
    private int _baselineCount; // protect core/compiler words

    private readonly struct DefinitionRecord
    {
        public readonly string Name;
        public readonly string? Module;
        public DefinitionRecord(string name, string? module)
        {
            Name = name;
            Module = module;
        }
    }

    internal void RegisterDefinition(string name) =>
        _definitions.Add(new DefinitionRecord(name, _currentModule));

    internal void ForgetWord(string token)
    {
        string? mod = null;
        string name = token;

        var cidx = token.IndexOf(':');
        if (cidx > 0)
        {
            mod = token[..cidx];
            name = token[(cidx + 1) ..];
        }

        for (int idx = _definitions.Count - 1; idx >= 0; idx--)
        {
            var def = _definitions[idx];
            if (!name.Equals(def.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (mod != null && !string.Equals(mod, def.Module, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (idx < _baselineCount)
            {
                throw new ForthException(ForthErrorCode.CompileError, "Cannot FORGET core word");
            }

            for (int j = _definitions.Count - 1; j >= idx; j--)
            {
                var d = _definitions[j];
                var key = (d.Module, d.Name);
                if (_dict.ContainsKey(key))
                {
                    _dict = _dict.Remove(key);
                }

                _decompile.Remove(d.Name);
                _deferred.Remove(d.Name);
                _values.Remove(d.Name);
            }

            _definitions.RemoveRange(idx, _definitions.Count - idx);
            _lastDefinedWord = null;
            return;
        }

        throw new ForthException(ForthErrorCode.UndefinedWord, $"FORGET target not found: {token}");
    }

    /// <summary>
    /// Adds a new synchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Delegate executed when the word runs.</param>
    public void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _currentModule };
        _dict = _dict.SetItem((_currentModule, name), w);
        RegisterDefinition(name);
    }

    /// <summary>
    /// Adds a new asynchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Async delegate executed when the word runs.</param>
    public void AddWordAsync(string name, Func<IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        var w = new Word(i => body(i)) { Name = name, Module = _currentModule };
        _dict = _dict.SetItem((_currentModule, name), w);
        RegisterDefinition(name);
    }

    internal bool TryResolveWord(string token, out Word? word)
    {
        var idx = token.IndexOf(':');
        if (idx > 0)
        {
            var mod = token[..idx];
            var wname = token[(idx + 1) ..];
            if (_dict.TryGetValue((mod, wname), out var wq))
            {
                word = wq;
                return true;
            }

            word = null;
            return false;
        }

        // Search current module
        if (!string.IsNullOrWhiteSpace(_currentModule)
            && _dict.TryGetValue((_currentModule, token), out var wc))
        {
            word = wc;
            return true;
        }

        // Search using modules in reverse
        for (int i = _usingModules.Count - 1; i >= 0; i--)
        {
            var mn = _usingModules[i];
            if (_dict.TryGetValue((mn, token), out var wu))
            {
                word = wu;
                return true;
            }
        }

        // Try module-less core dictionary entry
        if (_dict.TryGetValue((null, token), out var wdef))
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
        foreach (var kv in _dict)
        {
            var w = kv.Value;

            if (w is null)
            {
                continue;
            }

            if (w.IsHidden)
            {
                continue;
            }

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