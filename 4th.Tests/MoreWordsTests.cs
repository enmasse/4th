using Forth.Core;
using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class MoreWordsTests
{
    private sealed class TestIO : IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task Comparisons_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 <"));
        Assert.Equal(new long[] { -1 }, Longs(forth));
        Assert.True(await forth.EvalAsync("2 2 ="));
        Assert.Equal(new long[] { -1, -1 }, Longs(forth));
        Assert.True(await forth.EvalAsync("3 2 >"));
        Assert.Equal(new long[] { -1, -1, -1 }, Longs(forth));
    }

    [Fact]
    public async Task Rotations_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 3 ROT"));
        Assert.Equal(new long[] { 2, 3, 1 }, Longs(forth));
        Assert.True(await forth.EvalAsync("4 5 -ROT"));
        Assert.Equal(new long[] { 2, 3, 5, 1, 4 }, Longs(forth));
        Assert.True(await forth.EvalAsync("1 2 3 4 5 6 2ROT"));
        Assert.Equal(new long[] { 2, 3, 5, 1, 4, 3, 4, 5, 6, 1, 2 }, Longs(forth));
    }

    [Fact]
    public async Task Constant_DefinesValueWord()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("42 CONSTANT X"));
        Assert.True(await forth.EvalAsync("X X +"));
        Assert.Equal(new long[] { 84 }, Longs(forth));
    }

    [Fact]
    public async Task Emit_PrintsCharacter()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("81 EMIT"));
        Assert.Equal(new[] { "Q" }, io.Outputs);
    }

    [Fact]
    public async Task Env_Date_ReturnsCurrentDate()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("\"ENV\" DEFINITIONS DATE"));
        Assert.Single(forth.Stack);
        Assert.IsType<string>(forth.Stack[0]);
        var dateStr = (string)forth.Stack[0];
        // Should be in YYYY-MM-DD format
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", dateStr);
    }

    [Fact]
    public async Task Env_Time_ReturnsCurrentTime()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("\"ENV\" DEFINITIONS TIME"));
        Assert.Single(forth.Stack);
        Assert.IsType<string>(forth.Stack[0]);
        var timeStr = (string)forth.Stack[0];
        // Should be in HH:MM:SS format
        Assert.Matches(@"^\d{2}:\d{2}:\d{2}$", timeStr);
    }

    [Fact]
    public async Task GetEnv_ReturnsEnvironmentVariable()
    {
        // Set before creating interpreter so it's included in ENV wordlist
        Environment.SetEnvironmentVariable("FORTH_TEST_VAR", "test_value");
        var forth = new ForthInterpreter();
        try
        {
            Assert.True(await forth.EvalAsync("\"ENV\" DEFINITIONS FORTH_TEST_VAR"));
            Assert.Single(forth.Stack);
            Assert.IsType<string>(forth.Stack[0]);
            Assert.Equal("test_value", (string)forth.Stack[0]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("FORTH_TEST_VAR", null);
        }
    }

    [Fact]
    public async Task EnvWordlist_DoesNotIncludeNonExistentVariables()
    {
        var forth = new ForthInterpreter();
        // Try to access a variable that shouldn't exist
        await Assert.ThrowsAsync<ForthException>(() => forth.EvalAsync("\"ENV\" DEFINITIONS NON_EXISTENT_VAR_12345"));
    }
}
