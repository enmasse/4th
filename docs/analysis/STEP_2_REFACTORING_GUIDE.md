# Step 2 Guide: Refactor EvalInternalAsync to Use CharacterParser

## Overview

This document provides a detailed guide for refactoring `EvalInternalAsync` from token-based parsing to character-based parsing using `CharacterParser`. This is the most critical step in the migration.

## Current State Analysis

### Current Token-Based Flow
1. Tokenize entire line upfront (`Tokenizer.Tokenize(line)`)
2. Preprocess tokens (handle `[']`, bracket conditionals)
3. Loop through tokens with `TryReadNextToken()`
4. Token preprocessing handles special forms upfront

### Target Character-Based Flow
1. Initialize CharacterParser with source line
2. Parse one word at a time with `parser.ParseNext()`
3. >IN automatically updated as characters consumed
4. No preprocessing - parser handles special forms on-demand

## Key Challenges

### 1. Token Preprocessing
**Current**: Preprocesses `[']` and bracket forms before evaluation
**Solution**: CharacterParser handles these during parsing OR handle in evaluation loop

### 2. Bracket Conditional Skipping
**Current**: Uses token index to skip forward
**Solution**: CharacterParser can skip character-by-character, need to update `ProcessSkippedLine`

### 3. Immediate Word Body Expansion
**Current**: `_tokens.InsertRange(_tokenIndex, cw.BodyTokens)`
**Solution**: Need buffering mechanism or change immediate word expansion approach

### 4. Quoted String Auto-Push
**Current**: Checks token format `tok[0] == '"'`
**Solution**: CharacterParser returns string tokens already, logic may need adjustment

## Refactoring Steps

### Step 2.1: Initialize CharacterParser

**Location**: `EvalInternalAsync`, after setting `_currentSource`

```csharp
// OLD CODE (remove):
_tokens = Tokenizer.Tokenize(line);
_tokenCharPositions = ComputeTokenPositions(line, _tokens);

// NEW CODE (add):
// Initialize character-based parser
_parser = new CharacterParser(_currentSource);
_parser.SetPosition((int)_mem[_inAddr]); // Sync with >IN
```

### Step 2.2: Replace Token Loop

**Location**: Main `while` loop in `EvalInternalAsync`

```csharp
// OLD CODE:
while (TryReadNextToken(out var tok))

// NEW CODE:
while (TryParseNextWord(out var tok))
```

**Note**: `TryParseNextWord` already exists in `ForthInterpreter.Parsing.cs` and handles:
- Synchronizing with >IN
- Calling `_parser.ParseNext()`
- Updating >IN after parse

### Step 2.3: Remove Token Preprocessing

**Location**: Before main loop in `EvalInternalAsync`

```csharp
// REMOVE THIS ENTIRE BLOCK:
if (_tokens is not null && _tokens.Count > 0)
{
    var processed = new List<string>();
    for (int ti = 0; ti < _tokens.Count; ti++)
    {
        var t = _tokens[ti];
        if (t == "[']") { ... }
        if (t == "[" && ti + 2 < _tokens.Count && _tokens[ti + 2] == "]") { ... }
        processed.Add(t);
    }
    _tokens = processed;
}

_tokenIndex = 0; // Also remove
```

**Reason**: CharacterParser handles bracket forms directly during parsing

### Step 2.4: Update Bracket Conditional Skip Logic

**Challenge**: Current skip logic uses `TryReadNextToken()` and token index

**Options**:
1. Keep using tokenizer for skip mode (hybrid approach during migration)
2. Update `ProcessSkippedLine` to use character parser
3. Modify bracket conditional handling to work with character positions

**Recommended**: Option 1 (hybrid) for initial migration, then Option 2

```csharp
// In bracket skip section of interpret mode:
if (_bracketIfSkipping)
{
    // HYBRID APPROACH: Continue using token-based skip for now
    // Will refactor this in Step 4
    // Re-tokenize for skip mode
    _tokens = Tokenizer.Tokenize(_currentSource);
    _tokenCharPositions = ComputeTokenPositions(_currentSource, _tokens);
    _tokenIndex = 0;
    return await ProcessSkippedLine();
}
```

