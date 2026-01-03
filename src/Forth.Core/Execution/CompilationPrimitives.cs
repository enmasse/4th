using Forth.Core.Binding;
using Forth.Core.Execution;
using System.Threading.Tasks;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static class CompilationPrimitives
{
    [Primitive(":", IsImmediate = true, HelpString = ": <name> - begin a new definition")]
    private static Task Prim_Colon(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected name after ':'");
        i.Trace($"TOK : {name}");
        i.BeginDefinition(name);
        return Task.CompletedTask;
    }

    [Primitive(":NONAME", IsImmediate = true, HelpString = ":NONAME - begin an anonymous definition, leaving xt on stack")]
    private static Task Prim_ColonNoname(ForthInterpreter i)
    {
        i.Trace("TOK :NONAME");
        i.BeginDefinition(null);
        return Task.CompletedTask;
    }

    [Primitive(";", IsImmediate = true, HelpString = "; - finish a definition")]
    private static Task Prim_Semi(ForthInterpreter i) { i.Trace("TOK ;"); i.FinishDefinition(); return Task.CompletedTask; }

    [Primitive("[", IsImmediate = true, HelpString = "Switch to interpret state during compilation")]
    private static Task Prim_LBracket(ForthInterpreter i) { i.Trace($"TOK [ wasCompiling={i._isCompiling}"); i._isCompiling = false; i._mem[i.StateAddr] = 0; return Task.CompletedTask; }

    [Primitive("]", IsImmediate = true, HelpString = "Switch to compile state")]
    private static Task Prim_RBracket(ForthInterpreter i) { i.Trace($"TOK ] wasCompiling={i._isCompiling}"); i._isCompiling = true; i._mem[i.StateAddr] = 1; return Task.CompletedTask; }

    [Primitive("IF", IsImmediate = true, HelpString = "Begin an if-then construct")]
    private static Task Prim_IF(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "IF outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.IfFrame());
        return Task.CompletedTask;
    }

    [Primitive("AHEAD", IsImmediate = true, HelpString = "AHEAD - compile unconditional forward branch")]
    private static Task Prim_AHEAD(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "AHEAD outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.AheadFrame());
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
        if (i._controlStack.Count == 0)
            throw new ForthException(ForthErrorCode.CompileError, "THEN without matching control structure");
        var frame = i._controlStack.Peek();
        if (frame is Forth.Core.Interpreter.ForthInterpreter.IfFrame ifr)
        {
            i._controlStack.Pop();
            var thenPart = ifr.ThenPart;
            var elsePart = ifr.ElsePart;
            i.CurrentList().Add(async ii =>
            {
                ii.EnsureStack(1, "IF");
                var flag = ii.PopInternal();
                if (PrimitivesUtil.ToBool(flag))
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
        else if (frame is Forth.Core.Interpreter.ForthInterpreter.AheadFrame afr)
        {
            i._controlStack.Pop();
            // For AHEAD, the SkipPart is not executed, so no compilation needed.
            return Task.CompletedTask;
        }
        else
        {
            throw new ForthException(ForthErrorCode.CompileError, "THEN without IF or AHEAD");
        }
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
                if (!PrimitivesUtil.ToBool(flag))
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
                if (PrimitivesUtil.ToBool(flag))
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
            var start = PrimitivesUtil.ToLong(ii.PopInternal());
            var limit = PrimitivesUtil.ToLong(ii.PopInternal());
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
            var start = PrimitivesUtil.ToLong(ii.PopInternal());
            var limit = PrimitivesUtil.ToLong(ii.PopInternal());
            if (df.IsConditional && start == limit) { return; }
            long idx = start;
            while (true)
            {
                // Execute body at current index
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
                var step = PrimitivesUtil.ToLong(ii.PopInternal());
                if (step == 0) step = (start <= limit) ? 1 : -1; // avoid infinite loops

                var next = idx + step;
                // Terminate when updated index reaches or crosses the limit
                if (start <= limit)
                {
                    if (next >= limit) break;
                }
                else
                {
                    if (next <= limit) break;
                }
                idx = next;
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
        i.Trace($"TOK [IF] enter isCompiling={i._isCompiling} skipping={i._bracketIfSkipping} activeDepth={i._bracketIfActiveDepth} nesting={i._bracketIfNestingDepth}");
        // [IF] works in both interpret and compile mode - it's a compile-time conditional
        // If already skipping from outer [IF], just track nesting depth
        if (i._bracketIfSkipping)
        {
            i._bracketIfNestingDepth++;
            i._bracketIfActiveDepth++;
            i.Trace($"TOK [IF] nested skipping now activeDepth={i._bracketIfActiveDepth} nesting={i._bracketIfNestingDepth}");
            return Task.CompletedTask;
        }

        // Not currently skipping - evaluate condition
        object flagObj;
        if (i.Stack.Count > 0)
        {
            flagObj = i.PopInternal();
        }
        else
        {
            // Some legacy sources call [IF] without first pushing a flag; treat as false
            flagObj = 0L;
        }
        bool cond = PrimitivesUtil.ToBool(flagObj);

        i._bracketIfActiveDepth++;

        if (cond)
        {
            i.Trace($"TOK [IF] cond=true isCompiling={i._isCompiling} activeDepth={i._bracketIfActiveDepth}");
            return Task.CompletedTask;
        }

        i.Trace($"TOK [IF] cond=false -> skip isCompiling={i._isCompiling}");
        i._bracketIfNestingDepth = 0;
        i._bracketIfSeenElse = false;
        i._bracketIfSkipping = true;

        SkipBracketSection(i, skipElse: true);

        i.Trace($"TOK [IF] exit skip isCompiling={i._isCompiling} skipping={i._bracketIfSkipping} activeDepth={i._bracketIfActiveDepth}");
        return Task.CompletedTask;
    }

    [Primitive("[ELSE]", IsImmediate = true, HelpString = "[ELSE] - conditional compilation alternate part")]
    private static Task Prim_BRACKET_ELSE(ForthInterpreter i)
    {
        i.Trace($"TOK [ELSE] enter isCompiling={i._isCompiling} skipping={i._bracketIfSkipping} activeDepth={i._bracketIfActiveDepth} nesting={i._bracketIfNestingDepth}");
        if (i._bracketIfActiveDepth == 0 && !i._bracketIfSkipping)
        {
            throw new ForthException(ForthErrorCode.CompileError, "[ELSE] without [IF]");
        }

        if (i._bracketIfNestingDepth > 0)
        {
            i.Trace("TOK [ELSE] nested ignored");
            return Task.CompletedTask;
        }

        if (i._bracketIfSkipping)
        {
            i._bracketIfSkipping = false;
            i._bracketIfSeenElse = true;
            i.Trace($"TOK [ELSE] resume exec isCompiling={i._isCompiling}");
        }
        else
        {
            i._bracketIfSeenElse = true;
            i._bracketIfSkipping = true;
            i.Trace($"TOK [ELSE] start skip isCompiling={i._isCompiling}");
            SkipBracketSection(i, skipElse: false);
            i.Trace($"TOK [ELSE] exit skip isCompiling={i._isCompiling} skipping={i._bracketIfSkipping}");
        }

        return Task.CompletedTask;
    }

    [Primitive("[THEN]", IsImmediate = true, HelpString = "[THEN] - end bracket conditional")]
    private static Task Prim_BRACKET_THEN(ForthInterpreter i)
    {
        i.Trace($"TOK [THEN] enter isCompiling={i._isCompiling} skipping={i._bracketIfSkipping} activeDepth={i._bracketIfActiveDepth} nesting={i._bracketIfNestingDepth}");
        if (i._bracketIfActiveDepth == 0 && !i._bracketIfSkipping)
        {
            throw new ForthException(ForthErrorCode.CompileError, "[THEN] without [IF]");
        }

        if (i._bracketIfNestingDepth > 0)
        {
            i._bracketIfNestingDepth--;
            i.Trace($"TOK [THEN] nested dec nesting={i._bracketIfNestingDepth}");
            return Task.CompletedTask;
        }

        i._bracketIfSkipping = false;
        i._bracketIfSeenElse = false;
        i._bracketIfNestingDepth = 0;

        if (i._bracketIfActiveDepth > 0)
            i._bracketIfActiveDepth--;

        i.Trace($"TOK [THEN] exit isCompiling={i._isCompiling} activeDepth={i._bracketIfActiveDepth}");
        return Task.CompletedTask;
    }

    private static void SkipBracketSection(ForthInterpreter i, bool skipElse)
    {
        // Use character parser to skip remaining tokens on current source
        int depth = 0;

        while (i.TryParseNextWord(out var tok))
        {
            if (tok.Length == 0) continue;

            // If we entered skip mode while compiling, we must still honor ';' so we don't
            // leave the interpreter stuck in compile mode.
            if (i._isCompiling)
            {
                if (i._doesCollecting)
                {
                    if (tok == ";")
                    {
                        i.FinishDefinition();
                        continue;
                    }
                }
                else if (tok == ";")
                {
                    i.FinishDefinition();
                    continue;
                }
            }

            var upper = tok.ToUpperInvariant();

            if (upper == "[IF]")
            {
                depth++;
                i._bracketIfActiveDepth++;
                continue;
            }

            if (upper == "[THEN]")
            {
                if (depth == 0)
                {
                    // Found terminating [THEN] - end the skip
                    i._bracketIfSkipping = false;
                    i._bracketIfSeenElse = false;
                    i._bracketIfNestingDepth = 0;
                    if (i._bracketIfActiveDepth > 0) i._bracketIfActiveDepth--;
                    return;
                }
                depth--;
                if (i._bracketIfActiveDepth > 0) i._bracketIfActiveDepth--;
                continue;
            }

            if (skipElse && upper == "[ELSE]" && depth == 0)
            {
                // Found [ELSE] at our depth - resume execution of ELSE part
                i._bracketIfSkipping = false;
                i._bracketIfSeenElse = true;
                return;
            }

            // Otherwise ignore the token
        }
    }

    // [IF] [ELSE] [THEN] temporarily removed until full ANS bracket conditional design is finalized.

    [Primitive("CASE", IsImmediate = true, HelpString = "CASE - start a case structure ( sel -- sel )")]
    private static Task Prim_CASE(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "CASE outside compilation");
        i._controlStack.Push(new Forth.Core.Interpreter.ForthInterpreter.CaseFrame());
        return Task.CompletedTask;
    }

    [Primitive("OF", IsImmediate = true, HelpString = "OF - compare and branch ( sel test -- sel | )")]
    private static Task Prim_OF(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "OF outside compilation");
        var cf = i._controlStack.OfType<ForthInterpreter.CaseFrame>().LastOrDefault();
        if (cf is null)
            throw new ForthException(ForthErrorCode.CompileError, "OF without CASE");
        
        // ANS Forth: at compile time, code before OF pushes the test value
        // At runtime: ( selector test -- selector | )
        // OF compares: if equal, drops both and executes branch; else drops test and continues
        
        // The test value was compiled into DefaultPart (or previous branch's tail)
        // Move it to a new branch as the test value action
        cf.CurrentBranch = new List<Func<ForthInterpreter, Task>>();
        
        // Transfer the last action from DefaultPart to this branch as the test value
        if (cf.DefaultPart.Count > 0)
        {
            var testAction = cf.DefaultPart[cf.DefaultPart.Count - 1];
            cf.DefaultPart.RemoveAt(cf.DefaultPart.Count - 1);
            cf.CurrentBranch.Add(testAction);
        }
        
        cf.OfFrame = new ForthInterpreter.OfFrame { ParentBranch = cf.CurrentBranch };
        i._controlStack.Push(cf.OfFrame);
        return Task.CompletedTask;
    }

    [Primitive("ENDOF", IsImmediate = true, HelpString = "ENDOF - end a case branch")]
    private static Task Prim_ENDOF(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "ENDOF outside compilation");
        
        // Pop the OfFrame
        if (i._controlStack.Count == 0 || i._controlStack.Peek() is not ForthInterpreter.OfFrame)
            throw new ForthException(ForthErrorCode.CompileError, "ENDOF without OF");
        i._controlStack.Pop();
        
        // Find the CaseFrame
        var cf = i._controlStack.OfType<ForthInterpreter.CaseFrame>().LastOrDefault();
        if (cf is null || cf.CurrentBranch is null)
            throw new ForthException(ForthErrorCode.CompileError, "ENDOF without CASE");
        
        // Add the branch to the list
        cf.Branches.Add(cf.CurrentBranch);
        cf.CurrentBranch = null;
        cf.OfFrame = null;
        return Task.CompletedTask;
    }

    [Primitive("ENDCASE", IsImmediate = true, HelpString = "ENDCASE - end a CASE structure")]
    private static Task Prim_ENDCASE(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "ENDCASE outside compilation");
        var cf = i._controlStack.OfType<ForthInterpreter.CaseFrame>().LastOrDefault();
        if (cf is null)
            throw new ForthException(ForthErrorCode.CompileError, "ENDCASE without CASE");
        
        // Remove CaseFrame
        while (i._controlStack.Count > 0 && i._controlStack.Peek() != cf)
            i._controlStack.Pop();
        if (i._controlStack.Count > 0)
            i._controlStack.Pop();
        
        // Compile CASE execution with ANS semantics
        // At runtime: selector is on stack when CASE execution begins
        // For each OF branch: test value was compiled before OF, branch body between OF and ENDOF
        var branches = cf.Branches.ToList();
        var defaultPart = cf.DefaultPart.ToList();
        
        i.CurrentList().Add(async ii =>
        {
            ii.EnsureStack(1, "CASE");
            
            // Try each OF branch
            // Each branch contains: [test-value-action] [body-actions...]
            // The test value action pushes the test value onto the stack
            
            foreach (var branch in branches)
            {
                if (branch.Count == 0) continue;
                
                // Peek at selector (don't pop yet)
                var selector = PrimitivesUtil.ToLong(ii.Peek());
                
                // Execute the first action in the branch (pushes test value)
                await branch[0](ii);
                
                // Now we have: ( selector test-value -- )
                ii.EnsureStack(2, "OF comparison");
                
                var testVal = PrimitivesUtil.ToLong(ii.PopInternal());
                
                if (selector == testVal)
                {
                    // Match! Drop the selector and execute branch body
                    ii.PopInternal(); // drop selector
                    for (int k = 1; k < branch.Count; k++)
                        await branch[k](ii);
                    return; // Exit CASE after executing matching branch
                }
                // No match - test value already dropped, selector still on stack, continue to next branch
            }
            
            // No OF branch matched - drop selector and execute default code
            ii.PopInternal();
            foreach (var action in defaultPart)
                await action(ii);
        });
        return Task.CompletedTask;
    }

    [Primitive("CS-PICK", HelpString = "CS-PICK ( u -- x ) - copy u-th item from control-flow stack to data stack")]
    private static Task Prim_CS_PICK(ForthInterpreter i)
    {
        i.EnsureStack(1, "CS-PICK");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"CS-PICK: negative index {u}");
        var arr = i._controlStack.ToArray();
        int idx = arr.Length - 1 - (int)u;
        if (idx < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"CS-PICK: index {u} exceeds control stack depth");
        i.Push(arr[idx]);
        return Task.CompletedTask;
    }

    [Primitive("CS-ROLL", HelpString = "CS-ROLL ( u -- ) - rotate top u+1 control-flow stack items")]
    private static Task Prim_CS_ROLL(ForthInterpreter i)
    {
        i.EnsureStack(1, "CS-ROLL");
        var u = PrimitivesUtil.ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.StackUnderflow, $"CS-ROLL: negative count {u}");
        int count = (int)u + 1;
        if (count > i._controlStack.Count) throw new ForthException(ForthErrorCode.StackUnderflow, $"CS-ROLL: not enough items on control stack");
        var temp = new Forth.Core.Interpreter.ForthInterpreter.CompileFrame[count];
        for (int k = count - 1; k >= 0; k--) temp[k] = i._controlStack.Pop();
        // Rotate left: move first to end
        if (count > 0)
        {
            var first = temp[0];
            for (int k = 0; k < count - 1; k++) temp[k] = temp[k + 1];
            temp[count - 1] = first;
        }
        // Push back in order
        for (int k = count - 1; k >= 0; k--) i._controlStack.Push(temp[k]);
        return Task.CompletedTask;
    }

    [Primitive("LOCALS|", IsImmediate = true, HelpString = "LOCALS| <name>... | - declare local variables")]
    private static Task Prim_LOCALS_BAR(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "LOCALS| outside compilation");
        var locals = new List<string>();
        while (true)
        {
            var token = i.ReadNextTokenOrThrow("LOCALS| expects names followed by |");
            if (token == "|") break;
            locals.Add(token);
        }
        i._currentLocals = locals;
        // Add the setup code
        i.CurrentList().Add(ii =>
        {
            ii._locals = new Dictionary<string, object>();
            for (int k = 0; k < locals.Count; k++)
            {
                ii._locals[locals[k]] = ii.PopInternal();
            }
            return Task.CompletedTask;
        });
        return Task.CompletedTask;
    }

    [Primitive("(LOCAL)", IsImmediate = true, HelpString = "(LOCAL) <name> - declare a local variable")]
    private static Task Prim_LOCAL(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "LOCAL outside compilation");
        var name = i.ReadNextTokenOrThrow("LOCAL expects a name");
        
        // Initialize or append to the list of locals
        if (i._currentLocals == null)
        {
            i._currentLocals = new List<string> { name };
            // Add setup code to initialize the runtime locals dictionary
            i.CurrentList().Add(ii =>
            {
                ii._locals = new Dictionary<string, object>();
                return Task.CompletedTask;
            });
        }
        else
        {
            i._currentLocals.Add(name);
        }
        
        // Add code to pop and store this local at runtime
        i.CurrentList().Add(ii =>
        {
            ii.EnsureStack(1, "(LOCAL)");
            ii._locals![name] = ii.PopInternal();
            return Task.CompletedTask;
        });
        return Task.CompletedTask;
    }

    [Primitive("'", IsImmediate = true, HelpString = "Push execution token for a word")]
    private static Task Prim_Tick(ForthInterpreter i)
    {
        var name = i.ReadNextTokenOrThrow("Expected word after '");
        if (!i.TryResolveWord(name, out var wt) || wt is null)
        {
            // Fallback: WORDLIST-generated vocab names (VOCABn) may exist under core or current module.
            if (name.StartsWith("VOCAB", StringComparison.OrdinalIgnoreCase))
            {
                if (i._dict.TryGetValue((null, name), out var wcore)) wt = wcore;
                else if (!string.IsNullOrWhiteSpace(i._currentModule)
                    && i._dict.TryGetValue((i._currentModule, name), out var wmod)) wt = wmod;
                else
                {
                    // Last resort: scan dictionary for matching name under any module.
                    foreach (var kv in i._dict)
                    {
                        var w = kv.Value;
                        if (w is null) continue;
                        if (w.Name != null && w.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            wt = w;
                            break;
                        }
                    }
                }
            }
        }

        if (wt is null)
            throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
 
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

    [Primitive("IMMEDIATE", IsImmediate = true, HelpString = "Mark the most recently defined word as immediate")]
    private static Task Prim_IMMEDIATE(ForthInterpreter i)
    {
        if (i._lastDefinedWord is null)
            throw new ForthException(ForthErrorCode.CompileError, "IMMEDIATE with no previous definition");

        i._lastDefinedWord.IsImmediate = true;
        return Task.CompletedTask;
    }

    [Primitive("POSTPONE", IsImmediate = true, HelpString = "POSTPONE <name> - compile semantics of a word")]
    private static Task Prim_POSTPONE(ForthInterpreter i)
    {
        if (!i._isCompiling)
            throw new ForthException(ForthErrorCode.CompileError, "POSTPONE outside compilation");

        var name = i.ReadNextTokenOrThrow("POSTPONE expects a name");
        if (!i.TryResolveWord(name, out var w) || w is null)
            throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");

        if (w.IsImmediate)
        {
            // Execute the compile-time behavior of the immediate word now.
            return w.ExecuteAsync(i);
        }

        // Non-immediate: compile runtime execution of the referenced word.
        i.CurrentList().Add(ii => w.ExecuteAsync(ii));
        return Task.CompletedTask;
    }
}
