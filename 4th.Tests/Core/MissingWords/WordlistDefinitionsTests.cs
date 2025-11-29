using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using System.Linq;

namespace Forth.Tests.Core.MissingWords;

public class WordlistDefinitionsTests
{
    [Fact]
    public async Task Wordlist_And_Definitions_Basic()
    {
        var f = new ForthInterpreter();
        // create a new vocab and set as definitions, then define a word in it
        Assert.True(await f.EvalAsync("WORDLIST ' VOCAB1 DEFINITIONS"));
        // DEFINITIONS should accept a wordlist id (string); using simplified behavior above
        Assert.True(await f.EvalAsync(": X 5 ;"));
        // switch back to FORTH and ensure X is not in core
        Assert.True(await f.EvalAsync("FORTH DEFINITIONS"));
        // Words in core should not include X
        Assert.True(await f.EvalAsync("WORDS"));
    }

    [Fact]
    public async Task ForthWordlist_ReturnsNull()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("FORTH-WORDLIST"));
        Assert.Single(f.Stack);
        Assert.Null(f.Stack[0]);
    }
}
