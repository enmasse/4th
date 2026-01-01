using Xunit;
using System.Threading.Tasks;
using Forth.Core.Interpreter;
using System;
using System.Runtime.CompilerServices;

namespace Forth.Tests.Core.Async;

public class CustomAwaitableTests
{
    private static ForthInterpreter New() => new();

    private sealed class SimpleAwaitable
    {
        private readonly int _value;
        public SimpleAwaitable(int v) { _value = v; }
        public SimpleAwaiter GetAwaiter() => new SimpleAwaiter(_value);

        public struct SimpleAwaiter : INotifyCompletion
        {
            private readonly int _v;
            public SimpleAwaiter(int v) { _v = v; }
            public bool IsCompleted => true;
            public int GetResult() => _v;
            public void OnCompleted(Action continuation) { /* already completed */ }
        }
    }

    [Fact]
    public async Task Await_CustomAwaiter_ResultPushed()
    {
        var f = New();
        // push a custom awaitable object
        f.AddWord("PUSHA", i => i.Push(new SimpleAwaitable(77)));
        Assert.True(await f.EvalAsync("PUSHA AWAIT"));
        Assert.Single(f.Stack);
        Assert.Equal(77L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Taskq_CustomAwaitable_ReportsCompleted()
    {
        var f = New();
        f.AddWord("PUSHA", i => i.Push(new SimpleAwaitable(1)));
        Assert.True(await f.EvalAsync("PUSHA TASK?"));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }
}
