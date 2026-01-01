using Xunit;
using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Tests.Core.ControlFlow;

public class BracketIfMultiLineFileTest
{
    [Fact]
    public async Task BracketIf_SkipsMultiLineDefinition_WhenConditionFalse()
    {
        var f = new ForthInterpreter();
        
        // Define F= first so [UNDEFINED] F= returns FALSE
        var setupCode = @"
: F= ( F: r1 r2 -- ) ( -- flag )
    FDUP F0= IF FABS THEN FSWAP  
    FDUP F0= IF FABS THEN 
    0E F~ ;
";
        await f.EvalAsync(setupCode);
        
        // Now try the pattern from paranoia.4th - this should skip the redefinition
        var testCode = @"
[UNDEFINED] F= [IF]
    : F= ( F: r1 r2 -- ) ( -- flag )
        FDUP F0= IF FABS THEN FSWAP  
        FDUP F0= IF FABS THEN 
        0E F~ ;
[THEN]
";
        // This should NOT throw "IF outside compilation" error
        await f.EvalAsync(testCode);
        
        // If we got here without "IF outside compilation" error, the skipping worked!
        // Just verify F= exists and can be called (return value doesn't matter for this test)
        await f.EvalAsync("1.0e 1.0e F=");
        Assert.Single(f.Stack); // Should return some boolean value
    }
}
