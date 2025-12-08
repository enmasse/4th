# Session Summary: Hybrid Parser Synchronization Fix (Option C)

## Date: 2025-01-15

## Goal
Fix synchronization issues in the hybrid token/character-based parser to resolve:
1. paranoia.4th "IF outside compilation" errors (2 failing tests)
2. >IN manipulation test failures (4 failing + 9 skipped = 13 tests)

## Target
Achieve 876/876 tests passing (100%)

## Work Completed

### Step 1: Fixed WORD Primitive Synchronization ?
**File**: `src/Forth.Core/Execution/CorePrimitives.IOFormatting.cs`

**Problem**: WORD primitive was only advancing `_tokenIndex` by 1, even when parsing multiple tokens' worth of characters.

**Solution**: Modified WORD to skip ALL tokens that start before the new >IN position after parsing:
```csharp
// Synchronize token index with the new >IN position
// Skip all tokens that start before the new >IN position
if (i._tokens != null && i._tokenCharPositions != null)
{
    while (i._tokenIndex < i._tokens.Count && 
           i._tokenIndex < i._tokenCharPositions.Count)
    {
        var tokenPos = i._tokenCharPositions[i._tokenIndex];
        if (tokenPos >= newIn)
        {
            // This token starts at or after >IN, stop skipping
            break;
        }
        // This token was consumed by WORD, skip it
        i._tokenIndex++;
    }
}
```

**Result**: WORD now properly synchronizes token index with character position.

### Step 2: Attempted TryReadNextToken >IN Advancement ? (Reverted)
**File**: `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`

**Problem**: Tried to advance >IN after each token read to keep it synchronized.

**Issue Found**: Advancing >IN during token-based parsing breaks immediate words like `S"` that expect their tokens to still be available via ReadNextTokenOrThrow().

**Solution**: Reverted the >IN advancement. Left comment explaining why >IN is NOT advanced during normal token parsing:
```csharp
// NOTE: We do NOT advance >IN here during normal token-based parsing
// >IN is only meaningful for character-based parsing (like WORD primitive)
// When WORD is called, it will synchronize >IN with its character-level parsing
// and update _tokenIndex to match. This keeps the two parsing modes in sync
// without breaking immediate words like S" that expect tokens to be available.
```

**Result**: Back to 861/876 passing tests (same as before).

## Current Status

### Test Results: 861/876 (98.3%)
- **Passing**: 861 tests ?
- **Failing**: 6 tests ?
  - 2 paranoia.4th tests - "IF outside compilation" (still failing)
  - 4 >IN manipulation tests - need different approach
- **Skipped**: 9 tests (need to be unskipped)

### Analysis

**paranoia.4th Still Failing**: The WORD synchronization fix alone is not sufficient. The error still occurs because:
1. WORD now synchronizes better, but the synchronization happens AFTER many characters have been consumed
2. The bracket conditional skip logic still gets confused when many `[undefined]` checks occur
3. Need additional synchronization at the evaluation loop level

**>IN Tests Still Failing**: These require >IN to be maintained and advanced during normal parsing, but doing so breaks immediate words. Need a different approach.

## Next Steps (To Continue)

### Approach 1: Enhanced Synchronization Points
Add synchronization checks at more points in the evaluation loop:
1. Before executing each word, check if >IN and _tokenIndex are aligned
2. After WORD execution, verify synchronization
3. In bracket conditional skip mode, maintain synchronization more aggressively

### Approach 2: Dual-Mode >IN Handling
Separate >IN behavior for:
1. **Token mode**: >IN stays at 0 or is managed implicitly
2. **Character mode**: >IN is explicitly managed when WORD or >IN@ is used
3. Add a flag `_characterModeActive` to track which mode we're in

### Approach 3: Complete Character Parser Migration
Proceed with the full character-based parser migration (original plan):
- Replace token-based parsing entirely
- Use CharacterParser class for all parsing
- Eliminates synchronization issues by having only one parsing model

## Recommendation

I recommend **Approach 3** (complete migration to character-based parser):
- Approaches 1 and 2 are band-aids that add complexity
- The synchronization problem is fundamental to having two parsing models
- Character-based parsing is the ANS Forth standard
- We've already created the CharacterParser class foundation
- Est. 4-6 hours for full migration vs. 2-3 hours for more band-aids

## Files Modified This Session

1. ? `src/Forth.Core/Execution/CorePrimitives.IOFormatting.cs` - Enhanced WORD synchronization
2. ? `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs` - Added comment explaining >IN handling
3. ? `src/Forth.Core/Interpreter/CharacterParser.cs` - Created (foundation for full migration)
4. ? `src/Forth.Core/Interpreter/ForthInterpreter.cs` - Added _parser field
5. ? `CHARACTER_PARSER_MIGRATION_STATUS.md` - Documented migration options

## Key Insights

1. **Hybrid parsing is inherently problematic**: Having two parsing models that need to stay synchronized is complex and error-prone.

2. **Immediate words break >IN synchronization**: Any attempt to advance >IN during token parsing breaks immediate words like `S"` that expect tokens to be available.

3. **WORD synchronization helps but isn't enough**: The WORD fix improves synchronization but doesn't solve the fundamental issue of paranoia.4th having hundreds of `[undefined]` checks in skipped sections.

4. **>IN is character-level, tokens are word-level**: This mismatch is the root cause. ANS Forth expects character-level parsing, but we're using word-level tokens for performance.

5. **Full character parser is the right solution**: Rather than continuing to patch the hybrid approach, we should complete the migration to pure character-based parsing.

## To Resume This Session

1. Review this summary and CHARACTER_PARSER_MIGRATION_STATUS.md
2. Decide on approach:
   - Option A: Proceed with full character parser migration (recommended)
   - Option B: Try enhanced synchronization points (more complexity)
   - Option C: Try dual-mode >IN handling (even more complexity)
3. If choosing Option A, follow the 9-step plan in the previous planning session
4. Current codebase is in working state (861/876 passing)

## Critical Context for Restart

- **CharacterParser class exists** in `src/Forth.Core/Interpreter/CharacterParser.cs`
- **New parsing methods added** to ForthInterpreter: `TryParseNextWord()`, `ParseNextWordOrThrow()`
- **WORD primitive enhanced** with better token/character synchronization
- **Do NOT advance >IN in TryReadNextToken** - breaks immediate words
- **paranoia.4th pattern**: Many `[undefined] word [if] ... [then]` checks that call WORD and modify >IN
- **Test pass rate**: Started at 861/876, still at 861/876 after changes

---

**Session End**: Ready to continue with full character parser migration or alternative approach.
