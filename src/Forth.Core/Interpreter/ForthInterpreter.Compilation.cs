using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: compilation, definition handling
public partial class ForthInterpreter
{
    internal bool _isCompiling; // made internal
    internal string? _currentDefName; // internal
    internal List<Func<ForthInterpreter, Task>>? _currentInstructions; // internal
    internal readonly Stack<CompileFrame> _controlStack = new(); // internal

    // Definition and compilation state fields
    internal string? _lastCreatedName;
    internal long _lastCreatedAddr;
    internal bool _doesCollecting;
    internal List<string>? _doesTokens;
    internal List<string>? _currentDefTokens;
    internal Dictionary<string, Word?> _deferred = new(StringComparer.OrdinalIgnoreCase);
    internal Dictionary<string, string> _decompile = new(StringComparer.OrdinalIgnoreCase);
    internal Word? _lastDefinedWord;
    internal string? _lastDeferred;

    internal List<Func<ForthInterpreter, Task>> CurrentList() =>
        _controlStack.Count == 0 ? _currentInstructions! : _controlStack.Peek().GetCurrentList();

    // Begin a new definition named `name` (called by : primitive)
    internal void BeginDefinition(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ForthException(ForthErrorCode.CompileError, "Invalid name for definition");
        if (_isCompiling) throw new ForthException(ForthErrorCode.CompileError, "Nested definitions not supported");
        _isCompiling = true;
        _mem[_stateAddr] = 1;
        _currentDefName = name;
        _currentInstructions = new List<Func<ForthInterpreter, Task>>();
        _currentDefTokens = new List<string>();
    }

    // Finish current definition (called by ; primitive)
    internal void FinishDefinition()
    {
        if (!_isCompiling || string.IsNullOrEmpty(_currentDefName)) throw new ForthException(ForthErrorCode.CompileError, "; without matching :");
        var name = _currentDefName!;
        var instrs = _currentInstructions ?? new List<Func<ForthInterpreter, Task>>();
        var bodyTokens = _currentDefTokens != null ? new List<string>(_currentDefTokens) : null;

        // Create word that executes collected instruction delegates sequentially
        var word = new Word(async ii =>
        {
            foreach (var a in instrs)
                await a(ii).ConfigureAwait(false);
        }) { Name = name, Module = _currentModule, BodyTokens = bodyTokens };

        _dict = _dict.SetItem((_currentModule, name), word);

        // Store decompiled source for SEE/." etc.
        var decompText = bodyTokens is null || bodyTokens.Count == 0
            ? $": {name} ;"
            : $": {name} {string.Join(' ', bodyTokens)} ;";
        _decompile[name] = decompText;

        RegisterDefinition(name);
        _lastDefinedWord = word;

        // Reset compilation state
        _isCompiling = false;
        _mem[_stateAddr] = 0;
        _currentDefName = null;
        _currentInstructions = null;
        _currentDefTokens = null;
        _currentLocals = null;
    }
}