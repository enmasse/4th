using Forth;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class DefinitionAndErrorsTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task DefineSimpleWord_ThatAddsTwoNumbers()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync(": ADD2 + ;"));
        Assert.True(await forth.EvalAsync("5 7 ADD2"));
        Assert.Equal(new long[] { 12 }, Longs(forth));
    }

    [Fact]
    public async Task DefineWord_UsingExistingWords()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync(": SQUARE DUP * ;"));
        Assert.True(await forth.EvalAsync("4 SQUARE"));
        Assert.Equal(new long[] { 16 }, Longs(forth));
    }

    [Fact]
    public async Task UndefinedWord_ShouldThrow()
    {
        var forth = New();
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("FOOBAR"));
    }

    [Fact]
    public async Task StackUnderflow_ShouldThrow()
    {
        var forth = New();
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("+"));
    }
}
