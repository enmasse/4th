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

    private readonly Stack<CompilationContext> _compilationStack = new();

    private sealed class CompilationContext
    {
        public bool IsCompiling;
        public string? CurrentDefName;
        public List<Func<ForthInterpreter, Task>>? CurrentInstructions;
        public List<string>? CurrentDefTokens;
        public List<string>? CurrentLocals;
    }

    internal List<Func<ForthInterpreter, Task>> CurrentList() =>
        _controlStack.Count == 0 ? _currentInstructions! : _controlStack.Peek().GetCurrentList();

    // Begin a new definition named `name` (called by : primitive)
    internal void BeginDefinition(string? name)
    {
        if (name != null && string.IsNullOrWhiteSpace(name)) throw new ForthException(ForthErrorCode.CompileError, "Invalid name for definition");
        // Allow nesting only for :NONAME (name == null)
        if (name != null && _isCompiling) throw new ForthException(ForthErrorCode.CompileError, "Nested definitions not supported");

        Trace($"BeginDefinition name={(name ?? ":NONAME")} wasCompiling={_isCompiling} csDepth={_compilationStack.Count}");

        // Push current context
        var context = new CompilationContext
        {
            IsCompiling = _isCompiling,
            CurrentDefName = _currentDefName,
            CurrentInstructions = _currentInstructions,
            CurrentDefTokens = _currentDefTokens,
            CurrentLocals = _currentLocals
        };
        _compilationStack.Push(context);

        // Start new definition
        _isCompiling = true;
        _mem[_stateAddr] = 1;
        _currentDefName = name;
        _currentInstructions = new List<Func<ForthInterpreter, Task>>();
        _currentDefTokens = new List<string>();
        _currentLocals = null;

        Trace($"BeginDefinition end name={(name ?? ":NONAME")} isCompiling={_isCompiling} csDepth={_compilationStack.Count}");
    }

    // Finish current definition (called by ; primitive)
    internal void FinishDefinition()
    {
        if (!_isCompiling) throw new ForthException(ForthErrorCode.CompileError, "; without matching :");
        var name = _currentDefName;

        Trace($"FinishDefinition begin name={(name ?? ":NONAME")} csDepth={_compilationStack.Count}");

        var instrs = _currentInstructions ?? new List<Func<ForthInterpreter, Task>>();
        var bodyTokens = _currentDefTokens != null ? new List<string>(_currentDefTokens) : null;

        // If DOES> was used, capture its body and add patching code to the end of instructions
        string? capturedDoesBody = null;
        if (_doesCollecting && _doesTokens is { Count: > 0 })
        {
            capturedDoesBody = string.Join(' ', _doesTokens);
            
            // Add instruction that patches the last CREATEd word with DOES> behavior
            var doesBody = capturedDoesBody; // capture in closure
            instrs.Add(ii =>
            {
                if (string.IsNullOrEmpty(ii._lastCreatedName))
                    throw new ForthException(ForthErrorCode.CompileError, "DOES> used without preceding CREATE");
                    
                var createdName = ii._lastCreatedName;
                var createdAddr = ii._lastCreatedAddr;
                
                // Create new word with DOES> behavior: push address, then execute body
                var newWord = new Word(async intr =>
                {
                    intr.Push(createdAddr);
                    await intr.EvalAsync(doesBody).ConfigureAwait(false);
                })
                {
                    Name = createdName,
                    Module = ii._currentModule,
                    BodyAddr = createdAddr
                };

                // Replace the word in dictionary
                ii._dict = ii._dict.SetItem((ii._currentModule, createdName), newWord);
                ii.RegisterDefinition(createdName);
                ii._lastDefinedWord = newWord;
                
                // Clear _lastCreatedName so this DOES> doesn't affect subsequent CREATEs
                ii._lastCreatedName = null;
                
                return Task.CompletedTask;
            });
        }

        // Clear DOES> state
        _doesCollecting = false;
        _doesTokens = null;

        // Create word that executes collected instruction delegates sequentially
        var word = new Word(async ii =>
        {
            foreach (var a in instrs)
                await a(ii).ConfigureAwait(false);
        }) { Name = name, Module = _currentModule, BodyTokens = bodyTokens };

        if (name != null)
        {
            _dict = _dict.SetItem((_currentModule, name), word);

            // Store decompiled source for SEE/." etc.
            var decompText = bodyTokens is null || bodyTokens.Count == 0
                ? $": {name} ;"
                : $": {name} {string.Join(' ', bodyTokens)} ;";
            _decompile[name] = decompText;

            RegisterDefinition(name);
        }

        _lastDefinedWord = word;

        // For :NONAME, push the word onto the stack
        if (name == null)
        {
            Push(word);
        }

        // Pop and restore context
        if (_compilationStack.Count == 0)
        {
            throw new ForthException(ForthErrorCode.CompileError,
                $"Internal error: compilation stack empty in FinishDefinition (name={name})");
        }

        var context = _compilationStack.Pop();
        _isCompiling = context.IsCompiling;
        _mem[_stateAddr] = _isCompiling ? 1 : 0;
        _currentDefName = context.CurrentDefName;
        _currentInstructions = context.CurrentInstructions;
        _currentDefTokens = context.CurrentDefTokens;
        _currentLocals = context.CurrentLocals;

        Trace($"FinishDefinition end name={(name ?? ":NONAME")} restoredIsCompiling={_isCompiling} csDepth={_compilationStack.Count}");
    }
}