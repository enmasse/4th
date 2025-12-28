# Option A: Complete Character Parser Migration - Action Plan

## Date: 2025-01-15

## Goal
Replace ALL token-based parsing with character-based parsing to achieve full ANS Forth compliance and 876/876 tests passing.

## Current State
- **Test Pass Rate**: 84/876 (9.6%)
- **Root Cause**: Mixing token-based and character-based parsing
- **Code State**: Step 2 changes applied, needs completion

## Strategy: Incremental Replacement with Testing

### Phase 1: Remove ABORT Special Handling (30 min)

**Current Problem**:
```csharp
if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
{
    if (TryReadNextToken(out var maybeMsg) && ...  // <-- Uses token list
```

**Solution**: ABORT" primitive already handles this. Remove special case.

**Changes**:
1. Remove ABORT special handling from interpret mode (lines ~119-127)
2. Let ABORT" primitive handle message parsing
3. Test: ABORT and ABORT" primitives still work

**Expected Impact**: +50-100 tests passing

---

### Phase 2: Update S" Primitive (30 min)

**Current**: Uses `ReadNextTokenOrThrow()` to get quoted string token

**Solution**: Make S" consume string from character parser directly

**File**: `src/Forth.Core/Execution/CorePrimitives.DictionaryVocab.cs`

**Change**:
```csharp
[Primitive("S\"", IsImmediate = true)]
private static Task Prim_SQUOTE(ForthInterpreter i)
{
    // OLD: var next = i.ReadNextTokenOrThrow("Expected text after S\"");
    
    // NEW: Read until closing quote from character parser
    if (i._parser == null) throw new ForthException(...);
    
    var str = i._parser.ReadUntil('"');
    
    if (!i._isCompiling)
    {
        var addr = i.AllocateCountedString(str);
        i.Push(addr + 1);
        i.Push((long)str.Length);
    }
    else
    {
        i.CurrentList().Add(ii => {
            var addr = ii.AllocateCountedString(str);
            ii.Push(addr + 1);
            ii.Push((long)str.Length);
            return Task.CompletedTask;
        });
    }
    return Task.CompletedTask;
}
```

**Wait - Actually S" is already handled by CharacterParser!**

Looking at CharacterParser.cs line ~183-206, S" is already parsed as a composite token. The primitive just needs to NOT call ReadNextTokenOrThrow.

**Revised Solution**: S" primitive already gets the string from the token that CharacterParser created. No change needed! The issue is that it tries to read ANOTHER token after that.

Let me check the actual S" implementation...

---

### Phase 3: Update ." Primitive (20 min)

**Similar to S"** - needs to work with character parser

**File**: `src/Forth.Core/Execution/CorePrimitives.DictionaryVocab.cs`

---

### Phase 4: Update CREATE Primitive (45 min)

**Current**: Uses `ReadNextTokenOrThrow()` and `TryReadNextToken()`

**Challenge**: CREATE is state-smart (immediate vs compile mode)

**File**: `src/Forth.Core/Execution/CorePrimitives.DictionaryVocab.cs` (lines ~89-180)

**Solution**: Replace all ReadNextTokenOrThrow with character parser reads

---

### Phase 5: Update All Other Immediate Primitives (60 min)

