using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ReturnStackTests
{
    [Fact(Skip = "Template: implement >R R> R@")]
    public async Task Return_Stack_Operations()
    {
        var forth = new ForthInterpreter();
        // >R pushes to return stack and removes from data stack
        Assert.True(await forth.EvalAsync("10 >R"));
        Assert.Empty(forth.Stack);
        // R@ should peek return stack
        Assert.True(await forth.EvalAsync("R@"));
        // R> should move back to data stack
        Assert.True(await forth.EvalAsync("R>"));
        Assert.Equal(new long[] { 10 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
