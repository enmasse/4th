# Tokenizer Regression Tests - Change Log

**Date**: 2025-01-XX  
**Issue**: Added comprehensive regression tests for tokenizer, especially `.( ... )` immediate printing

## Summary

Added extensive regression tests for the tokenizer to ensure the `.( ... )` fix and other special tokenization behaviors are properly covered.

## Tests Added

### DotParen Tests (9 new tests)
Tests for `.( ... )` immediate printing functionality:

1. **`Tokenizer_DotParen_ShouldPrintImmediately_NoTokensCreated`**
   - Verifies that `.( Hello World )` prints immediately
   - Confirms no tokens are created for the `.( )` construct

2. **`Tokenizer_DotParen_WithOtherWords_ShouldNotAffectTokens`**
   - Tests `WORD1 .( test message ) WORD2`
   - Verifies only WORD1 and WORD2 become tokens
   - Message is printed, not tokenized

3. **`Tokenizer_DotParen_EmptyMessage_ShouldWorkCorrectly`**
   - Tests `.()` with empty message
   - Confirms empty messages work without errors

4. **`Tokenizer_DotParen_WithSpecialChars_ShouldPreserveContent`**
   - Tests `.( Test: 1+2=3 "quoted" )`
   - Verifies special characters are preserved in output

5. **`Tokenizer_DotParen_MissingClosingParen_ShouldThrow`**
   - Tests error handling for unclosed `.(` 
   - Confirms proper exception with descriptive message

6. **`Tokenizer_DotParen_Multiple_ShouldPrintAllMessages`**
   - Tests `.( First ) .( Second )`
   - Verifies multiple `.( )` on same line work correctly

7. **`Tokenizer_DotParen_WithLeadingAndTrailingSpaces_ShouldPreserve`**
   - Tests `.(   spaced   )`
   - Confirms whitespace preservation

8. **`Tokenizer_DotParen_NotConfusedWithDot_ShouldNotCreateDotToken`**
   - Regression test for the original bug
   - Ensures `.` token is not created when parsing `.(` 

9. **`Tokenizer_DotParen_NotConfusedWithParenComment`**
   - Contrasts `( comment )` (silent) with `.( message )` (prints)
   - Confirms different behavior between comment and immediate print

### Other Special Form Tests (6 new tests)

10. **`Tokenizer_SQuote_ShouldSkipOneLeadingSpace`**
    - Tests `S" hello"` tokenization
    - Verifies ANS Forth convention of skipping one space after `S"`

11. **`Tokenizer_DotQuote_ShouldHandleWhitespace`**
    - Tests `."  test  "` tokenization
    - Confirms whitespace handling for `."` 

12. **`Tokenizer_BracketTickBracket_ShouldBeOneToken`**
    - Tests `['] WORD` tokenization
    - Verifies `[']` is treated as single token

13. **`Tokenizer_LOCAL_ShouldBeOneToken`**
    - Tests `(LOCAL) name` tokenization
    - Confirms `(LOCAL)` special form handling

14. **`Tokenizer_DotBracketS_ShouldBeOneToken`**
    - Tests `.[S] test` tokenization
    - Verifies `.[S]` is treated as single token

15. **`Tokenizer_Semicolon_ShouldBeOwnToken`**
    - Tests `: WORD ; test` tokenization
    - Confirms semicolon gets its own token

## Test Coverage

### Before
- Basic tokenization tests
- Comment handling tests
- Quote handling tests
- **No specific tests for `.( )` construct**

### After
- **15 new tests** added
- **Comprehensive `.( )` coverage** (9 tests)
- **Special form validation** (6 tests)
- **Error case handling** (missing closing paren)
- **Edge cases** (empty messages, multiple on same line, whitespace)

## Test Results

All 29 tokenizer tests passing:
```
Test summary: total: 29, failed: 0, succeeded: 29, skipped: 0
```

## Key Behaviors Verified

### `.( ... )` Immediate Printing
- ? Prints text immediately during tokenization
- ? Does not create any tokens
- ? Preserves whitespace including leading space after `(`
- ? Works with special characters
- ? Handles empty messages
- ? Supports multiple `.( )` on same line
- ? Distinct from `.` (dot) word
- ? Distinct from `( )` comments
- ? Throws on missing closing `)`

### Other Special Forms
- ? `S"` skips one leading space per ANS Forth
- ? `."` handles whitespace correctly
- ? `[']` tokenized as single unit
- ? `(LOCAL)` recognized as special form
- ? `.[S]` tokenized as single unit
- ? `;` gets its own token

## Regression Protection

These tests protect against:
1. **Original bug**: `.( text )` being parsed as `.` + `( comment )`
2. **Token pollution**: `.( )` creating unwanted tokens
3. **Silent failures**: Error conditions not being caught
4. **Whitespace issues**: Incorrect handling of spaces
5. **Confusion with similar forms**: `.` vs `.(`, `(` vs `.(` 

## Files Modified

1. **4th.Tests/Core/Tokenizer/TokenizerTests.cs**
   - Added 15 new test methods
   - Enhanced test output with diagnostic messages
   - Organized tests into logical regions

## Related Issues

- Fixes the stack underflow issue in FloatingPointTests
- Related to PR for `.( ... )` tokenizer support
- Part of ANS Forth compliance improvements

## Notes

- Tests use `ITestOutputHelper` for diagnostic output
- Console output capturing used for print verification
- All tests follow AAA pattern (Arrange, Act, Assert)
- Test names are descriptive and self-documenting

## Future Enhancements

Consider adding:
- Performance tests for large `.( )` messages
- Multi-line `.( )` handling tests (if needed)
- Nested special form tests
- International character tests in `.( )`
