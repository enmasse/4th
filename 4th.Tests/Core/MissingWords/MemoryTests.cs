using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class MemoryTests
{
    [Fact]
    public async Task Memory_Fetch_Store_And_Here()
    {
        var forth = new ForthInterpreter();
        // VARIABLE x ALLOT or CREATE usage would provide an address; here we show intended asserts
        // When implemented: create a variable, store a value, fetch it
        Assert.True(await forth.EvalAsync("VARIABLE X"));
        // Use ANS-Forth ordering: value then address then '!'
        Assert.True(await forth.EvalAsync("123 X !"));
        Assert.True(await forth.EvalAsync("X @"));
        Assert.Equal(new long[] { 123 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task PAD_Address()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("HERE PAD"));
        var here = (long)forth.Stack[0];
        var pad = (long)forth.Stack[1];
        Assert.True(pad > here);
        // Test storing to PAD
        Assert.True(await forth.EvalAsync("DROP DROP 42 PAD ! PAD @"));
        Assert.Equal(new long[] { 42 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task Allocate()
    {
        var forth = new ForthInterpreter();
        // Allocate 10 bytes
        Assert.True(await forth.EvalAsync("10 ALLOCATE"));
        Assert.Equal(2, forth.Stack.Count);
        var ior = (long)forth.Pop();
        var addr = (long)forth.Pop();
        Assert.Equal(0L, ior); // success
        Assert.True(addr >= 1000000L); // in heap range
    }

    [Fact]
    public async Task AllocateNegative()
    {
        var forth = new ForthInterpreter();
        // Allocate negative size
        Assert.True(await forth.EvalAsync("-1 ALLOCATE"));
        Assert.Equal(2, forth.Stack.Count);
        var ior = (long)forth.Pop();
        var addr = (long)forth.Pop();
        Assert.Equal(-1L, ior); // error
        Assert.Equal(0L, addr); // undefined
    }

    [Fact]
    public async Task Free()
    {
        var forth = new ForthInterpreter();
        // Allocate then free
        Assert.True(await forth.EvalAsync("10 ALLOCATE"));
        var ior = (long)forth.Pop();
        var addr = (long)forth.Pop();
        Assert.Equal(0L, ior);

        Assert.True(await forth.EvalAsync($"{addr} FREE"));
        Assert.Single(forth.Stack);
        ior = (long)forth.Pop();
        Assert.Equal(0L, ior); // success
    }

    [Fact]
    public async Task FreeInvalid()
    {
        var forth = new ForthInterpreter();
        // Free invalid address
        Assert.True(await forth.EvalAsync("123 FREE"));
        Assert.Single(forth.Stack);
        var ior = (long)forth.Pop();
        Assert.Equal(-1L, ior); // error
    }

    [Fact]
    public async Task Unused_ReturnsRemainingCells()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("UNUSED"));
        Assert.Single(forth.Stack);
        var unused = (long)forth.Stack[0];
        Assert.True(unused > 0); // should have remaining space
    }
}
