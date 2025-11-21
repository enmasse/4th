# TODO: ANS?Forth conformity gap analysis

Goal
- Compare the current implementation against the ANS Forth core wordlist and identify words that are missing or partially implemented.

Method
- A scan of `Primitive` attributes and tests in the repository was used to determine what exists. A tool `tools/ans-diff` is available for automated comparison.

Status — implemented / obvious support (non-exhaustive)
- Definitions / compilation words: `:`, `;`, `IMMEDIATE`, `POSTPONE`, `[`, `]`, `'`, `LITERAL`
- Control flow: `IF`, `ELSE`, `THEN`, `BEGIN`, `WHILE`, `REPEAT`, `UNTIL`, `DO`, `LOOP`, `LEAVE`, `UNLOOP`, `I`, `RECURSE`
- Defining words: `CREATE`, `DOES>`, `VARIABLE`, `CONSTANT`, `VALUE`, `TO`, `DEFER`, `IS`, `MARKER`, `FORGET`
- Stack / memory: `@`, `!`, `C@`, `C!`, `,`, `ALLOT`, `HERE`, `COUNT`, `MOVE`, `FILL`, `ERASE`
- I/O: `.`, `.S`, `CR`, `EMIT`, `TYPE`, `WORDS`, pictured numeric (`<#`, `HOLD`, `#`, `#S`, `SIGN`, `#>`)
- File I/O (subset): `READ-FILE`, `WRITE-FILE`, `APPEND-FILE`, `FILE-EXISTS`, `INCLUDE`, `LOAD`
  - Baseline stream primitives implemented: `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
  - Byte-level handle operations implemented: `READ-FILE-BYTES`, `WRITE-FILE-BYTES`
  - Diagnostics: `LAST-WRITE-BYTES`, `LAST-READ-BYTES` implemented behind `#if DEBUG`
- Async / concurrency: `SPAWN`, `FUTURE`, `TASK`, `JOIN`, `AWAIT`, `TASK?` (implementation present; extended support for awaitables implemented)
- Exceptions / control: `CATCH`, `THROW`, `ABORT`, `EXIT`, `BYE`, `QUIT`
- Numeric base & parsing: `BASE`, `HEX`, `DECIMAL`, `>NUMBER`, `STATE`
- Introspection: `SEE` (module-qualified and basic decompile text present)
- Wordlist/search-order: `GET-ORDER`, `SET-ORDER` (implemented)
- Interactive input: `KEY`, `KEY?`, `ACCEPT`, `EXPECT`, `SOURCE`, `>IN`, `READ-LINE` (implemented; simplified semantics for buffer addressing)
- Extended arithmetic
  - `*/MOD: ( n1 n2 n3 -- rem quot )` implemented in core and `*/MOD` used in prelude

Notes from code inspection
- `GET-ORDER`/`SET-ORDER` use internal helpers and expose `FORTH` as `null` in the list.
- `ACCEPT`/`EXPECT` currently return a string and length (simplified; do not operate on raw c-addr memory in a true ANS manner).
- `AWAIT`/`TASK?` include explicit support for pattern-based awaitables that expose `GetAwaiter()` in addition to `Task`/`Task<T>` and `ValueTask`/`ValueTask<T>`. A helper `AwaitableHelper` handles detection, synchronous completion, and registering continuations.
- File/Block backing uses a memory-mapped-file path-based creation with a fallback to FileStream.

Progress / Repository tasks (current)
- [x] Create `tools/ans-diff` script to collect `Primitive` names and compare against ANS list
- [x] Implement high-priority words: `GET-ORDER`, `SET-ORDER`, `KEY`, `KEY?`, `ACCEPT`, `EXPECT`
- [x] Implement baseline file stream words: `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
- [x] Implement byte-oriented handle words: `READ-FILE-BYTES`, `WRITE-FILE-BYTES`
- [x] Gate diagnostics primitives behind DEBUG (`LAST-WRITE-BYTES`, `LAST-READ-BYTES`)
- [x] Implement block system words: `BLOCK`, `SAVE`, `BLK` (per-block files with atomic replace)
- [x] Decide and normalize truth values across primitives (true = -1)
- [x] Implement `READ-LINE`, `SOURCE`, `>IN`
- [x] Implement `WORDLIST`, `DEFINITIONS`, `FORTH` with Wordlist objects
- [x] Improve `AWAIT`/`TASK?` robustness (ValueTask / awaitable handling)
- [x] Add `D+`, `D-`, `M*` primitives and tests
- [x] Add `BLK` primitive and support for ' pushing unresolved names in tokenizer/Prim_Tick (assist WORDLIST usage)
- [x] Update `tools/ans-diff` to write report file and fail on missing words (non-zero exit)
- [x] Update CI workflow to run `tools/ans-diff`, capture report, and upload artifact
- [x] Split `ForthInterpreter` block system into a partial file (`ForthInterpreter.Blocks.cs`) to reduce file size and isolate block logic
- [x] Implement LRU eviction for block address/accessor cache and add corresponding unit tests
- [x] Add unit tests for LRU eviction and per-block backing behavior (`4th.Tests/Core/MissingWords/BlockLRUTests.cs`)
- [x] Install `IDisposableAnalyzers` and fix/quiet disposal warnings
- [x] Resolve nullable warnings reported during builds
- [x] Run full test suite locally and on CI; all tests pass (216/216)
- [x] Run `tools/ans-diff` and update `tools/ans-diff/report.md`

Remaining / next work items
- [ ] Add LRU cache size configuration (constructor param or interpreter setting)
- [ ] Consider tightening `ACCEPT`/`EXPECT`/`READ-LINE` semantics to match full c-addr/u ANS behavior (allocate/populate interpreter memory instead of returning strings)
- [ ] Review MMF accessor caching vs per-call accessors for performance; if caching is needed, reintroduce with clear ownership and disposal semantics (and add tests)
- [ ] Add a CI job to fail the build when `tools/ans-diff` reports missing words (currently report is uploaded; fail-on-missing is supported by the tool)
- [ ] Audit and consolidate any remaining analyzer suppressions; prefer code changes to targeted suppressions where practical

Decisions made
- Prefer creating MMF from path to avoid ambiguous ownership between a FileStream and the MMF object.
- For correctness and to satisfy IDisposable analyzers, MMF accessors were converted to per-operation creation/disposal in the recent change; this simplified lifecycle and resolved analyzer warnings and tests pass. If profiling shows performance regressions, we will reintroduce a safe cache with explicit disposal logic.

Recent activity (most recent first)
- 2025-11-25: Ran full test suite (216/216 passed) and updated `tools/ans-diff/report.md`.
- Installed `IDisposableAnalyzers`, fixed analyzer warnings across `ForthInterpreter.Blocks.cs` and related files.
- Resolved nullable reference warnings in concurrency/await code paths.
- Adjusted MMF accessor handling to a safer per-operation create/dispose model to remove ambiguous disposal patterns and pass analyzers.
- Updated `tools/ans-diff` invocation and CI workflow to produce and persist `report.md`.

If you want, I can:
- Expose `CLOSE-BLOCK-FILE` / `FLUSH-BLOCK-FILE` primitives (if not already present) and add lifecycle tests.
- Implement LRU eviction configuration and add deterministic tests for small cache sizes.
- Tighten ACCEPT/EXPECT/READ-LINE semantics to match full ANS c-addr/u behavior.
- Reintroduce an accessor cache with explicit ownership and disposal if profiling shows the per-call accessor approach is a bottleneck.
