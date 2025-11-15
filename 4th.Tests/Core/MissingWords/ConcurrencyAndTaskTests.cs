using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ConcurrencyAndTaskTests
{
    [Fact(Skip = "Template: implement SPAWN JOIN TASK YIELD")]
    public async Task Concurrency_Words()
    {
        var forth = new ForthInterpreter();
        // SPAWN should start a new task/fiber and push a task token
        Assert.True(await forth.EvalAsync("['] 1 SPAWN"));
        // JOIN should wait for spawned task to finish
        Assert.True(true);
    }
}
