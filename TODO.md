TODO: ANS?Forth conformity gap analysis

Goal
- Compare the current implementation against the ANS Forth core wordlist and identify words that are missing or partially implemented.

Method
- A scan of `Primitive` attributes and tests in the repository was used to determine what exists. A tool `tools/ans-diff` is now available for automated comparison.

Status — implemented / obvious support (non-exhaustive)
- Definitions / compilation words: `:`, `;`, `IMMEDIATE`, `POSTPONE`, `[`, `]`, `'`, `LITERAL`
- Control flow: `IF`, `ELSE`, `THEN`, `BEGIN`, `WHILE`, `REPEAT`, `UNTIL`, `DO`, `LOOP`, `LEAVE`, `UNLOOP`, `I`, `RECURSE`
- Defining words: `CREATE`, `DOES>`, `VARIABLE`, `CONSTANT`, `VALUE`, `TO`, `DEFER`, `IS`, `MARKER`, `FORGET`
- Stack / memory: `@`, `!`, `C@`, `C!`, `,`, `ALLOT`, `HERE`, `COUNT`, `MOVE`, `FILL`, `ERASE`
- I/O: `.`, `.S`, `CR`, `EMIT`, `TYPE`, `WORDS`, pictured numeric (`<#`, `HOLD`, `#`, `#S`, `SIGN`, `#>`)
- File I/O (subset): `READ-FILE`, `WRITE-FILE`, `APPEND-FILE`, `FILE-EXISTS`, `INCLUDE`
  (Extended / partial): `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE` (basic semantics present; need ANS ior/stack shape audit)
- Async / concurrency: `SPAWN`, `FUTURE`, `TASK`, `JOIN`, `AWAIT`, `TASK?` (ValueTask is handled via reflection today)
- Exceptions / control: `CATCH`, `THROW`, `ABORT`, `EXIT`, `BYE`, `QUIT`
- Numeric base & parsing: `BASE`, `HEX`, `DECIMAL`, `>NUMBER`, `STATE`
- Introspection: `SEE`

Newly implemented (since last review)
- Wordlist / search-order: `GET-ORDER`, `SET-ORDER`
- Interactive input: `KEY`, `KEY?`, `ACCEPT` (simplified; returns string and length)
- IForthIO: new methods `ReadKey()` and `KeyAvailable()` with default implementations so existing test IO implementations do not require changes
- Tooling: `tools/ans-diff` - scans `Primitive("...")` and compares against an embedded ANS list
- Tests: unit tests added for new words:
  - `4th.Tests/Core/MissingWords/IOKeyAndAcceptTests.cs`
  - `4th.Tests/Core/MissingWords/VocabOrderTests.cs`
  - All tests were executed locally and passed in the test suite
  - File IO diagnostics instrumentation (`LAST-WRITE-BYTES`, `LAST-READ-BYTES`) and expanded FileIO tests (now green)
  - Byte-level file primitives `READ-FILE-BYTES`, `WRITE-FILE-BYTES` (non-ANS; candidate for replacement by standard `READ-FILE` / `WRITE-FILE` usage)
  - Diagnostics gated behind `#if DEBUG` so test-only instrumentation is omitted in release builds
  - Comparison primitives normalized to ANS-style truth values (-1 for true)
  - Tests updated to expect -1 truth values and validated: full test suite passes locally
  - `tools/ans-diff` executed and `tools/ans-diff/report.md` updated
  - Changes committed and pushed to `origin/main`

Missing or incomplete ANS words (prioritized)
1. Full wordlist / search-order API and related words
   - `WORDLIST`, `DEFINITIONS`, `FORTH` (semantics/representation to verify)
2. Interactive input / source position
   - `EXPECT`, `SOURCE`, `>IN` (proper handling of interpreter input position is missing)
3. Completed file API (streams & metadata)
   - Standard signatures & ior ordering: confirm `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
   - Implement missing `READ-LINE` (ANS) and unify `READ-FILE-BYTES` / `WRITE-FILE-BYTES` into standard words or drop
4. Block system (if the goal includes block I/O)
   - `BLOCK`, `LOAD`, `SAVE`, `BLK` etc.
5. Robustness / semantic fixes
   - Improve `AWAIT`/`TASK?` so ValueTask and IValueTaskSource are handled without fragile type-name hacks
   - Ensure consistent all-bits-set (-1) representation for true
6. Double-cell words and advanced numeric operations
   - Verify / implement `D+`, `D-`, `M*`, `*/MOD`, etc. according to ANS

Recommendations — next steps
- Commit + push the recent changes (tooling, primitives, tests)
- Refine `ACCEPT` / `KEY` semantics (particularly `>IN` / `SOURCE`) and implement `EXPECT`
- Implement file stream API and block words if the target is full ANS compatibility
- Fix `AWAIT`/`TASK?` robustness (refactor task detection)
- Integrate `tools/ans-diff` into CI and generate machine-readable reports (JSON)
- Gate or remove diagnostic primitives (`LAST-WRITE-BYTES`, `LAST-READ-BYTES`) before release (keep behind a DEBUG or TEST module)
- Normalize truth value semantics to -1 (all bits set) for all comparison/logical words (audit tests expecting 1)
- Replace custom byte file primitives with portable pattern using standard `READ-FILE` / `WRITE-FILE`
- Add SOURCE / >IN / EXPECT to support interpreter source tracking & interactive input per ANS
- Add negative-number tests for `/`, `MOD`, `/MOD`, and `*/` to document sign/rounding semantics
- Consider implementing safe `*/MOD` primitive to avoid overflow in (n1*n2) before divide
- Integrate `tools/ans-diff` into CI to surface conformance regressions automatically

Repository tasks (can be automated)
- [x] Create `tools/ans-diff` script to collect `Primitive` names and compare against ANS list
- [x] Implement high-priority words (`GET-ORDER`, `SET-ORDER`, `KEY`, `KEY?`, `ACCEPT`)
- [ ] Improve `AWAIT`/`TASK?` robustness
- [x] Implement baseline file stream words (`OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`) — refine ANS compliance
- [ ] Add / run an ANS conformity test suite in CI
- [ ] Implement `READ-LINE`
- [ ] Implement `WORDLIST`, `DEFINITIONS`, `FORTH` sentinel exposure
- [ ] Implement `SOURCE`, `>IN`, `EXPECT`
- [ ] Normalize truth values (true = -1)
- [ ] Remove or gate diagnostics primitives
- [ ] Replace byte-oriented file primitives with standard equivalents / wrappers

Recent activity (2025-11-20)
--------------------------------
- Gated diagnostic primitives behind DEBUG and updated tests to skip when diagnostics are absent.
- Normalized comparison primitives to return -1 for true; updated tests and prelude where necessary.
- Ran full test suite locally: all tests passing (191/191).
- Ran `tools/ans-diff` and updated `tools/ans-diff/report.md` and committed changes.

If you want, I can:
- Add negative-number division tests and/or implement a safe `*/MOD` primitive.
- Add a CI job to run `tools/ans-diff` and fail on regressions.
