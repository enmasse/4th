using System.Threading.Tasks;
using Forth;
using Xunit;
using System.Linq;

namespace Forth.Tests;

public class ClrBindingTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task Bind_Sync_Static_Method()
    {
        var f = New();
        Assert.True(await f.InterpretAsync($"BIND Forth.Tests.AsyncTestTargets Add 2 ADDAB 3 4 ADDAB"));
        Assert.Equal(new long[]{7}, Longs(f));
    }

    [Fact]
    public async Task Bind_Async_Static_Method_Result()
    {
        var f = New();
        Assert.True(await f.InterpretAsync($"BIND Forth.Tests.AsyncTestTargets AddAsync 2 ADDAB 5 6 ADDAB"));
        Assert.Equal(new long[]{11}, Longs(f));
    }

    [Fact]
    public async Task Bind_Async_Void_Task()
    {
        var f = New();
        Assert.True(await f.InterpretAsync($"BIND Forth.Tests.AsyncTestTargets VoidDelay 1 DELAY 20 DELAY"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task Bind_ValueTask_Static_Method_Result()
    {
        var f = New();
        Assert.True(await f.InterpretAsync($"BIND Forth.Tests.AsyncTestTargets AddValueTask 2 ADDVT 2 9 ADDVT"));
        Assert.Equal(new long[]{11}, Longs(f));
    }

    [Fact]
    public async Task Bind_ValueTask_Void()
    {
        var f = New();
        Assert.True(await f.InterpretAsync($"BIND Forth.Tests.AsyncTestTargets VoidDelayValueTask 1 DVT 10 DVT"));
        Assert.Empty(f.Stack);
    }
}
