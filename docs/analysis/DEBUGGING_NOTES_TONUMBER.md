# >NUMBER Bug Investigation Notes - RESOLVED ?

## Problem
`>NUMBER` was losing the first digit when parsing multi-character strings:
- `S" 12" 0 0 >NUMBER` returned acc=2 instead of 12
- `S" 99" 0 0 >NUMBER` returned acc=9 instead of 99
- Single digits worked correctly: `S" 7" 0 0 >NUMBER` returned acc=7 ?

## Root Cause - FOUND ?

**There were TWO implementations of `>NUMBER` primitives:**

1. **`CorePrimitives.NumericBase.cs`** - `Prim_GTN` (OLD, BUGGY)
   - Complex implementation with multiple fallbacks
   - Had stack ordering confusion with addr/u forms
   - Was registered FIRST and shadowed the correct implementation

2. **`CorePrimitives.NumberParsing.cs`** - `Prim_ToNumber` (NEW, CORRECT)
   - Clean, straightforward implementation
   - Never got called because the old one was registered first

## Solution ?

1. **Removed duplicate `>NUMBER` primitive** from `CorePrimitives.NumericBase.cs`
2. **Kept clean implementation** in `CorePrimitives.NumberParsing.cs`
3. **Duplicate detection** we added to `CreateWords()` now prevents this issue in the future

## Test Results ?

**Before fix:**
- 2 tests passed (single digit cases)
- 18 tests failed (multi-digit cases)

**After fix:**
- **All 20 ToNumberIsolationTests pass!** ?
- Overall test suite: 339 passed, 2 failed (unrelated to >NUMBER)

## Files Modified

- `src/Forth.Core/Execution/CorePrimitives.cs` - Added duplicate detection ?
- `src/Forth.Core/Execution/CorePrimitives.NumericBase.cs` - Removed duplicate >NUMBER ?
- `src/Forth.Core/Execution/CorePrimitives.NumberParsing.cs` - Clean implementation (kept) ?
- `src/Forth.Core/Execution/CorePrimitives.DictionaryVocab.cs` - Already clean ?
- `4th.Tests/Core/Numbers/ToNumberIsolationTests.cs` - Comprehensive test suite ?

## Lessons Learned

1. **Check for duplicates** - The duplicate detection in `CreateWords()` now throws:
   ```
   System.InvalidOperationException: Duplicate primitive '>NUMBER' declared in methods 
   'Prim_ToNumber' and 'Prim_GTN'. Each primitive name must be unique within a module.
   ```

2. **Partial classes can hide duplicates** - Both implementations were in the same `internal static partial class CorePrimitives` but different files

3. **Registration order matters** - Methods are discovered via reflection, and the first registration wins

## Duplicate Detection Added

The `CreateWords()` method now includes:

```csharp
var seenPrimitives = new Dictionary<string, (string MethodName, string? Module)>(StringComparer.OrdinalIgnoreCase);

foreach (var method in methods)
{
    var attr = method.GetCustomAttribute<PrimitiveAttribute>();
    if (attr is not null)
    {
        var name = attr.Name;
        var module = attr.Module;
        
        // Check for duplicate primitive declarations
        if (seenPrimitives.TryGetValue(name, out var existing))
        {
            // Allow same name in different modules
            if (string.Equals(existing.Module, module, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Duplicate primitive '{name}' declared in methods '{existing.MethodName}' and '{method.Name}'. " +
                    $"Each primitive name must be unique within a module.");
            }
        }
        else
        {
            seenPrimitives[name] = (method.Name, module);
        }
        
        // ... continue with registration
    }
}
```

This prevents future duplicate primitives from being silently ignored.

## Status: RESOLVED ?

The >NUMBER bug is completely fixed. The comprehensive test suite validates all edge cases including:
- Single and multi-digit numbers
- Different bases (decimal, hex)
- Accumulator with non-zero starting values
- Start count offsets
- Memory layout verification
- String reading verification
