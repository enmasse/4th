using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Stack;

public class StackAndArithmeticTests
{
    [Fact]
    public async Task BasicArithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("2 3 + 4 *"));
        Assert.Equal(new long[] { 20 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
