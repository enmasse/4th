# Primitive container modernization follow-up plan

## Goals
- Make `FloatingPrimitives`, `IlPrimitives`, and `MemoryPrimitives` match the existing “modern” primitives pattern (e.g. `ArithmeticPrimitives`, `FileIoPrimitives`, `IoFormattingPrimitives`).
- Reduce very large files into cohesive, testable modules.
- Eliminate the temporary helper shim on `CorePrimitives` once no call sites remain.

## Proposed structure
### Float
Split `FloatingPrimitives` by responsibility:
- `FloatingStackPrimitives` (stack ops: `FDEPTH`, `FDUP`, `FDROP`, `FSWAP`, etc.)
- `FloatingArithmeticPrimitives` (`F+`, `F-`, `F*`, `F/`, `FNEGATE`, etc.)
- `FloatingMathPrimitives` (`FSIN`, `FCOS`, `FATAN2`, `FSQRT`, etc.)
- `FloatingComparePrimitives` (`F=`, `F<`, `F0=`, etc.)
- `FloatingFormatPrimitives` (`F.`, `FS.`, `FROUND`, formatting/conversion)

### IL
Split `IlPrimitives`:
- `IlBlockPrimitives` (token collection and compilation wrapper `IL{ ... }IL`)
- `IlOpcodePrimitives` (opcode parsing/emission helpers)
- `IlTypeResolutionPrimitives` (type/member resolve helpers)

Move pure parsing helpers into a shared internal utility (new file) if used by more than one IL class.

### Memory
Split `MemoryPrimitives`:
- `MemoryCellPrimitives` (`@`, `!`, `+!`, `2@`, `2!`, `,`, `C,`, etc.)
- `MemoryBytePrimitives` (`C@`, `C!`, `MOVE`, `CMOVE`, `CMOVE>`, `FILL`, `DUMP`)
- Keep allocation words in `MemoryAllocationPrimitives` (already split).

## Coding pattern guidelines (match modern files)
- One container per cohesive word set.
- Use `PrimitivesUtil` for conversions and shared comparer.
- Keep primitives as `private static Task` (or `async Task`) with `[Primitive]`.
- Prefer multi-line method bodies over ultra-long one-liners.
- Avoid reaching into interpreter internals unless other modern primitives already do.

## Migration steps
1. Identify groups of `[Primitive]` methods in each container by category.
2. Create new container class files and move primitives in small batches.
3. Ensure `CorePrimitives.CreateWords()` still finds them (namespace `Forth.Core.Execution`).
4. Run `dotnet build` and `dotnet test -m:1 /p:BuildInParallel=false /p:UseSharedCompilation=false` after each move.
5. Replace remaining `CorePrimitives.ToLong/ToBool/IsNumeric` call sites with `PrimitivesUtil.*`.
6. Remove the temporary `CorePrimitives` helper shim.
