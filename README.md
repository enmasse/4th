# 4th - ANS-like Forth implementation in .NET

This repository contains a .NET 10 implementation of a Forth-like interpreter and runtime focused on ANS-like core word compatibility and extensibility.

Key components:
- `src/Forth.Core` - core interpreter, primitives, and source generator for primitive registration
- `4th` - host program and REPL
- `4th.Tests` - unit tests covering core word semantics and interpreter behavior
- `tools/ans-diff` - helper that scans `Primitive("name")` attributes and compares against an embedded ANS core wordlist

Quick start
1. Build: `dotnet build` (requires .NET 10 SDK)
2. Run tests: `dotnet test`
3. Run tool: `dotnet run --project tools/ans-diff/ans-diff.csproj`

Conformance
- `tools/ans-diff` can be used to generate a report of which ANS core words are implemented, missing, or extra. Run it and check `tools/ans-diff/report.md`.

Contributing
- Open issues and PRs welcome. Follow repository coding conventions.

License
- MIT
