using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forth.Core.Binding;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    static partial void AddDictionaryVocabEntries(Dictionary<(string? Module, string Name), ForthInterpreter.Word> d)
    {
        d[(null, "MODULE")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after MODULE"); i._currentModule = name; if (string.IsNullOrWhiteSpace(i._currentModule)) throw new ForthException(ForthErrorCode.CompileError, "Invalid module name"); return Task.CompletedTask; }) { IsImmediate = true, Name = "MODULE" };
        d[(null, "END-MODULE")] = new(i => { i._currentModule = null; return Task.CompletedTask; }) { IsImmediate = true, Name = "END-MODULE" };
        d[(null, "USING")] = new(i => { var m = i.ReadNextTokenOrThrow("Expected name after USING"); if (!i._usingModules.Contains(m)) i._usingModules.Add(m); return Task.CompletedTask; }) { IsImmediate = true, Name = "USING" };

        d[(null, "LOAD-ASM")] = new(i => { var path = i.ReadNextTokenOrThrow("Expected path after LOAD-ASM"); var count = AssemblyWordLoader.Load(i, path); i.Push((long)count); return Task.CompletedTask; }) { IsImmediate = true, Name = "LOAD-ASM" };

        d[(null, "LOAD-ASM-TYPE")] = new(i =>
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
        }) { IsImmediate = true, Name = "LOAD-ASM-TYPE" };

        d[(null, "CREATE")] = new(i =>
        {
            var name = i.ReadNextTokenOrThrow("Expected name after CREATE");
            if (string.IsNullOrWhiteSpace(name))
                throw new ForthException(ForthErrorCode.CompileError, "Invalid name for CREATE");
            var addr = i._nextAddr;
            i._lastCreatedName = name;
            i._lastCreatedAddr = addr;
            i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule });
            i.RegisterDefinition(name);
            return Task.CompletedTask;
        }) { IsImmediate = true, Name = "CREATE" };

        d[(null, "DOES>")] = new(i => { if (string.IsNullOrEmpty(i._lastCreatedName)) throw new ForthException(ForthErrorCode.CompileError, "DOES> without CREATE"); i._doesCollecting = true; i._doesTokens = new List<string>(); return Task.CompletedTask; }) { IsImmediate = true, Name = "DOES>" };

        d[(null, "VARIABLE")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr = i._nextAddr++; i._mem[addr] = 0; i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(ii => { ii.Push(addr); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }); i.RegisterDefinition(name); return Task.CompletedTask; }) { IsImmediate = true, Name = "VARIABLE" };

        d[(null, "CONSTANT")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after CONSTANT"); i.EnsureStack(1, "CONSTANT"); var val = i.PopInternal(); i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(ii => { ii.Push(val); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }); i.RegisterDefinition(name); return Task.CompletedTask; }) { IsImmediate = true, Name = "CONSTANT" };

        d[(null, "VALUE")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after VALUE"); if (!i._values.ContainsKey(name)) i._values[name] = 0; i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(ii => { ii.Push(ii.ValueGet(name)); return Task.CompletedTask; }) { Name = name, Module = i._currentModule }); i.RegisterDefinition(name); return Task.CompletedTask; }) { IsImmediate = true, Name = "VALUE" };

        d[(null, "TO")] = new(i => { i.EnsureStack(1, "TO"); var name = i.ReadNextTokenOrThrow("Expected name after TO"); var vv = ToLong(i.PopInternal()); i.ValueSet(name, vv); return Task.CompletedTask; }) { IsImmediate = true, Name = "TO" };

        d[(null, "DEFER")] = new(i =>
        {
            var name = i.ReadNextTokenOrThrow("Expected name after DEFER");
            i._deferred[name] = null;
            i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(async ii =>
            {
                if (!ii._deferred.TryGetValue(name, out var target) || target is null)
                    throw new ForthException(ForthErrorCode.UndefinedWord, $"Deferred word not set: {name}");
                await target.ExecuteAsync(ii);
            }) { Name = name, Module = i._currentModule });
            i.RegisterDefinition(name);
            return Task.CompletedTask;
        }) { IsImmediate = true, Name = "DEFER" };

        d[(null, "IS")] = new(i => { i.EnsureStack(1, "IS"); var name = i.ReadNextTokenOrThrow("Expected deferred name after IS"); var xtObj = i.PopInternal(); if (xtObj is not ForthInterpreter.Word xt) throw new ForthException(ForthErrorCode.TypeError, "IS expects an execution token"); if (!i._deferred.ContainsKey(name)) throw new ForthException(ForthErrorCode.UndefinedWord, $"No such deferred: {name}"); i._deferred[name] = xt; return Task.CompletedTask; }) { IsImmediate = true, Name = "IS" };

        d[(null, "SEE")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after SEE"); var plain = name; var cidx = name.IndexOf(':'); if (cidx > 0) plain = name[(cidx + 1)..]; var text = i._decompile.TryGetValue(plain, out var s) ? s : $": {plain} ;"; i.WriteText(text); return Task.CompletedTask; }) { IsImmediate = true, Name = "SEE" };

        d[(null, "CHAR")] = new(i => { var s = i.ReadNextTokenOrThrow("Expected char after CHAR"); if (!i._isCompiling) i.Push(s.Length > 0 ? (long)s[0] : 0L); else i.CurrentList().Add(ii => { ii.Push(s.Length > 0 ? (long)s[0] : 0L); return Task.CompletedTask; }); return Task.CompletedTask; }) { IsImmediate = true, Name = "CHAR" };

        d[(null, "S\"")] = new(i =>
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
        }) { IsImmediate = true, Name = "S\"" };

        d[(null, "S")] = new(i =>
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
        }) { IsImmediate = true, Name = "S" };

        d[(null, ".\"")] = new(i =>
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
        }) { IsImmediate = true, Name = ".\"" };

        d[(null, "BIND")] = new(i =>
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
        }) { IsImmediate = true, Name = "BIND" };

        d[(null, "FORGET")] = new(i => { var name = i.ReadNextTokenOrThrow("Expected name after FORGET"); i.ForgetWord(name); return Task.CompletedTask; }) { IsImmediate = true, Name = "FORGET" };

        d[(null, "MARKER")] = new(i =>
        {
            var name = i.ReadNextTokenOrThrow("Expected name after MARKER");
            var snap = i.CreateMarkerSnapshot();
            i._dict = i._dict.SetItem((i._currentModule, name), new ForthInterpreter.Word(ii => { ii.RestoreSnapshot(snap); return Task.CompletedTask; }) { Name = name, Module = i._currentModule });
            i.RegisterDefinition(name);
            return Task.CompletedTask;
        }) { IsImmediate = true, Name = "MARKER" };
    }
}
