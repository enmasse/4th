using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ParsingAndStringsTests
{
    [Fact]
    public async Task Parsing_And_Strings()
    {
        var forth = new ForthInterpreter();
        // Use S to push a quoted string, then COUNT should leave the string and its length
        Assert.True(await forth.EvalAsync("S \" hello world\" COUNT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(" hello world", Assert.IsType<string>(forth.Stack[0]));
        Assert.Equal(12L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task SQuote_PushesAddrLen()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" hello\""));
        Assert.Equal(2, forth.Stack.Count);
        // ANS: S" pushes c-addr u (address first, then length)
        var caddr = (long)forth.Stack[0];
        var u = (long)forth.Stack[1];
        Assert.Equal(5L, u);
        var str = forth.ReadMemoryString(caddr, u);
        Assert.Equal("hello", str);
    }
}
