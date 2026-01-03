using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forth.Core.Binding;
using System.Linq;
using System.Collections.Immutable;

namespace Forth.Core.Execution;

internal static class DictionaryVocabPrimitives
{
    // Minimal Wordlist representation for DEFINITIONS/WORDLIST behavior
    public sealed class Wordlist
    {
        public string Name { get; }
        public Wordlist(string name) => Name = name;
        public override string ToString() => Name;
    }

    private const string VocabCounterKey = "__vocabCounter";

    [Primitive("WORDLIST", HelpString = "WORDLIST ( -- wid ) - create a new wordlist id and push it")]
    private static Task Prim_WORDLIST(ForthInterpreter i)
    {
        // Per-interpreter counter (keeps tests isolated)
        i._values.TryGetValue(VocabCounterKey, out var c);
        var next = c + 1;
        i._values[VocabCounterKey] = next;
        var id = $"VOCAB{next}";
        var wl = new Wordlist(id);
        // Push the Wordlist instance
        i.Push(wl);
        // Also create a named word that pushes this instance when executed so ' VOCABn resolves
        var created = new Word(ii => { ii.Push(wl); return Task.CompletedTask; }) { Name = id, Module = null };
        i._dict = i._dict.SetItem((null, id), created);

        // Also register in the current module (if any) so tick lookup can resolve it consistently.
        if (!string.IsNullOrWhiteSpace(i._currentModule))
        {
            i._dict = i._dict.SetItem((i._currentModule, id), created);
        }

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
    private static Task Prim_MODULE(ForthInterpreter i) { if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after MODULE"); i._currentModule = name; if (string.IsNullOrWhiteSpace(i._currentModule)) throw new ForthException(ForthErrorCode.CompileError, "Invalid module name"); return Task.CompletedTask; }

    [Primitive("END-MODULE", IsImmediate = true, HelpString = "END-MODULE - end the current module namespace")]
    private static Task Prim_ENDMODULE(ForthInterpreter i) { i._currentModule = null; return Task.CompletedTask; }

    [Primitive("USING", IsImmediate = true, HelpString = "USING <name> - add module to search order")]
    private static Task Prim_USING(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var module)) throw new ForthException(ForthErrorCode.CompileError, "Expected module name after USING");
        i._usingModules.Add(module);
        return Task.CompletedTask;
    }

    [Primitive("LOAD-ASM", IsImmediate = true, HelpString = "LOAD-ASM <path> - load assembly words from file")]
    private static Task Prim_LOADASM(ForthInterpreter i) { if (!i.TryParseNextWord(out var path)) throw new ForthException(ForthErrorCode.CompileError, "Expected path after LOAD-ASM"); var count = AssemblyWordLoader.Load(i, path); i.Push((long)count); return Task.CompletedTask; }

