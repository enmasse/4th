using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Parsing;

public class ParsingAndCompilationTests
{
    /// <summary>
    /// Intention: Verify immediate words execute during compilation and POSTPONE compiles semantics of named word.
    /// Expected: Constructed word behaves as if IF/THEN were compiled where POSTPONE placed them.
    /// </summary>
    [Fact(Skip = "Immediate words and POSTPONE not implemented yet")] 
    public void ImmediateAndPostpone()
    {
        var forth = new ForthInterpreter();
        // : T POSTPONE IF 1 POSTPONE THEN ; IMMEDIATE
    }

    /// <summary>
    /// Intention: Confirm tick (') returns execution token and ['] compiles it as a literal during compilation.
    /// Expected: Using xts allows passing and invoking words indirectly.
    /// </summary>
    [Fact(Skip = "Tick ' and ['] to obtain execution tokens not implemented yet")] 
    public void Tick_ExecutionToken()
    {
        var forth = new ForthInterpreter();
        // ' DUP  is an xt;  ['] DUP compiles literal xt
    }

    /// <summary>
    /// Intention: Ensure STATE reflects interpreter mode (interpreting vs compiling) as a flag value.
    /// Expected: STATE @ = 0 while interpreting and nonzero when inside a definition.
    /// </summary>
    [Fact]
    public void State_AndInterpretCompile()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("STATE @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);

        // Now during compile, before ';', the STATE cell must be non-zero
        Assert.True(forth.Interpret(": T STATE @ ;"));
        // After definition finished, STATE must be back to 0
        Assert.True(forth.Interpret("STATE @"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Validate search-order and WORDLIST control for vocabulary stacks.
    /// Expected: Words appear/disappear based on GET-ORDER/SET-ORDER and DEFINITIONS.
    /// </summary>
    [Fact(Skip = "SEARCH-ORDER and WORDLIST not implemented yet")] 
    public void Wordlists_SearchOrder()
    {
        var forth = new ForthInterpreter();
        // GET-ORDER SET-ORDER DEFINITIONS ONLY FORTH also etc
    }
}
