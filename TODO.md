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

Missing or incomplete ANS words (prioritized)
1. Full wordlist / search-order API and related words
   - `WORDLIST`, `DEFINITIONS`, `FORTH` (semantics/representation to verify)
2. Interactive input / source position
   - `EXPECT`, `SOURCE`, `>IN` (proper handling of interpreter input position is missing)
3. Completed file API (streams & metadata)
   - `OPEN-FILE`, `CLOSE-FILE`, `READ-LINE`, `FILE-SIZE`, `REPOSITION-FILE`
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

Repository tasks (can be automated)
- [x] Create `tools/ans-diff` script to collect `Primitive` names and compare against ANS list
- [x] Implement high-priority words (`GET-ORDER`, `SET-ORDER`, `KEY`, `KEY?`, `ACCEPT`)
- [ ] Improve `AWAIT`/`TASK?` robustness
- [ ] Implement file stream words (`OPEN-FILE`, `CLOSE-FILE`, `FILE-SIZE`, `REPOSITION-FILE`)
- [ ] Add / run an ANS conformity test suite in CI

Notes
- This file was updated automatically after new primitives and tests were added. Do you want me to commit these changes now?
