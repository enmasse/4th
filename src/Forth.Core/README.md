# Forth.Core

Minimal async-capable Forth interpreter core library targeting .NET 9.

## Features
- Object-based parameter stack (numbers, Tasks, other objects)
- Word definition at runtime (`: NAME ... ;`)
- Control flow: `IF ELSE THEN`, `BEGIN UNTIL`, `BEGIN WHILE REPEAT`
- Async interop: bind .NET methods returning `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` via `BIND` / `BINDASYNC` and await using `AWAIT`
- Primitive stack ops: `DUP DROP SWAP OVER ROT -ROT`
- Variables & constants: `VARIABLE`, `CONSTANT`, memory access `@ !`
- Concurrency helpers: `SPAWN` (alias existing Task), `YIELD`

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

## NuGet
Package metadata is included; build pipeline packs on pushes to `main`.

## License
MIT
