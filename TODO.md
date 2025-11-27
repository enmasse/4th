# TODO: ANS/Forth conformity gap analysis

_Last updated: 2025-11-28_

Goal
- Compare the current implementation against the ANS Forth core wordlist and identify words that are missing or partially implemented.

Method
- A scan of `Primitive` attributes and tests in the repository is used to determine what exists. Tool `tools/ans-diff` automates comparison and can fail CI on missing words.

Status — implemented / obvious support (non-exhaustive)
- Definitions / compilation words: `:`, `;`, `IMMEDIATE`, `POSTPONE`, `[`, `]`, `'`, `LITERAL`
- Control flow: `IF`, `ELSE`, `THEN`, `BEGIN`, `WHILE`, `REPEAT`, `UNTIL`, `DO`, `LOOP`, `LEAVE`, `UNLOOP`, `I`, `RECURSE`
- Defining words: `CREATE`, `DOES>`, `VARIABLE`, `CONSTANT`, `VALUE`, `TO`, `DEFER`, `IS`, `MARKER`, `FORGET`
- Stack / memory: `@`, `!`, `C@`, `C!`, `,`, `ALLOT`, `HERE`, `COUNT`, `MOVE`, `FILL`, `ERASE`
- I/O: `.`, `.S`, `CR`, `EMIT`, `TYPE`, `WORDS`, pictured numeric (`<#`, `HOLD`, `#`, `#S`, `SIGN`, `#>`)
- File I/O (subset): `READ-FILE`, `WRITE-FILE`, `APPEND-FILE`, `FILE-EXISTS`, `INCLUDE`, `LOAD`
  - Stream primitives: `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
  - Byte-level handle ops: `READ-FILE-BYTES`, `WRITE-FILE-BYTES`
  - Diagnostics (DEBUG): `LAST-WRITE-BYTES`, `LAST-READ-BYTES`
  - Diagnostics (DEBUG): `_lastWriteBuffer/_lastReadBuffer` diagnostics accessible for tests
- Async / concurrency: `SPAWN`, `FUTURE`, `TASK`, `JOIN`, `AWAIT`, `TASK?` (+ generic awaitable support)
- Exceptions / control: `CATCH`, `THROW`, `ABORT`, `EXIT`, `BYE`, `QUIT`
- Numeric base & parsing: `BASE`, `HEX`, `DECIMAL`, `>NUMBER` (extended), `STATE`
- Introspection: `SEE` (module-qualified + decompile text)
- Wordlist/search-order: `GET-ORDER`, `SET-ORDER`, `WORDLIST`, `DEFINITIONS`, `FORTH`
- Interactive input: `KEY`, `KEY?`, `ACCEPT`, `EXPECT`, `SOURCE`, `>IN`, `READ-LINE`
- Extended arithmetic: `*/MOD` plus double-cell ops `D+`, `D-`, `M*`

Recent extensions
- Inline IL: stabilized `IL{ ... }IL`
  - DynamicMethod signature now `(ForthInterpreter intr, ForthStack stack)`; `ldarg.0` is interpreter, `ldarg.1` is stack
  - Local type inference (declare `object` for `Pop()` results, `long` for arithmetic); consistent `LocalBuilder`-based `ldloc/stloc/ldloca`
  - Normalize non-virtual method calls to `call` even if `callvirt` token is used
  - Added comprehensive tests for fixed-slot/short/inline locals, increment, and POP/PUSH via interpreter and via `ForthStack`
- `TYPE` now supports: plain string, counted string address, (addr u) memory form, and string+length form; rejects bare numeric per tests.
- `WRITE-FILE` / `APPEND-FILE` accept string, counted string address, or (addr u) memory range.
- `>NUMBER` extended to accept counted string address and (addr u) forms in addition to raw string.
- Tokenizer updated: S" skips at most one leading space; supports accurate literal lengths.
- INCLUDE/LOAD: changed to evaluate entire file contents in a single `EvalAsync` call to preserve bracketed conditional constructs spanning line boundaries.
- Token preprocessing: synthesize common bracket composites (e.g. `[IF]`, `[ELSE]`, `[THEN]`) from separated `[` `IF` `]` sequences so older test sources and style variants are handled.
- `SkipBracketSection` improved to accept both composite tokens and separated bracket sequences when scanning for matching `[ELSE]`/`[THEN]`.
- ACCEPT/EXPECT/READ-LINE: updated to read character-by-character for proper ANS conformity, handling partial reads and CR/LF termination.
- Implemented full file-handle API in the interpreter (`OpenFileHandle`, `CloseFileHandle`, `ReadFileIntoMemory`, `WriteMemoryToFile`) and corresponding Forth words.
  - `WRITE-FILE` / `APPEND-FILE` accept string or counted-string/address forms and populate interpreter diagnostics (`_lastWriteBuffer`, `_lastWritePositionAfter`, `_lastReadBuffer`, etc.) used by tests.
  - Handle-based operations (open/read/write/reposition/close) use share-friendly FileStream modes to allow concurrent reads by other processes/tests.
  - `READ-FILE-BYTES` / `WRITE-FILE-BYTES` validate negative-length inputs and throw `CompileError` as tests expect.
- INCLUDE/LOAD: changed to unquote file arguments and evaluate whole file contents in a single `EvalAsync` call so bracketed conditional constructs spanning lines are preserved.
- Diagnostics: added last-read/last-write buffers and positions so introspection tests can validate live stream behavior.
- Tokenizer updates and bracketed-conditional handling were adjusted earlier; file inclusion behavior complements those changes to avoid unmatched bracket conditionals.
- Fixed generated test dependency on `ttester.4th` by copying it to output directory via project configuration.

Notes
- `GET-ORDER`/`SET-ORDER` expose core wordlist sentinel as `FORTH` (internally `null`).
- `ACCEPT`/`EXPECT` currently implement buffer write semantics; review for strict ANS compliance (edge cases like newline handling, partial fills).
- Awaitable detection centralized in `AwaitableHelper`.
- Block system implemented with per-block files + LRU cache + atomic save.
- INCLUDE/LOAD accept quoted string stack arguments and unquote before file access.

Progress / Repository tasks (current)
- [x] Create `tools/ans-diff` and integrate report artifact
- [x] Implement search-order + wordlist primitives
- [x] Implement KEY / KEY? / ACCEPT / EXPECT / SOURCE / >IN
- [x] Implement baseline file and byte-stream words
- [x] Diagnostics primitives behind DEBUG
- [x] Block system: BLOCK / SAVE / BLK + LRU eviction
- [x] Double-cell arithmetic words D+ D- M*
- [x] Truth value normalization (true = -1)
- [x] Extend TYPE for counted and memory forms
- [x] Extend WRITE-FILE / APPEND-FILE for counted and memory forms
- [x] Extend >NUMBER for counted and memory forms
- [x] Improve S" tokenizer handling (leading space rule)
- [x] Fix bracketed conditional handling across lines (INCLUDE/LOAD change + token preprocessing + SkipBracketSection fix)
- [x] Full test suite passing (257/257)
- [x] Full test suite passing (261/261)
- [x] ans-diff report updated (CI ready to fail on missing words)
- [x] Add unit tests for `TEST-IO` / `ADD-INPUT-LINE` (xUnit)
- [x] Add tester-harness Forth tests for `ADD-INPUT-LINE`
- [x] Remove legacy `tests/forth/framework.4th` compatibility wrapper
- [x] Add Roslyn source-generator `4th.Tests.Generators` to emit xUnit wrappers for `.4th` files
- [x] Generator emits `ForthGeneratedTests.g.cs` wrapping `.4th` files as `[Fact]` methods per TESTING group, grouped by file (nested classes for multi-test files)
- [x] Build/run generator and validate generated tests (rebuild + `dotnet test`)
- [x] Generator emits `ForthGeneratedTests.g.cs` wrapping `.4th` files as `[Fact]` methods per TESTING group
- [x] Inline IL: local typing + non-virtual `call` normalization + signature swap; add comprehensive tests
- [x] Enable `EnforceExtendedAnalyzerRules` property in `4th.Tests.Generators` project to satisfy analyzer guidance
- [x] Tighten ACCEPT/EXPECT/READ-LINE semantics for edge-case conformity (character-by-character reading)
- [x] Duplicate new tests as ttester tests (both .4th and .tester.4th formats)
- [x] Implement diagnostics and fix INCLUDE/LOAD to preserve multi-line bracketed constructs
- [x] Fix generated test dependency on `ttester.4th` by copying to output directory

Remaining / next work items
- [x] Add configurable LRU block cache size (interpreter ctor or setter)
- [x] Performance profiling for per-operation file accessor vs cached accessor; reintroduce safe cache if needed
- [x] Analyzer clean-up: add missing XML docs (e.g. Tokenizer) or suppress intentionally for internal-only types
- [x] Benchmark memory vs string path for TYPE / WRITE-FILE to guide future optimization
- [x] Consider unified string allocation helper for counted strings to reduce duplication
- [x] Add negative tests for new (addr u) file operations (invalid length, out-of-range addresses)
- [x] Add fast path optimization for pictured numeric conversion (#S loops) if profiling indicates hotspot
- [ ] Integrate `tools/ans-diff` execution into CI pipeline (run after build and write report artifact)
- [ ] Performance profiling for per-operation file accessor vs cached accessor; reintroduce safe cache if needed
- [ ] Analyzer clean-up: add missing XML docs (e.g. Tokenizer) or suppress intentionally for internal-only types
- [ ] Benchmark memory vs string path for TYPE / WRITE-FILE to guide future optimization
- [ ] Consider unified string allocation helper for counted strings to reduce duplication
- [ ] Add negative tests for new (addr u) file operations (invalid length, out-of-range addresses) — basic negative checks implemented, expand coverage
- [ ] Add fast path optimization for pictured numeric conversion (#S loops) if profiling indicates hotspot
- [ ] Integrate `tools/ans-diff` execution into CI pipeline (run after build and write report artifact)

Decisions made
- Favor per-call MMF accessor disposal for clarity and analyzer satisfaction; revisit only with profiling evidence.
- Extended primitives prefer pattern matching and numeric helper detection over additional stack peeks to keep code concise.
- Kept tokenizer S" behavior minimal (single leading space skip) to avoid unintended trimming.
- Inline IL: chose `(ForthInterpreter, ForthStack)` param order for inline IL dynamic methods; `ldarg.0` is interpreter.
- ACCEPT/EXPECT/READ-LINE: implemented character-by-character reading using ReadKey() for accurate partial read handling and CR/LF termination.

Potential future extensions
- Implement true BLOCK editor primitives (LOAD, LIST variants) and block-level caching policies.
- Add optional `ENV` wordlist or mechanism for platform/environment queries.
- Introduce configurable BASE parsing for signed/unsigned distinction (e.g. `>UNUMBER`).

Recent activity (most recent first)
- Completed performance benchmarking: Measured significant gains from ForthValue refactoring with BenchmarkDotNet, confirming reduced boxing overhead and efficient stack operations.
- Verified full test suite: All 257 tests pass consistently, validating the refactoring and new features.
- Completed refactoring to use ForthValue internally: All primitives, interpreter logic, and stack operations now use ForthValue for efficient, typed operations without boxing.
- Maintained public API compatibility: IForthInterpreter interface remains object-based to avoid breaking changes for external consumers.
- Updated all core primitives to handle ForthValue types correctly, including arithmetic, memory, IO, and control flow operations.
- Verified refactoring with full test suite: All 257 tests pass, confirming correctness and performance improvements.
- Implemented compatibility layer in ForthInterpreter: Public methods convert between object and ForthValue seamlessly.
- Changed ForthStack to implement IReadOnlyList<ForthValue> for full list interface including indexing.
- Implemented IReadOnlyCollection<ForthValue> directly in ForthStack, removing AsReadOnly method for cleaner API.
- Refactored ForthStack to use a custom immutable linked list for O(1) push/pop operations, improving performance over ImmutableList's O(log n).
- Refactored ForthStack to use typed ForthValue struct, storing primitives as values without boxing to match CLR evaluation stack structure.
- Implemented interpreter file-handle API and Forth file primitives, added diagnostics used by tests.
- Fixed INCLUDE/LOAD behavior to evaluate full file text to preserve multi-line constructs.
- Completed changes and validated with full test run: all tests pass (261/261).
- Completed performance benchmarking: Measured gains from ForthValue refactoring, confirming reduced boxing overhead and efficient stack operations.
- Refactored ForthStack and ForthValue internals for performance; public `IForthInterpreter` API remains object-based for compatibility.
- Fixed generated test dependency on `ttester.4th` by adding Content item to copy it to output directory.

## Pending Tasks
- [ ] Consider exposing ForthValue in future major version for direct typed API access
- [ ] Optimize memory usage in ForthValue struct if needed (currently 24 bytes per value)
- [ ] Consider exposing ForthValue in a future major version for direct typed API access
- [ ] Optimize memory usage in ForthValue struct if needed (currently ~24 bytes per value)
- [x] Add benchmarks to measure performance gains from reduced boxing
- [x] Update documentation to reflect internal ForthValue usage
- [x] Review and potentially simplify primitive implementations now that typing is consistent
