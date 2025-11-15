using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class FloatingPointTests
{
    [Fact(Skip = "Template: implement floating-point words F. F+ F- F* F/ FNEGATE FVARIABLE FCONSTANT")]
    public async Task FloatingPoint_Arithmetic()
    {
        var forth = new ForthInterpreter();
        // When implemented: push floating point values and operate on them
        Assert.True(await forth.EvalAsync("1.5 2.5 F+"));
        // After implementation, stack should contain 4.0 (or equivalent representation)
        // This assert is a placeholder showing intent
        Assert.True(forth.Stack.Any());
    }
}
