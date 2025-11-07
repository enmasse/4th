# 4th

A minimal Forth interpreter in C# (.NET 9), with a simple REPL and xUnit tests.

## Projects
- `src/Forth.Core` – Core library (`ForthInterpreter`, `IForthInterpreter`, `IForthIO`, `ForthException`, `Tokenizer`)
- `4th` – Console REPL (type `BYE` to exit; prints ` ok` on success)
- `4th.Tests` – xUnit test suite
- `tools/TestRunner` – Small manual runner (optional)

## Build & Test
- Build: `dotnet build`
- Run tests: `dotnet test`
- Run REPL: `dotnet run --project 4th/4th.csproj`

## REPL Usage
Examples:
- `1 2 + CR .` ? prints `3`
- `: SQUARE DUP * ;` then `7 SQUARE CR .` ? prints `49`
- `: FLOOR5 DUP 6 < IF DROP 5 ELSE 1 - THEN ;` then `8 FLOOR5 CR .` ? prints `7`
- Exit: `BYE` or `QUIT`

## Implemented Words
- Arithmetic: `+ - * /`
- Comparison: `< = >`
- Stack: `DUP DROP SWAP OVER ROT -ROT`
- Defining: `: name ... ;` `CONSTANT` `VARIABLE` `@ !`
- Control flow: `IF ELSE THEN` `BEGIN WHILE REPEAT` `BEGIN UNTIL` `EXIT`
- Literals & misc: `CHAR [CHAR] LITERAL`
- I/O: `. EMIT CR`
- Comments: `( ... )` and `\` to end of line

## License
MIT – see `LICENSE`.
