using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Stack;

public class StackManipulationTests
{
    [Fact]
    public async Task DupSwapOverRot()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 DUP SWAP OVER ROT"));
        Assert.Equal(new long[] { 1, 2, 2, 2 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
