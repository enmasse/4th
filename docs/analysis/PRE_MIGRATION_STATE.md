# Character-Based Parser Migration - Pre-Migration State

## Date: 2025-01-15

## Pre-Migration Status

### Test Results: 861/876 (98.3%)
- **Passing**: 861 tests ?
- **Failing**: 6 tests ?
  - 2 paranoia.4th tests - "IF outside compilation" error
  - 4 >IN manipulation tests - need proper character-level tracking
- **Skipped**: 9 tests (need to be unskipped)

### Files to be Modified

1. **Core Evaluation**:
   - `src/Forth.Core/Interpreter/ForthInterpreter.Evaluation.cs` - Main evaluation loop
   - `src/Forth.Core/Interpreter/ForthInterpreter.cs` - Parser field already added

2. **Primitives**:
   - `src/Forth.Core/Execution/CorePrimitives.DictionaryVocab.cs` - S", .", parsing words
   - `src/Forth.Core/Execution/CorePrimitives.Compilation.cs` - [IF], [ELSE], [THEN]
   - `src/Forth.Core/Execution/CorePrimitives.IOFormatting.cs` - WORD (already enhanced), SAVE-INPUT, RESTORE-INPUT

3. **Infrastructure**:
   - `src/Forth.Core/Interpreter/Tokenizer.cs` - TO BE DELETED
   - `src/Forth.Core/Interpreter/CharacterParser.cs` - Already created ?

4. **Tests**:
   - `4th.Tests/Core/Tokenizer/TokenizerTests.cs` - Migrate to CharacterParserTests
   - `4th.Tests/Core/Parsing/InPrimitiveRegressionTests.cs` - Unskip tests

### CharacterParser Status

? **READY**: `src/Forth.Core/Interpreter/CharacterParser.cs` exists with:
- ParseNext() - Main token parsing
- ParseWord(delimiter) - For WORD primitive
- Position tracking and >IN synchronization
- Handles all special forms (comments, strings, bracket conditionals)

### Migration Strategy

**Incremental Approach**:
1. Keep Tokenizer.cs until evaluation loop is fully migrated
2. Test after each major change
3. Roll back if regressions exceed acceptable threshold (>50 new failures)
4. Document each step for potential troubleshooting

**Rollback Plan**:
- Git commit before migration starts
- Can revert entire migration if needed
- All changes are in version control

### Expected Outcomes

**Immediate (Step 2-4)**:
- Many tests will fail temporarily (~200-400 failures expected)
- Evaluation loop will use CharacterParser
- Immediate words will use character-based parsing

**Mid-Migration (Step 5-6)**:
- Remove tokenizer completely
- Clean up token-based infrastructure  
- ~100-200 failures expected

**Final (Step 7-9)**:
- All tests updated and passing
- 876/876 (100%) target achieved
- Paranoia.4th tests passing
- >IN manipulation tests passing

### Risk Mitigation

1. **Small commits**: Commit after each working step
2. **Test frequently**: Run health check after each file modification
3. **Document issues**: Record any unexpected problems
4. **Pair review**: Session summary documents track all changes

### Critical Success Factors

1. **CharacterParser.ParseNext() correctness**: Must handle all token types correctly
2. **>IN synchronization**: Must update >IN as characters are consumed
3. **Immediate word integration**: S", .", etc. must consume from parser correctly
4. **Bracket conditional skip**: Must work with character-based position tracking

---

**Ready to proceed**: All prerequisites met, CharacterParser ready, backup plan in place.
