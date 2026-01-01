# Complete Character-Based Parser Migration - Comprehensive Plan

**Date**: 2025-01-XX  
**Goal**: Eliminate hybrid token/character parser architecture and implement pure character-based parsing  
**Expected Outcome**: 878/878 tests passing (100%), all >IN tests working

---

## Executive Summary

This document outlines the complete migration from the current hybrid token/character-based parser to a pure character-based parser. This migration will:

1. ? **Fix all >IN manipulation tests** (5 skipped tests)
2. ? **Fix REFILL tests** (2 skipped tests)
3. ? **Eliminate token/character synchronization issues**
4. ? **Achieve 100% ANS Forth compliance** for parsing
5. ? **Simplify codebase** by removing hybrid architecture

---

## Current State Analysis

### Tokenizer Dependencies (DEPRECATED - To Remove)

**Files Using Token-Based Parsing**:
1. `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`
   - `_tokens` field (DEPRECATED)
   - `_tokenIndex` field (DEPRECATED)
   - `TryReadNextToken()` method (DEPRECATED)
   
2. `src/Forth.Core/Interpreter/Tokenizer.cs`
   - **File to be deleted** after migration
   - Currently handles all token preprocessing

3. `4th.Tests/Core/Tokenizer/TokenizerTests.cs`
   - **File to be migrated** to CharacterParserTests.cs

### Character Parser Infrastructure (NEW - Already Complete)

**Completed**:
- ? `src/Forth.Core/Interpreter/CharacterParser.cs` (character-level parser)
- ? `_parser` field in ForthInterpreter
- ? `TryParseNextWord()` method (NEW)
- ? `ParseNextWordOrThrow()` method (NEW)

### Immediate Parsing Primitives Using Token-Based (To Migrate)

