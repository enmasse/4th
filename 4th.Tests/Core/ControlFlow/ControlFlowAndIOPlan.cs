using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.ControlFlow;

public class ControlFlowAndIOPlan
{
    [Fact]
    public async Task IfElseThen_Branching()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync(": ABS DUP 0 < IF NEGATE THEN ;"));
        Assert.True(await forth.EvalAsync("-5 ABS"));
        Assert.Equal(new long[] { 5 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
