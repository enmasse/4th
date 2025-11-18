using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class DoubleCellTests
{
    [Fact]
    public async Task Double_Cell_Stack_Operations()
    {
        var forth = new ForthInterpreter();
        // stack: a b c d
        Assert.True(await forth.EvalAsync("1 2 3 4 2DUP"));
        // Expect: 1 2 3 4 3 4 (two top cells duplicated)
        Assert.Equal(new long[] { 1, 2, 3, 4, 3, 4 }, forth.Stack.Select(o => (long)o).ToArray());

        // 2DROP should remove two cells
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("1 2 3 4 2DROP"));
        Assert.Equal(new long[] { 1, 2 }, forth2.Stack.Select(o => (long)o).ToArray());
    }
}
