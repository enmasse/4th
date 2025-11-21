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
  - `*/MOD` implemented in core using BigInteger to avoid overflow

Notes from code inspection
- `GET-ORDER`/`SET-ORDER` use internal `GetOrder`/`SetOrder` helpers and expose `FORTH` as `null` in the list.
- `ACCEPT`/`EXPECT` currently return a string and length (simplified; do not operate on raw c-addr memory in a true ANS manner). `READ-LINE` implemented as a simplified ANS-compatible primitive
- `AWAIT`/`TASK?` now include explicit support for pattern-based awaitables that expose `GetAwaiter()` in addition to `Task`/`Task<T>` and `ValueTask`/`ValueTask<T>`. A helper `AwaitableHelper` handles detection, synchronous completion, and registering continuations via `OnCompleted`/`UnsafeOnCompleted`.
- `READ-FILE-BYTES` / `WRITE-FILE-BYTES` provide low-level memory IO used by tests; they may be candidates for replacement with strictly ANS semantics.
- Pictured numeric / prelude words (e.g., `TRUE`, `FALSE`, `*/MOD` definition in `prelude.4th`) are present and tested.

Missing or incomplete ANS words (prioritized)
1. Wordlist / vocabulary control
   - `WORDLIST`, `DEFINITIONS` and full `FORTH` sentinel exposure (implemented)
2. Interactive source tracking
   - (now implemented) `SOURCE` and `>IN` are available; `READ-LINE` implemented as a simplified primitive
3. Block system (optional)
   - `BLOCK`, `SAVE`, `BLK` implemented with a per-block file backend and MMF/filestream fallback; `LOAD` (runtime loader) is present. See notes below.
4. Read-line primitive
   - `READ-LINE` implemented; `ACCEPT`/`EXPECT` still use simplified behavior and may be refined to match full c-addr/u semantics
5. Truth value normalization
   - Some primitives (e.g., `KEY?`, `FILE-EXISTS`) return `-1` for true; ensure all bindings and modules follow the normalized convention (true = -1).
6. AWAIT / TASK? robustness
   - Now improved: added GetAwaiter-pattern support and tests. Consider further performance hardening (delegate caching) if needed.
7. Additional double-cell and advanced math words
   - `D+`, `D-`, `M*` implemented; `*/MOD` exists

Recommendations — next steps (actionable)
- Decide on truth-value normalization policy and update primitives/tests consistently (normalize to `-1` recommended for ANS compatibility).
- Iterate on block-system behavior if stricter ANS semantics are required (e.g., stable c-addr vs. zero-copy pointer exposure, eviction policy).
- Consider performance hardening for awaitable handling: cache MethodInfos or compiled delegates for `GetAwaiter`/`OnCompleted`/`GetResult`.
- Consider replacing low-level `READ-FILE-BYTES` / `WRITE-FILE-BYTES` tests with higher-level `READ-FILE` / `WRITE-FILE` usage if ANS compliance is the goal.
- Add a CI job to run `tools/ans-diff` and fail on regressions; export JSON output for machine checks.

Repository tasks (current)
- [x] Create `tools/ans-diff` script to collect `Primitive` names and compare against ANS list
- [x] Implement high-priority words: `GET-ORDER`, `SET-ORDER`, `KEY`, `KEY?`, `ACCEPT`, `EXPECT`
- [x] Implement baseline file stream words: `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
- [x] Implement byte-oriented handle words: `READ-FILE-BYTES`, `WRITE-FILE-BYTES`
- [x] Gate diagnostics primitives behind DEBUG (`LAST-WRITE-BYTES`, `LAST-READ-BYTES`)
- [x] Implement block system words: `BLOCK`, `SAVE`, `BLK` (per-block files with atomic replace) — `LOAD` runtime loader present
- [x] Decide and normalize truth values across primitives (true = -1)
- [x] Implement `READ-LINE`, `SOURCE`, `>IN`
- [x] Implement `WORDLIST`, `DEFINITIONS`, `FORTH` with Wordlist objects
- [x] Improve `AWAIT`/`TASK?` robustness (ValueTask / awaitable handling)
- [x] Add `D+`, `D-`, `M*` primitives and tests
- [x] Add `BLK` primitive and support for ' pushing undefined names (assist WORDLIST usage)
- [x] Update `tools/ans-diff` to write report file and fail on missing words (non-zero exit)
- [x] Update CI workflow to run `tools/ans-diff`, capture report, and upload artifact

- [ ] Add LRU eviction for block address/accessor cache to limit address-space growth
- [ ] Consider tightening ACCEPT/EXPECT/READ-LINE semantics to match full c-addr/u behavior

Recent activity (2025-11-21 -> 2025-11-25)
--------------------------------
- Added double-cell arithmetic primitives `D+`, `D-`, and `M*` with unit tests (`DoubleCellArithmeticTests`).
- Added `BLK` primitive and adjusted `Prim_Tick` to push raw name strings when unresolved to support `WORDLIST ' <name> DEFINITIONS` patterns in tests.
- Updated `tools/ans-diff` to write `tools/ans-diff/report.md` directly and return non-zero when ANS words are missing; fixed invocation to avoid terminal hangs in CI.
- Updated `.github/workflows/ci.yml` to run `tools/ans-diff` and upload the `report.md` artifact (guarded by `if: always()`).
- All unit tests pass locally (212/212) after changes.

If you want, I can:
- Expose `CLOSE-BLOCK-FILE` / `FLUSH-BLOCK-FILE` primitives and add tests for lifecycle operations.
- Implement LRU eviction for block addresses/accessors to limit address-space growth.
- Add a CI workflow step to fail the build when `tools/ans-diff` reports missing words (currently it is run and the report is uploaded; it already returns non-zero on missing words).
