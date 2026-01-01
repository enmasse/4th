# ANS Forth Floating-Point Compliance Fixes

## Summary
Fixed multiple ANS Forth compliance issues in the floating-point implementation to ensure `paranoia.4th` and other standard Forth programs run correctly.

## Changes Made

### 1. FLOOR Returns Double (Not Long)
**Issue**: `FLOOR` was returning a `long` integer, but ANS Forth specifies it should return a floating-point value.

**Fix**: Modified `FLOOR` to return `Math.Floor(d)` as a `double` instead of `(long)Math.Floor(d)`.

**Impact**: 
- `paranoia.4th` uses `FLOOR` in expressions like `Half F@ X F@ F+ FLOOR Radix F@ F*` expecting a float result
- Programs can now chain FLOOR with other floating-point operations without type conversion

**Example**:
```forth
\ Before (wrong): 3.7 FLOOR returns 3 (long)
\ After (correct): 3.7 FLOOR returns 3.0 (double)
3.7d FLOOR 1.0d F+  \ Now works correctly, yields 4.0
```

### 2. FLOATS Returns n*8 for Double Precision
**Issue**: `FLOATS` was returning `n` unchanged, but ANS Forth specifies it should scale by the float-cell size (8 bytes for double precision).

**Fix**: Modified `FLOATS` to return `n * 8L` to indicate 8-byte double-precision floats.

**Impact**:
- `paranoia.4th` uses `1 FLOATS` to detect precision: expects 4 for single, 8 for double
- Memory allocation and addressing calculations now work correctly

**Example**:
```forth
\ Before (wrong): 1 FLOATS returns 1
\ After (correct): 1 FLOATS returns 8
1 FLOATS  \ Returns 8 (bytes per double-precision float)
3 FLOATS  \ Returns 24 (3 floats * 8 bytes each)
```

### 3. FLN Added as Alias for FLOG
**Issue**: Many Forth systems expect `FLN` (Floating-point Natural Logarithm) as a standard word, but only `FLOG` was provided.

**Fix**: Added `FLN` primitive that calls `Prim_FLOG` internally.

**Impact**:
- `paranoia.4th` checks for FLN and defines it if missing: `[UNDEFINED] FLN [IF] : FLN FLOG ; [THEN]`
- Now works without needing the fallback definition

**Example**:
```forth
\ Both now work identically
1.0d FLOG  \ Returns 0.0 (natural log of 1)
1.0d FLN   \ Returns 0.0 (same, using alias)
```

### 4. Enhanced F~ with Full ANS Semantics
**Issue**: `F~` only implemented absolute tolerance (`r3 > 0`), missing exact equality and relative tolerance modes.

**Fix**: Implemented full ANS Forth F~ semantics:
- **r3 > 0**: Absolute difference test: `|r1 - r2| < r3`
- **r3 = 0**: Exact equality test: `r1 == r2`
- **r3 < 0**: Relative difference test: `|r1 - r2| < |r3 * (r1+r2)/2|`

**Impact**:
- Supports all three comparison modes used by advanced floating-point tests
- `paranoia.4th` and similar tests can properly validate floating-point behavior

**Example**:
```forth
\ Absolute tolerance (positive r3)
1.0d 1.001d 0.01d F~   \ true: difference 0.001 < 0.01

\ Exact equality (zero r3)
1.5d 1.5d 0.0d F~      \ true: exact match
1.5d 1.50001d 0.0d F~  \ false: not exact

\ Relative tolerance (negative r3)
100.0d 100.5d -0.01d F~  \ true: 0.5% error < 1%
100.0d 102.0d -0.01d F~  \ false: 2% error >= 1%
```

## Testing
Added comprehensive test suite covering:
1. ? FLOOR returns double type and can be used in float expressions
2. ? FLOATS correctly returns 8 bytes for double precision
3. ? FLN alias works identically to FLOG
4. ? F~ handles all three tolerance modes per ANS Forth spec
5. ? Integration test verifying paranoia.4th expectations

## Files Modified
- `src/Forth.Core/Execution/CorePrimitives.Floating.cs`
- `4th.Tests/Core/MissingWords/FloatingPointTests.cs`

## Compatibility
These changes improve ANS Forth compliance without breaking existing functionality. Programs relying on the old behavior may need updates:
- Code expecting `FLOOR` to return integer: Change to use `F>S` or `FTRUNC` for integer conversion
- Code expecting `FLOATS` to return `n`: Adjust memory calculations to account for 8-byte cells

## References
- ANS Forth Floating-Point Extension Specification
- `paranoia.4th` test suite expectations
- IEEE 754 floating-point standard
