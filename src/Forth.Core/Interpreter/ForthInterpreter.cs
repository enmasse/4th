using System; 
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Binding;
using Forth.Core.Execution;
using System.Collections.Immutable; // added

namespace Forth.Core.Interpreter;

public class ForthInterpreter : IForthInterpreter
{
    private readonly ForthStack _stack = new();
    private readonly ForthStack _rstack = new();
    private readonly Dictionary<string, Word> _dict = new(StringComparer.OrdinalIgnoreCase);
    private readonly IForthIO _io;
    private bool _exitRequested;
    private bool _isCompiling;
    private string? _currentDefName;
    private List<Func<ForthInterpreter, Task>>? _currentInstructions;
    private readonly Stack<CompileFrame> _controlStack = new();
    private readonly Dictionary<long,long> _mem = new();
    private long _nextAddr = 1;
    private readonly HashSet<object> _consumedTaskResults = new();

    private readonly Dictionary<string,long> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, Word>> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _usingModules = new();
    private string? _currentModule;

    private readonly ConcurrentDictionary<string, Func<ForthInterpreter, Task>> _ilCache = new();
    private readonly ControlFlowRuntime _controlFlow = new();

    private readonly long _stateAddr;
    internal long StateAddr => _stateAddr;
    private readonly long _baseAddr;
    internal long BaseAddr => _baseAddr;

    private StringBuilder? _picBuf;
    private readonly Dictionary<string, Word?> _deferred = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _decompile = new(StringComparer.OrdinalIgnoreCase);
    private List<string>? _currentDefTokens;
    private Word? _lastDefinedWord;

    // CREATE ... DOES> tracking
    private string? _lastCreatedName;
    private long _lastCreatedAddr;
    private bool _doesCollecting;
    private List<string>? _doesTokens;

    private List<string>? _tokens; // current token stream
    private int _tokenIndex;       // current token index

    // definition order tracking for classic FORGET
    private readonly List<DefinitionRecord> _definitions = new();
    private int _baselineCount; // protect core/compiler words

