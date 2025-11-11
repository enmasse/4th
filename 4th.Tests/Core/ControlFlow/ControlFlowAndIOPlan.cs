using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;

namespace Forth.Tests.Core.ControlFlow;

public class ControlFlowAndIOPlan
{
    [Fact]
    public void IfElseThen_Branching()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": ABS DUP 0 < IF NEGATE THEN ;"));
        Assert.True(forth.Interpret("-5 ABS"));
        Assert.Equal(new long[] { 5 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
