using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Defining;

public class DefiningWordsTests
{
    [Fact(Skip = "CREATE ... DOES> not implemented yet")] 
    public void CreateDoes_Basic()
    {
        var forth = new ForthInterpreter();
        // :NONAME CREATE COUNTER 0 , DOES> 1 + DUP TO COUNTER ;
    }

    [Fact(Skip = "VALUE and TO not implemented yet")] 
    public void ValueAndTo_Assignment()
    {
        var forth = new ForthInterpreter();
        // VALUE X 10 TO X X should yield 10
    }

    [Fact(Skip = "DEFER and IS not implemented yet")] 
    public void DeferAndIs_Rebinding()
    {
        var forth = new ForthInterpreter();
        // DEFER ACT : HELLO 123 ; ' HELLO IS ACT ACT should push 123
    }

    [Fact(Skip = "CONSTANT word not implemented yet")] 
    public void Constant_Definition()
    {
        var forth = new ForthInterpreter();
        // 99 CONSTANT N  N should push 99
    }
}
