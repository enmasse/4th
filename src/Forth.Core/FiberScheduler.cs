using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Forth;

internal sealed class Fiber
{
    public int Id { get; }
    public List<long> Stack { get; } = new();
    public Queue<Func<ForthInterpreter, Fiber, Task>> Instructions { get; } = new();
    public bool Waiting { get; set; }
    public bool Completed { get; set; }

    internal Fiber(int id)
    {
        Id = id;
    }

    public void WaitOn(Task task, FiberScheduler scheduler)
    {
        Waiting = true;
        task.ContinueWith(_ =>
        {
            Waiting = false;
            scheduler.Enqueue(this);
        }, TaskScheduler.Default);
    }
}

internal sealed class FiberScheduler
{
    private readonly ConcurrentQueue<Fiber> _runQueue = new();
    private int _nextId = 1;

    public Fiber CreateFiber()
    {
        var f = new Fiber(_nextId++);
        _runQueue.Enqueue(f);
        return f;
    }

    public void Enqueue(Fiber fiber)
    {
        if (!fiber.Completed)
            _runQueue.Enqueue(fiber);
    }

    public async Task RunAsync(ForthInterpreter interp, CancellationToken ct = default)
    {
        while (_runQueue.TryDequeue(out var fiber))
        {
            if (ct.IsCancellationRequested) break;
            if (fiber.Completed || fiber.Waiting) continue;
            if (fiber.Instructions.Count == 0)
            {
                fiber.Completed = true;
                continue;
            }
            var instr = fiber.Instructions.Dequeue();
            try
            {
                await instr(interp, fiber).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                fiber.Completed = true;
                interp.StoreObject(ex);
            }
            if (!fiber.Completed && !fiber.Waiting && fiber.Instructions.Count > 0)
                _runQueue.Enqueue(fiber);
        }
    }
}
