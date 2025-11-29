using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class FacilityTests
{
    [Fact]
    public async Task Page_ClearsScreen()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // PAGE should execute without error
        Assert.True(await forth.EvalAsync("PAGE"));
        // No stack effect, stack should remain empty
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task TimeAndDate_ReturnsCurrentTimeAndDate()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // TIME&DATE should push sec min hour day month year
        Assert.True(await forth.EvalAsync("TIME&DATE"));
        Assert.Equal(6, forth.Stack.Count);
        var year = (long)forth.Stack[5];
        var month = (long)forth.Stack[4];
        var day = (long)forth.Stack[3];
        var hour = (long)forth.Stack[2];
        var min = (long)forth.Stack[1];
        var sec = (long)forth.Stack[0];
        // Check ranges
        Assert.InRange(sec, 0, 59);
        Assert.InRange(min, 0, 59);
        Assert.InRange(hour, 0, 23);
        Assert.InRange(day, 1, 31);
        Assert.InRange(month, 1, 12);
        Assert.True(year > 2000); // reasonable year
    }

    [Fact]
    public async Task AtXy_PositionsCursor()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // AT-XY should consume col and row from stack
        Assert.True(await forth.EvalAsync("5 10 AT-XY"));
        // No stack effect beyond consumption
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task Ms_DelaysExecution()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // MS should consume u from stack
        Assert.True(await forth.EvalAsync("1 MS"));
        // No stack effect beyond consumption
        Assert.Empty(forth.Stack);
    }

    private sealed class TestIO : IForthIO
    {
        public void Print(string text) { }
        public void PrintNumber(long number) { }
        public void NewLine() { }
        public string? ReadLine() => null;
    }
}