using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class BitwiseAndComparisonTests
{
    [Fact]
    public async Task Bitwise_And_Comparisons()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("6 3 AND")); // 6 & 3 = 2
        Assert.True(await forth.EvalAsync("2 =")); // compare top-of-stack with 2
        // After implementing = the stack will reflect boolean as non-zero/zero
    }
}
