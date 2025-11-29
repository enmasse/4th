# TODO: ANS/Forth conformity gap analysis

## Resolved Issues
- ~~Correct `>NUMBER` parsing semantics~~ **[RESOLVED 2025-01-XX]**:
  - ~~Ensure proper accumulation for decimal and hex bases.~~
  - ~~Handle remainder and partial conversion per ANS.~~
  - ~~Add tests for mixed digits, base changes, and remainder reporting.~~
  - Investigate c-addr off-by-one: confirm `S"` pushes `addr+1` and correct `u`; fix first-digit loss (e.g., "123" -> 123 not 23), and verify hex remainder case ("FFZ" in HEX -> 255, rem=1, digits=2).
- ~~Adjust block `LIST` formatting~~ **[RESOLVED 2025-11-27]**:
  - ~~Match expected line-number prefix and content formatting.~~
  - ~~Avoid stray nulls/control characters in output.~~
  - ~~Add tests for typical and empty lines.~~
- ~~Fix `APPEND-FILE` behavior~~ **[RESOLVED 2025-11-27]**:
  - ~~Prevent duplicated content and stray control characters.~~
  - ~~Ensure newline handling matches expectations.~~
  - ~~Add tests for multiple appends and exact byte content.~~
- ~~Tighten bracket conditionals `[IF] [ELSE] [THEN]` semantics~~ **[RESOLVED 2025-11-27]**:
  - ~~Fully align skipping and nesting behavior with ANS Forth.~~
  - ~~Support mixed composite tokens and separated bracket forms reliably.~~
  - ~~Add more tests covering nested, empty branches, and edge tokenization cases.~~

_Last updated: 2025-11-27_

## Goal
- Compare the current implementation against ANS Forth word sets (Core, Core-Ext, File, Block, optional Float) and identify words that are missing or partially implemented.

## Method
- A scan of `Primitive` attributes and tests in the repository is used to determine what exists. Tool `tools/ans-diff` automates comparison and can fail CI on missing words. It now supports multiple sets via `--sets=` (e.g. `--sets=core,core-ext,file,block,float` or `--sets=all`) and `--fail-on-missing=` to toggle CI failures.

## Status — implemented / obvious support (non-exhaustive)
- Definitions / compilation words: `:`, `;`, `IMMEDIATE`, `POSTPONE`, `[`, `]`, `'`, `LITERAL`
- Control flow: `IF`, `ELSE`, `THEN`, `BEGIN`, `WHILE`, `REPEAT`, `UNTIL`, `DO`, `LOOP`, `LEAVE`, `UNLOOP`, `I`, `J`, `RECURSE`
- Defining words: `CREATE`, `DOES>`, `VARIABLE`, `CONSTANT`, `VALUE`, `TO`, `DEFER`, `IS`, `MARKER`, `FORGET`, `>BODY`
- Stack / memory: `@`, `!`, `C@`, `C!`, `,`, `ALLOT`, `HERE`, `PAD`, `COUNT`, `MOVE`, `FILL`, `ERASE`, `S>D`, `SP!`, `SP@`
- I/O: `.`, `.S`, `CR`, `EMIT`, `TYPE`, `WORDS`, pictured numeric (`<#`, `HOLD`, `#`, `#S`, `SIGN`, `#>`)
- Strings & parsing: `S"`, `S`, `."`, `WORD`
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
- Interactive input: `KEY`, `KEY?`, `ACCEPT`, `EXPECT`, `SOURCE`, `>IN`, `READ-LINE`, `WORD`
- Extended arithmetic: `*/MOD` plus double-cell ops `D+`, `D-`, `M*`, `M+`, `SM/REM`, `FM/MOD`
- Environment queries: `ENV` wordlist with `OS`, `CPU`, `MEMORY`, `MACHINE`, `USER`, `PWD`
- Help system: `HELP` (general help or word-specific)

