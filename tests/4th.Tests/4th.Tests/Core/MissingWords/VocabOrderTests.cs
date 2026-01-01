using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;
using Forth.Core;

namespace Forth.Tests.Core.MissingWords;

public class VocabOrderTests
{
    [Fact]
    public async Task GetOrder_DefaultContainsFORTH()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("GET-ORDER"));
        Assert.True(f.Stack.Count >= 1);
        var count = (long)f.Stack[^1];
        Assert.True(count >= 1);
    }

    [Fact]
    public async Task SetOrder_ChangesAndGetOrderReflects()
    {
        var f = new ForthInterpreter();
        await f.EvalAsync("MODULE M : FOO 42 ; END-MODULE USING M");
        // Set order to M then FORTH
        Assert.True(await f.EvalAsync("\" M\" \" FORTH\" 2 SET-ORDER"));
        Assert.True(await f.EvalAsync("GET-ORDER"));
        var c = (int)(long)f.Stack[^1];
        Assert.True(c >= 2);
        // Extract the last 'c' pushed names (they are just before the count)
        var names = f.Stack.Skip(f.Stack.Count - 1 - c).Take(c).Select(o => o as string).ToArray();
        Assert.Contains("M", names);
        Assert.Contains("FORTH", names);
    }
}
