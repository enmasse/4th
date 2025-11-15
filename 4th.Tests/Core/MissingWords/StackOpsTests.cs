using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class StackOpsTests
{
    [Fact]
    public async Task NIP_Drops_Second_Item()
    {
        // a b -- b
        var forth = new ForthInterpreter();
        // When implemented, 1 2 NIP should leave only 2 on the stack
        Assert.True(await forth.EvalAsync("1 2 NIP"));
        Assert.Equal(new long[] { 2 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task TUCK_Duplicates_Top_Under_Second()
    {
        // a b -- b a b
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 TUCK"));
        Assert.Equal(new long[] { 2, 1, 2 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task QuestionDup_Only_Duplicates_NonZero()
    {
        var forth = new ForthInterpreter();
        // non-zero should duplicate
        Assert.True(await forth.EvalAsync("5 ?DUP"));
        Assert.Equal(new long[] { 5, 5 }, forth.Stack.Select(o => (long)o).ToArray());

        // zero should not duplicate (stack will be just the 0)
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("0 ?DUP"));
        Assert.Equal(new long[] { 0 }, forth2.Stack.Select(o => (long)o).ToArray());
    }
}