## Recent extensions
- Implemented missing ANS-tracked words: `."`, `ABORT"`, `>BODY`, `M/MOD`, `S"` with regression tests.
- Implemented Core-Ext `WITHIN` with regression tests.
- Implemented Core-Ext `COMPARE` with regression tests.
- Implemented Core-Ext `/STRING` with regression tests.
- Implemented Core-Ext `ALLOCATE` and `FREE` with regression tests.
- Tokenizer: recognize `ABORT"` composite and skip one leading space after the opening quote.
- IDE: suppressed IDE0051 on `CorePrimitives` to avoid shading reflection-invoked primitives.
- ans-diff: robust repo-root resolution and improved `[Primitive("…")]` regex to handle escapes; now detects `."`, `ABORT"`, `S"` reliably. Added multi-set tracking (Core/Core-Ext/File/Block/Float), CLI selection via `--sets=`, and `--fail-on-missing` switch. Report now includes present/missing/extras for the selected sets.
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
- Added ENV wordlist for environment queries (OS, CPU, MEMORY, MACHINE, USER, PWD).
- Enhanced REPL with command history, HELP/STACK commands, and improved error handling.
- Extended HELP primitive to show general help when no word specified.
- Added comprehensive tests validating all README examples.
- Implemented additional ANS core words: 0>, 1+, 1-, 2*, 2/, ABS, U<, UM*, UM/MOD, with regression tests.
- Fixed APPEND-FILE data disambiguation to prevent duplicated content and ensure correct appending behavior.
- Fixed LIST block formatting to trim null characters, ensuring clean output without control characters.

## Notes
- **Duplicate primitive detection**: `CreateWords()` now validates that each primitive name is unique within its module, preventing silent shadowing issues.
- `GET-ORDER`/`SET-ORDER` expose core wordlist sentinel as `FORTH` (internally `null`).
- `ACCEPT`/`EXPECT` currently implement buffer write semantics; review for strict ANS compliance (edge cases like newline handling, partial fills).
- Awaitable detection centralized in `AwaitableHelper`.
- Block system implemented with per-block files + LRU cache + atomic save.
- INCLUDE/LOAD accept quoted string stack arguments and unquote before file access.
- ENV wordlist provides system environment information.
- HELP shows general help or word-specific help.

## Progress / Repository tasks (current)
- [x] Fix `>NUMBER` first-digit loss bug and add duplicate primitive detection
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
- [x] Full test suite passing (349/349)
- [x] ans-diff report updated (CI ready to fail on missing words)
- [x] Add unit tests for `TEST-IO` / `ADD-INPUT-LINE` (xUnit)
- [x] Add tester-harness Forth tests for `ADD-INPUT-LINE`
- [x] Fix `ADD-INPUT-LINE` numeric / counted-addr disambiguation and update tests (handled counted strings, addr/len pair ordering)
- [x] Remove legacy `tests/forth/framework.4th` compatibility wrapper
- [x] Add Roslyn source-generator `4th.Tests.Generators` to emit xUnit wrappers for `.4th` files
- [x] Generator emits `ForthGeneratedTests.g.cs` wrapping `.4th` files as `[Fact]` methods per TESTING group, grouped by file (nested classes for multi-test files)
- [x] Build/run generator and validate generated tests (rebuild + `dotnet test`)
- [x] Mark the unified string allocation helper as completed.
- [x] Performance profiling for per-operation file accessor vs cached accessor; reintroduce safe cache if needed
- [x] Analyzer clean-up: add missing XML docs (e.g. Tokenizer) or suppress intentionally for internal-only types
- [x] Benchmark memory vs string path for TYPE / WRITE-FILE to guide future optimization
- [x] Consider unified string allocation helper for counted strings to reduce duplication
- [x] Add negative tests for new (addr u) file operations (invalid length, out-of-range addresses) — expanded coverage implemented
- [x] Implement SEARCH primitive with regression tests

## Potential future extensions
- Implement additional ANS Forth words (e.g., floating-point extensions, more file operations).
- Support for binary file I/O or more advanced block operations.
- Implement true BLOCK editor primitives (LOAD, LIST variants) and block-level caching policies.
- Add optional `ENV` wordlist or mechanism for platform/environment queries.
- Introduce configurable BASE parsing for signed/unsigned distinction (e.g. `>UNUMBER`).

## Missing ANS Forth words (tracked by `ans-diff`)
- Depends on selected sets. Core subset currently reports none; Core-Ext/File/Block/Float will list gaps until implemented.

## Current gaps (from latest ans-diff for sets: Core, Core-Ext, File, Block, Float)
- BUFFER
- CASE
- CREATE-FILE
- DEFER!
- DEFER@
- DELETE-FILE
- EMPTY-BUFFERS
- ENDCASE
- ENDOF
- F>S
- FABS
- FACOS
- FASIN
- FATAN2
- FCOS
- FEXP
- FILE-POSITION
- FILE-STATUS
- FLOG
- FLOOR
- FLUSH
- FROUND
- FSIN
- FTAN
- INCLUDE-FILE
- INCLUDED
- OF
- REFILL
- RENAME-FILE
- RESIZE
- RESTORE-INPUT
- S>F
- SAVE-BUFFERS
- SAVE-INPUT
- SCR
- SOURCE-ID
- UNUSED
- UPDATE