    [Primitive("LOAD-ASM-TYPE", IsImmediate = true, HelpString = "LOAD-ASM-TYPE <type> - load assembly words from a type's assembly")]
    private static Task Prim_LOADASMTYPE(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var tn)) throw new ForthException(ForthErrorCode.CompileError, "Expected type after LOAD-ASM-TYPE");
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
        // CREATE is state-smart: immediate during interpretation, compiled during compilation
        if (i._isCompiling)
        {
            // Compile runtime behavior that reads the name when executed
            // This allows dynamic patterns like: ": MAKER CREATE DUP , ;" then "100 MAKER VALUE-HOLDER"
            // Note: This breaks the pattern ": TESTER 10 20 CREATE DUMMY 30 ;" (documented limitation)
            // The limitation exists because we cannot distinguish at compile-time between:
            //   1. Static names that are part of the definition: ": FOO CREATE BAR ;"
            //   2. Dynamic names provided at runtime: ": MAKER CREATE ;" then "100 MAKER NAME"
            // We choose to support (2) as it's more common in real Forth code.
            i.CurrentList().Add(ii =>
            {
                // Read name from runtime source using character parser
                if (!ii.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CREATE");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ForthException(ForthErrorCode.CompileError, "Invalid name for CREATE");
                
                var addr = ii._nextAddr;
                ii._lastCreatedName = name;
                ii._lastCreatedAddr = addr;
                
                var created = new Word(iii => { iii.Push(addr); return Task.CompletedTask; }) 
                { 
                    Name = name, 
                    Module = ii._currentModule, 
                    BodyAddr = addr 
                };
                ii._dict = ii._dict.SetItem((ii._currentModule, name), created);
                ii.RegisterDefinition(name);
                ii._lastDefinedWord = created;
                ii._decompile[name] = $": {name} ;";
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }
        else
        {
            // In interpret mode: execute immediately (stack unchanged)
            if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CREATE");
            if (string.IsNullOrWhiteSpace(name))
                throw new ForthException(ForthErrorCode.CompileError, "Invalid name for CREATE");
            var addr = i._nextAddr;
            i._lastCreatedName = name;
            i._lastCreatedAddr = addr;
            var created = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) 
            { 
                Name = name, 
                Module = i._currentModule, 
                BodyAddr = addr 
            };
            i._dict = i._dict.SetItem((i._currentModule, name), created);
            i.RegisterDefinition(name);
            i._lastDefinedWord = created;
            i._decompile[name] = $": {name} ;";
            return Task.CompletedTask;
        }
    }

    [Primitive("DOES>", IsImmediate = true, HelpString = "DOES> - begin definition of runtime behavior for last CREATE")]
    private static Task Prim_DOES(ForthInterpreter i)
    {
        if (i._isCompiling)
        {
            // In compile mode: start collecting DOES> body tokens  
            i._doesCollecting = true;
            i._doesTokens = new List<string>();
            
            // Note: The actual patching code will be compiled when ; is encountered and DOES> collection finishes
            // We mark that DOES> is active so FinishDefinition knows to handle it
        }
        else
        {
            // In interpret mode: collect remaining tokens on the line and apply immediately
            if (string.IsNullOrEmpty(i._lastCreatedName))
                throw new ForthException(ForthErrorCode.CompileError, "DOES> used without preceding CREATE");
            
            var doesTokens = new List<string>();
            // Collect all remaining words from the current input
            while (i.TryParseNextWord(out var tok))
            {
                doesTokens.Add(tok);
            }
            
            if (doesTokens.Count == 0)
                throw new ForthException(ForthErrorCode.CompileError, "DOES> requires a body");
            
            var createdName = i._lastCreatedName;
            var createdAddr = i._lastCreatedAddr;
            var doesBody = string.Join(' ', doesTokens);
            
            // Create new word with DOES> behavior: push address, then execute body
            var newWord = new Word(async intr =>
            {
                intr.Push(createdAddr);
                await intr.EvalAsync(doesBody).ConfigureAwait(false);
            })
            {
                Name = createdName,
                Module = i._currentModule,
                BodyAddr = createdAddr
            };

            // Replace the word in dictionary
            i._dict = i._dict.SetItem((i._currentModule, createdName), newWord);
            i.RegisterDefinition(createdName);
            i._lastDefinedWord = newWord;
            
            // Clear _lastCreatedName so subsequent DOES> won't accidentally apply to this word
            i._lastCreatedName = null;
        }
        
        return Task.CompletedTask;
    }

    [Primitive("VARIABLE", IsImmediate = true, HelpString = "VARIABLE <name> - define a named storage cell")]
    private static Task Prim_VARIABLE(ForthInterpreter i) { if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after VARIABLE"); var addr = i._nextAddr++; i._mem[addr] = 0; var createdVar = new Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule, BodyAddr = addr }; i._dict = i._dict.SetItem((i._currentModule, name), createdVar); i.RegisterDefinition(name); i._lastDefinedWord = createdVar; return Task.CompletedTask; }

    [Primitive("CONSTANT", IsImmediate = true, HelpString = "CONSTANT <name> - define a constant with top value")]
    private static Task Prim_CONSTANT(ForthInterpreter i) { if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after CONSTANT"); i.EnsureStack(1, "CONSTANT"); var val = i.PopInternal(); var createdConst = new Word(ii => { ii.Push(val); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdConst); i.RegisterDefinition(name); i._lastDefinedWord = createdConst; i._decompile[name] = $": {name} {val} ;"; return Task.CompletedTask; }

    [Primitive("VALUE", IsImmediate = true, HelpString = "VALUE <name> - define a named variable-like value (consumes stack)")]
    private static Task Prim_VALUE(ForthInterpreter i) { if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected text after VALUE"); i.EnsureStack(1, "VALUE"); var val = PrimitivesUtil.ToLong(i.PopInternal()); i._values[name] = val; var createdValue = new Word(ii => { ii.Push(ii.ValueGet(name)); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }; i._dict = i._dict.SetItem((i._currentModule, name), createdValue); i.RegisterDefinition(name); i._lastDefinedWord = createdValue; i._decompile[name] = $": {name} ;"; return Task.CompletedTask; }

    [Primitive("TO", IsImmediate = true, HelpString = "TO <name> - set a VALUE to the top of stack")]
    private static Task Prim_TO(ForthInterpreter i) 
    { 
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after TO"); 
        if (!i._isCompiling)
        {
            // Interpret mode: execute immediately
            i.EnsureStack(1, "TO"); 
            var vv = PrimitivesUtil.ToLong(i.PopInternal()); 
            i.ValueSet(name, vv); 
        }
        else
        {
            // Compile mode: compile runtime behavior
            i.CurrentList().Add(ii => 
            { 
                ii.EnsureStack(1, "TO"); 
                var vv = PrimitivesUtil.ToLong(ii.PopInternal()); 
                ii.ValueSet(name, vv); 
                return Task.CompletedTask; 
            });
        }
        return Task.CompletedTask; 
    }

    [Primitive("DEFER", IsImmediate = true, HelpString = "DEFER <name> - define a deferred execution token")]
    private static Task Prim_DEFER(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after DEFER");
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
        i._lastDeferred = name;
        return Task.CompletedTask;
    }

    [Primitive("IS", IsImmediate = true, HelpString = "IS <name> - set a deferred to an execution token")]
    private static Task Prim_IS(ForthInterpreter i) { i.EnsureStack(1, "IS"); if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected deferred name after IS"); var xtObj = i.PopInternal(); if (xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "IS expects an execution token"); if (!i._deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord, $"No such deferred: {name}"); i._deferred[name] = xt; return Task.CompletedTask; }

    [Primitive("DEFER!", HelpString = "DEFER! ( xt -- ) - set the execution token of the most recently defined deferred word")]
    private static Task Prim_DEFERSTORE(ForthInterpreter i)
    {
        i.EnsureStack(1, "DEFER!");
        var xtObj = i.PopInternal();
        if (xtObj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "DEFER! expects an execution token");
        if (string.IsNullOrEmpty(i._lastDeferred)) throw new ForthException(ForthErrorCode.UndefinedWord, "No deferred word defined yet");
        i._deferred[i._lastDeferred] = xt;
        return Task.CompletedTask;
    }

    [Primitive("DEFER@", HelpString = "DEFER@ ( -- xt ) - get the execution token of the most recently defined deferred word")]
    private static Task Prim_DEFERFETCH(ForthInterpreter i)
    {
        if (string.IsNullOrEmpty(i._lastDeferred)) throw new ForthException(ForthErrorCode.UndefinedWord, "No deferred word defined yet");
        var xt = i._deferred[i._lastDeferred];
        if (xt is null) throw new ForthException(ForthErrorCode.UndefinedWord, $"Deferred word not set: {i._lastDeferred}");
        i.Push(xt);
        return Task.CompletedTask;
    }

    [Primitive("SEE", IsImmediate = true, HelpString = "SEE <name> - display decompiled definition or placeholder")]
    private static Task Prim_SEE(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after SEE");
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
    private static Task Prim_CHAR(ForthInterpreter i) { if (!i.TryParseNextWord(out var s)) throw new ForthException(ForthErrorCode.CompileError, "Expected char after CHAR"); if (!i._isCompiling) i.Push(s.Length > 0 ? (long)s[0] : 0L); else i.CurrentList().Add(ii => { ii.Push(s.Length > 0 ? (long)s[0] : 0L); return Task.CompletedTask; }); return Task.CompletedTask; }

    [Primitive("[CHAR]", IsImmediate = true, HelpString = "[CHAR] <c> - compile character code literal")]
    private static Task Prim_BRACKETCHAR(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var s)) throw new ForthException(ForthErrorCode.CompileError, "Expected char after [CHAR]");
        var code = s.Length > 0 ? (long)s[0] : 0L;
        i.CurrentList().Add(ii => { ii.Push(code); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    [Primitive("[']", IsImmediate = true, HelpString = "['] <name> - compile execution token literal")]
    private static Task Prim_BRACKETTICK(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after ['");
        if (!i.TryResolveWord(name, out var word) || word is null) throw new ForthException(ForthErrorCode.UndefinedWord, $"Word not found: {name}");
        
        if (!i._isCompiling)
        {
            // In interpret mode: push the execution token immediately
            i.Push(word as object);
        }
        else
        {
            // In compile mode: compile code to push the execution token at runtime
            i.CurrentList().Add(ii => { ii.Push(word as object); return Task.CompletedTask; });
        }
        return Task.CompletedTask;
    }

    [Primitive("S\"", IsImmediate = true, HelpString = "S\" ( \"ccc<quote>\" -- c-addr u ) - parse string literal")]
    private static Task Prim_SQUOTE(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var next)) throw new ForthException(ForthErrorCode.CompileError, "Expected text after S\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
            throw new ForthException(ForthErrorCode.CompileError, "S\" expects quoted token");

        // Treat content as raw text. Control characters should be introduced via Forth words (e.g. CR, EMIT),
        // not via C-style escapes.
        var str = next[1..^1];
        if (str.Length > 0 && str[0] == ' ') str = str[1..];

        if (!i._isCompiling)
        {
            var addr = i.AllocateCountedString(str);
            i.Push(addr + 1);
            i.Push((long)str.Length);
        }
        else
        {
            i.CurrentList().Add(ii =>
            {
                var addr = ii.AllocateCountedString(str);
                ii.Push(addr + 1);
                ii.Push((long)str.Length);
                return Task.CompletedTask;
            });
        }

        return Task.CompletedTask;
    }

    [Primitive("S", IsImmediate = true, HelpString = "S <text> - push string literal (string) or compile-time push")]
    private static Task Prim_S(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var next)) throw new ForthException(ForthErrorCode.CompileError, "Expected text after S");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"' )
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
        if (!i.TryParseNextWord(out var typeName)) throw new ForthException(ForthErrorCode.CompileError, "Expected type name after BIND");
        if (!i.TryParseNextWord(out var methodName)) throw new ForthException(ForthErrorCode.CompileError, "Expected method name after BIND");
        if (!i.TryParseNextWord(out var argCountTok)) throw new ForthException(ForthErrorCode.CompileError, "Expected arg count after BIND");
        if (!int.TryParse(argCountTok, out var argCount)) throw new ForthException(ForthErrorCode.CompileError, "BIND: invalid argcount");
        if (!i.TryParseNextWord(out var wordName)) throw new ForthException(ForthErrorCode.CompileError, "Expected word name after BIND");
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
        if (!i.TryParseNextWord(out var next)) throw new ForthException(ForthErrorCode.CompileError, "Expected text after .\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"' )
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

    [Primitive("ABORT\"", IsImmediate = true, HelpString = "ABORT\" <message> - abort with message if flag true")]
    private static Task Prim_ABORTQUOTE(ForthInterpreter i)
    {
        if (!i.TryParseNextWord(out var next)) throw new ForthException(ForthErrorCode.CompileError, "Expected text after ABORT\"");
        if (next.Length < 2 || next[0] != '"' || next[^1] != '"' )
            throw new ForthException(ForthErrorCode.CompileError, "ABORT\" expects quoted token");
        var str = next[1..^1];
        
        if (!i._isCompiling)
        {
            // Interpret mode: execute immediately
            // Note: In interpret mode, there's no flag on the stack - we always abort with the message
            // This matches the pattern: ABORT "message" throws immediately
            throw new ForthException(ForthErrorCode.Unknown, str);
        }
        else
        {
            // Compile mode: compile code that checks flag and aborts conditionally
            var captured = str;
            i.CurrentList().Add(ii =>
            {
                ii.EnsureStack(1, "ABORT\"");
                var flag = ii.PopInternal();
                if (PrimitivesUtil.ToBool(flag))
                {
                    throw new ForthException(ForthErrorCode.Unknown, captured);
                }
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }
    }

    [Primitive(">BODY", HelpString = ">BODY ( xt|addr -- a-addr ) - get data-field address for CREATEd word or passthrough addr")]
    private static Task Prim_TO_BODY(ForthInterpreter i)
    {
        i.EnsureStack(1, ">BODY");
        var top = i.PopInternal();
        switch (top)
        {
            case Word w when w.BodyAddr.HasValue:
                i.Push(w.BodyAddr.Value);
                return Task.CompletedTask;
            case long addr:
                // If numeric address already, return as-is (common extension behavior)
                i.Push(addr);
                return Task.CompletedTask;
            default:
                throw new ForthException(ForthErrorCode.TypeError, ">BODY expects a CREATEd word or address");
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
        var count = (int)PrimitivesUtil.ToLong(countObj);
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
            else if (obj is Wordlist wl)
            {
                list.Add(wl.Name);
            }
            else
            {
                throw new ForthException(ForthErrorCode.TypeError, "SET-ORDER expects string names, FORTH, or Wordlist");
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
        if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after MARKER");
        var snap = i.CreateMarkerSnapshot();
        var created = new Word(ii => { ii.RestoreSnapshot(snap); return Task.CompletedTask; }) { Name = name, Module = i._currentModule };
        i._dict = i._dict.SetItem((i._currentModule, name), created);
        i.RegisterDefinition(name);
        i._lastDefinedWord = created;
        i._decompile[name] = $": {name} ;";
        return Task.CompletedTask;
    }

    [Primitive("FORGET", IsImmediate = true, HelpString = "FORGET <name> - remove a definition from the dictionary")]
    private static Task Prim_FORGET(ForthInterpreter i) { if (!i.TryParseNextWord(out var name)) throw new ForthException(ForthErrorCode.CompileError, "Expected name after FORGET"); i.ForgetWord(name); return Task.CompletedTask; }

    [Primitive("REPORT-ERRORS", HelpString = "REPORT-ERRORS - report collected test errors (stub)")]
    private static Task Prim_REPORT_ERRORS(ForthInterpreter i)
    {
        // Minimal stub to satisfy tests that expect the word to be present.
        // The full errorreport.fth defines a Forth implementation; when loaded it will override this stub.
        return Task.CompletedTask;
    }

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
            int len = (int)PrimitivesUtil.ToLong(lenCell);
            var chars = new char[len];
            for (int k = 0; k < len; k++) { i.MemTryGet(addr + k, out var v); chars[k] = (char)(PrimitivesUtil.ToLong(v) & 0xFF); }
            i.PopInternal(); // len
            i.PopInternal(); // addr
            query = new string(chars);
        }
        else if (i.Stack.Count >= 1 && i.Stack[^1] is long countedAddr)
        {
            // counted string address form produced by S" or custom literal words
            i.MemTryGet(countedAddr, out var lenObj);
            int len = (int)PrimitivesUtil.ToLong(lenObj);
            var chars = new char[len];
            for (int k = 0; k < len; k++) { i.MemTryGet(countedAddr + 1 + k, out var v); chars[k] = (char)(PrimitivesUtil.ToLong(v) & 0xFF); }
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

    [Primitive("EVALUATE", IsAsync = true, HelpString = "EVALUATE ( i*x c-addr u -- j*x ) - interpret the string")]
    private static async Task Prim_EVALUATE(ForthInterpreter i)
    {
        i.EnsureStack(2, "EVALUATE");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var addr = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "EVALUATE negative length");
        var str = i.ReadMemoryString(addr, u);
        await i.EvalAsync(str);
        return;
    }

    [Primitive("FIND", HelpString = "FIND ( c-addr -- c-addr 0 | xt 1 | xt -1 ) - find word in dictionary")]
    private static Task Prim_FIND(ForthInterpreter i)
    {
        i.EnsureStack(1, "FIND");
        var countedAddr = PrimitivesUtil.ToLong(i.PopInternal());
        i.MemTryGet(countedAddr, out var lenObj);
        int len = (int)PrimitivesUtil.ToLong(lenObj);
        var chars = new char[len];
        for (int k = 0; k < len; k++)
        {
            i.MemTryGet(countedAddr + 1 + k, out var v);
            chars[k] = (char)(PrimitivesUtil.ToLong(v) & 0xFF);
        }
        var str = new string(chars);
#pragma warning disable CS8604 // word is not null here
        if (i.TryResolveWord(str, out var word))
        {
            i.Push(word);
            i.Push(word.IsImmediate ? 1L : -1L);
        }
        else
        {
            i.Push(countedAddr);
            i.Push(0L);
        }
#pragma warning restore CS8604
        return Task.CompletedTask;
    }

    [Primitive("PREVIOUS", HelpString = "PREVIOUS - remove the most recently added module from the search order")]
    private static Task Prim_PREVIOUS(ForthInterpreter i)
    {
        if (i._usingModules.Count > 0)
        {
            i._usingModules.RemoveAt(i._usingModules.Count - 1);
        }
        return Task.CompletedTask;
    }

    [Primitive("FORTH-WORDLIST", HelpString = "FORTH-WORDLIST ( -- wid ) - push the wordlist id for the FORTH wordlist")]
    private static Task Prim_FORTH_WORDLIST(ForthInterpreter i)
    {
        i.Push(null!);
        return Task.CompletedTask;
    }

    [Primitive("GET-CURRENT", HelpString = "GET-CURRENT ( -- wid ) - get the current compilation wordlist")]
    private static Task Prim_GETCURRENT(ForthInterpreter i)
    {
        i.Push(i._currentModule!);
        return Task.CompletedTask;
    }

    [Primitive("SET-CURRENT", IsImmediate = true, HelpString = "SET-CURRENT ( wid -- ) - set the current compilation wordlist")]
    private static Task Prim_SETCURRENT(ForthInterpreter i)
    {
        i.EnsureStack(1, "SET-CURRENT");
        var obj = i.PopInternal();
        string? s = null;
        if (obj is string ss) s = ss.Trim();
        else if (obj is Word w && !string.IsNullOrEmpty(w.Name)) s = w.Name.Trim();
        else if (obj is Wordlist wl) s = wl.Name;
        else if (obj == null) s = null;
        if (s is not null || obj == null)
        {
            if (s == null || string.Equals(s, "FORTH", StringComparison.OrdinalIgnoreCase))
                i._currentModule = null;
            else
                i._currentModule = s;
            return Task.CompletedTask;
        }
        throw new ForthException(ForthErrorCode.TypeError, "SET-CURRENT expects a wordlist id (string, Word, or Wordlist)");
    }

    [Primitive("SEARCH-WORDLIST", HelpString = "SEARCH-WORDLIST ( c-addr u wid -- 0 | xt 1 | xt -1 ) - search wordlist for word")]
    private static Task Prim_SEARCHWORDLIST(ForthInterpreter i)
    {
        i.EnsureStack(3, "SEARCH-WORDLIST");
        var wid = i.PopInternal();
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        var caddr = PrimitivesUtil.ToLong(i.PopInternal());
        var name = i.ReadMemoryString(caddr, u);
        string? module = null;
        if (wid is string s) module = s;
        else if (wid is Wordlist wl) module = wl.Name;
        else if (wid == null) module = null;
        else throw new ForthException(ForthErrorCode.TypeError, "SEARCH-WORDLIST expects wid as string, Wordlist, or null");
        if (i._dict.TryGetValue((module, name), out var word))
        {
            i.Push(word);
            i.Push(word.IsImmediate ? 1L : -1L);
        }
        else
        {
            i.Push(0L);
        }
        return Task.CompletedTask;
    }

    [Primitive("ONLY", HelpString = "ONLY ( -- ) - set the search order to the minimum wordlist (FORTH only)")]
    private static Task Prim_ONLY(ForthInterpreter i)
    {
        i.SetOrder(new List<string?> { null });
        return Task.CompletedTask;
    }

    [Primitive("ALSO", HelpString = "ALSO ( -- ) - duplicate the first wordlist in the search order")]
    private static Task Prim_ALSO(ForthInterpreter i)
    {
        if (i._usingModules.Count > 0)
            i._usingModules.Add(i._usingModules[i._usingModules.Count - 1]);
        return Task.CompletedTask;
    }
}
