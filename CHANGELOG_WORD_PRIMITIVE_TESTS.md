# Regression Tests for WORD Primitive - Changelog

## Date: 2025-01-XX

### Summary
Added comprehensive regression tests for the `WORD` primitive (ANS Forth Core word).

### Changes

#### New Test File
- **File**: `4th.Tests/Core/Parsing/WordPrimitiveTests.cs`
- **Test Count**: 20 comprehensive tests
- **Purpose**: Ensure the `WORD` primitive behaves correctly according to ANS Forth specification

### Test Coverage

The test suite covers the following aspects of the `WORD` primitive:

1. **Basic Functionality**
   - `Word_BasicSpaceDelimiter` - Tests basic parsing with space delimiter (ASCII 32)
   - `Word_ParsesUpToDelimiter` - Verifies parsing stops at the specified delimiter
   - `Word_AtEndOfInput` - Tests parsing the last word in input

2. **Delimiter Handling**
   - `Word_SkipsLeadingDelimiters` - Ensures leading delimiters are skipped
   - `Word_ConsecutiveDelimiters` - Tests handling of multiple delimiters between words
   - `Word_WithDifferentDelimiters` - Tests various delimiter characters (comma, period, hyphen)
   - `Word_ColonDelimiter` - Tests colon as a delimiter
   - `Word_TabDelimiter` - Tests alternative delimiters

3. **Edge Cases**
   - `Word_EmptyResult` - Tests behavior when no input remains
   - `Word_SingleCharacter` - Tests parsing a single character word
   - `Word_EmptySourceAfterParsing` - Tests subsequent WORD calls after exhausting input

4. **Input Handling**
   - `Word_AdvancesInputPointer` - Verifies >IN is correctly advanced
   - `Word_PreservesSpecialCharacters` - Ensures special characters (non-delimiter) are preserved
   - `Word_HandlesNumbers` - Tests parsing numeric strings
   - `Word_LongString` - Tests parsing longer words (50+ characters)

5. **Integration**
   - `Word_InDefinition` - Tests WORD inside colon definitions
   - `Word_WithCount` - Tests WORD followed by COUNT
   - `Word_WithParse` - Compares WORD behavior (skips leading delimiters)

6. **Memory Management**
   - `Word_ReturnsCountedString` - Verifies counted string format (length byte + chars)
   - `Word_AllocatesNewMemory` - Confirms each WORD call advances HERE

### Implementation Details

**Key Pattern Used**: Tests define a colon definition containing `WORD`, then invoke it with test input. This allows `WORD` to access the rest of the input line from SOURCE, matching real-world usage.

Example pattern:
```forth
: TEST 32 WORD ; TEST hello
```

This pattern:
1. Defines a word `TEST` that calls `WORD` with space delimiter
2. Invokes `TEST` with "hello" as remaining input
3. `WORD` parses "hello" from the source and returns a counted string address

**Helper Method**: `ReadCountedString` - Utility to read a counted string from memory given an address

### ANS Forth Compliance

The `WORD` primitive is part of the ANS Forth Core specification:
- **Stack Effect**: `( char "<chars>ccc<char>" -- c-addr )`
- **Behavior**: Parses characters delimited by `char`, skipping leading delimiters
- **Result**: Returns address of counted string (first byte is length, followed by characters)

### Test Results
- **Total Tests**: 20
- **Passed**: 20 ?
- **Failed**: 0
- **Skipped**: 0

All tests pass successfully, confirming the `WORD` primitive implementation is correct and stable.

### Future Enhancements
Potential additional tests could cover:
- Interaction with >IN manipulation
- WORD within blocks (block source)
- File-based input sources
- Unicode/extended character handling
- Performance benchmarks for long strings
