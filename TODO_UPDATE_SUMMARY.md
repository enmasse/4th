# TODO.md Update Summary

## Mark as RESOLVED

The following item should be marked as completed:

```markdown
- ~~Correct `>NUMBER` parsing semantics~~ **[RESOLVED 2025-01-XX]**:
  - ~~Ensure proper accumulation for decimal and hex bases.~~
  - ~~Handle remainder and partial conversion per ANS.~~
  - ~~Add tests for mixed digits, base changes, and remainder reporting.~~
  - ~~Investigate c-addr off-by-one: confirm `S"` pushes `addr+1` and correct `u`; fix first-digit loss (e.g., "123" -> 123 not 23), and verify hex remainder case ("FFZ" in HEX -> 255, rem=1, digits=2).~~
  - **Root cause**: Duplicate `>NUMBER` primitive implementations. 
  - **Solution**: Removed old buggy version from `CorePrimitives.NumericBase.cs`, kept clean implementation in `CorePrimitives.NumberParsing.cs`. 
  - **Prevention**: Added duplicate primitive detection to `CreateWords()`. 
  - **Status**: All 20 ToNumberIsolationTests pass. 
  - **Details**: See `DEBUGGING_NOTES_TONUMBER.md` and `CHANGELOG_TONUMBER_FIX.md`
```

## Add to "Recent extensions" section

```markdown
- **Fixed `>NUMBER` bug**: Removed duplicate primitive implementation that was causing first-digit loss. Added duplicate primitive detection to `CreateWords()` method. All parsing tests now pass.
```

## Add to "Notes" section

```markdown
- **Duplicate primitive detection**: `CreateWords()` now validates that each primitive name is unique within its module, preventing silent shadowing issues.
```

## Add to "Progress / Repository tasks (current)" section

```markdown
- [x] Fix `>NUMBER` first-digit loss bug and add duplicate primitive detection
```

## Files to Review

1. `TODO.md` - Main task tracking file (needs manual update)
2. `DEBUGGING_NOTES_TONUMBER.md` - Detailed investigation notes (already created)
3. `CHANGELOG_TONUMBER_FIX.md` - Change log summary (already created)

## Test Status

- ? All 20 ToNumberIsolationTests pass
- ? 339/341 total tests pass (2 failures unrelated to >NUMBER)
- ? No regressions introduced
