using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: snapshot (MARKER) support
public partial class ForthInterpreter
{
    internal sealed class MarkerSnapshot
    {
        public ImmutableDictionary<(string? Module, string Name), Word> Dict { get; }
        public ImmutableList<string> UsingModules { get; }
        public ImmutableDictionary<string, long> Values { get; }
        public ImmutableDictionary<long, long> Memory { get; }
        public ImmutableDictionary<string, Word?> Deferred { get; }
        public ImmutableDictionary<string, string> Decompile { get; }
        public ImmutableArray<(string Name, string? Module)> Definitions { get; }
        public long NextAddr { get; }
        public MarkerSnapshot(
            ImmutableDictionary<(string? Module, string Name), Word> dict,
            ImmutableList<string> usingModules,
            ImmutableDictionary<string, long> values,
            ImmutableDictionary<long, long> memory,
            ImmutableDictionary<string, Word?> deferred,
            ImmutableDictionary<string, string> decompile,
            ImmutableArray<(string Name, string? Module)> definitions,
            long nextAddr)
        {
            Dict = dict; UsingModules = usingModules; Values = values; Memory = memory; Deferred = deferred; Decompile = decompile; Definitions = definitions; NextAddr = nextAddr;
        }
    }

    internal MarkerSnapshot CreateMarkerSnapshot()
    {
        var dict = _dict.ToImmutableDictionary();
        var usingMods = _usingModules.ToImmutableList();
        var values = _values.ToImmutableDictionary(System.StringComparer.OrdinalIgnoreCase);
        var memory = _mem.ToImmutableDictionary();
        var deferred = _deferred.ToImmutableDictionary(System.StringComparer.OrdinalIgnoreCase);
        var decompile = _decompile.ToImmutableDictionary(System.StringComparer.OrdinalIgnoreCase);
        var defs = _definitions.Select(d => (d.Name, d.Module)).ToImmutableArray();
        return new MarkerSnapshot(dict, usingMods, values, memory, deferred, decompile, defs, _nextAddr);
    }

    internal ForthInterpreter(MarkerSnapshot snapshot, IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0;
        long baseValue = 10;
        if (snapshot.Memory.TryGetValue(snapshot.Memory.Keys.FirstOrDefault(k => k == 2), out var snapBase))
            baseValue = snapBase;
        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = baseValue;
        _dict = snapshot.Dict;
        _loadPrelude = Task.CompletedTask;
        _usingModules.AddRange(snapshot.UsingModules);
        foreach (var kvp in snapshot.Values) _values[kvp.Key] = kvp.Value;
        var maxSnapAddr = snapshot.NextAddr;
        if (maxSnapAddr >= _nextAddr) _nextAddr = maxSnapAddr + 1;
        foreach (var kvp in snapshot.Memory)
            if (kvp.Key != _stateAddr && kvp.Key != _baseAddr) _mem[kvp.Key] = kvp.Value;
        foreach (var kvp in snapshot.Deferred) _deferred[kvp.Key] = kvp.Value;
        foreach (var kvp in snapshot.Decompile) _decompile[kvp.Key] = kvp.Value;
        foreach (var (name, module) in snapshot.Definitions)
            _definitions.Add(new(name, module));
    }

    internal void RestoreSnapshot(MarkerSnapshot snap)
    {
        _isCompiling = false;
        _currentDefName = null;
        _currentInstructions = null;
        _currentDefTokens = null;
        _controlStack.Clear();
        _doesCollecting = false;
        _doesTokens = null;
        _lastCreatedName = null;
        _lastCreatedAddr = 0;
        _tokens = null;
        _tokenIndex = 0;
        _dict = snap.Dict;
        _usingModules.Clear();
        _usingModules.AddRange(snap.UsingModules);
        _values.Clear();
        foreach (var kv in snap.Values) _values[kv.Key] = kv.Value;
        _deferred.Clear();
        foreach (var kv in snap.Deferred) _deferred[kv.Key] = kv.Value;
        _decompile.Clear();
        foreach (var kv in snap.Decompile) _decompile[kv.Key] = kv.Value;
        _mem.Clear();
        foreach (var kv in snap.Memory) _mem[kv.Key] = kv.Value;
        _mem[_stateAddr] = 0;
        _nextAddr = snap.NextAddr;
        _definitions.Clear();
        foreach (var (name, module) in snap.Definitions) _definitions.Add(new(name, module));
        _lastDefinedWord = null;
        if (_definitions.Count > 0)
        {
            var last = _definitions[^1];
            _dict.TryGetValue((last.Module, last.Name), out _lastDefinedWord);
        }
    }
}