using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class CreateBodyAndMMODTests
{
    [Fact]
    public async Task Body_For_CreateAndVariable_ReturnsAddress()
    {
        var f = new ForthInterpreter();
        // CREATE should create a word whose >BODY yields its data address
        Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
        Assert.True(await f.EvalAsync("' BUF >BODY"));
        Assert.Single(f.Stack);
        var addr1 = (long)f.Stack[0];
        Assert.True(addr1 > 0);
        f.Pop();

        // VARIABLE should behave similarly
        Assert.True(await f.EvalAsync("VARIABLE X"));
        Assert.True(await f.EvalAsync("' X >BODY"));
        Assert.Single(f.Stack);
        var addr2 = (long)f.Stack[0];
        Assert.True(addr2 > 0);
        // Storing and fetching via >BODY address should work
        f.Pop();
        Assert.True(await f.EvalAsync("123 ' X >BODY ! ' X >BODY @"));
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task MSlashMod_SymmetricDivision_Basics()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("7 3 M/MOD"));
        Assert.Equal(2L, (long)f.Stack[^1]); // quot
        Assert.Equal(1L, (long)f.Stack[^2]); // rem

        var f2 = new ForthInterpreter();
        Assert.True(await f2.EvalAsync("-7 3 M/MOD"));
        Assert.Equal(-2L, (long)f2.Stack[^1]);
        Assert.Equal(-1L, (long)f2.Stack[^2]);

        var f3 = new ForthInterpreter();
        Assert.True(await f3.EvalAsync("7 -3 M/MOD"));
        Assert.Equal(-2L, (long)f3.Stack[^1]);
        Assert.Equal(1L, (long)f3.Stack[^2]);

        var f4 = new ForthInterpreter();
        Assert.True(await f4.EvalAsync("-7 -3 M/MOD"));
        Assert.Equal(2L, (long)f4.Stack[^1]);
        Assert.Equal(-1L, (long)f4.Stack[^2]);
    }
}
