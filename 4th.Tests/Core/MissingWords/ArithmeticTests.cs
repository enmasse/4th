using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ArithmeticTests
{
    [Fact]
    public async Task Arithmetic_Helpers()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 1+"));
        Assert.Equal(new long[] { 2 }, forth.Stack.Select(o => (long)o).ToArray());

        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("5 NEGATE"));
        Assert.Equal(new long[] { -5 }, forth2.Stack.Select(o => (long)o).ToArray());
        
        // Test other arithmetic helpers
        var forth3 = new ForthInterpreter();
        Assert.True(await forth3.EvalAsync("5 1-"));
        Assert.Equal(new long[] { 4 }, forth3.Stack.Select(o => (long)o).ToArray());
        
        var forth4 = new ForthInterpreter();
        Assert.True(await forth4.EvalAsync("3 2*"));
        Assert.Equal(new long[] { 6 }, forth4.Stack.Select(o => (long)o).ToArray());
        
        var forth5 = new ForthInterpreter();
        Assert.True(await forth5.EvalAsync("8 2/"));
        Assert.Equal(new long[] { 4 }, forth5.Stack.Select(o => (long)o).ToArray());
        
        var forth6 = new ForthInterpreter();
        Assert.True(await forth6.EvalAsync("-7 ABS"));
        Assert.Equal(new long[] { 7 }, forth6.Stack.Select(o => (long)o).ToArray());
        
        var forth7 = new ForthInterpreter();
        Assert.True(await forth7.EvalAsync("7 3 MOD"));
        Assert.Equal(new long[] { 1 }, forth7.Stack.Select(o => (long)o).ToArray());
    }
}
