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
- Async / concurrency: `SPAWN`, `FUTURE`, `TASK`, `JOIN`, `AWAIT`, `TASK?` (implementation present; some robustness/normalization work remains)
- Exceptions / control: `CATCH`, `THROW`, `ABORT`, `EXIT`, `BYE`, `QUIT`
- Numeric base & parsing: `BASE`, `HEX`, `DECIMAL`, `>NUMBER`, `STATE`
- Introspection: `SEE` (module-qualified and basic decompile text present)
- Wordlist/search-order: `GET-ORDER`, `SET-ORDER` (implemented)
- Interactive input: `KEY`, `KEY?`, `ACCEPT`, `EXPECT` (implemented; simplified semantics for buffer addressing)
- Extended arithmetic: `*/MOD` implemented in core using BigInteger to avoid overflow

Notes from code inspection
- `GET-ORDER`/`SET-ORDER` use internal `GetOrder`/`SetOrder` helpers and expose `FORTH` as `null` in the list.
- `ACCEPT`/`EXPECT` both return a string and length (simplified; do not operate on raw c-addr memory in a true ANS manner) — may be revisited for full c-addr semantics.
- `TASK?` and `AWAIT` use reflection-based heuristics to interop with `ValueTask<>` and `Task<>`. This works but is fragile (preexisting TODO remains to harden handling).
- `READ-FILE-BYTES` / `WRITE-FILE-BYTES` provide low-level memory IO used by tests; they may be candidates for replacement with strictly ANS semantics.
- Pictured numeric / prelude words (e.g., `TRUE`, `FALSE`, `*/MOD` definition in `prelude.4th`) are present and tested.

Missing or incomplete ANS words (prioritized)
1. Wordlist / vocabulary control
   - `WORDLIST`, `DEFINITIONS` and full `FORTH` sentinel exposure (not present)
2. Interactive source tracking
   - `SOURCE` and `>IN` (interpreter input position tracking) are not implemented
3. Block system (optional)
   - `BLOCK`, `LOAD`/`SAVE`, `BLK` semantics (partial `LOAD` is present as runtime loader, block device semantics are not implemented)
4. Read-line primitive
   - `READ-LINE` (explicit ANS-style line read) is not provided; `ACCEPT`/`EXPECT` supply simplified behavior
5. Truth value normalization
   - Some primitives (e.g., `KEY?`, `FILE-EXISTS`) return `-1` for true; `TASK?` previously returned `1` for true. Truth values have been normalized to `-1` in core primitives, but review remaining bindings and modules.
6. AWAIT / TASK? robustness
   - Improve handling of `ValueTask`, `ValueTask<T>`, and non-standard awaitables without fragile reflection
7. Additional double-cell and advanced math words
   - `D+`, `D-`, `M*` not found; `*/MOD` exists

Recommendations — next steps (actionable)
- Decide on truth-value normalization policy and update primitives/tests consistently (normalize to `-1` recommended for ANS compatibility).
- Implement `SOURCE` / `>IN` and refine `ACCEPT`/`EXPECT` to operate with c-addr/u semantics or provide a compatibility shim in the prelude.
- Implement `WORDLIST` / `DEFINITIONS` / `FORTH` exposure to match ANS vocabulary model or document intentional divergence.
- Harden `AWAIT`/`TASK?` handling: prefer feature-detection rather than type-name-based reflection; consider helper utilities for awaitable unwrapping.
- Consider replacing low-level `READ-FILE-BYTES` / `WRITE-FILE-BYTES` tests with higher-level `READ-FILE` / `WRITE-FILE` usage if ANS compliance is the goal.
- Add a CI job to run `tools/ans-diff` and fail on regressions; export JSON output for machine checks.

Repository tasks (current)
- [x] Create `tools/ans-diff` script to collect `Primitive` names and compare against ANS list
- [x] Implement high-priority words: `GET-ORDER`, `SET-ORDER`, `KEY`, `KEY?`, `ACCEPT`, `EXPECT`
- [x] Implement baseline file stream words: `OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`
- [x] Implement byte-oriented handle words: `READ-FILE-BYTES`, `WRITE-FILE-BYTES`
- [x] Gate diagnostics primitives behind DEBUG (`LAST-WRITE-BYTES`, `LAST-READ-BYTES`)
- [ ] Implement block system words: `BLOCK`, `LOAD`, `SAVE`, `BLK`
- [x] Decide and normalize truth values across primitives (true = -1)
- [ ] Implement `READ-LINE` or refine `ACCEPT`/`EXPECT` to match c-addr semantics
- [ ] Implement `WORDLIST`, `DEFINITIONS`, `FORTH`
- [ ] Implement `SOURCE`, `>IN`
- [ ] Improve `AWAIT`/`TASK?` robustness (ValueTask / awaitable handling)
- [ ] Add ANS conformity run to CI (ans-diff) and fail on unexpected regressions
- [ ] Add/verify `D+`, `D-`, `M*` if double-cell arithmetic is required

Recent activity (2025-11-20)
--------------------------------
- `tools/ans-diff` executed and `tools/ans-diff/report.md` refreshed.
- Core and prelude tests exercised locally; many missing-word tests implemented and passing in the local run.
- File I/O primitives and byte-level tests implemented and covered by unit tests.
- Normalized boolean truth values to ANS convention (true = -1): updated `ToLong`, `ClrBinder`, `TASK?`, `AWAIT`, and `.S`; updated tests accordingly. Full test suite passes (194/194).

If you want, I can:
- Open PR that normalizes boolean-returning primitives to `-1` and update tests accordingly.
- Implement `SOURCE` / `>IN` and adjust `ACCEPT`/`EXPECT` to operate on memory buffer semantics.
- Add a GitHub Actions workflow step to run `tools/ans-diff` and upload `report.md` as an artifact.
