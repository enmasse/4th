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
        return Task.CompletedTask;
    }

    [Primitive("DOES>", IsImmediate = true, HelpString = "DOES> - begin definition of runtime behavior for last CREATE")]
    private static Task Prim_DOES(ForthInterpreter i) { if (string.IsNullOrEmpty(i._lastCreatedName)) throw new ForthException(ForthErrorCode.CompileError, "DOES> without CREATE"); i._doesCollecting = true; i._doesTokens = new List<string>(); return Task.CompletedTask; }

    [Primitive("VARIABLE", IsImmediate = true, HelpString = "VARIABLE <name> - define a named storage cell")]
    private static Task Prim_VARIABLE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr = i._nextAddr++; i._mem[addr] = 0; var createdVar = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdVar); i.RegisterDefinition(name); i._lastDefinedWord = createdVar; return Task.CompletedTask; }

    [Primitive("CONSTANT", IsImmediate = true, HelpString = "CONSTANT <name> - define a constant with top value")]
    private static Task Prim_CONSTANT(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after CONSTANT"); i.EnsureStack(1, "CONSTANT"); var val = i.PopInternal(); var createdConst = new Word(ii => { ii.Push(val); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdConst); i.RegisterDefinition(name); i._lastDefinedWord = createdConst; return Task.CompletedTask; }

    [Primitive("VALUE", IsImmediate = true, HelpString = "VALUE <name> - define a named variable-like value")]
    private static Task Prim_VALUE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after VALUE"); if (!i._values.ContainsKey(name)) i._values[name] = 0; var createdValue = new Word(ii => { ii.Push(ii.ValueGet(name)); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdValue); i.RegisterDefinition(name); i._lastDefinedWord = createdValue; return Task.CompletedTask; }

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
        return Task.CompletedTask;
    }

    [Primitive("IS", IsImmediate = true, HelpString = "IS <name> - set a deferred to an execution token")]
    private static Task Prim_IS(ForthInterpreter i) { i.EnsureStack(1, "IS"); var name = i.ReadNextTokenOrThrow("Expected deferred name after IS"); var xtObj = i.PopInternal(); if (xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "IS expects an execution token"); if (!i._deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord, $"No such deferred: {name}"); i._deferred[name] = xt; return Task.CompletedTask; }

    [Primitive("SEE", IsImmediate = true, HelpString = "SEE <name> - display decompiled definition or placeholder")]
    private static Task Prim_SEE(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after SEE"); var plain = name; var cidx = name.IndexOf(':'); if (cidx > 0) plain = name[(cidx + 1)..]; var text = i._decompile.TryGetValue(plain, out var s) ? s : $": {plain} ;"; i.WriteText(text); return Task.CompletedTask; }

    [Primitive("CHAR", IsImmediate = true, HelpString = "CHAR <c> - push character code for character literal")]
    private static Task Prim_CHAR(ForthInterpreter i) { var s = i.ReadNextTokenOrThrow("Expected char after CHAR"); if (!i._isCompiling) i.Push(s.Length > 0 ? (long)s[0] : 0L); else i.CurrentList().Add(ii => { ii.Push(s.Length > 0 ? (long)s[0] : 0L); return Task.CompletedTask; }); return Task.CompletedTask; }

    [Primitive("S\"", IsImmediate = true, HelpString = "S\" <text> - push counted string literal (c-addr u)")]
    private static Task Prim_SQUOTE(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after S\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, "S\" expects quoted token");
        var str = next[1..^1];
        if (!i._isCompiling)
            i.Push(str);
        else
            i.CurrentList().Add(ii => { ii.Push(str); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    [Primitive("S", IsImmediate = true, HelpString = "S <text> - push string literal (c-addr u)")]
    private static Task Prim_S(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after S");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, "S expects quoted token");
        var str = next[1..^1];
        if (!i._isCompiling)
            i.Push(str);
        else
        {
            i.CurrentList().Add(ii => { ii.Push(str); return Task.CompletedTask; });
        }
        return Task.CompletedTask;
    }

    [Primitive(".\"", IsImmediate = true, HelpString = ".\" <text> - print text at interpret time or compile as print")]
    private static Task Prim_DOTQUOTE(ForthInterpreter i)
    {
        var next = i.ReadNextTokenOrThrow("Expected text after .\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, ".\" expects quoted token");
        var str = next[1..^1].TrimStart();
        if (!i._isCompiling)
        {
            i.WriteText(str);
        }
        else
        {
            i.CurrentList().Add(ii => { ii.WriteText(str); return Task.CompletedTask; });
        }
        return Task.CompletedTask;
    }

    [Primitive("BIND", IsImmediate = true, HelpString = "BIND <type> <method> <argcount> <forthname> - bind CLR method to forth word")]
    private static Task Prim_BIND(ForthInterpreter i)
    {
        var typeName = i.ReadNextTokenOrThrow("type after BIND");
        var methodName = i.ReadNextTokenOrThrow("method after BIND");
        var argToken = i.ReadNextTokenOrThrow("arg count after BIND");
        if (!int.TryParse(argToken, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var argCount))
            throw new ForthException(ForthErrorCode.CompileError, "Invalid arg count");
        var forthName = i.ReadNextTokenOrThrow("forth name after BIND");
        var bound = ClrBinder.CreateBoundWord(typeName, methodName, argCount);
        i._dict = i._dict.SetItem((i._currentModule, forthName), bound);
        i.RegisterDefinition(forthName);
        return Task.CompletedTask;
    }

    [Primitive("FORGET", IsImmediate = true, HelpString = "FORGET <name> - remove a definition from the dictionary")]
    private static Task Prim_FORGET(ForthInterpreter i) { var name = i.ReadNextTokenOrThrow("Expected name after FORGET"); i.ForgetWord(name); return Task.CompletedTask; }

    [Primitive("MARKER", IsImmediate = true, HelpString = "MARKER <name> - create a marker that restores state when executed")]
    private static Task Prim_MARKER(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after MARKER");
        var snap = i.CreateMarkerSnapshot();
        var created = new Word(ii => { ii.RestoreSnapshot(snap); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), created);
        i.RegisterDefinition(name);
        i._lastDefinedWord = created;
        return Task.CompletedTask;
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
}
