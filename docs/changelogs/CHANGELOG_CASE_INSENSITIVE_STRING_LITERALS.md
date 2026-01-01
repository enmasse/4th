# Case-Insensitive String Literals Fix

## Summary

Fixed Forth tokenizer to handle case-insensitive string literal prefixes (`S"` and `s"`), making the implementation properly case-insensitive for ANS Forth compliance.

## Problem

The tokenizer was only recognizing uppercase `S"` for string literals. When code used lowercase `s"`, it was treated as a separate word `S` followed by a quoted string, which caused:
- Stack underflow errors in paranoia.4th patched code
- Non-standard behavior deviating from ANS Forth case-insensitivity

## Root Cause

In `src/Forth.Core/Interpreter/Tokenizer.cs`, the string literal detection was:
```csharp
if (c == 'S' && i + 1 < input.Length && input[i + 1] == '"')
```

This only matched uppercase `S`.

## Solution

Modified tokenizer to accept both cases and normalize to uppercase for primitive lookup:
```csharp
if ((c == 'S' || c == 's') && i + 1 < input.Length && input[i + 1] == '"')
{
    // ...
    list.Add("S\"");  // Normalize to uppercase for primitive lookup
    // ...
}
```

## Files Changed

- `src/Forth.Core/Interpreter/Tokenizer.cs`
  - Updated `S"` detection to handle both `S"` and `s"`
  - Added normalization comment for `."`

## Testing

Created comprehensive tests in `4th.Tests/Core/CaseInsensitivity/CaseInsensitivityTests.cs`:

1. ? **SQuote_LowercaseWorks** - Verifies lowercase `s"` produces `(c-addr u)`
2. ? **PrimitivesAreCaseInsensitive** - Confirms general case-insensitivity
3. ? **DefinedWordsAreCaseInsensitive** - Validates user-defined words are case-insensitive

All paranoia string literal tests now pass:
- ? ParanoiaPatternWithCorrectSQuote
- ? ParanoiaPatternWithLowercaseSQuote  
- ? UppercaseSQuoteProducesCorrectStack
- ? LowercaseSQuoteProducesStringObject (now works correctly)

## Impact

- ? **Paranoia.4th initialization** now works with both `s"` and `S"`
- ? **ANS Forth compliance** improved - case-insensitivity is core requirement
- ? **Better compatibility** with existing Forth code using either case
- ?? **Paranoia still fails** on different issues (IF outside compilation) - see TODO.md

## ANS Forth Compliance

ANS Forth X3.215-1994 specifies that word names are case-insensitive. Our tokenizer now properly normalizes string literal prefixes to uppercase before dictionary lookup, ensuring case-insensitive behavior.

## Related Issues

- Fixes the root cause identified in PARANOIA_ROOT_CAUSE_ANALYSIS.md
- Paranoia.4th has additional bugs beyond the `s"`/`S"` issue
- See TODO.md for remaining known issues with paranoia.4th test

## Testing Summary

**Before Fix:**
- lowercase `s"` ? pushed String object (1 stack item)
- uppercase `S"` ? pushed (c-addr u) (2 stack items)

**After Fix:**
- lowercase `s"` ? pushes (c-addr u) (2 stack items) ?
- uppercase `S"` ? pushes (c-addr u) (2 stack items) ?
