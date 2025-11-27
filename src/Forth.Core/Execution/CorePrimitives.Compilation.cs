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
    private static Task Prim_IF(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "IF outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.IfFrame());
        return Task.CompletedTask;
    }

    [Primitive("ELSE", IsImmediate = true, HelpString = "Begin else-part of an if construct")]
    private static Task Prim_ELSE(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "ELSE outside compilation");
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.IfFrame ifr)
            throw new ForthException(ForthErrorCode.CompileError, "ELSE without IF");
        ifr.ElsePart ??= new();
        ifr.InElse = true;
        return Task.CompletedTask;
    }

    [Primitive("THEN", IsImmediate = true, HelpString = "End an if construct")]
    private static Task Prim_THEN(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "THEN outside compilation");
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
            {
                foreach (var a in thenPart) await a(ii);
            }
            else if (elsePart is not null)
            {
                foreach (var a in elsePart) await a(ii);
            }
        });
        return Task.CompletedTask;
    }

    [Primitive("BEGIN", IsImmediate = true, HelpString = "Begin a loop construct")]
    private static Task Prim_BEGIN(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "BEGIN outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.BeginFrame());
        return Task.CompletedTask;
    }

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
    private static Task Prim_REPEAT(ForthInterpreter i) => RepeatImpl(i);

    private static Task RepeatImpl(ForthInterpreter i)
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

    [Primitive("AGAIN", IsImmediate = true, HelpString = "AGAIN - end a BEGIN...AGAIN infinite loop")]
    private static Task Prim_AGAIN(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.BeginFrame bf)
            throw new ForthException(ForthErrorCode.CompileError, "AGAIN without BEGIN");
        if (bf.InWhile)
            throw new ForthException(ForthErrorCode.CompileError, "AGAIN after WHILE use REPEAT");
        i._controlStack.Pop();
        var body = bf.PrePart;
        i.CurrentList().Add(async ii =>
        {
            while (true)
            {
                foreach (var a in body)
                    await a(ii);
            }
        });
        return Task.CompletedTask;
    }

    [Primitive("DO", IsImmediate = true, HelpString = "Begin a counted loop ( limit start -- )")]
    private static Task Prim_DO(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "DO outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.DoFrame());
        return Task.CompletedTask;
    }

    [Primitive("?DO", IsImmediate = true, HelpString = "Begin a conditional counted loop ( limit start -- ) executes body only if start != limit")]
    private static Task Prim_QDO(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "?DO outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.DoFrame { IsConditional = true });
        return Task.CompletedTask;
    }

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
            if (df.IsConditional && start == limit) { return; }
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

    [Primitive("+LOOP", IsImmediate = true, HelpString = "+LOOP - end DO loop with runtime step ( step from stack )")]
    private static Task Prim_PlusLOOP(ForthInterpreter i)
    {
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not Forth.Core.Interpreter.ForthInterpreter.DoFrame df)
            throw new ForthException(ForthErrorCode.CompileError, "+LOOP without DO");
        i._controlStack.Pop();
        var body = df.Body;
        i.CurrentList().Add(async ii =>
        {
            // Setup: consume start and limit once
            ii.EnsureStack(2, "+LOOP setup");
            var start = ToLong(ii.PopInternal());
            var limit = ToLong(ii.PopInternal());
            if (df.IsConditional && start == limit) { return; }
            long idx = start;
            while (true)
            {
                // Termination check before executing body with current index
                if (idx >= limit && start <= limit) break;
                if (idx <= limit && start > limit) break;

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

                // Consume step value provided by body before +LOOP
                ii.EnsureStack(1, "+LOOP");
                var step = ToLong(ii.PopInternal());
                if (step == 0) step = (start <= limit) ? 1 : -1; // avoid infinite loops
                idx += step;
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

    [Primitive("[IF]", IsImmediate = true, HelpString = "[IF] ( flag -- ) - conditional compilation start (interpret/compile)")]
    private static Task Prim_BRACKET_IF(ForthInterpreter i)
    {
        i.EnsureStack(1, "[IF]");
        var flagObj = i.PopInternal();
        bool cond = ToBool(flagObj);
        if (cond) return Task.CompletedTask; // keep emitting tokens
        // Skip until matching [ELSE] or [THEN] at same nesting level
        SkipBracketSection(i, skipElse: true);
        return Task.CompletedTask;
    }

    [Primitive("[ELSE]", IsImmediate = true, HelpString = "[ELSE] - conditional compilation alternate part")]
    private static Task Prim_BRACKET_ELSE(ForthInterpreter i)
    {
        // We arrive here only if first part compiled/emitted; now skip until [THEN]
        SkipBracketSection(i, skipElse: false);
        return Task.CompletedTask;
    }

    [Primitive("[THEN]", IsImmediate = true, HelpString = "[THEN] - end bracket conditional")]
    private static Task Prim_BRACKET_THEN(ForthInterpreter i) => Task.CompletedTask; // no-op when reached

    private static void SkipBracketSection(ForthInterpreter i, bool skipElse)
    {
        if (i._tokens is null) return; // nothing to skip
        int depth = 0;
        for (int idx = i._tokenIndex; idx < i._tokens.Count; idx++)
        {
            var t = i._tokens[idx];
            // Normalize possible separated bracket tokens into a single composite representation when present: "[" "IF" "]"
            string comp = t.ToUpperInvariant();
            bool consumedComposite = false;
            if (t == "[" && idx + 2 < i._tokens.Count && i._tokens[idx + 2] == "]")
            {
                comp = ("[" + i._tokens[idx + 1].ToUpperInvariant() + "]");
                consumedComposite = true;
            }

            if (comp == "[IF]") { depth++; if (consumedComposite) { idx += 2; } continue; }
            if (comp == "[THEN]")
            {
                if (depth == 0)
                {
                    // Found terminating [THEN]
                    // If we consumed a composite, idx already advanced to the closing bracket index; set token index to next
                    i._tokenIndex = (consumedComposite ? idx + 1 : idx) + 1; // consume
                    return;
                }
                depth--;
                if (consumedComposite) { idx += 2; }
                continue;
            }
            if (skipElse && comp == "[ELSE]" && depth == 0)
            {
                // Stop before ELSE so ELSE body executes
                i._tokenIndex = (consumedComposite ? idx + 1 : idx) + 1; // consume [ELSE]
                return;
            }
            if (consumedComposite)
            {
                // If we synthesized a composite token but didn't match any of the above cases, advance past closing bracket
                idx += 2;
            }
        }
        // If we reach here, no matching terminator
        throw new ForthException(ForthErrorCode.CompileError, "Unmatched bracket conditional");
    }

    // [IF] [ELSE] [THEN] temporarily removed until full ANS bracket conditional design is finalized.
}
