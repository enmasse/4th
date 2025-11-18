using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class MemoryTests
{
    [Fact]
    public async Task Memory_Fetch_Store_And_Here()
    {
        var forth = new ForthInterpreter();
        // VARIABLE x ALLOT or CREATE usage would provide an address; here we show intended asserts
        // When implemented: create a variable, store a value, fetch it
        Assert.True(await forth.EvalAsync("VARIABLE X"));
        Assert.True(await forth.EvalAsync("X 123 !"));
        Assert.True(await forth.EvalAsync("X @"));
        Assert.Equal(new long[] { 123 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
