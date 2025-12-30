# TODO: 4th (ANS/Forth) gap analysis

## Current status
- `dotnet test`: **806 total / 803 passed / 0 failed / 3 skipped** (local run)
- Major parser/tokenizer migrations are complete enough for the test suite to be stable.

## Skipped tests (intentional)
1. `Forth.Tests.Compliance.Forth2012ComplianceTests.ParanoiaTest`
   - Long-running / hangs.
2. `Forth.Tests.Compliance.ErrorReportDiagnosticTests.Diagnostic_LoadErrorReportLineByLine`
   - Diagnostic-only; line-by-line loading is not representative (colon definitions span multiple lines). Use whole-file `INCLUDE`.
3. `Forth.Tests.Compliance.ParanoiaIsolationTests.ParanoiaWithStackTrace`
   - Diagnostic-only and long-running.

## Next work items (actionable)
### 1) Paranoia
- Add an opt-in harness (`RUN_PARANOIA=1`) with a hard timeout so it can run in CI without hanging.
- Consider a smaller “smoke” paranoia subset that must complete quickly.
- If desired, investigate the hang using the captured dump or by adding periodic progress output.

### 2) Error report / INCLUDE semantics
- Keep whole-file `INCLUDE` as the supported mode.
- If line-by-line include is required, implement a true “input source” stack so colon definitions can span lines across `EvalAsync` calls.

### 3) Documentation cleanup
- Consolidate session writeups into a single `docs/` (or `SESSION_SUMMARIES/`) folder.
- Keep a single changelog per large feature area instead of per-session markdown.

## Tooling
- `tools/ans-diff` is the authoritative tracker for missing words/sets; keep it aligned with CI and update `--sets=` lists as coverage expands.

---

## Notes
- Historical session logs were removed from this file to keep it actionable. See the session summary markdown files in the repo for detailed archaeology.
