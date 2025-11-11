using System.Threading.Tasks;
namespace Forth.Tests.Core.Binding;

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

    // Throwing variants for tests
    public static int ThrowSync() => throw new System.InvalidOperationException("sync fail");
    public static Task ThrowTask() => Task.FromException(new System.InvalidOperationException("task fail"));
    public static async Task<int> ThrowTaskT()
    {
        await Task.Delay(1);
        throw new System.InvalidOperationException("taskT fail");
    }
    public static ValueTask ThrowValueTask() => ValueTask.FromException(new System.InvalidOperationException("vt fail"));
    public static async ValueTask<int> ThrowValueTaskT()
    {
        await Task.Delay(1);
        throw new System.InvalidOperationException("vtT fail");
    }
}
