using Xunit;
using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class BlockSystemTests
{
    [Fact]
    public async Task BlockLoadAndSave()
    {
        var f = new ForthInterpreter();
        // Create a block buffer via WORDLIST and set definitions directly
        Assert.True(await f.EvalAsync("WORDLIST DEFINITIONS"));
        // Use simplified ACCEPT to get a string and then SAVE to block 1
        Assert.True(await f.EvalAsync("S\" hello\" 5 1 SAVE"));
        Assert.True(await f.EvalAsync("1 BLOCK"));
        // BLK should push current block number
        Assert.True(await f.EvalAsync("BLK"));
        var blk = (int)(long)f.Stack[^1];
        Assert.Equal(1, blk);
    }
}
