# 4th - ANS-like Forth implementation in .NET

This repository contains a .NET 9 implementation of a Forth-like interpreter and runtime focused on ANS-like core word compatibility and extensibility.

## Features

- **ANS Forth Compatibility**: Implements 99/99 ANS core words with CI checks for conformity
- **Async Support**: Native async/await integration with `SPAWN`, `FUTURE`, `TASK`, `AWAIT`
- **File I/O**: Complete file operations including `READ-FILE`, `WRITE-FILE`, `OPEN-FILE`, etc.
- **Modules**: Modular word definitions with search order and namespaces
- **Inline IL**: Direct .NET IL emission with `IL{ ... }IL` syntax
- **Environment Queries**: `ENV` wordlist for system information (`OS`, `CPU`, `MEMORY`, etc.)
- **Interactive REPL**: Command history, HELP/STACK commands, error debugging
- **Extensible**: Easy to add new primitives and integrate with .NET code
- **High Performance**: Optimized stack operations and memory management
- **Comprehensive Testing**: 271 tests covering all features and README examples

## Quick Start

### Prerequisites
- .NET 9 SDK

### Build and Test
```bash
# Clone the repository
git clone https://github.com/enmasse/4th.git
cd 4th

# Build the solution
dotnet build

# Run all tests (271 tests, all passing)
dotnet test

# Check ANS conformity
dotnet run --project tools/ans-diff
```

### Using the Interpreter

#### Programmatic Usage
```csharp
using Forth.Core;

// Create an interpreter
var forth = new ForthInterpreter();

// Evaluate Forth code
await forth.EvalAsync("5 3 + ."); // Prints 8

// Add custom words
forth.AddWord("HELLO", i => i.WriteText("Hello, World!"));

// Use async features
await forth.EvalAsync("SPAWN 1 DELAY AWAIT ."); // Prints 1 after delay
```

#### REPL
```bash
dotnet run --project 4th
```

In the REPL:
```
> 5 3 + .
8 ok
> : SQUARE DUP * ;
> 4 SQUARE .
16 ok
> HELP
Forth interpreter commands:
  WORDS - list all words
  HELP - show this help
  HELP <word> - show help for specific word
  BYE - exit

Forth syntax: stack-based, postfix notation
> STACK
<empty>
> 1 2 3
> STACK
1 2 3
```

Features:
- Command history with ?/? arrows
- HELP and STACK commands
- Error messages with stack display

## Tutorial

### Basic Operations

Forth uses a stack-based postfix notation. Numbers are pushed to the stack, operators pop operands and push results.

```forth
5 3 + .    \ Add 5 and 3, print result (8)
10 2 / .   \ Divide 10 by 2, print (5)
```

### Defining Words

Use `:` to start a definition, `;` to end it:

```forth
: DOUBLE 2 * ;    \ Define DOUBLE
5 DOUBLE .        \ Prints 10
```

### Control Flow

```forth
: TEST 0= IF "ZERO" ELSE "NON-ZERO" THEN TYPE ;

5 TEST    \ Prints NON-ZERO
0 TEST    \ Prints ZERO
```

### Loops

```forth
: COUNT-TO 1+ 1 DO I . LOOP ;

5 COUNT-TO    \ Prints 1 2 3 4 5
```

### File I/O

```forth
S" hello.txt" S" Hello, World!" WRITE-FILE
S" hello.txt" READ-FILE TYPE
```

### Async Operations

```forth
: ASYNC-ADD SPAWN 2 3 + AWAIT . ;

ASYNC-ADD    \ Prints 5 asynchronously
```

### Modules

```forth
MODULE MATH
: FACTORIAL DUP 1 > IF DUP 1- RECURSE * ELSE DROP 1 THEN ;
END-MODULE

MATH USING
5 FACTORIAL .    \ Prints 120
```

### Environment Queries

Access environment information via the ENV module:

```forth
ENV USING
ENV:OS .     \ Prints OS name
ENV:CPU .    \ Prints CPU count
ENV:MEMORY . \ Prints working set memory
```

## Advanced Features

### Inline IL

Direct .NET IL code:

```forth
IL{
    ldarg.0    \ Load interpreter
    ldc.i4.5   \ Push 5
    call void [Forth.Core]Forth.Core.Interpreter.ForthInterpreter::Push(object)
}IL
```

### Extending the Interpreter

Add custom primitives:

```csharp
forth.AddWord("MY-WORD", i => {
    var x = (long)i.Pop();
    i.Push(x * 2);
});
```

### Binding .NET Types

```forth
BIND System.Console WriteLine 1 HELLO
"Hello from .NET!" HELLO
```

## API Reference

### IForthInterpreter Interface

- `Task<bool> EvalAsync(string line)` - Evaluate Forth code
- `void Push(object value)` - Push to stack
- `object Pop()` - Pop from stack
- `IReadOnlyList<object> Stack` - Access stack contents
- `void AddWord(string name, Action<IForthInterpreter> body)` - Add sync word
- `void AddWordAsync(string name, Func<IForthInterpreter, Task> body)` - Add async word

## Project Structure

- `src/Forth.Core` - Core interpreter and primitives
- `4th` - Console REPL application
- `4th.Tests` - Comprehensive test suite (271 tests)
- `tools/ans-diff` - ANS conformity checker
- `4th.Benchmarks` - Performance benchmarks
- `modules/` - Optional extensions (ProtoActor, etc.)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

### Development Setup

```bash
# Install dependencies
dotnet restore

# Run tests continuously
dotnet watch test

# Generate test coverage
dotnet test --collect:"XPlat Code Coverage"
```

## License

MIT License - see LICENSE file for details.

## Related Links

- [ANS Forth Standard](https://forth-standard.org/)
- [Forth Programming Language](https://en.wikipedia.org/wiki/Forth_(programming_language))
