# Forth.ProtoActor module

Proto.Actor bindings for the Forth runtime. This module exposes a minimal set of words to experiment with actors and async interop.

Target framework: .NET 9

## Loading the module

- From C#
  - `forth.LoadAssemblyWords(typeof(Forth.ProtoActor.ProtoActorModule).Assembly);`
  - Then in Forth: `USING Proto ...`

- From Forth (type-based)
  - `LOAD-ASM-TYPE Forth.ProtoActor.ProtoActorModule USING Proto ...`

Both approaches leverage the interpreter’s module system. The `[ForthModule("Proto")]` attribute on `ProtoActorModule` scopes the registered words under the `Proto` module; `USING Proto` brings them into the search path.

## Words

- `START` ? `-- 0`
  - Placeholder to signal readiness; currently just pushes 0.
- `SPAWN-ECHO` ? `-- pid`
  - Spawns an actor that replies to any numeric message with the same number. Pushes a `Proto.PID`.
- `ASK-LONG` ? `pid n -- task<long>`
  - Sends `n` to `pid` with request/response semantics and pushes a `Task<long>`.
  - Use `AWAIT` in Forth to await and push the numeric result back on the stack.
- `SHUTDOWN` ? `pid --`
  - Shuts down the owning `ActorSystem` for `pid`.

Implementation note: a private map tracks owning `ActorSystem` per `PID` to support `ASK-LONG` and `SHUTDOWN`.

## Tests

File: `4th.Tests/ProtoActorModuleTests.cs`

1) `LoadProtoActorModuleAndSpawnEcho`
   - Loads the module from C#: `forth.LoadAssemblyWords(typeof(ProtoActorModule).Assembly)`
   - Evaluates: `USING Proto SPAWN-ECHO`
   - Asserts a single stack item exists and is a `Proto.PID` (checked via `FullName`).

2) `AskLongReturnsValue`
   - Loads the module from C#.
   - Evaluates: `USING Proto SPAWN-ECHO DUP 42 ASK-LONG AWAIT`
     - `SPAWN-ECHO` pushes `pid`
     - `DUP` duplicates the `pid`
     - `42` pushes a number
     - `ASK-LONG` pushes a `Task<long>`
     - `AWAIT` awaits and pushes the result
   - Asserts the top of stack is `42`.

These tests validate:
- Dynamic module discovery and registration via C# API
- Module scoping/`USING` resolution
- Async interop with `Task<T>` through `ASK-LONG` + `AWAIT`

## Example session

```
LOAD-ASM-TYPE Forth.ProtoActor.ProtoActorModule USING Proto
SPAWN-ECHO        \ -- pid
DUP 123 ASK-LONG AWAIT   \ -- 123
```
