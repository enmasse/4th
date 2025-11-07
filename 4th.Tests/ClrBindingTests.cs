using System.Threading.Tasks;
using Forth;
using Xunit;

namespace Forth.Tests;

public static class AsyncTestTargets
{
    public static int Add(int a, int b) => a + b;
    public static async Task<int> AddAsync(int a, int b)
    {
        await Task.Delay(10);
        return a + b;
    }
    public static async Task VoidDelay(int ms)
    {
        await Task.Delay(ms);
    }
    public static async ValueTask<int> AddValueTask(int a, int b)
    {
        await Task.Delay(5);
        return a + b;
    }
    public static async ValueTask VoidDelayValueTask(int ms)
    {
        await Task.Delay(ms);
    }
}

public class ClrBindingTests
{
    private static IForthInterpreter New() => new ForthInterpreter();

    [Fact]
    public void Bind_Sync_Static_Method()
    {
        var f = New();
        Assert.True(f.Interpret($"BIND Forth.Tests.AsyncTestTargets Add 2 ADDAB 3 4 ADDAB"));
        Assert.Equal(new long[]{7}, f.Stack);
    }

    [Fact]
    public void Bind_Async_Static_Method_Result()
    {
        var f = New();
        Assert.True(f.Interpret($"BIND Forth.Tests.AsyncTestTargets AddAsync 2 ADDAB 5 6 ADDAB"));
        // Should push object id (since generic Task<int>) or direct int? We push int.
        Assert.Equal(new long[]{11}, f.Stack);
    }

    [Fact]
    public void Bind_Async_Void_Task()
    {
        var f = New();
        Assert.True(f.Interpret($"BIND Forth.Tests.AsyncTestTargets VoidDelay 1 DELAY 20 DELAY"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public void Bind_ValueTask_Static_Method_Result()
    {
        var f = New();
        Assert.True(f.Interpret($"BIND Forth.Tests.AsyncTestTargets AddValueTask 2 ADDVT 2 9 ADDVT"));
        Assert.Equal(new long[]{11}, f.Stack);
    }

    [Fact]
    public void Bind_ValueTask_Void()
    {
        var f = New();
        Assert.True(f.Interpret($"BIND Forth.Tests.AsyncTestTargets VoidDelayValueTask 1 DVT 10 DVT"));
        Assert.Empty(f.Stack);
    }
}
