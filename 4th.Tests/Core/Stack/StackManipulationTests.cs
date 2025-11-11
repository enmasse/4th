using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;

namespace Forth.Tests.Core.Stack;

public class StackManipulationTests
{
    [Fact]
    public void DupSwapOverRot()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 DUP SWAP OVER ROT"));
        Assert.Equal(new long[] { 2, 1, 1, 2 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
