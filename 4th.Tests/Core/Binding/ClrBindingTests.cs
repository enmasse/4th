using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;

namespace Forth.Tests.Core.Binding;

public class ClrBindingTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task Bind_Sync_Static_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets Add 2 ADDAB 3 4 ADDAB"));
        Assert.Equal(new long[]{7}, Longs(f));
    }

    [Fact]
    public async Task Bind_Async_Static_Method_Result()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets AddAsync 2 ADDAB 5 6 ADDAB"));
        Assert.Equal(new long[]{11}, Longs(f));
    }

    [Fact]
    public async Task Bind_Async_Void_Task()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets VoidDelay 1 DELAY 20 DELAY"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task Bind_ValueTask_Static_Method_Result()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets AddValueTask 2 ADDVT 2 9 ADDVT"));
        Assert.Equal(new long[]{11}, Longs(f));
    }

    [Fact]
    public async Task Bind_ValueTask_Void()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets VoidDelayValueTask 1 DVT 10 DVT"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task Catch_Sync_Exception_From_Bound_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets ThrowSync 0 TSYNC ' TSYNC CATCH"));
        var vals = Longs(f);
        Assert.Single(vals);
        Assert.NotEqual(0, vals[0]);
    }

    [Fact]
    public async Task Catch_Task_Exception_From_Bound_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets ThrowTask 0 TTASK ' TTASK CATCH"));
        var vals = Longs(f);
        Assert.Single(vals);
        Assert.NotEqual(0, vals[0]);
    }

    [Fact]
    public async Task Catch_TaskT_Exception_From_Bound_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets ThrowTaskT 0 TTASKT ' TTASKT CATCH"));
        var vals = Longs(f);
        Assert.Single(vals);
        Assert.NotEqual(0, vals[0]);
    }

    [Fact]
    public async Task Catch_ValueTask_Exception_From_Bound_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets ThrowValueTask 0 TVT ' TVT CATCH"));
        var vals = Longs(f);
        Assert.Single(vals);
        Assert.NotEqual(0, vals[0]);
    }

    [Fact]
    public async Task Catch_ValueTaskT_Exception_From_Bound_Method()
    {
        var f = New();
        Assert.True(await f.EvalAsync($"BIND Forth.Tests.Core.Binding.AsyncTestTargets ThrowValueTaskT 0 TVTT ' TVTT CATCH"));
        var vals = Longs(f);
        Assert.Single(vals);
        Assert.NotEqual(0, vals[0]);
    }
}
