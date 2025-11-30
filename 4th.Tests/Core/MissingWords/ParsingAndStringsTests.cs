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
        Assert.True(forth.Stack.Count == 2);
        // ANS: S" pushes c-addr u (address first, then length)
        var caddr = (long)forth.Stack[0];
        var u = (long)forth.Stack[1];
        Assert.Equal(5L, u);
        var str = forth.ReadMemoryString(caddr, u);
        Assert.Equal("hello", str);
    }

    [Fact]
    public async Task SlashString()
    {
        var forth = new ForthInterpreter();
        // Basic case: skip 2 chars from "hello" -> "llo"
        Assert.True(await forth.EvalAsync("S\" hello\" 2 /STRING"));
        Assert.True(forth.Stack.Count == 2);
        var addr = (long)forth.Stack[0];
        var u = (long)forth.Stack[1];
        Assert.Equal(3L, u);
        var str = forth.ReadMemoryString(addr, u);
        Assert.Equal("llo", str);

        // Edge case: n=0, should be unchanged
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("S\" test\" 0 /STRING"));
        Assert.Equal(2, forth2.Stack.Count);
        addr = (long)forth2.Stack[0];
        u = (long)forth2.Stack[1];
        Assert.Equal(4L, u);
        str = forth2.ReadMemoryString(addr, u);
        Assert.Equal("test", str);

        // Edge case: n=u, result u=0
        var forth3 = new ForthInterpreter();
        Assert.True(await forth3.EvalAsync("S\" abc\" 3 /STRING"));
        Assert.Equal(2, forth3.Stack.Count);
        addr = (long)forth3.Stack[0];
        u = (long)forth3.Stack[1];
        Assert.Equal(0L, u);

        // Edge case: n > u, negative u
        var forth4 = new ForthInterpreter();
        Assert.True(await forth4.EvalAsync("S\" hi\" 5 /STRING"));
        Assert.Equal(2, forth4.Stack.Count);
        addr = (long)forth4.Stack[0];
        u = (long)forth4.Stack[1];
        Assert.Equal(-3L, u); // 2 - 5 = -3
    }

    [Fact]
    public async Task SLiteral_CompilesStringLiteral()
    {
        var forth = new ForthInterpreter();
        // Define a word that uses SLITERAL
        Assert.True(await forth.EvalAsync(": test S\" hello\" SLITERAL ; test"));
        Assert.Equal(2, forth.Stack.Count);
        var addr = (long)forth.Stack[0];
        var u = (long)forth.Stack[1];
        Assert.Equal(5L, u);
        var str = forth.ReadMemoryString(addr, u);
        Assert.Equal("hello", str);
    }

    [Fact]
    public async Task SaveInput_PushesState()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("SAVE-INPUT"));
        Assert.Equal(5, forth.Stack.Count);
        var n = (long)forth.Pop();
        Assert.Equal(4L, n);
        var source = (string)forth.Pop();
        var index = (long)forth.Pop();
        var inVal = forth.Pop();
        var id = (long)forth.Pop();
        Assert.Equal(-1L, id); // string eval
        Assert.Equal(0L, (long)inVal);
        Assert.Equal(1L, index); // after consuming SAVE-INPUT
        Assert.Equal("SAVE-INPUT", source);
    }

    [Fact]
    public async Task RestoreInput_SucceedsWithCorrectState()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("SAVE-INPUT"));
        // Stack has id inVal index source n
        Assert.True(await forth.EvalAsync("RESTORE-INPUT"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Pop()); // false, success
    }

    [Fact]
    public async Task RestoreInput_FailsWithWrongN()
    {
        var forth = new ForthInterpreter();
        forth.Push(3L); // wrong n
        Assert.True(await forth.EvalAsync("RESTORE-INPUT"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Pop()); // true, fail
    }

    [Fact]
    public async Task SQuote_PushesString()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" world\""));
        Assert.Single(forth.Stack);
        Assert.IsType<string>(forth.Stack[0]);
        Assert.Equal("world", (string)forth.Stack[0]);
    }
}
