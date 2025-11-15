using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ArithmeticTests
{
    [Fact(Skip = "Template: implement arithmetic helpers 1+ 1- 2* 2/ NEGATE ABS MOD")]
    public async Task Arithmetic_Helpers()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 1+"));
        Assert.Equal(new long[] { 2 }, forth.Stack.Select(o => (long)o).ToArray());

        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("5 NEGATE"));
        Assert.Equal(new long[] { -5 }, forth2.Stack.Select(o => (long)o).ToArray());
    }
}
