# Project Rules

## Naming
- Forth words: upper-case by convention (e.g. DUP, AWAIT). Mixed case allowed but avoid ambiguity.
- Async words added from C#: use suffix ASYNC (e.g. INCASYNC) unless returning Task purely internal.
- Module names: alphanumeric, no spaces. Prefer PascalCase (e.g. Proto, DynMod).
- Bound CLR methods via BIND/BINDASYNC: exported word names should be concise and upper-case.

## Modules
- Use `MODULE <name>` / `END-MODULE` for grouping custom words; expose via `USING <name>`.
- C# modules: decorate implementing class with `[ForthModule("Name")]` and implement `IForthWordModule`.
- Avoid name collisions; prefer module qualification (e.g. Proto:SPAWN-FORTH) when integrating.

## Dynamic Loading
- Prefer `LOAD-ASM-TYPE <Full.Type.Name>` over `LOAD-ASM <path>` for portability.
- Paths with spaces discouraged; copy/rename if needed.

## Actor Integration
- Use `SPAWN-FORTH` to host an interpreter; send code with `FORTH-EVAL` and a quoted string.
- Keep actor messages idempotent; no mutation outside interpreter state.

## Fast Paths
- IL fast path applies to single lines containing only numbers and + - * /.
- Sync IR fast path applies to simple lines without control flow, async, module, or variable words; supports: numbers, strings, CHAR, + - * / DUP DROP SWAP OVER ROT -ROT.
- Favor small, arithmetic-heavy lines to benefit from compilation.

## Word Design
- Stack effect documented in comments when adding new primitives (e.g. `\ ( n1 n2 -- sum )`).
- Avoid words with side effects beyond memory, stack, I/O, unless clearly named.

## Error Handling
- Throw `ForthException` for stack underflow, undefined words, type errors.
- Use `EnsureStack(interp, needed, wordName)` before consuming stack items.

## Async
- Avoid `async void` in all production code; prefer `Task`/`ValueTask`.
- Words returning Task must push the Task (unless awaited internally) and be awaited via AWAIT.
- Do not hide long-running synchronous operations in a sync word; prefer async pattern.
- Discourage returning `Task.CompletedTask` for non-trivial work; wrap synchronous CPU-bound operations in `Task.Run` to avoid blocking callers.

## Memory
- VARIABLE allocates address; use `!` (store) and `@` (fetch) words (provided by primitives).
- Avoid manual manipulation of `_mem` outside primitive implementations.

## Performance
- Keep hot words minimal (single arithmetic op or stack manipulation).
- Consider adding IL / IR support before expanding primitives widely.

## Testing
- Practice TDD: write tests first, then implementation.
- Add tests for each new word (core or module) covering normal and error scenarios.
- Actor words: test spawn, eval, error propagation.
- Dynamic loading: test both type-based and path-based if path logic changes.
- Comment each test with a short note of the intent so readers see what it verifies.

## Quality
- Fix all compiler warnings; treat warnings as errors when feasible.
- Prefer expression-bodied members for simple properties/methods.
- Prefer switch expressions where they improve clarity.
- Add XML documentation comments for all public types and members.
- Apply the Single Responsibility Principle pragmatically: keep classes and methods focused, but avoid over-fragmentation when cohesion would suffer.

## Process & Planning
- Always update the plan status when you complete work. If no further updates are needed, the plan is done for real.
- Include explicit last steps for "run build", "run tests", "commit and push", and "finalize plan".
- If a step has no work (e.g., nothing to commit), still mark it completed with a short note ("clean").
- After the last step, finalize/close the plan so progress is 100%. If progress is <100%, complete or skip remaining steps explicitly.

## Security
- Disallow untrusted assembly paths; validate existence before load.
- Reflection binding restricted to methods with expected arity.

## Contribution
- Include benchmark or rationale for adding new fast path ops.
- Update README / RULES when adding categories (e.g. new async patterns or control structures).

## Style
- C# uses nullable enabled, implicit usings.
- Keep public surface minimal; internal helpers preferred.

## Future Extensions (Guidance)
- Bytecode layer replacing sync IR (avoid premature complexity).
- Additional control flow words must define clear compile-time behavior.
- Word unloading needs module introspection (planned, not implemented).
