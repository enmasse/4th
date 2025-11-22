using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive(":", IsImmediate = true, HelpString = ": <name> - begin a new definition")]
    private static Task Prim_Colon(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after ':'");
        i.BeginDefinition(name);
        return Task.CompletedTask;
    }

    [Primitive(";", IsImmediate = true, HelpString = "; - finish a definition")]
    private static Task Prim_Semi(ForthInterpreter i) { i.FinishDefinition(); return Task.CompletedTask; }

    [Primitive("IMMEDIATE", IsImmediate = true, HelpString = "Mark the last defined word as immediate")]
    private static Task Prim_IMMEDIATE(ForthInterpreter i)
    {
        if (i._lastDefinedWord is null)
            throw new ForthException(ForthErrorCode.CompileError, "No recent definition to mark IMMEDIATE");
        i._lastDefinedWord.IsImmediate = true;
        return Task.CompletedTask;
    }

    [Primitive("POSTPONE", IsImmediate = true, IsAsync = true, HelpString = "Postpone execution of a word until compile-time or add its execution to the current definition")]
    private static Task Prim_POSTPONE(ForthInterpreter i) => PostponeImpl(i);

    private static async Task PostponeImpl(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("POSTPONE expects a name");
        if (i.TryResolveWord(name, out var wpost) && wpost is not null)
        {
            if (wpost.IsImmediate)
                await wpost.ExecuteAsync(i);
            else i.CurrentList().Add(ii => wpost.ExecuteAsync(ii));
            return;
        }
        switch (name.ToUpperInvariant())
        {
            case "IF": case "ELSE": case "THEN": case "BEGIN": case "WHILE": case "REPEAT": case "UNTIL":
            case "DO": case "LOOP": case "LEAVE": case "LITERAL": case "[": case "]": case "'": case "RECURSE":
                i._tokens!.Insert(i._tokenIndex, name);
                return;
        }
        throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
    }

    [Primitive("'", IsImmediate = true, HelpString = "Push execution token for a word")]
    private static Task Prim_Tick(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected word after '");
        if (!i.TryResolveWord(name, out var wt) || wt is null)
        {
            // Simplified behavior: if word is undefined, push the name as a string
            // This supports patterns like: WORDLIST ' VOCAB1 DEFINITIONS
            i.Push(name);
            return Task.CompletedTask;
        }
        if (!i._isCompiling)
            i.Push(wt);
        else
            i.CurrentList().Add(ii => { ii.Push(wt); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    [Primitive("LITERAL", IsImmediate = true, HelpString = "Compile a literal value into the current definition")]
    private static Task Prim_LITERAL(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "LITERAL outside compilation");
        i.EnsureStack(1, "LITERAL");
        var val = i.PopInternal();
        i.CurrentList().Add(ii => { ii.Push(val); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    [Primitive("[", IsImmediate = true, HelpString = "Switch to interpret state during compilation")]
    private static Task Prim_LBracket(ForthInterpreter i) { i._isCompiling = false; i._mem[i.StateAddr] = 0; return Task.CompletedTask; }

    [Primitive("]", IsImmediate = true, HelpString = "Switch to compile state")]
    private static Task Prim_RBracket(ForthInterpreter i) { i._isCompiling = true; i._mem[i.StateAddr] = 1; return Task.CompletedTask; }

    [Primitive("IF", IsImmediate = true, HelpString = "Begin an if-then construct")]
    private static Task Prim_IF(ForthInterpreter i) { i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.IfFrame()); return Task.CompletedTask; }

    [Primitive("ELSE", IsImmediate = true, HelpString = "Begin else-part of an if construct")]
    private static Task Prim_ELSE(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.IfFrame ifr)
            throw new ForthException(ForthErrorCode.CompileError, "ELSE without IF");
        if (ifr.ElsePart is not null)
            throw new ForthException(ForthErrorCode.CompileError, "Multiple ELSE");
        ifr.ElsePart = new();
        ifr.InElse = true;
        return Task.CompletedTask;
    }

    [Primitive("THEN", IsImmediate = true, HelpString = "End an if construct")]
    private static Task Prim_THEN(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.IfFrame ifr)
            throw new ForthException(ForthErrorCode.CompileError, "THEN without IF");
        i._controlStack.Pop();
        var thenPart = ifr.ThenPart;
        var elsePart = ifr.ElsePart;
        i.CurrentList().Add(async ii =>
        {
            ii.EnsureStack(1, "IF");
            var flag = ii.PopInternal();
            if (ToBool(flag))
                foreach (var a in thenPart)
                    await a(ii);
            else if (elsePart is not null)
                foreach (var a in elsePart)
                    await a(ii);
        });
        return Task.CompletedTask;
    }

    [Primitive("BEGIN", IsImmediate = true, HelpString = "Begin a loop construct")]
    private static Task Prim_BEGIN(ForthInterpreter i) { i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.BeginFrame()); return Task.CompletedTask; }

    [Primitive("WHILE", IsImmediate = true, HelpString = "Begin a conditional part of a BEGIN...REPEAT loop")]
    private static Task Prim_WHILE(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.BeginFrame bf)
            throw new ForthException(ForthErrorCode.CompileError, "WHILE without BEGIN");
        if (bf.InWhile)
            throw new ForthException(ForthErrorCode.CompileError, "Multiple WHILE");
        bf.InWhile = true;
        return Task.CompletedTask;
    }

    [Primitive("REPEAT", IsImmediate = true, HelpString = "End a BEGIN...REPEAT loop")]
    private static Task Prim_REPEAT(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.BeginFrame bf)
            throw new ForthException(ForthErrorCode.CompileError, "REPEAT without BEGIN");
        if (!bf.InWhile)
            throw new ForthException(ForthErrorCode.CompileError, "REPEAT requires WHILE");
        i._controlStack.Pop();
        var pre = bf.PrePart;
        var mid = bf.MidPart;
        i.CurrentList().Add(async ii =>
        {
            while (true)
            {
                foreach (var a in pre)
                    await a(ii);
                ii.EnsureStack(1, "WHILE");
                var flag = ii.PopInternal();
                if (!ToBool(flag))
                    break;
                foreach (var b in mid)
                    await b(ii);
            }
        });
        return Task.CompletedTask;
    }

    [Primitive("UNTIL", IsImmediate = true, HelpString = "End a BEGIN...UNTIL loop")]
    private static Task Prim_UNTIL(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.BeginFrame bf)
            throw new ForthException(ForthErrorCode.CompileError, "UNTIL without BEGIN");
        if (bf.InWhile)
            throw new ForthException(ForthErrorCode.CompileError, "UNTIL after WHILE use REPEAT");
        i._controlStack.Pop();
        var body = bf.PrePart;
        i.CurrentList().Add(async ii =>
        {
            while (true)
            {
                foreach (var a in body)
                    await a(ii);
                ii.EnsureStack(1, "UNTIL");
                var flag = ii.PopInternal();
                if (ToBool(flag))
                    break;
            }
        });
        return Task.CompletedTask;
    }

    [Primitive("DO", IsImmediate = true, HelpString = "Begin a counted loop ( limit start -- )")]
    private static Task Prim_DO(ForthInterpreter i) { i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.DoFrame()); return Task.CompletedTask; }

    [Primitive("LOOP", IsImmediate = true, HelpString = "End a counted DO...LOOP")]
    private static Task Prim_LOOP(ForthInterpreter i) => LoopImpl(i);

    private static Task LoopImpl(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.DoFrame df)
            throw new ForthException(ForthErrorCode.CompileError, "LOOP without DO");
        i._controlStack.Pop();
        var body = df.Body;
        i.CurrentList().Add(async ii =>
        {
            ii.EnsureStack(2, "DO");
            var start = ToLong(ii.PopInternal());
            var limit = ToLong(ii.PopInternal());
            long step = start <= limit ? 1L : -1L;
            for (long idx = start; idx != limit; idx += step)
            {
                ii.PushLoopIndex(idx);
                try
                {
                    foreach (var a in body)
                        await a(ii);
                }
                catch (Forth.Core.Interpreter.ForthInterpreter.LoopLeaveException)
                {
                    break;
                }
                finally
                {
                    ii.PopLoopIndexMaybe();
                }
            }
        });
        return Task.CompletedTask;
    }

    [Primitive("LEAVE", IsImmediate = true, HelpString = "Leave the nearest DO...LOOP")]
    private static Task Prim_LEAVE(ForthInterpreter i) => LeaveImpl(i);

    private static Task LeaveImpl(ForthInterpreter i)
    {
        bool inside = false;
        foreach (var f in i._controlStack)
            if (f is Forth.Core.Interpreter.ForthInterpreter.DoFrame)
            {
                inside = true;
                break;
            }
        if (!inside)
            throw new ForthException(ForthErrorCode.CompileError, "LEAVE outside DO...LOOP");
        i.CurrentList().Add(ii => throw new Forth.Core.Interpreter.ForthInterpreter.LoopLeaveException());
        return Task.CompletedTask;
    }

    [Primitive("RECURSE", IsImmediate = true, HelpString = "Compile a call to the current definition (supports recursive definitions)")]
    private static Task Prim_RECURSE(ForthInterpreter i)
    {
        if (!i._isCompiling || string.IsNullOrEmpty(i._currentDefName))
            throw new ForthException(ForthErrorCode.CompileError, "RECURSE outside of a definition");
        if (!i.TryResolveWord(i._currentDefName, out var self) || self is null)
        {
            var name = i._currentDefName;
            i.CurrentList().Add(async ii =>
            {
                if (!ii.TryResolveWord(name, out var w) || w is null)
                    throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined self word: {name}");
                await w.ExecuteAsync(ii);
            });
            return Task.CompletedTask;
        }
        i.CurrentList().Add(async ii => await self.ExecuteAsync(ii));
        return Task.CompletedTask;
    }

    // [IF] [ELSE] [THEN] temporarily removed until full ANS bracket conditional design is finalized.
}