**Already Migrated** (35+ primitives):
- ? MODULE, USING, LOAD-ASM (Session 1)
- ? CREATE, DEFER, SEE (Session 1)
- ? S", .", ABORT" (Session 1)
- ? MARKER, BIND, etc. (Session 1)
- ? APPEND-FILE, READ-FILE, FILE-EXISTS, OPEN-FILE, INCLUDE, LOAD-FILE (Session 2)
- ? RUN-NEXT (Session 2)
- ? VARIABLE, CONSTANT, VALUE, TO, IS, CHAR, FORGET (Session 3)
- ? IL{ (Session 4)

**Remaining** (0 primitives):
- None! All immediate parsing words already use character parser

### >IN Integration Points

**Current State**:
- ? >IN primitive returns correct address
- ? SOURCE primitive returns character address
- ? Character parser syncs with >IN
- ? Token-based loop doesn't respect >IN changes
- ? WORD primitive uses hybrid logic
- ? 5 >IN tests skipped

---

## Migration Steps (10 Steps)

### Step 1: Audit Dependencies ? COMPLETE
- [x] Identify all token-based code
- [x] Document immediate parsing words
- [x] List test files to update
- [x] Create removal checklist

### Step 2: Refactor EvalInternalAsync Main Loop
**File**: `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`

**Changes**:
1. Remove `Tokenizer.Tokenize()` call
2. Remove token preprocessing logic (already in CharacterParser)
3. Remove `_parseBuffer` token queue
4. Keep only character parser loop
5. Remove token-index based bracket conditional skip (replaced with character-based)

**Current Token-Based Code** (~100 lines to remove):
```csharp
// REMOVE: Token preprocessing
_tokens = Tokenizer.Tokenize(_currentSource);
_tokenIndex = 0;
_parseBuffer?.Clear();

// REMOVE: Token loop
while (TryReadNextToken(out var tok))
```

**New Character-Based Code** (already exists):
```csharp
// KEEP: Character parser initialization
_parser = new CharacterParser(_currentSource);
_parser.SetPosition((int)_mem[_inAddr]);

// KEEP: Character parser loop
while (TryParseNextWord(out var tok))
```

### Step 3: Update WORD Primitive
**File**: `src/Forth.Core/Execution/CorePrimitives.IOFormatting.cs`

**Current Issues**:
- Uses token synchronization hack
- Skips tokens when >IN changes
- Complex fallback logic

**New Implementation**:
```csharp
[Primitive("WORD", HelpString = "WORD ( char \"<chars>ccc<char>\" -- c-addr )")]
private static Task Prim_WORD(ForthInterpreter i)
{
    i.EnsureStack(1, "WORD");
    var delim = (char)ToLong(i.PopInternal());
    
    // SIMPLIFIED: Pure character-based parsing
    if (i._parser != null)
    {
        var parsedWord = i._parser.ParseWord(delim);
        i.MemSet(i.InAddr, (long)i._parser.Position);
        var addr = i.AllocateCountedString(parsedWord);
        i.Push(addr);
        return Task.CompletedTask;
    }
    
    // Fallback for non-parser contexts
    throw new ForthException(ForthErrorCode.CompileError, "WORD requires active parser");
}
```

### Step 4: Update Bracket Conditionals
**File**: `src/Forth.Core/Execution/CorePrimitives.Compilation.cs`

**Changes**:
- Remove token-index based skip logic
- Use pure character parser position
- Simplify SkipBracketSection (already uses character parser)

**Current State**: Already mostly character-based  
**Remaining**: Remove any token-index references in skip logic

### Step 5: Remove Tokenizer Infrastructure
**Files to Modify**:
1. `src/Forth.Core/Interpreter/ForthInterpreter.cs`
   - Remove `_tokens` field
   - Remove `_tokenIndex` field
   - Remove `_parseBuffer` field (if still used for tokens)

2. `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs`
   - Remove `TryReadNextToken()` method
   - Remove `ReadNextTokenOrThrow()` method
   - Remove any token-based fallback paths

3. `src/Forth.Core/Interpreter/Tokenizer.cs`
   - **DELETE FILE** entirely

### Step 6: Update INCLUDE/LOAD Primitives
**File**: `src/Forth.Core/Execution/CorePrimitives.FileIO.cs`

**Current State**: Already use character parser (whole-file evaluation)  
**Changes**: Verify no token dependencies remain

### Step 7: Unskip and Fix >IN Tests
**File**: `4th.Tests/Core/Parsing/InPrimitiveRegressionTests.cs`

**Tests to Unskip** (5 tests):
1. `In_WithWord` - WORD character consumption
2. `In_Rescan_Pattern` - Rescan via >IN ! 0
3. `In_WithSourceAndType` - /STRING and TYPE with >IN
4. `In_WithEvaluate` - >IN with EVALUATE
5. `In_WithSaveRestore` - SAVE-INPUT/RESTORE-INPUT

**Expected Fixes**:
- WORD will now update >IN correctly (pure character-based)
- Rescan pattern will work (character parser can reset position)
- SOURCE/TYPE will work (character addresses)
- EVALUATE will work (source stack with per-context >IN)
- SAVE/RESTORE will work (serializes character position)

### Step 8: Migrate Tokenizer Tests
**File**: `4th.Tests/Core/Tokenizer/TokenizerTests.cs`

**Options**:
A. Rename to `CharacterParserTests.cs` and test CharacterParser
B. Delete tests (CharacterParser already well-tested)
C. Keep minimal tests for backwards compatibility

**Recommendation**: Option A - migrate tests to CharacterParser

**New Test Structure**:
```csharp
public class CharacterParserTests
{
    [Fact]
    public void CharacterParser_ParsesWords()
    {
        var parser = new CharacterParser("WORD1 WORD2 WORD3");
        Assert.True(parser.TryParseNext(out var tok1));
        Assert.Equal("WORD1", tok1);
        Assert.True(parser.TryParseNext(out var tok2));
        Assert.Equal("WORD2", tok2);
        Assert.True(parser.TryParseNext(out var tok3));
        Assert.Equal("WORD3", tok3);
    }
    
    [Fact]
    public void CharacterParser_HandlesStringLiterals()
    {
        var parser = new CharacterParser("S\" hello\"");
        Assert.True(parser.TryParseNext(out var tok1));
        Assert.Equal("S\"", tok1);
        Assert.True(parser.TryParseNext(out var tok2));
        Assert.Equal("\"hello\"", tok2);
    }
    
    // ... more tests for comments, special forms, etc.
}
```

### Step 9: Run Full Test Suite and Fix Regressions
**Build and Test**:
```bash
dotnet build
dotnet test --no-build
```

**Expected Results**:
- **Before Migration**: 864/878 passing (98.6%)
- **After Step 2-5**: ~400-600/878 passing (50-70%) - temporary breakage
- **After Step 6-7**: ~850-870/878 passing (97-99%)
- **After Step 8**: 878/878 passing (100%) ? TARGET

**Common Regressions to Fix**:
1. String literal handling edge cases
2. Comment parsing in complex scenarios
3. Bracket conditional nesting
4. DOES> token collection
5. ABORT" error message parsing

### Step 10: Update Documentation
**Files to Update**:
1. `TODO.md`
   - Mark character parser migration complete
   - Remove hybrid parser documentation
   - Update >IN test status

2. Create `CHANGELOG_COMPLETE_CHARACTER_PARSER_MIGRATION.md`
   - Document all changes
   - Show before/after test results
   - List breaking changes (if any)

3. `README.md`
   - Update parsing model description
   - Note ANS Forth compliance improvements

---

## Risk Mitigation

### High-Risk Areas

1. **Main Evaluation Loop** (Step 2)
   - **Risk**: Breaking 400+ tests temporarily
   - **Mitigation**: Work in feature branch, commit frequently
   - **Rollback**: Keep backup of ForthInterpreter.Evaluation.cs

2. **WORD Primitive** (Step 3)
   - **Risk**: Many tests depend on WORD behavior
   - **Mitigation**: Extensive testing of delimiter parsing
   - **Rollback**: Keep old implementation commented out

3. **Bracket Conditionals** (Step 4)
   - **Risk**: Multi-line conditional handling
   - **Mitigation**: Already mostly character-based
   - **Rollback**: Revert SkipBracketSection changes

### Medium-Risk Areas

4. **Tokenizer Removal** (Step 5)
   - **Risk**: Missing dependencies discovered late
   - **Mitigation**: Thorough grep for "Tokenizer" references
   - **Rollback**: Restore Tokenizer.cs from git

5. **>IN Tests** (Step 7)
   - **Risk**: Unexpected edge cases in >IN manipulation
   - **Mitigation**: Tests already written, just unskip
   - **Rollback**: Re-skip tests if still failing

### Low-Risk Areas

6. **Test Migration** (Step 8)
   - **Risk**: None - tests are independent
   - **Mitigation**: N/A
   - **Rollback**: Keep old tests

---

## Timeline Estimate

| Step | Description | Time | Status |
|------|-------------|------|--------|
| 1 | Audit dependencies | 30 min | ? COMPLETE |
| 2 | Refactor EvalInternalAsync | 1-2 hours | ? PENDING |
| 3 | Update WORD primitive | 30 min | ? PENDING |
| 4 | Update bracket conditionals | 30 min | ? PENDING |
| 5 | Remove Tokenizer | 30 min | ? PENDING |
| 6 | Verify INCLUDE/LOAD | 15 min | ? PENDING |
| 7 | Unskip >IN tests | 1 hour | ? PENDING |
| 8 | Migrate tokenizer tests | 1 hour | ? PENDING |
| 9 | Fix regressions | 2-4 hours | ? PENDING |
| 10 | Documentation | 30 min | ? PENDING |
| **TOTAL** | **Full migration** | **7-10 hours** | **IN PROGRESS** |

---

## Success Criteria

### Must Have ?
1. ? All 878 tests passing (100%)
2. ? All 5 >IN tests passing (unskipped)
3. ? No Tokenizer.cs in codebase
4. ? No token-based parsing code remaining
5. ? Character parser used exclusively

### Nice to Have ?
1. ? Improved performance (character parsing is faster)
2. ? Cleaner codebase (less synchronization logic)
3. ? Better error messages (character-level positions)
4. ? Full REFILL support (2 tests unskipped)

---

## Rollback Plan

If migration fails or introduces critical bugs:

### Immediate Rollback (< 1 hour)
```bash
git checkout main
git branch -D character-parser-migration
```

### Partial Rollback (Keep CharacterParser)
1. Restore `Tokenizer.cs` from main
2. Restore `ForthInterpreter.Evaluation.cs` from main
3. Keep `CharacterParser.cs` for future use
4. Re-skip >IN tests

### Forward Fix (Recommended)
1. Record observations for each regression
2. Fix regressions incrementally
3. Commit fixes frequently
4. Don't rollback unless >5 hours spent debugging

---

## Next Actions

**Immediate** (Start now):
1. ? Create feature branch: `git checkout -b character-parser-complete-migration`
2. ? Commit current state: `git commit -am "Checkpoint before character parser migration"`
3. ? Begin Step 2: Refactor EvalInternalAsync main loop

**After Step 2**:
- Run tests: `dotnet test`
- Expect 50-70% pass rate
- Analyze failures
- Proceed to Step 3

---

## Conclusion

This migration is comprehensive but achievable in 7-10 hours. The CharacterParser infrastructure is already complete, immediate parsing words are already migrated, and we have clear success criteria.

**Expected Final State**:
- ? 878/878 tests passing (100%)
- ? Pure character-based parsing
- ? All >IN tests working
- ? All REFILL tests working
- ? Simpler, cleaner codebase
- ? Full ANS Forth parsing compliance

**Let's proceed with the migration!** ??