### Step 2.5: Handle Immediate Word Body Expansion

**Challenge**: Current code does `_tokens.InsertRange()` to expand immediate words

**Options**:
1. Keep tokens list temporarily for this feature
2. Change immediate word expansion to push tokens onto a buffer
3. Re-parse immediate word body each time (inefficient)

**Recommended**: Option 2 - Add parse buffer

```csharp
// Add to ForthInterpreter.Parsing.cs:
internal Queue<string>? _parseBuffer;

// Modify TryParseNextWord:
internal bool TryParseNextWord(out string word)
{
    // Check buffer first
    if (_parseBuffer != null && _parseBuffer.Count > 0)
    {
        word = _parseBuffer.Dequeue();
        return true;
    }
    
    // ... rest of existing logic
}

// In compilation mode where immediate words expanded:
if (cw.BodyTokens is { Count: > 0 })
{
    // OLD: _tokens!.InsertRange(_tokenIndex, cw.BodyTokens);
    // NEW: Add to parse buffer
    _parseBuffer ??= new Queue<string>();
    foreach (var token in cw.BodyTokens)
    {
        _parseBuffer.Enqueue(token);
    }
    continue;
}
```

### Step 2.6: Update ProcessSkippedLine (Step 4 work)

**Note**: This is complex enough to be its own step. For Step 2, use hybrid approach.

## Testing Strategy

### Expected Failures After Step 2

**Estimate**: 200-400 tests will fail initially

**Categories of Failures**:
1. **Bracket conditional tests** - Skip logic needs update
2. **Immediate word tests** - Body expansion needs buffer
3. **>IN tests** - Should start PASSING! (this is the goal)
4. **File loading tests** - May have issues with multi-line constructs

### Health Check After Step 2

```powershell
.\health.ps1
```

**Success Criteria**:
- Build succeeds
- Some tests still pass (target: >400/876)
- >IN tests show improvement
- No infinite loops or crashes

### Incremental Testing

1. Test basic evaluation: `2 3 + .`
2. Test word definition: `: TEST 42 ; TEST .`
3. Test immediate words: `[ 1 2 + ] LITERAL .`
4. Test bracket conditionals: `0 [IF] .S [THEN]`
5. Test file loading: `"prelude.4th" INCLUDED`

## Rollback Plan

If Step 2 breaks too much (>600 failures):

1. Revert `EvalInternalAsync` changes
2. Keep `CharacterParser.cs` and helper methods
3. Document what broke
4. Refine approach based on failures
5. Try again with more hybrid approach

## Next Steps After Step 2

1. **Step 3**: Update immediate parsing words (S", .", ABORT")
2. **Step 4**: Refactor bracket conditional skip logic
3. **Step 5**: Update SAVE-INPUT/RESTORE-INPUT
4. **Step 6**: Remove Tokenizer completely
5. **Step 7**: Update remaining tests
6. **Step 8**: Full test suite pass
7. **Step 9**: Documentation and cleanup

## Key Files to Modify

1. `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs` - Main refactoring
2. `src/Forth.Core/Interpreter/ForthInterpreter.Parsing.cs` - Add parse buffer
3. `src/Forth.Core/Interpreter/ForthInterpreter.BracketConditionals.cs` - Update skip logic (Step 4)

## Important Notes

?? **This is a breaking change** - Many tests will fail temporarily

? **Expected outcome** - Foundation for 100% ANS Forth compliance

?? **Goal** - Make >IN work correctly for all ANS Forth patterns

?? **Document everything** - Track what breaks and why

## Commit Strategy

1. **Before Step 2**: Commit current state (already done in PRE_MIGRATION_STATE.md)
2. **After Step 2**: Commit with message "WIP: Character parser migration Step 2 - expect test failures"
3. **After fixes**: Commit each sub-step fix separately
4. **Final**: "Complete character parser migration - 876/876 tests passing"

## Summary

This guide provides the roadmap for Step 2. The refactoring is significant but methodical:

- Remove token preprocessing
- Replace token loop with character parser loop
- Handle special cases (skip mode, immediate words)
- Test incrementally
- Fix regressions systematically

The end result will be a cleaner, more ANS Forth compliant parser that eliminates the token/character synchronization issues.
