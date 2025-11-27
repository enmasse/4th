using Forth.Core;
using Forth.Core.Interpreter;
using System.IO;
using Xunit;

namespace Forth.Tests.Core;

/// <summary>
/// Tests for examples shown in the README.md
/// </summary>
public class ReadmeExamplesTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task ProgrammaticUsage_Example()
    {
        // Create an interpreter
        var forth = new ForthInterpreter();

        // Evaluate Forth code
        await forth.EvalAsync("5 3 + ."); // Should print 8

        // Add custom words
        forth.AddWord("HELLO", i => ((ForthInterpreter)i).WriteText("Hello, World!"));
    }

    [Fact]
    public async Task BasicOperations_Example()
    {
        var f = New();
        await f.EvalAsync("5 3 + ."); // Add 5 and 3, print result (8)
        await f.EvalAsync("10 2 / ."); // Divide 10 by 2, print (5)
    }

    [Fact]
    public async Task DefiningWords_Example()
    {
        var f = New();
        await f.EvalAsync(": DOUBLE 2 * ;"); // Define DOUBLE
        await f.EvalAsync("5 DOUBLE ."); // Should print 10
    }

    [Fact]
    public async Task ControlFlow_Example()
    {
        var f = New();
        await f.EvalAsync(": TEST 0= IF \"ZERO\" ELSE \"NON-ZERO\" THEN TYPE ;");
        await f.EvalAsync("5 TEST"); // Should print NON-ZERO
        await f.EvalAsync("0 TEST"); // Should print ZERO
    }

    [Fact]
    public async Task Loops_Example()
    {
        var f = New();
        await f.EvalAsync(": COUNT-TO 1+ 1 DO I . LOOP ;");
        await f.EvalAsync("5 COUNT-TO"); // Should print 1 2 3 4 5
    }

    [Fact]
    public async Task FileIO_Example()
    {
        var f = New();
        // Create a temp file for testing
        var tempFile = Path.Combine(Path.GetTempPath(), "test.txt");
        try
        {
            await f.EvalAsync($"S \"Hello, World!\" S \"{tempFile}\" WRITE-FILE");
            await f.EvalAsync($"S \"{tempFile}\" READ-FILE TYPE");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    // [Fact]
    // public async Task AsyncOperations_Example()
    // {
    //     var f = New();
    //     await f.EvalAsync(": ASYNC-ADD SPAWN 2 3 + AWAIT . ;");
    //     await f.EvalAsync("ASYNC-ADD"); // Should print 5
    // }

    [Fact]
    public async Task Modules_Example()
    {
        var f = New();
        await f.EvalAsync("MODULE MATH");
        await f.EvalAsync(": FACTORIAL DUP 2 < IF DROP 1 ELSE DUP 1- RECURSE * THEN ;");
        await f.EvalAsync("END-MODULE");
        await f.EvalAsync("\"MATH\" USING");
        await f.EvalAsync("3 FACTORIAL ."); // Should print 6
    }

    // [Fact]
    // public async Task InlineIL_Example()
    // {
    //     var f = New();
    //     await f.EvalAsync("IL{ ldarg.0 ldc.i4.5 call void ForthInterpreter::Push(object) }IL");
    //     Assert.Equal(5L, f.Pop());
    // }

    // [Fact]
    // public async Task BindingDotNet_Example()
    // {
    //     var f = New();
    //     await f.EvalAsync("BIND System.Console WriteLine 1 HELLO");
    //     await f.EvalAsync("\"Hello from .NET!\" HELLO");
    // }

    [Fact]
    public async Task EnvironmentQueries_Example()
    {
        var f = New();
        await f.EvalAsync("\"ENV\" USING");

        // Test that ENV words exist and return values
        await f.EvalAsync("ENV:OS");
        Assert.IsType<string>(f.Pop());

        await f.EvalAsync("ENV:CPU");
        Assert.IsType<long>(f.Pop());
    }
}