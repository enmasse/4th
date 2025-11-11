using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Memory;

public class MemoryAndStringTests
{
    [Fact(Skip = "C@ and C! (byte fetch/store) not implemented yet")] 
    public void ByteFetchStore()
    {
        var forth = new ForthInterpreter();
        // CREATE BUF 10 ALLOT  65 BUF C!  BUF C@ should push 65
    }

    [Fact(Skip = "+! (add store) not implemented yet")] 
    public void PlusStore()
    {
        var forth = new ForthInterpreter();
        // VARIABLE X 10 X ! 5 X +! X @ should push 15
    }

    [Fact(Skip = "MOVE FILL ERASE not implemented yet")] 
    public void MoveFillErase()
    {
        var forth = new ForthInterpreter();
        // CREATE A 20 ALLOT CREATE B 20 ALLOT A B 20 MOVE
    }

    [Fact(Skip = "S\" string literal and TYPE not implemented yet")] 
    public void SQuoteType_Output()
    {
        var forth = new ForthInterpreter();
        // S" HELLO" TYPE should print HELLO
    }
}
