using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forth.Core.Binding;
using System.Linq;
using System.Collections.Immutable;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    // Minimal Wordlist representation for DEFINITIONS/WORDLIST behavior
    private sealed class Wordlist
    {
        public string Name { get; }
        public Wordlist(string name) => Name = name;
        public override string ToString() => Name;
    }

    private static int _vocabCounter = 0;

    [Primitive("WORDLIST", HelpString = "WORDLIST ( -- wid ) - create a new wordlist id and push it")]
    private static Task Prim_WORDLIST(ForthInterpreter i)
    {
        var id = $"VOCAB{System.Threading.Interlocked.Increment(ref _vocabCounter)}";
        var wl = new Wordlist(id);
        // Push the Wordlist instance
        i.Push(wl);
        // Also create a named word that pushes this instance when executed so ' VOCABn resolves
        var created = new Word(ii => { ii.Push(wl); return Task.CompletedTask; }) { Name = id, Module = null };
        i._dict = i._dict.SetItem((null, id), created);
        i.RegisterDefinition(id);
        i._lastDefinedWord = created;
        // Add decompile placeholder
        i._decompile[id] = $": {id} ;";
        return Task.CompletedTask;
    }

    [Primitive("DEFINITIONS", IsImmediate = true, HelpString = "DEFINITIONS ( wid -- ) - set current compilation wordlist")]
    private static Task Prim_DEFINITIONS(ForthInterpreter i)
    {
        i.EnsureStack(1, "DEFINITIONS");
        var obj = i.PopInternal();
        string? s = null;
        if (obj is string ss) s = ss.Trim();
        else if (obj is Word w && !string.IsNullOrEmpty(w.Name)) s = w.Name.Trim();
        else if (obj is Wordlist wl) s = wl.Name;

        if (s is not null)
        {
            if (string.Equals(s, "FORTH", StringComparison.OrdinalIgnoreCase))
                i._currentModule = null;
            else
                i._currentModule = s;
            return Task.CompletedTask;
        }
        throw new ForthException(ForthErrorCode.TypeError, "DEFINITIONS expects a wordlist id (string, Word, or Wordlist)");
    }

    [Primitive("FORTH", HelpString = "FORTH ( -- wid ) - push the FORTH sentinel for core dictionary")]
    private static Task Prim_FORTH(ForthInterpreter i)
    {
        i.Push("FORTH");
        return Task.CompletedTask;
    }

    // Provide BLK primitive so ans-diff detects it as a primitive (push current block number)
    [Primitive("BLK", HelpString = "BLK ( -- n ) - push current block number")]
    private static Task Prim_BLK(ForthInterpreter i)
    {
        i.Push((long)i.GetCurrentBlockNumber());
        return Task.CompletedTask;
    }

    [Primitive("MODULE", IsImmediate = true, HelpString = "MODULE <name> - start a module namespace")]
    private static Task Prim_MODULE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after MODULE"); i._currentModule = name; if (string.IsNullOrWhiteSpace(i._currentModule)) throw new ForthException(ForthErrorCode.CompileError, "Invalid module name"); return Task.CompletedTask; }

    [Primitive("END-MODULE", IsImmediate = true, HelpString = "END-MODULE - end the current module namespace")]
    private static Task Prim_ENDMODULE(ForthInterpreter i) { i._currentModule = null; return Task.CompletedTask; }

    [Primitive("USING", IsImmediate = true, HelpString = "USING <module> - import a module into current namespace")]
    private static Task Prim_USING(ForthInterpreter i) { var m = i.ReadNextTokenOrThrow("Expected name after USING"); if (!i._usingModules.Contains(m)) i._usingModules.Add(m); return Task.CompletedTask; }

    [Primitive("LOAD-ASM", IsImmediate = true, HelpString = "LOAD-ASM <path> - load assembly words from file")]
    private static Task Prim_LOADASM(ForthInterpreter i) { var path = i.ReadNextTokenOrThrow("Expected path after LOAD-ASM"); var count = AssemblyWordLoader.Load(i, path); i.Push((long)count); return Task.CompletedTask; }

    [Primitive("LOAD-ASM-TYPE", IsImmediate = true, HelpString = "LOAD-ASM-TYPE <type> - load assembly words from a type's assembly")]
    private static Task Prim_LOADASMTYPE(ForthInterpreter i)
    {
        var tn = i.ReadNextTokenOrThrow("Expected type after LOAD-ASM-TYPE");
        Type? t = Type.GetType(tn, false, false);
        if (t == null)
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(tn, false, false);
                if (t != null) break;
            }
        if (t == null)
            throw new ForthException(ForthErrorCode.CompileError, $"Type not found: {tn}");
        var count = i.LoadAssemblyWords(t.Assembly);
        i.Push((long)count);
        return Task.CompletedTask;
    }

    [Primitive("CREATE", IsImmediate = true, HelpString = "CREATE <name> - create a new data-definition word")]
    private static Task Prim_CREATE(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after CREATE");
        if (string.IsNullOrWhiteSpace(name))
            throw new ForthException(ForthErrorCode.CompileError, "Invalid name for CREATE");
        var addr = i._nextAddr;
        i._lastCreatedName = name;
        i._lastCreatedAddr = addr;
        var created = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), created);
        i.RegisterDefinition(name);
        i._lastDefinedWord = created;
        // record decompile placeholder
        i._decompile[name] = $": {name} ;";
        return Task.CompletedTask;
    }

    [Primitive("DOES>", IsImmediate = true, HelpString = "DOES> - begin definition of runtime behavior for last CREATE")]
    private static Task Prim_DOES(ForthInterpreter i) { if (string.IsNullOrEmpty(i._lastCreatedName)) throw new ForthException(ForthErrorCode.CompileError, "DOES> without CREATE"); i._doesCollecting = true; i._doesTokens = new List<string>(); return Task.CompletedTask; }

    [Primitive("VARIABLE", IsImmediate = true, HelpString = "VARIABLE <name> - define a named storage cell")]
    private static Task Prim_VARIABLE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr = i._nextAddr++; i._mem[addr] = 0; var createdVar = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdVar); i.RegisterDefinition(name); i._lastDefinedWord = createdVar; return Task.CompletedTask; }

    [Primitive("CONSTANT", IsImmediate = true, HelpString = "CONSTANT <name> - define a constant with top value")]
    private static Task Prim_CONSTANT(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after CONSTANT"); i.EnsureStack(1, "CONSTANT"); var val = i.PopInternal(); var createdConst = new Word(ii => { ii.Push(val); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdConst); i.RegisterDefinition(name); i._lastDefinedWord = createdConst; i._decompile[name] = $": {name} {val} ;"; return Task.CompletedTask; }

    [Primitive("VALUE", IsImmediate = true, HelpString = "VALUE <name> - define a named variable-like value")]
    private static Task Prim_VALUE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected text after VALUE"); if (!i._values.ContainsKey(name)) i._values[name] = 0; var createdValue = new Word(ii => { ii.Push(ii.ValueGet(name)); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdValue); i.RegisterDefinition(name); i._lastDefinedWord = createdValue; i._decompile[name] = $": {name} ;"; return Task.CompletedTask; }

    [Primitive("TO", IsImmediate = true, HelpString = "TO <name> - set a VALUE to the top of stack")]
    private static Task Prim_TO(ForthInterpreter i) { i.EnsureStack(1, "TO"); var name = i.ReadNextTokenOrThrow("Expected name after TO"); var vv = ToLong(i.PopInternal()); i.ValueSet(name, vv); return Task.CompletedTask; }

    [Primitive("DEFER", IsImmediate = true, HelpString = "DEFER <name> - define a deferred execution token")]
    private static Task Prim_DEFER(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after DEFER");
        i._deferred[name] = null;
        var created = new Word(async ii =>
        {
            if (!ii._deferred.TryGetValue(name, out var target) || target is null)
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Deferred word not set: {name}");
            await target.ExecuteAsync(ii);
        }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), created);
        i.RegisterDefinition(name);
        i._lastDefinedWord = created;
        i._decompile[name] = $": {name} ;";
        return Task.CompletedTask;
    }

    [Primitive("IS", IsImmediate = true, HelpString = "IS <name> - set a deferred to an execution token")]
    private static Task Prim_IS(ForthInterpreter i) { i.EnsureStack(1, "IS"); var name = i.ReadNextTokenOrThrow("Expected deferred name after IS"); var xtObj = i.PopInternal(); if (xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "IS expects an execution token"); if (!i._deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord, $"No such deferred: {name}"); i._deferred[name] = xt; return Task.CompletedTask; }

    [Primitive("SEE", IsImmediate = true, HelpString = "SEE <name> - display decompiled definition or placeholder")]
    private static Task Prim_SEE(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after SEE");
        // First try to resolve the actual word (handles module-qualified names)
        if (i.TryResolveWord(name, out var resolved) && resolved is not null)
        {
            var displayName = resolved.Name ?? name;
            if (resolved.BodyTokens is not null && resolved.BodyTokens.Count > 0)
            {
                var text = $": {displayName} {string.Join(' ', resolved.BodyTokens)} ;";
                i.WriteText(text);
                return Task.CompletedTask;
            }
            // No body tokens available; fallthrough to decompile table or placeholder
        }

        var plain = name;
        var cidx = name.IndexOf(':');
        if (cidx > 0) plain = name[(cidx + 1)..];
        if (i._decompile.TryGetValue(plain, out var s) && !string.IsNullOrEmpty(s))
        {
            i.WriteText(s);
            return Task.CompletedTask;
        }

        i.WriteText($": {plain} ;");
        return Task.CompletedTask;
    }

    [Primitive("CHAR", IsImmediate = true, HelpString = "CHAR <c> - push character code for character literal")]
    private static Task Prim_CHAR(ForthInterpreter i) { var s = i.ReadNextTokenOrThrow("Expected char after CHAR"); if (!i._isCompiling) i.Push(s.Length > 0 ? (long)s[0] : 0L); else i.CurrentList().Add(ii => { ii.Push(s.Length > 0 ? (long)s[0] : 0L); return Task.CompletedTask; }); return Task.CompletedTask; }

    [Primitive("S\"", IsImmediate = true, HelpString = "S\" <text> - ANS counted string literal ( -- c-addr ) where c-addr points to length cell")]
    private static Task Prim_SQUOTE(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after S\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, "S\" expects quoted token");
        var str = next[1..^1];
        // Allocate counted string
        var addr = i.AllocateCountedString(str);
        if (!i._isCompiling)
            i.Push((long)addr); // counted string address
        else
        {
            var capturedAddr = addr;
            i.CurrentList().Add(ii => { ii.Push((long)capturedAddr); return Task.CompletedTask; });
        }
        return Task.CompletedTask;
    }

    [Primitive("S", IsImmediate = true, HelpString = "S <text> - push string literal (string) or compile-time push")]
    private static Task Prim_S(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after S");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, "S expects quoted token");
        var str = next[1..^1];
        if (!i._isCompiling)
            i.Push(str);
        else
            i.CurrentList().Add(ii => { ii.Push(str); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    [Primitive("BIND", HelpString = "BIND <type> <method> <argcount> <name> - bind a CLR method to a forth word")]
    private static Task Prim_BIND(ForthInterpreter i)
    {
        var typeName = i.ReadNextTokenOrThrow("Expected type name after BIND");
        var methodName = i.ReadNextTokenOrThrow("Expected method name after BIND");
        var argCountTok = i.ReadNextTokenOrThrow("Expected arg count after BIND");
        if (!int.TryParse(argCountTok, out var argCount)) throw new ForthException(ForthErrorCode.CompileError, "BIND: invalid argcount");
        var wordName = i.ReadNextTokenOrThrow("Expected word name after BIND");
        var created = ClrBinder.CreateBoundWord(typeName, methodName, argCount);
        created.Name = wordName;
        created.Module = i._currentModule;
        i._dict = i._dict.SetItem((i._currentModule, wordName), created);
        i.RegisterDefinition(wordName);
        i._lastDefinedWord = created;
        return Task.CompletedTask;
    }

    [Primitive(".\"", IsImmediate = true, HelpString = ".\" <text> - print quoted text (interpret/compile-time)")]
    private static Task Prim_DOTQUOTE(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after .\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, ".\" expects quoted token");
        var str = next[1..^1];
        if (!i._isCompiling)
        {
            i.WriteText(str);
            return Task.CompletedTask;
        }
        else
        {
            var captured = str;
            i.CurrentList().Add(ii => { ii.WriteText(captured); return Task.CompletedTask; });
            return Task.CompletedTask;
        }
    }

    [Primitive("GET-ORDER", HelpString = "GET-ORDER ( -- wid... count ) - push current search order list and count")]
    private static Task Prim_GETORDER(ForthInterpreter i)
    {
        var order = i.GetOrder(); // ImmutableList<string?> with most-recent-first then null
        // Push each module name as string; represent core as "FORTH"
        foreach (var name in order)
        {
            if (name is null) i.Push("FORTH");
            else i.Push(name);
        }
        i.Push((long)order.Count);
        return Task.CompletedTask;
    }

    [Primitive("SET-ORDER", IsImmediate = true, HelpString = "SET-ORDER ( wid... count -- ) - set the search order from stack")]
    private static Task Prim_SETORDER(ForthInterpreter i)
    {
        i.EnsureStack(1, "SET-ORDER");
        var countObj = i.PopInternal();
        var count = (int)ToLong(countObj);
        if (count < 0) throw new ForthException(ForthErrorCode.CompileError, "SET-ORDER expects non-negative count");
        var list = new List<string?>();
        for (int idx = 0; idx < count; idx++)
        {
            var obj = i.PopInternal();
            if (obj is string s)
            {
                s = s.Trim();
                if (string.Equals(s, "FORTH", StringComparison.OrdinalIgnoreCase)) list.Add(null);
                else list.Add(s);
            }
            else
            {
                throw new ForthException(ForthErrorCode.TypeError, "SET-ORDER expects string names or FORTH");
            }
        }
        // The stack had names pushed in order; we popped them LIFO so reverse to get intended order
        list.Reverse();
        i.SetOrder(list);
        return Task.CompletedTask;
    }

    [Primitive("MARKER", IsImmediate = true, HelpString = "MARKER <name> - create a marker that restores state when executed")]
    private static Task Prim_MARKER(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after MARKER");
        var snap = i.CreateMarkerSnapshot();
        var created = new Word(ii => { ii.RestoreSnapshot(snap); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), created);
        i.RegisterDefinition(name);
        i._lastDefinedWord = created;
        i._decompile[name] = $": {name} ;";
        return Task.CompletedTask;
    }

    [Primitive("FORGET", IsImmediate = true, HelpString = "FORGET <name> - remove a definition from the dictionary")]
    private static Task Prim_FORGET(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after FORGET"); i.ForgetWord(name); return Task.CompletedTask; }

    [Primitive("ENVIRONMENT?", HelpString = "ENVIRONMENT? ( c-addr u | counted-addr -- false | value true ) - query environment")]
    private static Task Prim_ENVIRONMENTQ(ForthInterpreter i)
    {
        // Supports queries used by tester.fs: FLOATING, FLOATING-STACK
        // Accept either counted string address or (addr len) or plain string previously pushed
        string? query = null;
        if (i.Stack.Count >= 2 && i.Stack[^1] is long lenCell && i.Stack[^2] is long addrCell)
        {
            // treat as (addr len)
            long addr = addrCell;
            int len = (int)CorePrimitives.ToLong(lenCell);
            var chars = new char[len];
            for (int k = 0; k < len; k++) { i.MemTryGet(addr + k, out var v); chars[k] = (char)(CorePrimitives.ToLong(v) & 0xFF); }
            i.PopInternal(); // len
            i.PopInternal(); // addr
            query = new string(chars);
        }
        else if (i.Stack.Count >= 1 && i.Stack[^1] is long countedAddr)
        {
            // counted string address form produced by S" or custom literal words
            i.MemTryGet(countedAddr, out var lenObj);
            int len = (int)CorePrimitives.ToLong(lenObj);
            var chars = new char[len];
            for (int k = 0; k < len; k++) { i.MemTryGet(countedAddr + 1 + k, out var v); chars[k] = (char)(CorePrimitives.ToLong(v) & 0xFF); }
            i.PopInternal();
            query = new string(chars);
        }
        else if (i.Stack.Count >= 1 && i.Stack[^1] is string s)
        {
            query = s; i.PopInternal();
        }
        if (query is null) { i.Push(0L); return Task.CompletedTask; }
        query = query.Trim().ToUpperInvariant();
        switch (query)
        {
            case "FLOATING":
            case "FLOATING-STACK":
                // Provide value TRUE then flag TRUE
                i.Push(-1L); // value (TRUE)
                i.Push(-1L); // recognized flag (TRUE)
                break;
            default:
                i.Push(0L); // not recognized -> false only
                break;
        }
        return Task.CompletedTask;
    }
}
