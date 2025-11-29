using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ProgrammingToolsTests
{
    [Fact]
    public async Task SourceId_ReturnsInputSourceIdentifier()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // SOURCE-ID should push the source id
        Assert.True(await forth.EvalAsync("SOURCE-ID"));
        Assert.Single(forth.Stack);
        var id = (long)forth.Stack[0];
        // For string evaluation (EvalAsync), it should be -1
        Assert.Equal(-1L, id);
    }

    private sealed class TestIO : IForthIO
    {
        public void Print(string text) { }
        public void PrintNumber(long number) { }
        public void NewLine() { }
        public string? ReadLine() => null;
    }
}