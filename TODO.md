# TODO: ANS/Forth conformity gap analysis

_Last updated: 2025-11-23_

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

Notes
- `GET-ORDER`/`SET-ORDER` expose core wordlist sentinel as `FORTH` (internally `null`).
- `ACCEPT`/`EXPECT` currently implement buffer write semantics; review for strict ANS compliance (edge cases like newline handling, partial fills).
- Awaitable detection centralized in `AwaitableHelper`.
- Block system implemented with per-block files + LRU cache + atomic save.

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
- [x] Full test suite passing (250/250)
- [x] ans-diff report updated (CI ready to fail on missing words)
- [x] Add unit tests for `TEST-IO` / `ADD-INPUT-LINE` (xUnit)
- [x] Add tester-harness Forth tests for `ADD-INPUT-LINE`
- [x] Remove legacy `tests/forth/framework.4th` compatibility wrapper
- [x] Add Roslyn source-generator `4th.Tests.Generators` to emit xUnit wrappers for `.4th` files
- [x] Generator emits `ForthGeneratedTests.g.cs` wrapping `.4th` files as `[Fact]` methods
- [x] Build/run generator and validate generated tests (rebuild + `dotnet test`)
- [x] Inline IL: local typing + non-virtual `call` normalization + signature swap; add comprehensive tests
- [x] Enable `EnforceExtendedAnalyzerRules` property in `4th.Tests.Generators` project to satisfy analyzer guidance

Remaining / next work items
- [ ] Optional: tighten ACCEPT/EXPECT/READ-LINE semantics (edge-case conformity: handling of CR/LF, partial reads)
- [ ] Add configurable LRU block cache size (interpreter ctor or setter)
- [ ] Performance profiling for per-operation file accessor vs cached accessor; reintroduce safe cache if needed
- [ ] Analyzer clean-up: add missing XML docs (e.g. Tokenizer) or suppress intentionally for internal-only types
- [ ] Benchmark memory vs string path for TYPE / WRITE-FILE to guide future optimization
- [ ] Consider unified string allocation helper for counted strings to reduce duplication
- [ ] Add negative tests for new (addr u) file operations (invalid length, out-of-range addresses)
- [ ] Add fast path optimization for pictured numeric conversion (#S loops) if profiling indicates hotspot
- [ ] Integrate `tools/ans-diff` execution into CI pipeline (run after build and write report artifact)
- [ ] Enable `EnforceExtendedAnalyzerRules` property in `4th.Tests.Generators` project to satisfy analyzer guidance

Decisions made
- Favor per-call MMF accessor disposal for clarity and analyzer satisfaction; revisit only with profiling evidence.
- Extended primitives prefer pattern matching and numeric helper detection over additional stack peeks to keep code concise.
- Kept tokenizer S" behavior minimal (single leading space skip) to avoid unintended trimming.
- Inline IL: chose `(ForthInterpreter, ForthStack)` param order for inline IL dynamic methods; `ldarg.0` is interpreter.

Potential future extensions
- Implement true BLOCK editor primitives (LOAD, LIST variants) and block-level caching policies.
- Add optional `ENV` wordlist or mechanism for platform/environment queries.
- Introduce configurable BASE parsing for signed/unsigned distinction (e.g. `>UNUMBER`).

Recent activity (most recent first)
- Modified Roslyn source-generator to split .4th test files into individual [Fact] methods per TESTING group for better isolation and reporting (250 tests now).
- Combined related tests in .tester.4th files into single T{ }T blocks using ttester.4th's multi-result capabilities.
- Added TESTING comments to group related tests in each .tester.4th file.
- Changed all .tester.4th files to reference ttester.4th instead of tester.fs.
- Enabled EnforceExtendedAnalyzerRules in 4th.Tests.Generators project.
- Inline IL: fix var opcode handling; add tests for fixed/short/inline locals, increment, POP/PUSH (interpreter + stack). Normalize non-virtual calls; swap dynamic method args order. Full suite passing (241/241).
- Fixed ADD-INPUT-LINE ambiguity: prefer (addr u) pair over counted-addr when both patterns match.
- Added xUnit tests exercising `TEST-IO`/`ADD-INPUT-LINE` (direct string, counted-addr, addr/u) at `4th.Tests/Core/Modules/TestIOModuleTests.cs`.
- Added Forth tester-harness tests at `tests/forth/add-input-line-tests.tester.4th` and a compatibility `.4th` variant.
- Removed legacy `tests/forth/framework.4th` file and updated test set.
- Added Roslyn source-generator `4th.Tests.Generators/ForthTestGenerator.cs` to generate xUnit wrappers for `.4th` files.
- Generator produces `ForthGeneratedTests.g.cs` during build; rebuild to make Test Explorer discover tests.
- Ran `ans-diff` and wrote `tools/ans-diff/report.md`.
- Full test suite passing after changes (233/233).
