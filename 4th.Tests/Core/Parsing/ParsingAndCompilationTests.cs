using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Parsing;

public class ParsingAndCompilationTests
{
    [Fact(Skip = "Immediate words and POSTPONE not implemented yet")] 
    public void ImmediateAndPostpone()
    {
        var forth = new ForthInterpreter();
        // : T POSTPONE IF 1 POSTPONE THEN ; IMMEDIATE
    }

    [Fact(Skip = "Tick ' and ['] to obtain execution tokens not implemented yet")] 
    public void Tick_ExecutionToken()
    {
        var forth = new ForthInterpreter();
        // ' DUP  is an xt;  ['] DUP compiles literal xt
    }

    [Fact(Skip = "STATE and compile/interpret distinction not implemented yet")] 
    public void State_AndInterpretCompile()
    {
        var forth = new ForthInterpreter();
        // STATE @ 0= while interpreting, nonzero while compiling
    }

    [Fact(Skip = "SEARCH-ORDER and WORDLIST not implemented yet")] 
    public void Wordlists_SearchOrder()
    {
        var forth = new ForthInterpreter();
        // GET-ORDER SET-ORDER DEFINITIONS ONLY FORTH also etc
    }
}
