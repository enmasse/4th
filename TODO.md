# TODO: ANS/Forth conformity gap analysis

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
- `TYPE` now supports: plain string, counted string address, (addr u) memory form, and string+length form; rejects bare numeric per tests.
- `WRITE-FILE` / `APPEND-FILE` accept string, counted string address, or (addr u) memory range.
- `>NUMBER` extended to accept counted string address and (addr u) forms in addition to raw string.
- Tokenizer updated: S" skips at most one leading space; supports accurate literal lengths.

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
- [x] Full test suite passing (225/225)
- [x] ans-diff report updated (CI ready to fail on missing words)
- [x] Add `ans-diff` project to solution and fix LangVersion to `preview`
- [x] Run `ans-diff` and write report to `tools/ans-diff/report.md`

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

Decisions made
- Favor per-call MMF accessor disposal for clarity and analyzer satisfaction; revisit only with profiling evidence.
- Extended primitives prefer pattern matching and numeric helper detection over additional stack peeks to keep code concise.
- Kept tokenizer S" behavior minimal (single leading space skip) to avoid unintended trimming.

Potential future extensions
- Implement true BLOCK editor primitives (LOAD, LIST variants) and block-level caching policies.
- Add optional `ENV` wordlist or mechanism for platform/environment queries.
- Introduce configurable BASE parsing for signed/unsigned distinction (e.g. `>UNUMBER`).

Recent activity (most recent first)
- Extended TYPE, >NUMBER, file primitives; all tests passing (225/225).
- Tokenizer S" adjustments merged.
- Prior: block system + LRU tests, analyzer & nullable warnings fixed, ans-diff integration.

If needed, next actionable micro-task candidates
- Add XML docs for Tokenizer (clear CS1591 warnings)
- Implement configurable block cache size
- Add tests for counted-address WRITE-FILE with zero-length strings
