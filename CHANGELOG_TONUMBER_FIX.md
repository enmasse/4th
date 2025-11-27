# >NUMBER Bug Fix - Change Log

**Date**: 2025-01-XX  
**Issue**: `>NUMBER` primitive was losing the first digit when parsing multi-character numbers

## Changes Made

### 1. Root Cause Analysis
- Discovered **duplicate `>NUMBER` primitive implementations**:
  - `CorePrimitives.NumericBase.cs` - `Prim_GTN` (old, buggy implementation)
  - `CorePrimitives.NumberParsing.cs` - `Prim_ToNumber` (new, correct implementation)
- The old implementation was registered first via reflection, shadowing the correct one

### 2. Files Modified

#### `src/Forth.Core/Execution/CorePrimitives.cs`
- **Added duplicate primitive detection** in `CreateWords()` method
- Throws `InvalidOperationException` if same primitive name is declared twice in the same module
- Prevents future duplicate primitives from being silently ignored

#### `src/Forth.Core/Execution/CorePrimitives.NumericBase.cs`
- **Removed duplicate `>NUMBER` primitive** (`Prim_GTN` method)
- Kept only `BASE`, `DECIMAL`, `HEX`, and `STATE` primitives

#### `src/Forth.Core/Execution/CorePrimitives.NumberParsing.cs`
- **No changes** - clean implementation already present and working correctly

#### `DEBUGGING_NOTES_TONUMBER.md`
- **Created comprehensive debugging notes** documenting the investigation and resolution

## Test Results

### Before Fix
- **2 passed** / 18 failed (only single-digit tests worked)
- Multi-digit parsing returned only the last digit (e.g., "12" ? 2, "99" ? 9)

### After Fix
- ? **All 20 `ToNumberIsolationTests` pass**
- ? **339/341 total tests pass** (2 failures unrelated to >NUMBER)
- Validates: single/multi-digit, hex, different bases, accumulators, memory layout

## Impact

### Positive
- ? `>NUMBER` now works correctly for all numeric parsing scenarios
- ? Duplicate primitive detection prevents similar bugs in the future
- ? Clear error messages when duplicates are detected
- ? No regressions in existing functionality

### Breaking Changes
- None - the fix restores correct behavior

## Lessons Learned

1. **Partial classes can hide duplicates** - Both implementations were in `CorePrimitives` but in different files
2. **Reflection order matters** - First discovered method wins when duplicates exist
3. **Testing is essential** - Comprehensive test suite caught the regression immediately
4. **Prevention over cure** - Added guard rails to prevent future duplicates

## Duplicate Detection Details

The new validation in `CreateWords()`:

```csharp
var seenPrimitives = new Dictionary<string, (string MethodName, string? Module)>(
    StringComparer.OrdinalIgnoreCase);

foreach (var method in methods)
{
    var attr = method.GetCustomAttribute<PrimitiveAttribute>();
    if (attr is not null)
    {
        var name = attr.Name;
        var module = attr.Module;
        
        if (seenPrimitives.TryGetValue(name, out var existing))
        {
            if (string.Equals(existing.Module, module, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Duplicate primitive '{name}' declared in methods " +
                    $"'{existing.MethodName}' and '{method.Name}'. " +
                    $"Each primitive name must be unique within a module.");
            }
        }
        else
        {
            seenPrimitives[name] = (method.Name, module);
        }
        
        // Continue with registration...
    }
}
```

## Related Documentation
- See `DEBUGGING_NOTES_TONUMBER.md` for detailed investigation notes
- Test suite: `4th.Tests/Core/Numbers/ToNumberIsolationTests.cs`
