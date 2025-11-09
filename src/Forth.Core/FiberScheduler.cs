using System.Collections.Concurrent;
using System.Threading.Tasks;
using Forth.Core.Interpreter;
using System;
using System.Threading;

namespace Forth.Core.Scheduling;

internal sealed class Fiber
{
    public int Id { get; }
    public List<long> Stack { get; } = new();
    public Queue<Func<ForthInterpreter, Fiber, Task>> Instructions { get; set; } = new();
    public bool Waiting { get; set; }
    public bool Completed { get; set; }

    internal Fiber(int id) => Id = id;

    public void WaitOn(Task task, FiberScheduler scheduler)
    {
        Waiting = true;
        scheduler.RegisterWait(this, task);
        task.ContinueWith(_ =>
        {
            Waiting = false;
            scheduler.CompleteWait(this);
            scheduler.Enqueue(this);
        }, TaskScheduler.Default);
    }

    public void Prepend(Func<ForthInterpreter, Fiber, Task> instr)
    {
        if (Completed) return;
        if (Instructions.Count == 0)
        {
            Instructions.Enqueue(instr);
            return;
        }
        var list = new List<Func<ForthInterpreter, Fiber, Task>>(Instructions.Count + 1) { instr };
        while (Instructions.Count > 0)
            list.Add(Instructions.Dequeue());
        Instructions = new Queue<Func<ForthInterpreter, Fiber, Task>>(list);
    }
}

internal sealed class FiberScheduler
{
    private readonly ConcurrentQueue<Fiber> _runQueue = new();
    private readonly ConcurrentDictionary<int, Task> _waiting = new();
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

    internal void RegisterWait(Fiber fiber, Task task) => _waiting[fiber.Id] = task;
    internal void CompleteWait(Fiber fiber) => _waiting.TryRemove(fiber.Id, out _);

    public async Task RunAsync(ForthInterpreter interp, CancellationToken ct = default)
    {
        while (true)
        {
            if (_runQueue.TryDequeue(out var fiber))
            {
                if (ct.IsCancellationRequested) break;
                if (fiber.Completed || fiber.Waiting)
                    continue;
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
                    interp.Push(ex);
                }
                if (!fiber.Completed && !fiber.Waiting && fiber.Instructions.Count > 0)
                    _runQueue.Enqueue(fiber);
            }
            else
            {
                if (_waiting.IsEmpty)
                    break;
                var tasks = _waiting.Values.ToArray();
                if (tasks.Length == 0) continue;
                await Task.WhenAny(tasks).ConfigureAwait(false);
            }
        }
    }
}
