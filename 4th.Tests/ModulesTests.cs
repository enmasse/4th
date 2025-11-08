using Forth;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class ModulesTests
{
    [Fact]
    public async Task DefineWordInModuleAndCallQualified()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("MODULE M1 : ADD2 + ; END-MODULE"));
        Assert.True(await f.EvalAsync("5 7 M1:ADD2"));
        Assert.Equal(12L, (long)f.Stack[^1]);
    }

    [Fact]
    public async Task UsingModuleAllowsUnqualified()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("MODULE M2 : SQUARE DUP * ; END-MODULE"));
        Assert.True(await f.EvalAsync("USING M2 9 SQUARE"));
        Assert.Equal(81L, (long)f.Stack[^1]);
    }
}
