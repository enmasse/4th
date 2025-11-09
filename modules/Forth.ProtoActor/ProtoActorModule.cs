using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Forth.Core;
using Forth.Core.Modules;
using Forth.Core.Interpreter;
using Forth.Core.Binding;
using Proto;

namespace Forth.ProtoActor;

/// <summary>
/// Proto.Actor integration words under the <c>Proto</c> module (e.g. SPAWN-ECHO, ASK-LONG, SPAWN-FORTH, FORTH-EVAL).
/// </summary>
[ForthModule("Proto")] // USAGE: USING Proto ...
public sealed class ProtoActorModule : IForthWordModule
{
    private static readonly ConcurrentDictionary<PID, ActorSystem> _systems = new();

    /// <summary>
    /// Registers Proto.Actor related words on the interpreter.
    /// </summary>
    public void Register(IForthInterpreter forth)
    {
        // START (placeholder) ? pushes 0
        forth.AddWord("START", i => { i.Push(0L); });

        // SPAWN-ECHO ? pid (actor echoes number replies with same number)
        forth.AddWord("SPAWN-ECHO", i =>
        {
            var system = new ActorSystem();
            var props = Props.FromFunc(ctx =>
            {
                if (ctx.Message is long l) ctx.Respond(l);
                return Task.CompletedTask;
            });
            var pid = system.Root.Spawn(props);
            _systems[pid] = system;
            i.Push(pid);
        });

        // ASK-LONG: pid n -> task<long>
        forth.AddWord("ASK-LONG", i =>
        {
            var nObj = i.Pop();
            var pidObj = i.Pop();
            if (pidObj is not PID pid) throw new ForthException(ForthErrorCode.TypeError, "ASK-LONG expects PID then number");
            if (!_systems.TryGetValue(pid, out var system)) throw new ForthException(ForthErrorCode.TypeError, "Unknown PID (no system)");
            var task = system.Root.RequestAsync<long>(pid, ToLong(nObj));
            i.Push(task);
        });

        // SHUTDOWN: pid -- (shuts down owning system)
        forth.AddWordAsync("SHUTDOWN", async i =>
        {
            var pidObj = i.Pop();
            if (pidObj is PID pid && _systems.TryRemove(pid, out var system))
            {
                await system.ShutdownAsync();
            }
        });

        // SPAWN-FORTH ? pid
        forth.AddWord("SPAWN-FORTH", i =>
        {
            var system = new ActorSystem();
            var props = Props.FromProducer(() => new ForthInterpreterActor());
            var pid = system.Root.Spawn(props);
            _systems[pid] = system;
            i.Push(pid);
        });

        // FORTH-EVAL: pid "source" -- task<ForthEvalResponse>
        forth.AddWord("FORTH-EVAL", i =>
        {
            var srcObj = i.Pop();
            var pidObj = i.Pop();
            if (pidObj is not PID pid) throw new ForthException(ForthErrorCode.TypeError, "FORTH-EVAL expects PID then source");
            if (!_systems.TryGetValue(pid, out var system)) throw new ForthException(ForthErrorCode.TypeError, "Unknown PID (no system)");
            var raw = srcObj?.ToString() ?? string.Empty;
            if (raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"') raw = raw.Substring(1, raw.Length - 2);
            var req = new ForthEvalRequest(raw);
            var task = system.Root.RequestAsync<ForthEvalResponse>(pid, req);
            i.Push(task);
        });
    }

    private static long ToLong(object v) => v switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        char c => c,
        bool bo => bo ? 1L : 0L,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
    };
}
