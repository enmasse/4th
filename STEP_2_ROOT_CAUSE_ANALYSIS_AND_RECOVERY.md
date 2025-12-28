# Step 2 Root Cause Analysis and Recovery Plan

## Date: 2025-01-15

## Summary

Step 2 character parser migration resulted in 84/876 passing (9.6%) due to **mixing token-based and character-based parsing** in the evaluation loop.

## Root Cause

### The Problem
Several code paths in `EvalInternalAsync` still use the DEPRECATED token-based parsing methods:
- `TryReadNextToken()` - reads from token list
- `ReadNextTokenOrThrow()` - reads from token list

While the main loop uses the NEW character-based method:
- `TryParseNextWord()` - uses CharacterParser

### Specific Problem Areas

1. **ABORT handling** (line ~119-127):
   ```csharp
   if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
   {
       if (TryReadNextToken(out var maybeMsg) && ...  // <-- PROBLEM: uses token list
   ```

2. **CREATE immediate word** - uses `ReadNextTokenOrThrow` in `CorePrimitives.DictionaryVocab.cs`

3. **All immediate parsing words** (S", .", ABORT") - use `ReadNextTokenOrThrow`

### Why This Causes 783 Failures

When code mixes parsing modes:
- CharacterParser advances through the source
- Token-based code tries to read from `_tokens` list
- `_tokens` list is OUT OF SYNC with character position
- Results in missing tokens, duplicate processing, or undefined behavior

## Recovery Options

### Option A: Complete the Character Parser Migration (Recommended)

**Description**: Replace ALL remaining `TryReadNextToken` / `ReadNextTokenOrThrow` calls with character parser equivalents.

**Changes Needed**:
1. Remove ABORT special handling in evaluation loop
2. Make ABORT" primitive consume its message from character parser
3. Update immediate parsing words (S", .", CREATE) to use character parser
4. Remove ALL token list dependencies

**Pros**:
- Completes Step 2 properly
- Clean, consistent parsing approach
- Sets up for Steps 3-9

**Cons**:
- Significant additional work (3-4 hours)
- More temporary test failures
- Risky - might break more things

**Expected Result**: 400-600/876 passing after fixes

### Option B: Rollback and Take Incremental Approach

**Description**: Revert Step 2 changes and take a more gradual migration path.

**Alternative Strategy**:
1. Keep tokenizer as primary parser
2. Add character parser ONLY for specific primitives that need it (WORD, >IN)
3. Maintain hybrid mode longer
4. Migrate evaluation loop last (or never)

**Pros**:
- Lower risk
- Can fix paranoia.4th and >IN tests incrementally
- No massive regression

**Cons**:
- Hybrid complexity remains
- May never achieve full ANS Forth compliance
- Synchronization issues persist

**Expected Result**: Back to 861/876, fix issues incrementally

### Option C: Targeted Hybrid Fix (Middle Ground)

**Description**: Keep both parsers but make them cooperate better.

**Changes**:
1. Keep token-based parsing for main loop
2. Use character parser ONLY for >IN-sensitive operations
3. Add explicit synchronization points
4. Accept some tests will fail

**Pros**:
- Lower immediate risk than Option A
- Better than full rollback
- Learn from Step 2 experience

**Cons**:
- Still has synchronization complexity
- May not achieve 100% compliance
- Technical debt remains

**Expected Result**: 700-800/876 passing

## Recommendation

### Short Term: Option B (Rollback)

**Reasoning**:
- Step 2 showed the migration is MORE complex than anticipated
- Need to update 10+ immediate parsing words
- Need to handle special cases (ABORT, CREATE, etc.)
- Risk of cascading failures is high

**Action Plan**:
1. Revert Step 2 changes (keep CharacterParser.cs)
2. Return to 861/876 baseline
3. Fix paranoia.4th with targeted token/character sync
4. Fix >IN tests with localized character parser use
5. Document lessons learned

### Long Term: Revisit Full Migration

After fixing the 6 remaining failures with hybrid approach:
- Re-evaluate if full migration is worth it
- Consider if 876/876 passing is achievable without it
- May decide hybrid is "good enough" for 100% tests passing

## Implementation: Rollback to Pre-Step-2 State

### Files to Revert

1. `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`
   - Remove CharacterParser initialization
   - Remove TryParseNextWord loop
   - Restore TryReadNextToken loop
   - Remove parse buffer logic
   - Keep token preprocessing

2. `src/Forth.Core/Interpreter/ForthInterpreter.Parsing.cs`
   - Remove _parseBuffer field
   - Revert TryParseNextWord changes (remove buffer check)

### Files to Keep

1. `src/Forth.Core/Interpreter/CharacterParser.cs` - Keep for future use
2. All documentation files - Keep for reference

### Expected Outcome

- Test pass rate: Back to 861/876 (98.3%)
- Remaining 6 failures: Same as before Step 2
- Clean slate to attempt targeted fixes

## Lessons Learned

1. **Underestimated Complexity**: Character parser migration touches MORE code than expected
2. **Immediate Words are Hard**: Parsing words that consume input need special handling
3. **All-or-Nothing**: Can't mix parsing modes in evaluation loop
4. **Test More Frequently**: Should have tested after EACH sub-step, not all at once
5. **Hybrid is Tricky**: Synchronization between parsers is inherently fragile

## Next Steps

1. **Decision Point**: Choose Option A, B, or C
2. **Execute**: Follow chosen option's action plan
3. **Test**: Run health check after each change
4. **Document**: Update TODO.md with final outcome

## Files Created This Session

- `SESSION_SUMMARY_STEP_2_MIGRATION.md` - Initial Step 2 summary
- `STEP_2_ROOT_CAUSE_ANALYSIS_AND_RECOVERY.md` - This file

## Current State

- **Code**: Step 2 changes applied, failing
- **Tests**: 84/876 passing (9.6%)
- **Status**: Blocked, waiting for decision on recovery path
