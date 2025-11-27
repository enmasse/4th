# Forth.Core

Minimal async-capable Forth interpreter core library targeting .NET 9.

## Features
- High-performance core: typed `ForthValue` structs internally (no boxing) with object-based public API
- Runtime word definition: `: NAME ... ;`, `IMMEDIATE`, `POSTPONE`, `LITERAL`
- Control flow: `IF ELSE THEN`, `BEGIN WHILE REPEAT`, `BEGIN UNTIL`, `DO LOOP`, `LEAVE`, `UNLOOP`, `RECURSE`
- Async interop: bind .NET methods returning `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` via `BIND` / `BINDASYNC` and await using `AWAIT`
- Concurrency: `SPAWN`, `TASK`, `JOIN`, `TASK?`, `FUTURE`, `YIELD`
- Stack & memory: `DUP DROP SWAP OVER ROT -ROT`, `@ ! C@ C!`, `ALLOT HERE PAD`, `MOVE FILL ERASE`, double-cell arithmetic
- File & streams: `OPEN-FILE CLOSE-FILE READ-FILE WRITE-FILE APPEND-FILE FILE-SIZE REPOSITION-FILE`, byte ops `READ-FILE-BYTES WRITE-FILE-BYTES`
- Blocks: `BLOCK SAVE BLK` with LRU cache
- Strings & I/O: `S"`, `."`, `TYPE`, `WORD`, pictured numeric `<# # #S HOLD SIGN #>`
- Exceptions/control: `CATCH THROW ABORT ABORT" EXIT BYE QUIT`
- Search order & modules: `WORDLIST GET-ORDER SET-ORDER DEFINITIONS FORTH`, `MODULE USING PREVIOUS`
- Introspection: `SEE` decompiler, `HELP` system
- Environment: `ENV` wordlist (`OS`, `CPU`, `MEMORY`, `MACHINE`, `USER`, `PWD`)
- Inline IL: `IL{ ... }IL` for advanced scenarios
- Interactive REPL with history and debugging commands
- ANS compliance tracking via `tools/ans-diff` across Core, Core-Ext, File, Block, Float sets

## Performance
The interpreter uses an optimized internal representation with `ForthValue` structs to avoid boxing primitive types, providing significant performance improvements for stack operations while preserving full backward compatibility through the public `object`-based API.

## Basic Usage
```csharp
var forth = new ForthInterpreter();
await forth.EvalAsync("1 2 3 +");
Console.WriteLine(string.Join(",", forth.Stack)); // 1,5
```

Bind and await an async method:
```csharp
await forth.EvalAsync("BINDASYNC MyNamespace.MyType AddAsync 2 ADDAB 5 7 ADDAB AWAIT");
```

Access environment info:
```csharp
await forth.EvalAsync("USING ENV OS");
```

Include and load files:
```csharp
await forth.EvalAsync("INCLUDE \"./script.4th\"");
```

Run ANS compliance diff:
```powershell
dotnet run --project tools/ans-diff -- --sets=all --fail-on-missing=true
```

## NuGet
Package metadata is included; build pipeline packs on pushes to `main`.

## License
MIT
