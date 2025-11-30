using System.Linq;
using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Async;

public class FiberSpawnTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task SpawnDuplicateTaskAndAwaitBoth()
    {
        var f = New();
        await f.EvalAsync("BIND Forth.Tests.Core.Binding.AsyncTestTargets AddAsync 2 ADDAB");
        await f.EvalAsync("10 32 ADDAB AWAIT");
        Assert.Equal(42L, (long)f.Pop());
    }

    [Fact]
    public async Task YieldDoesNotLoseStack()
    {
        var f = New();
        await f.EvalAsync("1 2 3 YIELD 4 5");
        Assert.Equal(new long[]{1,2,3,4,5}, f.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task ColonNoname_CreatesAnonymousWord()
    {
        var f = New();
        await f.EvalAsync(":NONAME 42 ;");
        Assert.Single(f.Stack);
        var xt = f.Stack[0];
        Assert.IsType<Word>(xt);
        var word = (Word)xt;
        Assert.Null(word.Name); // anonymous
        // Execute the word
        await f.EvalAsync("EXECUTE");
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }
}
