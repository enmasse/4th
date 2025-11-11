using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;

namespace Forth.Tests.Core.Stack;

public class StackAndArithmeticTests
{
    [Fact]
    public void BasicArithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("2 3 + 4 *"));
        Assert.Equal(new long[] { 20 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
