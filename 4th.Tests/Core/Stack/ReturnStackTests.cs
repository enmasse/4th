using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Stack;

public class ReturnStackTests
{
    [Fact(Skip = ">R and R> (return stack transfer) not implemented yet")] 
    public void ReturnStack_BasicTransfer()
    {
        var forth = new ForthInterpreter();
        // 1 2 >R R> should leave 1 2 on the data stack in most Forths
        // forth.Interpret("1 2 >R R>");
    }

    [Fact(Skip = "2>R and 2R> (double-cell transfer) not implemented yet")] 
    public void ReturnStack_DoubleCellTransfer()
    {
        var forth = new ForthInterpreter();
        // forth.Interpret("10 20 2>R 2R>");
    }

    [Fact(Skip = "RP@ (return stack pointer fetch) not implemented yet")] 
    public void ReturnStack_RPFetch()
    {
        var forth = new ForthInterpreter();
        // forth.Interpret("RP@");
    }
}
