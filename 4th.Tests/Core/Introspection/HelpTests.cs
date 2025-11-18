using Forth.Core.Interpreter;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Introspection;

public class HelpTests
{
    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    [Fact]
    public async Task Help_PrintsHelpForPrimitiveByName()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Use HELP like SEE: HELP CREATE
        Assert.True(await forth.EvalAsync("HELP CREATE"));
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("CREATE <name> - create a new data-definition word", io.Outputs[0]);
        Assert.Equal("\n", io.Outputs[1]);
    }

    [Fact]
    public async Task Help_PrintsNoHelpForCreatedWordWhenNoHelpSet()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // CREATE defines BOB; use HELP BOB to show no help available
        Assert.True(await forth.EvalAsync("CREATE BOB HELP BOB"));
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("No help available for BOB", io.Outputs[0]);
        Assert.Equal("\n", io.Outputs[1]);
    }

    [Fact]
    public async Task Help_PrintsNoHelpForUnknownName()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("HELP FOO"));
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("No help available for FOO", io.Outputs[0]);
        Assert.Equal("\n", io.Outputs[1]);
    }
}
