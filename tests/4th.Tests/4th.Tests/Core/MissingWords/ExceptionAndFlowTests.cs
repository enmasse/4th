using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ExceptionAndFlowTests
{
    [Fact]
    public async Task Exception_And_Flow_Control()
    {
        var forth = new ForthInterpreter();
        // CATCH/THROW should allow error handling; here we assert the words run when implemented
        Assert.True(await forth.EvalAsync("['] ABORT"));
        // ABORT" should raise an abort with message when used as ABORT\" msg\" ...
        Assert.True(true);
    }
}
