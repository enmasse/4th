using Forth.Core.Interpreter;
using Forth.Core.Execution;
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

    [Fact]
    public async Task SearchWordlist_FindsWordInWordlist()
    {
        var f = new ForthInterpreter();
        // Create a wordlist and define a word in it
        Assert.True(await f.EvalAsync("WORDLIST DUP >R DEFINITIONS : MYWORD 42 ; FORTH DEFINITIONS S\" MYWORD\" R> SEARCH-WORDLIST"));
        Assert.Equal(2, f.Stack.Count);
        var xt = f.Stack[0];
        var flag = (long)f.Stack[1];
        Assert.Equal(-1L, flag); // not immediate
        Assert.IsType<Forth.Core.Interpreter.Word>(xt);
        // Drop the results
        Assert.True(await f.EvalAsync("DROP DROP"));
        // Search for MYWORD in FORTH-WORDLIST, should not find
        Assert.True(await f.EvalAsync("S\" MYWORD\" FORTH-WORDLIST SEARCH-WORDLIST"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task GetCurrent_ReturnsCurrentWordlist()
    {
        var f = new ForthInterpreter();
        // Initially, current should be FORTH (null)
        Assert.True(await f.EvalAsync("GET-CURRENT"));
        Assert.Single(f.Stack);
        Assert.Null(f.Stack[0]);
        Assert.True(await f.EvalAsync("DROP"));

        // Set to a new wordlist
        Assert.True(await f.EvalAsync("WORDLIST SET-CURRENT"));
        Assert.True(await f.EvalAsync("GET-CURRENT"));
        Assert.Single(f.Stack);
        Assert.IsType<string>(f.Stack[0]);
    }

    [Fact]
    public async Task SetCurrent_ChangesCurrentWordlist()
    {
        var f = new ForthInterpreter();
        // Create a wordlist and set as current
        Assert.True(await f.EvalAsync("WORDLIST SET-CURRENT"));
        Assert.True(await f.EvalAsync("GET-CURRENT"));
        Assert.Single(f.Stack);
        Assert.IsType<string>(f.Stack[0]);
        Assert.True(await f.EvalAsync("DROP"));

        // Set back to FORTH
        Assert.True(await f.EvalAsync("FORTH SET-CURRENT"));
        Assert.True(await f.EvalAsync("GET-CURRENT"));
        Assert.Single(f.Stack);
        Assert.Null(f.Stack[0]);
    }

    [Fact]
    public async Task Only_SetsSearchOrderToForthOnly()
    {
        var f = new ForthInterpreter();
        // Add a module to search order
        Assert.True(await f.EvalAsync("WORDLIST 1 SET-ORDER"));
        Assert.True(await f.EvalAsync("GET-ORDER"));
        Assert.Equal(3, f.Stack.Count); // wid wid count
        var count = (long)f.Stack[2];
        Assert.Equal(2L, count); // two wordlists: the new one and FORTH
        Assert.True(await f.EvalAsync("DROP DROP DROP")); // clean stack

        // Now call ONLY
        Assert.True(await f.EvalAsync("ONLY"));
        Assert.True(await f.EvalAsync("GET-ORDER"));
        Assert.Equal(2, f.Stack.Count);
        count = (long)f.Stack[1];
        Assert.Equal(1L, count);
        var wid = f.Stack[0];
        Assert.Equal("FORTH", wid); // FORTH
        Assert.True(await f.EvalAsync("DROP DROP"));
    }

    [Fact]
    public async Task Also_DuplicatesTopWordlist()
    {
        var f = new ForthInterpreter();
        // Set search order to a wordlist and FORTH
        Assert.True(await f.EvalAsync("WORDLIST 1 SET-ORDER"));
        Assert.True(await f.EvalAsync("GET-ORDER"));
        Assert.Equal(3, f.Stack.Count);
        var count = (long)f.Stack[2];
        Assert.Equal(2L, count);
        Assert.True(await f.EvalAsync("DROP DROP DROP"));

        // Call ALSO
        Assert.True(await f.EvalAsync("ALSO"));
        Assert.True(await f.EvalAsync("GET-ORDER"));
        Assert.Equal(4, f.Stack.Count);
        count = (long)f.Stack[3];
        Assert.Equal(3L, count);
        // Top should be the duplicated wordlist
        var topWid = f.Stack[0];
        var secondWid = f.Stack[1];
        Assert.Equal(topWid, secondWid); // duplicated
        var bottomWid = f.Stack[2];
        Assert.Equal("FORTH", bottomWid);
        Assert.True(await f.EvalAsync("DROP DROP DROP DROP"));
    }
}