**Primitives to update**:
- CHAR
- [CHAR]
- [']
- TO
- MODULE
- END-MODULE
- USING
- LOAD-ASM
- LOAD-ASM-TYPE
- VARIABLE
- CONSTANT
- VALUE
- DEFER
- IS
- MARKER
- FORGET
- SEE
- BIND

**Pattern**:
```csharp
// OLD:
var name = i.ReadNextTokenOrThrow("Expected name after WORD");

// NEW:
if (i._parser == null) throw new ForthException(...);
i._parser.SkipWhitespace();
var name = i._parser.ParseNext();
if (name == null) throw new ForthException("Expected name after WORD");
```

---

### Phase 6: Remove ReadNextTokenOrThrow Method (15 min)

Once all callers are updated, remove the deprecated method.

**File**: `src/Forth.Core/Interpreter/ForthInterpreter.Parsing.cs`

**Action**: Delete or mark obsolete

---

### Phase 7: Remove TryReadNextToken Method (15 min)

**File**: `src/Forth.Core/Interpreter/ForthInterpreter.Parsing.cs`

**Action**: Delete or mark obsolete

---

### Phase 8: Test and Fix Regressions (90-120 min)

**Strategy**:
1. Run health check after EACH phase
2. Document failures
3. Fix incrementally
4. Don't move to next phase until previous stabilizes

**Expected failure categories**:
- String parsing issues (S", .", ABORT")
- Name parsing issues (CREATE, VARIABLE, etc.)
- Quoted string handling
- Bracket conditionals (already partially broken)

---

### Phase 9: Remove Token Infrastructure (30 min)

Once all tests passing, remove deprecated token-based code:
1. Remove `_tokens` field
2. Remove `_tokenIndex` field
3. Remove `_tokenCharPositions` field
4. Remove Tokenizer.cs usage from evaluation loop
5. Keep Tokenizer.cs for bracket conditional skip mode (or refactor that too)

---

### Phase 10: Final Testing and Documentation (30 min)

1. Run full test suite: `.\health.ps1`
2. Expected: 876/876 passing (100%)
3. Update TODO.md
4. Create completion summary document
5. Celebrate! ??

---

## Detailed Implementation: Phase 1 (Remove ABORT Special Handling)

### Step 1.1: Examine Current Code

**File**: `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`

**Lines**: ~119-127 (interpret mode)

```csharp
if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
{
    if (TryReadNextToken(out var maybeMsg) && maybeMsg.Length >= 2 && maybeMsg[0] == '"' && maybeMsg[^1] == '"')
    {
        throw new ForthException(ForthErrorCode.Unknown, maybeMsg[1..^1]);
    }
    throw new ForthException(ForthErrorCode.Unknown, "ABORT");
}

if (tok.StartsWith("ABORT\"", StringComparison.OrdinalIgnoreCase))
{
    var msg = tok[6..^1];
    throw new ForthException(ForthErrorCode.Unknown, msg);
}
```

### Step 1.2: Understand Why This Exists

This code handles two cases:
1. `ABORT` alone - throws "ABORT" message
2. `ABORT "message"` - where message is a separate quoted token

**BUT**: CharacterParser already handles ABORT" as a composite token (line ~216-234 in CharacterParser.cs)

The ABORT" primitive (CorePrimitives.DictionaryVocab.cs) compiles the message into the word body.

### Step 1.3: The Fix

**Simply remove both special cases!**

ABORT is a regular word that will be resolved and executed.
ABORT" is handled by CharacterParser and the primitive.

**Action**: Delete lines ~119-132 in ForthInterpreter.Evaluation.cs

### Step 1.4: Expected Result

- ABORT still works (word lookup succeeds, primitive throws)
- ABORT" still works (CharacterParser creates token, primitive compiles)
- No special case code needed
- +50-100 tests should start passing

---

## Testing Strategy

### After Each Phase:

```powershell
# Quick build
dotnet build --no-incremental

# Run tests
.\health.ps1

# Count passing
# Document failures
# Analyze root causes
# Fix or move to next phase
```

### Success Criteria:

**Phase 1**: 150-200/876 passing
**Phase 2-5**: Gradual improvement, 300-600/876
**Phase 6-8**: Major improvements, 700-850/876  
**Phase 9**: Cleanup, maintain pass rate
**Phase 10**: 876/876 passing (100%) ??

---

## Risk Mitigation

### If Pass Rate Drops Below 50/876:

1. **STOP** - Something fundamentally broken
2. Revert last phase
3. Analyze what broke
4. Try different approach
5. Document and adjust plan

### If Pass Rate Stalls (No Improvement for 2 Phases):

1. Analyze failures in detail
2. Identify common patterns
3. May need to fix test expectations
4. May need to adjust CharacterParser behavior

### If Time Exceeds Estimate:

1. Document progress
2. Take break
3. Resume with fresh perspective
4. Consider hybrid approach for remaining issues

---

## Rollback Plan

If Option A proves too difficult:
1. Revert all changes to Evaluation.cs
2. Revert changes to primitive files
3. Return to 861/876 baseline
4. Execute Option B (Rollback and incremental fixes)

---

## Timeline

**Optimistic**: 4 hours
**Realistic**: 6 hours
**Pessimistic**: 8-10 hours (includes debugging and fixes)

**Recommended**: Work in 2-hour blocks with breaks

---

## Let's Begin!

Starting with **Phase 1: Remove ABORT Special Handling**

This is the lowest-risk, highest-impact change. Let's do it!
