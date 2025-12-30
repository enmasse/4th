using System;
using System.Collections.Generic;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter
{
    /// <summary>Enable in-memory tracing for diagnostics.</summary>
    internal bool EnableTrace { get; set; }

    private const int TraceCapacity = 4096;
    private readonly Queue<string> _trace = new();

    internal void Trace(string message)
    {
        if (!EnableTrace) return;
        if (_trace.Count >= TraceCapacity)
            _trace.Dequeue();
        _trace.Enqueue($"{DateTime.UtcNow:HH:mm:ss.fff} {message}");
    }

    /// <summary>Get the current trace buffer content.</summary>
    internal string GetTraceDump()
    {
        return string.Join(Environment.NewLine, _trace);
    }
}