    // snapshots for potential FORGET implementation later
    private readonly List<ImmutableDictionary<string, Word>> _dictSnapshots = new();
    internal void SnapshotWords() => _dictSnapshots.Add(_dict.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase));

    private readonly struct DefinitionRecord
    {
        public readonly string Name;
        public readonly string? Module;
        public DefinitionRecord(string name, string? module){ Name=name; Module=module; }
    }
    private void RegisterDefinition(string name) => _definitions.Add(new DefinitionRecord(name, _currentModule));

    // Token stream helpers for immediate words
    internal void SetTokenStream(List<string> tokens)
    {
        _tokens = tokens;
        _tokenIndex = 0;
    }
    internal bool TryReadNextToken(out string token)
    {
        token = string.Empty;
        if (_tokens is null) return false;
        while (_tokenIndex < _tokens.Count)
        {
            var t = _tokens[_tokenIndex++];
            if (t.Length == 0) continue;
            token = t;
            return true;
        }
        return false;
    }
    internal string ReadNextTokenOrThrow(string context)
    {
        if (!TryReadNextToken(out var t))
            throw new ForthException(ForthErrorCode.CompileError, context);
        return t;
    }

    // Definition helpers used by immediate words
    internal void BeginDefinition(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ForthException(ForthErrorCode.CompileError, "Invalid word name");
        _currentDefName = name;
        _currentInstructions = new();
        _controlStack.Clear();
        _isCompiling = true;
        _currentDefTokens = new();
        _mem[_stateAddr] = 1;
    }
    internal void FinishDefinition()
    {
        if (_currentInstructions is null || string.IsNullOrEmpty(_currentDefName))
            throw new ForthException(ForthErrorCode.CompileError, "No open definition to end");
        if (_controlStack.Count != 0)
            throw new ForthException(ForthErrorCode.CompileError, "Unmatched control structure");
        var body = _currentInstructions;
        var nameForDecomp = _currentDefName;
        var compiled = new Word(async intr => { try { foreach (var a in body) await a(intr); } catch (ExitWordException) { } });
        compiled.BodyTokens = _currentDefTokens is { Count: > 0 } ? new List<string>(_currentDefTokens) : null;
        compiled.Name = _currentDefName;
        compiled.Module = _currentModule;
        TargetDict()[_currentDefName] = compiled; RegisterDefinition(_currentDefName); _lastDefinedWord = compiled;
        var bodyText = _currentDefTokens is { Count: > 0 } ? string.Join(' ', _currentDefTokens) : string.Empty;
        _decompile[nameForDecomp] = string.IsNullOrEmpty(bodyText) ? $": {nameForDecomp} ;" : $": {nameForDecomp} {bodyText} ;";
        _isCompiling = false; _mem[_stateAddr] = 0; _currentDefName = null; _currentInstructions = null; _currentDefTokens = null;
    }

    public ForthInterpreter(IForthIO? io = null)
    {
        _io = io ?? new ConsoleForthIO();
        _stateAddr = _nextAddr++; _mem[_stateAddr] = 0;
        _baseAddr = _nextAddr++; _mem[_baseAddr] = 10;
        InstallPrimitives();
        SnapshotWords(); // snapshot after primitives (step 1)
        CompilerWordsInstaller.Install(this); // step 2 compiler/defining words via nested installer
        _baselineCount = _definitions.Count; // record baseline for core/compiler words
    }

    // nested static installer for compiler/defining/control-flow words
    private static class CompilerWordsInstaller
    {
        public static void Install(ForthInterpreter intr)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, Word>(StringComparer.OrdinalIgnoreCase);

            // ':' begin definition (immediate)
            builder[":"] = new Word(i => { var name = i.ReadNextTokenOrThrow("Expected name after ':'"); i.BeginDefinition(name); }) { IsImmediate = true, Name = ":" };
            // ';' end definition (immediate)
            builder[";"] = new Word(i => { i.FinishDefinition(); }) { IsImmediate = true, Name = ";" };
            // IMMEDIATE
            builder["IMMEDIATE"] = new Word(i => { if (i._lastDefinedWord is null) throw new ForthException(ForthErrorCode.CompileError, "No recent definition to mark IMMEDIATE"); i._lastDefinedWord.IsImmediate = true; }) { IsImmediate = true, Name = "IMMEDIATE" };
            // POSTPONE
            builder["POSTPONE"] = new Word(async i => {
                var name = i.ReadNextTokenOrThrow("POSTPONE expects a name");
                if (i.TryResolveWord(name, out var wpost) && wpost is not null)
                {
                    if (wpost.IsImmediate) await wpost.ExecuteAsync(i); else i.CurrentList().Add(async ii => await wpost.ExecuteAsync(ii));
                    return;
                }
                switch (name.ToUpperInvariant())
                {
                    case "IF": case "ELSE": case "THEN": case "BEGIN": case "WHILE": case "REPEAT": case "UNTIL":
                    case "DO": case "LOOP": case "LEAVE": case "LITERAL": case "[": case "]": case "'":
                        i._tokens!.Insert(i._tokenIndex, name); return;
                }
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
            }) { IsImmediate = true, Name = "POSTPONE" };
            // ' (tick)
            builder["'"] = new Word(i => { var name = i.ReadNextTokenOrThrow("Expected word after '"); if (!i.TryResolveWord(name, out var wt) || wt is null) throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}"); if (!i._isCompiling) i.Push(wt); else i.CurrentList().Add(ii => { ii.Push(wt); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "'" };
            // LITERAL
            builder["LITERAL"] = new Word(i => { if (!i._isCompiling) throw new ForthException(ForthErrorCode.CompileError, "LITERAL outside compilation"); EnsureStack(i,1,"LITERAL"); var val=i.PopInternal(); i.CurrentList().Add(ii=> { ii.Push(val); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "LITERAL" };
            // [ and ]
            builder["["] = new Word(i => { i._isCompiling=false; i._mem[i._stateAddr]=0; }) { IsImmediate = true, Name = "[" };
            builder["]"] = new Word(i => { i._isCompiling=true; i._mem[i._stateAddr]=1; }) { IsImmediate = true, Name = "]" };
            // Control flow words
            builder["IF"] = new Word(i => { i._controlStack.Push(new IfFrame()); }) { IsImmediate = true, Name = "IF" };
            builder["ELSE"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not IfFrame ifr) throw new ForthException(ForthErrorCode.CompileError,"ELSE without IF"); if(ifr.ElsePart is not null) throw new ForthException(ForthErrorCode.CompileError,"Multiple ELSE"); ifr.ElsePart=new(); ifr.InElse=true; }) { IsImmediate = true, Name = "ELSE" };
            builder["THEN"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not IfFrame ifr) throw new ForthException(ForthErrorCode.CompileError,"THEN without IF"); i._controlStack.Pop(); var thenPart=ifr.ThenPart; var elsePart=ifr.ElsePart; i.CurrentList().Add(async ii=> { EnsureStack(ii,1,"IF"); var flag=ii.PopInternal(); if(ToBool(flag)) foreach(var a in thenPart) await a(ii); else if(elsePart is not null) foreach(var a in elsePart) await a(ii); }); }) { IsImmediate = true, Name = "THEN" };
            builder["BEGIN"] = new Word(i => { i._controlStack.Push(new BeginFrame()); }) { IsImmediate = true, Name = "BEGIN" };
            builder["WHILE"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"WHILE without BEGIN"); if(bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"Multiple WHILE"); bf.InWhile=true; }) { IsImmediate = true, Name = "WHILE" };
            builder["REPEAT"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"REPEAT without BEGIN"); if(!bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"REPEAT requires WHILE"); i._controlStack.Pop(); var pre=bf.PrePart; var mid=bf.MidPart; i.CurrentList().Add(async ii=> { while(true){ foreach(var a in pre) await a(ii); EnsureStack(ii,1,"WHILE"); var flag=ii.PopInternal(); if(!ToBool(flag)) break; foreach(var b in mid) await b(ii);} }); }) { IsImmediate = true, Name = "REPEAT" };
            builder["UNTIL"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"UNTIL without BEGIN"); if(bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"UNTIL after WHILE use REPEAT"); i._controlStack.Pop(); var body=bf.PrePart; i.CurrentList().Add(async ii=> { while(true){ foreach(var a in body) await a(ii); EnsureStack(ii,1,"UNTIL"); var flag=ii.PopInternal(); if(ToBool(flag)) break; } }); }) { IsImmediate = true, Name = "UNTIL" };
            builder["DO"] = new Word(i => { i._controlStack.Push(new DoFrame()); }) { IsImmediate = true, Name = "DO" };
            builder["LOOP"] = new Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not DoFrame df) throw new ForthException(ForthErrorCode.CompileError,"LOOP without DO"); i._controlStack.Pop(); var body=df.Body; i.CurrentList().Add(async ii=> { EnsureStack(ii,2,"DO"); var start=ToLongPublic(ii.Pop()); var limit=ToLongPublic(ii.Pop()); long step=start<=limit?1L:-1L; for(long idx=start; idx!=limit; idx+=step){ ii.PushLoopIndex(idx); try { foreach(var a in body) await a(ii);} catch(LoopLeaveException){ break; } finally { ii.PopLoopIndexMaybe(); } } }); }) { IsImmediate = true, Name = "LOOP" };
            builder["LEAVE"] = new Word(i => { bool inside=false; foreach(var f in i._controlStack) if(f is DoFrame){ inside=true; break; } if(!inside) throw new ForthException(ForthErrorCode.CompileError,"LEAVE outside DO...LOOP"); i.CurrentList().Add(ii=> { throw new LoopLeaveException(); }); }) { IsImmediate = true, Name = "LEAVE" };
            // Defining & meta words
            builder["MODULE"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after MODULE"); i._currentModule=name; if(string.IsNullOrWhiteSpace(i._currentModule)) throw new ForthException(ForthErrorCode.CompileError,"Invalid module name"); if(!i._modules.ContainsKey(i._currentModule)) i._modules[i._currentModule]= new(StringComparer.OrdinalIgnoreCase); }) { IsImmediate = true, Name = "MODULE" };
            builder["END-MODULE"] = new Word(i => { i._currentModule=null; }) { IsImmediate = true, Name = "END-MODULE" };
            builder["USING"] = new Word(i => { var m=i.ReadNextTokenOrThrow("Expected name after USING"); if(!i._usingModules.Contains(m)) i._usingModules.Add(m); }) { IsImmediate = true, Name = "USING" };
            builder["LOAD-ASM"] = new Word(i => { var path=i.ReadNextTokenOrThrow("Expected path after LOAD-ASM"); var count=AssemblyWordLoader.Load(i,path); i.Push((long)count); }) { IsImmediate = true, Name = "LOAD-ASM" };
            builder["LOAD-ASM-TYPE"] = new Word(i => { var tn=i.ReadNextTokenOrThrow("Expected type after LOAD-ASM-TYPE"); Type? t=Type.GetType(tn,false,false); if(t==null) foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){ t=asm.GetType(tn,false,false); if(t!=null) break; } if(t==null) throw new ForthException(ForthErrorCode.CompileError,$"Type not found: {tn}"); var count=i.LoadAssemblyWords(t.Assembly); i.Push((long)count); }) { IsImmediate = true, Name = "LOAD-ASM-TYPE" };
            builder["CREATE"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after CREATE"); if(string.IsNullOrWhiteSpace(name)) throw new ForthException(ForthErrorCode.CompileError,"Invalid name for CREATE"); var addr=i._nextAddr; i._lastCreatedName=name; i._lastCreatedAddr=addr; i.TargetDict()[name]= new Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "CREATE" };
            builder[","] = new Word(i => { EnsureStack(i,1,","); var v=ToLongPublic(i.Pop()); i._mem[i._nextAddr++]=v; }) { IsImmediate = true, Name = "," };
            builder["DOES>"] = new Word(i => { if(string.IsNullOrEmpty(i._lastCreatedName)) throw new ForthException(ForthErrorCode.CompileError,"DOES> without CREATE"); i._doesCollecting=true; i._doesTokens=new List<string>(); }) { IsImmediate = true, Name = "DOES>" };
            builder["ALLOT"] = new Word(i => { EnsureStack(i,1,"ALLOT"); var cells=ToLongPublic(i.Pop()); if(cells<0) throw new ForthException(ForthErrorCode.CompileError,"Negative ALLOT size"); for(long k=0;k<cells;k++) i._mem[i._nextAddr++]=0; }) { IsImmediate = true, Name = "ALLOT" };
            builder["VARIABLE"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr=i._nextAddr++; i._mem[addr]=0; i.TargetDict()[name]= new Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "VARIABLE" };
            builder["CONSTANT"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after CONSTANT"); EnsureStack(i,1,"CONSTANT"); var val=i.PopInternal(); i.TargetDict()[name]= new Word(ii=> ii.Push(val)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "CONSTANT" };
            builder["VALUE"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after VALUE"); if(!i._values.ContainsKey(name)) i._values[name]=0; i.TargetDict()[name]= new Word(ii=> ii.Push(ii.ValueGet(name))) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "VALUE" };
            builder["TO"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after TO"); EnsureStack(i,1,"TO"); var vv=ToLongPublic(i.Pop()); i.ValueSet(name,vv); }) { IsImmediate = true, Name = "TO" };
            builder["DEFER"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after DEFER"); i._deferred[name]=null; i.TargetDict()[name]= new Word(async ii=> { if(!ii._deferred.TryGetValue(name,out var target) || target is null) throw new ForthException(ForthErrorCode.UndefinedWord,$"Deferred word not set: {name}"); await target.ExecuteAsync(ii); }) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "DEFER" };
            builder["IS"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected deferred name after IS"); EnsureStack(i,1,"IS"); var xtObj=i.PopInternal(); if(xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError,"IS expects an execution token"); if(!i._deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord,$"No such deferred: {name}"); i._deferred[name]=xt; }) { IsImmediate = true, Name = "IS" };
            builder["SEE"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after SEE"); var plain=name; var cidx=name.IndexOf(':'); if(cidx>0) plain=name[(cidx+1)..]; var text=i._decompile.TryGetValue(plain,out var s) ? s : $": {plain} ;"; i.WriteText(text); }) { IsImmediate = true, Name = "SEE" };
            builder["CHAR"] = new Word(i => { var s=i.ReadNextTokenOrThrow("Expected char after CHAR"); if(!i._isCompiling) i.Push(s.Length>0?(long)s[0]:0L); else i.CurrentList().Add(ii=> { ii.Push(s.Length>0?(long)s[0]:0L); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "CHAR" };
            builder["S\""] = new Word(i => { var next=i.ReadNextTokenOrThrow("Expected text after S\""); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new ForthException(ForthErrorCode.CompileError,"S\" expects quoted token"); var str=next[1..^1]; if(!i._isCompiling) i.Push(str); else i.CurrentList().Add(ii=> { ii.Push(str); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "S\"" };
            builder["S"] = new Word(i => { var next=i.ReadNextTokenOrThrow("Expected text after S"); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new ForthException(ForthErrorCode.CompileError,"S expects quoted token"); var str=next[1..^1]; if(!i._isCompiling) i.Push(str); else i.CurrentList().Add(ii=> { ii.Push(str); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "S" };
            builder["BIND"] = new Word(i => { var typeName=i.ReadNextTokenOrThrow("type after BIND"); var methodName=i.ReadNextTokenOrThrow("method after BIND"); var argToken=i.ReadNextTokenOrThrow("arg count after BIND"); if(!int.TryParse(argToken,NumberStyles.Integer,CultureInfo.InvariantCulture,out var argCount)) throw new ForthException(ForthErrorCode.CompileError,"Invalid arg count"); var forthName=i.ReadNextTokenOrThrow("forth name after BIND"); i.TargetDict()[forthName]= ClrBinder.CreateBoundWord(typeName,methodName,argCount); i.RegisterDefinition(forthName); }) { IsImmediate = true, Name = "BIND" };
            builder["BINDASYNC"] = new Word(i => { var typeName=i.ReadNextTokenOrThrow("type after BINDASYNC"); var methodName=i.ReadNextTokenOrThrow("method after BINDASYNC"); var argToken=i.ReadNextTokenOrThrow("arg count after BINDASYNC"); if(!int.TryParse(argToken,NumberStyles.Integer,CultureInfo.InvariantCulture,out var argCount)) throw new ForthException(ForthErrorCode.CompileError,"Invalid arg count"); var forthName=i.ReadNextTokenOrThrow("forth name after BINDASYNC"); i.TargetDict()[forthName]= ClrBinder.CreateBoundTaskWord(typeName,methodName,argCount); i.RegisterDefinition(forthName); }) { IsImmediate = true, Name = "BINDASYNC" };
            builder["FORGET"] = new Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after FORGET"); i.ForgetWord(name); }) { IsImmediate = true, Name = "FORGET" };

            // merge into interpreter dictionary (respect possible existing words)
            foreach (var kv in builder)
            {
                intr._dict[kv.Key] = kv.Value;
            }
            intr.SnapshotWords(); // snapshot after compiler words
        }
    }

    private void ForgetWord(string token)
    {
        string? mod = null; string name = token;
        var cidx = token.IndexOf(':');
        if (cidx > 0){ mod = token[..cidx]; name = token[(cidx+1)..]; }
        for (int idx = _definitions.Count - 1; idx >= 0; idx--)
        {
            var def = _definitions[idx];
            if (!name.Equals(def.Name, StringComparison.OrdinalIgnoreCase)) continue;
            if (mod != null && !string.Equals(mod, def.Module, StringComparison.OrdinalIgnoreCase)) continue;
            if (idx < _baselineCount) throw new ForthException(ForthErrorCode.CompileError, "Cannot FORGET core word");
            for (int j = _definitions.Count - 1; j >= idx; j--)
            {
                var d = _definitions[j];
                var dict = string.IsNullOrWhiteSpace(d.Module) ? _dict : (_modules.TryGetValue(d.Module!, out var md) ? md : null);
                if (dict != null) dict.Remove(d.Name);
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

    public IReadOnlyList<object> Stack => _stack.AsReadOnly();
    public void Push(object value) => _stack.Push(value);
    public object Pop() => _stack.Pop();
    public object Peek() => _stack.Peek();

    public void AddWord(string name, Action<IForthInterpreter> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        TargetDict()[name] = new Word(i => body(i)) { Name = name, Module = _currentModule };
        RegisterDefinition(name);
    }
    public void AddWordAsync(string name, Func<IForthInterpreter, Task> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(body);
        TargetDict()[name] = new Word(i => body(i)) { Name = name, Module = _currentModule };
        RegisterDefinition(name);
    }

    private Dictionary<string, Word> TargetDict()
    {
        if (!string.IsNullOrWhiteSpace(_currentModule))
        {
            if (!_modules.TryGetValue(_currentModule!, out var d))
            {
                d = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);
                _modules[_currentModule!] = d;
            }
            return d;
        }
        return _dict;
    }

    private bool TryResolveWord(string token, out Word? word)
    {
        var idx = token.IndexOf(':');
        if (idx > 0)
        {
            var mod = token[..idx];
            var wname = token[(idx+1)..];
            if (_modules.TryGetValue(mod, out var mdict) && mdict.TryGetValue(wname, out var wq)) { word = wq; return true; }
            word = null; return false;
        }
        if (!string.IsNullOrWhiteSpace(_currentModule) && _modules.TryGetValue(_currentModule!, out var cur) && cur.TryGetValue(token, out var wc)) { word = wc; return true; }
        for (int i = _usingModules.Count - 1; i >= 0; i--)
        {
            var mn = _usingModules[i];
            if (_modules.TryGetValue(mn, out var md) && md.TryGetValue(token, out var wu)) { word = wu; return true; }
        }
        return _dict.TryGetValue(token, out word);
    }

    internal object PopInternal() => Pop();
    internal static void EnsureStack(ForthInterpreter interp, int needed, string word)
    {
        if (interp._stack.Count < needed)
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
    }
    internal static long ToLongPublic(object v) => ToLong(v);
    private static long ToLong(object v) => v switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        char c => c,
        bool bo => bo ? 1L : 0L,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
    };
    private static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };
    private bool IsResultConsumed(object taskRef) => _consumedTaskResults.Contains(taskRef);
    private void MarkResultConsumed(object taskRef) => _consumedTaskResults.Add(taskRef);

    // Return stack helpers
    internal void RPush(object value) => _rstack.Push(value);
    internal object RPop() => _rstack.Pop();
    internal int RCount => _rstack.Count;

    internal long ValueGet(string name) => _values.TryGetValue(name, out var v) ? v : 0L;
    internal void ValueSet(string name, long v) => _values[name] = v;

    internal void PushLoopIndex(long idx) => _controlFlow.Push(idx);
    internal void PopLoopIndex() => _controlFlow.Pop();
    internal void PopLoopIndexMaybe() => _controlFlow.PopMaybe();
    internal void Unloop() => _controlFlow.Unloop();
    internal long CurrentLoopIndex() => _controlFlow.Current();

    internal void PicturedBegin() => _picBuf = new StringBuilder();
    internal void PicturedHold(char ch){ _picBuf ??= new StringBuilder(); _picBuf.Insert(0, ch);}    
    internal void PicturedHoldDigit(long digit){ int d=(int)digit; char ch=(char)(d<10?'0'+d:'A'+(d-10)); PicturedHold(ch);}    
    internal string PicturedEnd(){ var s=_picBuf?.ToString()??string.Empty; _picBuf=null; return s; }

    internal IEnumerable<string> GetAllWordNames()
    {
        var names = new List<string>();
        foreach (var kv in _dict)
        {
            var w = kv.Value; if (w is null) continue; if (w.IsHidden) continue; var n = w.Name ?? kv.Key; names.Add(n);
        }
        foreach (var mod in _modules)
        {
            var mname = mod.Key;
            foreach (var kv in mod.Value)
            {
                var w = kv.Value; if (w is null) continue; if (w.IsHidden) continue; var n = w.Name ?? kv.Key; names.Add($"{mname}:{n}");
            }
        }
        names.Sort(StringComparer.OrdinalIgnoreCase);
        return names;
    }

    public async Task<bool> EvalAsync(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (_ilCache.TryGetValue(line, out var cached)) { await cached(this); return !_exitRequested; }
        _tokens = Tokenizer.Tokenize(line);
        _tokenIndex = 0;
        if (!_isCompiling && _ilCache.Count < 1024 && IlScriptCompiler.TryCompile(_tokens, out var runner)) { _ilCache[line]=runner; await runner(this); return !_exitRequested; }
        if (!_isCompiling)
        {
            var script = CompileSync(line);
            if (script is not null) { script.Run(this); _tokens=null; return !_exitRequested; }
        }
        while (TryReadNextToken(out var tok))
        {
            if (tok.Length==0) continue;
            if (!_isCompiling)
            {
                if (_doesCollecting) { _doesTokens!.Add(tok); continue; }
                if (tok.Equals("MODULE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after MODULE"); _currentModule = name; if (string.IsNullOrWhiteSpace(_currentModule)) throw new ForthException(ForthErrorCode.CompileError,"Invalid module name"); if (!_modules.ContainsKey(_currentModule)) _modules[_currentModule]= new(StringComparer.OrdinalIgnoreCase); continue; }
                if (tok.Equals("END-MODULE", StringComparison.OrdinalIgnoreCase)) { _currentModule=null; continue; }
                if (tok.Equals("USING", StringComparison.OrdinalIgnoreCase)) { var m=ReadNextTokenOrThrow("Expected name after USING"); if(!_usingModules.Contains(m)) _usingModules.Add(m); continue; }
                if (tok.Equals("LOAD-ASM", StringComparison.OrdinalIgnoreCase)) { var path=ReadNextTokenOrThrow("Expected path after LOAD-ASM"); var count=AssemblyWordLoader.Load(this,path); Push((long)count); continue; }
                if (tok.Equals("LOAD-ASM-TYPE", StringComparison.OrdinalIgnoreCase)) { var tn=ReadNextTokenOrThrow("Expected type after LOAD-ASM-TYPE"); Type? t=Type.GetType(tn,false,false); if(t==null) foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){ t=asm.GetType(tn,false,false); if(t!=null) break;} if(t==null) throw new ForthException(ForthErrorCode.CompileError,$"Type not found: {tn}"); var count=LoadAssemblyWords(t.Assembly); Push((long)count); continue; }
                if (tok.Equals("CREATE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after CREATE"); if(string.IsNullOrWhiteSpace(name)) throw new ForthException(ForthErrorCode.CompileError,"Invalid name for CREATE"); var addr=_nextAddr; _lastCreatedName=name; _lastCreatedAddr=addr; TargetDict()[name]= new Word(intr=> intr.Push(addr)) { Name = name, Module = _currentModule}; RegisterDefinition(name); continue; }
                if (tok.Equals(",", StringComparison.OrdinalIgnoreCase)) { EnsureStack(this,1,","); var v=ToLong(PopInternal()); _mem[_nextAddr++]=v; continue; }
                if (tok.Equals("DOES>", StringComparison.OrdinalIgnoreCase)) { if (string.IsNullOrEmpty(_lastCreatedName)) throw new ForthException(ForthErrorCode.CompileError,"DOES> without CREATE"); _doesCollecting=true; _doesTokens=new List<string>(); continue; }
                if (tok.Equals("ALLOT", StringComparison.OrdinalIgnoreCase)) { EnsureStack(this,1,"ALLOT"); var cells=ToLong(PopInternal()); if(cells<0) throw new ForthException(ForthErrorCode.CompileError,"Negative ALLOT size"); for(long k=0;k<cells;k++) _mem[_nextAddr++]=0; continue; }
                if (tok.Equals("SEE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after SEE"); var plain=name; var cidx=name.IndexOf(':'); if(cidx>0) plain=name[(cidx+1)..]; var text=_decompile.TryGetValue(plain,out var s) ? s : $": {plain} ;"; WriteText(text); continue; }
                if (tok==":") { var name=ReadNextTokenOrThrow("Expected name after ':'"); BeginDefinition(name); continue; }
                if (tok.Equals("VARIABLE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr=_nextAddr++; _mem[addr]=0; TargetDict()[name]= new Word(intr=> intr.Push(addr)) { Name = name, Module = _currentModule }; RegisterDefinition(name); continue; }
                if (tok.Equals("CONSTANT", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after CONSTANT"); EnsureStack(this,1,"CONSTANT"); var val=PopInternal(); TargetDict()[name]= new Word(intr=> intr.Push(val)) { Name = name, Module = _currentModule }; RegisterDefinition(name); continue; }
                if (tok.Equals("VALUE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after VALUE"); if(!_values.ContainsKey(name)) _values[name]=0; TargetDict()[name]= new Word(intr=> intr.Push(intr.ValueGet(name))) { Name = name, Module = _currentModule }; RegisterDefinition(name); continue; }
                if (tok.Equals("TO", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after TO"); EnsureStack(this,1,"TO"); var vv=ToLongPublic(PopInternal()); ValueSet(name,vv); continue; }
                if (tok.Equals("DEFER", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after DEFER"); _deferred[name]=null; TargetDict()[name]= new Word(async intr=> { if(!_deferred.TryGetValue(name,out var target) || target is null) throw new ForthException(ForthErrorCode.UndefinedWord,$"Deferred word not set: {name}"); await target.ExecuteAsync(intr);}) { Name = name, Module = _currentModule }; RegisterDefinition(name); continue; }
                if (tok.Equals("IS", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected deferred name after IS"); EnsureStack(this,1,"IS"); var xtObj=PopInternal(); if(xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError,"IS expects an execution token"); if(!_deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord,$"No such deferred: {name}"); _deferred[name]=xt; continue; }
                if (tok=="'") { var wn=ReadNextTokenOrThrow("Expected word after '"); if(!TryResolveWord(wn,out var wtok) || wtok is null) throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word: {wn}"); Push(wtok); continue; }
                if (tok.Equals("CHAR", StringComparison.OrdinalIgnoreCase)) { var s=ReadNextTokenOrThrow("Expected char after CHAR"); Push(s.Length>0?(long)s[0]:0L); continue; }
                if (tok.Equals("IMMEDIATE", StringComparison.OrdinalIgnoreCase)) { if (_lastDefinedWord is null) throw new ForthException(ForthErrorCode.CompileError,"No recent definition to mark IMMEDIATE"); _lastDefinedWord.IsImmediate=true; continue; }
                if (tok.Equals("POSTPONE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("POSTPONE expects a name"); if(TryResolveWord(name,out var wpost) && wpost is not null){ if (wpost.IsImmediate){ await wpost.ExecuteAsync(this);} else { _currentInstructions?.Add(async intr=> await wpost.ExecuteAsync(intr)); } continue; } throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word: {name}"); }
                if (tok.Equals("[", StringComparison.Ordinal)) { _isCompiling=false; _mem[_stateAddr]=0; continue; }
                if (tok.Equals("]", StringComparison.Ordinal)) { _isCompiling=true; _mem[_stateAddr]=1; continue; }
                if (tok.Length>=2 && tok[0]=='"' && tok[^1]=='"') { Push(tok[1..^1]); continue; }
                if (tok.Equals("S", StringComparison.OrdinalIgnoreCase) || tok.Equals("S\"", StringComparison.OrdinalIgnoreCase)) { var next=ReadNextTokenOrThrow("Expected text after S\""); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new ForthException(ForthErrorCode.CompileError,"S\" expects quoted token"); Push(next[1..^1]); continue; }
                if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase)) { if(TryReadNextToken(out var maybeMsg) && maybeMsg.Length>=2 && maybeMsg[0]=='"' && maybeMsg[^1]=='"'){ throw new ForthException(ForthErrorCode.Unknown, maybeMsg[1..^1]); } throw new ForthException(ForthErrorCode.Unknown,"ABORT"); }
                if (tok.Equals("BIND", StringComparison.OrdinalIgnoreCase) || tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase)) { bool asyncBind=tok.Equals("BINDASYNC", StringComparison.OrdinalIgnoreCase); var typeName=ReadNextTokenOrThrow("type after BIND"); var methodName=ReadNextTokenOrThrow("method after BIND"); var argToken=ReadNextTokenOrThrow("arg count after BIND"); if(!int.TryParse(argToken,NumberStyles.Integer,CultureInfo.InvariantCulture,out var argCount)) throw new ForthException(ForthErrorCode.CompileError,"Invalid arg count"); var forthName=ReadNextTokenOrThrow("forth name after BIND"); TargetDict()[forthName]= asyncBind? ClrBinder.CreateBoundTaskWord(typeName,methodName,argCount):ClrBinder.CreateBoundWord(typeName,methodName,argCount); RegisterDefinition(forthName); continue; }
                if (tok.Equals("FORGET", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("Expected name after FORGET"); ForgetWord(name); continue; }
                if (TryParseNumber(tok, out var num)) { Push(num); continue; }
                if (TryResolveWord(tok, out var word) && word is not null) { await word.ExecuteAsync(this); continue; }
                throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word: {tok}");
            }
            else
            {
                if (tok != ";") _currentDefTokens?.Add(tok);
                if (tok == ";") { FinishDefinition(); continue; }
                if (tok.Equals("IMMEDIATE", StringComparison.OrdinalIgnoreCase)) { if(_lastDefinedWord is null) throw new ForthException(ForthErrorCode.CompileError,"No recent definition to mark IMMEDIATE"); _lastDefinedWord.IsImmediate=true; continue; }
                if (tok.Equals("POSTPONE", StringComparison.OrdinalIgnoreCase)) { var name=ReadNextTokenOrThrow("POSTPONE expects a name"); if(TryResolveWord(name,out var wpost) && wpost is not null){ if (wpost.IsImmediate){ await wpost.ExecuteAsync(this);} else { CurrentList().Add(async intr=> await wpost.ExecuteAsync(intr)); } continue; }
                    var up = name.ToUpperInvariant();
                    switch (up)
                    {
                        case "IF": case "ELSE": case "THEN": case "BEGIN": case "WHILE": case "REPEAT": case "UNTIL":
                        case "DO": case "LOOP": case "LEAVE": case "LITERAL": case "[": case "]": case "'":
                            _tokens!.Insert(_tokenIndex, name);
                            continue;
                        default:
                            throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word: {name}");
                    }
                }
                if (tok.Equals("IF", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new IfFrame()); continue; }
                if (tok.Equals("ELSE", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not IfFrame ifr) throw new ForthException(ForthErrorCode.CompileError,"ELSE without IF"); if(ifr.ElsePart is not null) throw new ForthException(ForthErrorCode.CompileError,"Multiple ELSE"); ifr.ElsePart=new(); ifr.InElse=true; continue; }
                if (tok.Equals("THEN", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not IfFrame ifr) throw new ForthException(ForthErrorCode.CompileError,"THEN without IF"); _controlStack.Pop(); var thenPart=ifr.ThenPart; var elsePart=ifr.ElsePart; CurrentList().Add(async intr => { EnsureStack(intr,1,"IF"); var flag=intr.PopInternal(); if(ToBool(flag)) foreach(var a in thenPart) await a(intr); else if(elsePart is not null) foreach(var a in elsePart) await a(intr);}); continue; }
                if (tok.Equals("BEGIN", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new BeginFrame()); continue; }
                if (tok.Equals("WHILE", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"WHILE without BEGIN"); if(bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"Multiple WHILE"); bf.InWhile=true; continue; }
                if (tok.Equals("REPEAT", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"REPEAT without BEGIN"); if(!bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"REPEAT requires WHILE"); _controlStack.Pop(); var pre=bf.PrePart; var mid=bf.MidPart; CurrentList().Add(async intr => { while(true){ foreach(var a in pre) await a(intr); EnsureStack(intr,1,"WHILE"); var flag=intr.PopInternal(); if(!ToBool(flag)) break; foreach(var b in mid) await b(intr);} }); continue; }
                if (tok.Equals("UNTIL", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not BeginFrame bf) throw new ForthException(ForthErrorCode.CompileError,"UNTIL without BEGIN"); if(bf.InWhile) throw new ForthException(ForthErrorCode.CompileError,"UNTIL after WHILE use REPEAT"); _controlStack.Pop(); var body=bf.PrePart; CurrentList().Add(async intr => { while(true){ foreach(var a in body) await a(intr); EnsureStack(intr,1,"UNTIL"); var flag=intr.PopInternal(); if(ToBool(flag)) break; } }); continue; }
                if (tok.Equals("DO", StringComparison.OrdinalIgnoreCase)) { _controlStack.Push(new DoFrame()); continue; }
                if (tok.Equals("LOOP", StringComparison.OrdinalIgnoreCase)) { if(_controlStack.Count==0 || _controlStack.Peek() is not DoFrame df) throw new ForthException(ForthErrorCode.CompileError,"LOOP without DO"); _controlStack.Pop(); var body=df.Body; CurrentList().Add(async intr => { EnsureStack(intr,2,"DO"); var start=ToLongPublic(intr.Pop()); var limit=ToLongPublic(intr.Pop()); long step=start<=limit?1L:-1L; for(long idx=start; idx!=limit; idx+=step){ intr.PushLoopIndex(idx); try { foreach(var a in body) await a(intr);} catch(LoopLeaveException){ break; } finally { intr.PopLoopIndexMaybe(); } } }); continue; }
                if (tok.Equals("LEAVE", StringComparison.OrdinalIgnoreCase)) { bool inside=false; foreach(var f in _controlStack) if(f is DoFrame){ inside=true; break;} if(!inside) throw new ForthException(ForthErrorCode.CompileError,"LEAVE outside DO...LOOP"); CurrentList().Add(intr=> { throw new LoopLeaveException(); }); continue; }
                if (tok.Equals("LITERAL", StringComparison.OrdinalIgnoreCase)) { EnsureStack(this,1,"LITERAL"); var value=PopInternal(); _currentInstructions!.Add(intr=> { intr.Push(value); return Task.CompletedTask; }); continue; }
                if (tok.Length>=2 && tok[0]=='"' && tok[^1]=='"') { var s=tok[1..^1]; _currentInstructions!.Add(intr=> { intr.Push(s); return Task.CompletedTask; }); continue; }
                if (tok.Equals("S", StringComparison.OrdinalIgnoreCase) || tok.Equals("S\"", StringComparison.OrdinalIgnoreCase)) { var next=ReadNextTokenOrThrow("Expected text after S\""); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new ForthException(ForthErrorCode.CompileError,"S\" expects quoted token"); var sLit=next[1..^1]; _currentInstructions!.Add(intr=> { intr.Push(sLit); return Task.CompletedTask; }); continue; }
                if (tok=="'") { var wn=ReadNextTokenOrThrow("Expected word after '"); if(!TryResolveWord(wn,out var wtok) || wtok is null) throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word: {wn}"); var wt=wtok; _currentInstructions!.Add(intr=> { intr.Push(wt); return Task.CompletedTask; }); continue; }
                if (TryParseNumber(tok, out var lit)) { CurrentList().Add(intr=> { intr.Push(lit); return Task.CompletedTask; }); continue; }
                if (TryResolveWord(tok, out var cw) && cw is not null) { if(cw.IsImmediate){ if(cw.BodyTokens is {Count:>0}){ _tokens!.InsertRange(_tokenIndex, cw.BodyTokens); continue;} await cw.ExecuteAsync(this); continue;} CurrentList().Add(async intr=> await cw.ExecuteAsync(intr)); continue; }
                throw new ForthException(ForthErrorCode.UndefinedWord,$"Undefined word in definition: {tok}");
            }
        }
        if (_doesCollecting && !string.IsNullOrEmpty(_lastCreatedName) && _doesTokens is {Count:>0})
        {
            var bodyLine = string.Join(' ', _doesTokens);
            var addr = _lastCreatedAddr;
            var newWord = new Word(async intr => { intr.MemTryGet(addr, out var cur); intr.Push(addr); intr.Push(cur); await intr.EvalAsync(bodyLine).ConfigureAwait(false); }) { Name = _lastCreatedName, Module = _currentModule };
            TargetDict()[_lastCreatedName] = newWord; RegisterDefinition(_lastCreatedName); _lastDefinedWord=newWord;
            _doesCollecting=false; _doesTokens=null; _lastCreatedName=null;
        }
        _tokens=null; // clear stream
        return !_exitRequested;
    }

    private List<Func<ForthInterpreter, Task>> CurrentList() => _controlStack.Count==0 ? _currentInstructions! : _controlStack.Peek().GetCurrentList();

    private bool TryParseNumber(string token, out long value)
    {
        long GetBase(long def){ MemTryGet(_baseAddr,out var b); return b<=0?def:b; }
        return NumberParser.TryParse(token, GetBase, out value);
    }

    private void InstallPrimitives() => CorePrimitives.Install(this, _dict);

    internal object StackTop()=>_stack.Top();
    internal object StackNthFromTop(int n)=>_stack.NthFromTop(n);
    internal void DropTop()=>_stack.DropTop();
    internal void SwapTop2()=>_stack.SwapTop2();
    internal void MemTryGet(long addr, out long v){ _mem.TryGetValue(addr,out v); }
    internal void MemSet(long addr,long v){ _mem[addr]=v; }
    internal void RequestExit()=>_exitRequested=true;
    internal void WriteNumber(long n)=>_io.PrintNumber(n);
    internal void NewLine()=>_io.NewLine();
    internal void WriteText(string s)=>_io.Print(s);
    internal void ThrowExit()=>throw new ExitWordException();

    internal ForthCompiledScript? CompileSync(string line)
    {
        var tokens=Tokenizer.Tokenize(line);
        var steps=new List<Action<ForthInterpreter>>();
        int i=0; while(i<tokens.Count){ var tok=tokens[i++]; if(tok.Length==0) continue; if(tok is ":" or ";" or "AWAIT" or "SPAWN" or "[" or "]" or "IF" or "ELSE" or "THEN" or "BEGIN" or "WHILE" or "REPEAT" or "UNTIL") return null; if(tok.Equals("MODULE",StringComparison.OrdinalIgnoreCase)|| tok.Equals("END-MODULE",StringComparison.OrdinalIgnoreCase)|| tok.Equals("USING",StringComparison.OrdinalIgnoreCase)) return null; if(tok.Equals("LOAD-ASM",StringComparison.OrdinalIgnoreCase)|| tok.Equals("LOAD-ASM-TYPE",StringComparison.OrdinalIgnoreCase)) return null; if(tok.Equals("VARIABLE",StringComparison.OrdinalIgnoreCase)|| tok.Equals("CONSTANT",StringComparison.OrdinalIgnoreCase)) return null; if(tok.Equals("CHAR",StringComparison.OrdinalIgnoreCase)){ if(i>=tokens.Count) return null; var s=tokens[i++]; steps.Add(intr=> intr.Push((long)(s.Length>0?s[0]:'\0'))); continue;} if(tok.Length>=2 && tok[0]=='"' && tok[^1]=='"'){ var s=tok[1..^1]; steps.Add(intr=> intr.Push(s)); continue;} if(TryParseNumber(tok,out var numVal)){ steps.Add(intr=> intr.Push(numVal)); continue;} if(TryResolveWord(tok,out var w) && w is not null && !w.IsAsync){ switch(tok.ToUpperInvariant()){ case "+": steps.Add(intr=> { EnsureStack(intr,2,"+"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2+b2);} ); break; case "-": steps.Add(intr=> { EnsureStack(intr,2,"-"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2-b2);} ); break; case "*": steps.Add(intr=> { EnsureStack(intr,2,"*"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); intr.Push(a2*b2);} ); break; case "/": steps.Add(intr=> { EnsureStack(intr,2,"/"); var b2=ToLongPublic(intr.Pop()); var a2=ToLongPublic(intr.Pop()); if(b2==0) throw new ForthException(ForthErrorCode.DivideByZero,"Divide by zero"); intr.Push(a2/b2);} ); break; case "DUP": steps.Add(intr=> { EnsureStack(intr,1,"DUP"); intr.Push(intr.StackTop()); }); break; case "DROP": steps.Add(intr=> { EnsureStack(intr,1,"DROP"); intr.DropTop(); }); break; case "SWAP": steps.Add(intr=> { EnsureStack(intr,2,"SWAP"); intr.SwapTop2(); }); break; case "OVER": steps.Add(intr=> { EnsureStack(intr,2,"OVER"); intr.Push(intr.StackNthFromTop(2)); }); break; case "ROT": steps.Add(intr=> { EnsureStack(intr,3,"ROT"); var c=intr.PopInternal(); var b3=intr.PopInternal(); var a3=intr.PopInternal(); intr.Push(b3); intr.Push(c); intr.Push(a3); }); break; case "-ROT": steps.Add(intr=> { EnsureStack(intr,3,"-ROT"); var c=intr.PopInternal(); var b3=intr.PopInternal(); var a3=intr.PopInternal(); intr.Push(c); intr.Push(a3); intr.Push(b3); }); break; default: return null; } continue;} return null; }
        return new ForthCompiledScript(steps);
    }

    internal void WithModule(string name, Action action){ var prev=_currentModule; _currentModule=name; try{ action(); } finally { _currentModule=prev; } }
    public int LoadAssemblyWords(Assembly asm)=> AssemblyWordLoader.RegisterFromAssembly(this, asm);
    public void BeginModuleScope(string moduleName, Action register){ ArgumentException.ThrowIfNullOrWhiteSpace(moduleName); ArgumentNullException.ThrowIfNull(register); WithModule(moduleName, register);}    

    internal sealed class Word
    {
        private readonly Func<ForthInterpreter, Task> _run;
        public bool IsAsync { get; }
        public bool IsImmediate { get; set; }
        public List<string>? BodyTokens { get; set; }
        public string? Name { get; set; }
        public string? Module { get; set; }
        public bool IsHidden { get; set; }
        public Word(Action<ForthInterpreter> sync){ _run= intr => { sync(intr); return Task.CompletedTask; }; IsAsync=false; }
        public Word(Func<ForthInterpreter, Task> asyncRun){ _run=asyncRun; IsAsync=true; }
        public Task ExecuteAsync(ForthInterpreter intr)=> _run(intr);
    }
    private abstract class CompileFrame { public abstract List<Func<ForthInterpreter, Task>> GetCurrentList(); }
    private sealed class IfFrame: CompileFrame { public List<Func<ForthInterpreter, Task>> ThenPart { get; } = new(); public List<Func<ForthInterpreter, Task>>? ElsePart { get; set; } public bool InElse { get; set; } public override List<Func<ForthInterpreter, Task>> GetCurrentList()=> InElse? (ElsePart ??= new()) : ThenPart; }
    private sealed class BeginFrame: CompileFrame { public List<Func<ForthInterpreter, Task>> PrePart { get; } = new(); public List<Func<ForthInterpreter, Task>> MidPart { get; } = new(); public bool InWhile { get; set; } public override List<Func<ForthInterpreter, Task>> GetCurrentList()=> InWhile? MidPart:PrePart; }
    private sealed class DoFrame: CompileFrame { public List<Func<ForthInterpreter, Task>> Body { get; } = new(); public override List<Func<ForthInterpreter, Task>> GetCurrentList()=> Body; }
    private sealed class ExitWordException: Exception {}
    private sealed class LoopLeaveException: Exception {}
}
