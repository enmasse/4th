# Primitive container modernization follow-up plan

## Status
This plan has been largely executed.

Completed:
- Split floating primitives into:
  - `FloatingStackPrimitives`
  - `FloatingArithmeticPrimitives`
  - `FloatingMathPrimitives`
  - `FloatingComparePrimitives`
  - `FloatingFormatPrimitives`
  - Remaining float storage/definition/conversion words stay in `FloatingPrimitives`.
- Split memory primitives into:
  - `MemoryCellPrimitives`
  - `MemoryBytePrimitives`
  - `MemorySpacePrimitives`
  - `InputBufferPrimitives`
  - Allocation remains in `MemoryAllocationPrimitives`.
- Split IL block primitive into `IlBlockPrimitives` with implementation in helpers:
  - `IlEmitter`
  - `IlResolution`
- Removed empty legacy/placeholder containers (`FloatingLegacyPrimitives`, `MemoryLegacyPrimitives`, `IlPrimitives`, `MemoryPrimitives`, `IlOpcodePrimitives`, `IlResolutionPrimitives`).
- Migrated remaining call sites off `CorePrimitives.ToLong/ToBool/IsNumeric`.
- Removed the temporary helper shim from `CorePrimitives`.

## Remaining work (optional refinements)
- If desired, further split `FloatingPrimitives` (definitions/storage) into more focused containers.
- Consider renaming `InputBufferPrimitives` to align with your naming taxonomy (if you prefer `SourcePrimitives` or similar).
- If IL grows additional words beyond `IL{`, introduce actual `[Primitive]` methods in dedicated IL containers rather than placeholders.

## Notes
- `CorePrimitives.CreateWords()` continues to discover primitives via reflection in `Forth.Core.Execution`.
- Always run `dotnet build` and `dotnet test -m:1 /p:BuildInParallel=false /p:UseSharedCompilation=false` after moving primitives to catch duplicate registrations.
