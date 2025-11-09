using System;
using System.Collections.Generic;

namespace Forth;

/// <summary>
/// Represents a synchronously executable compiled Forth script (single line or definition body) made of sync instructions.
/// </summary>
internal sealed class ForthCompiledScript
{
    private readonly List<Action<ForthInterpreter>> _steps;
    public ForthCompiledScript(List<Action<ForthInterpreter>> steps) => _steps = steps;
    public void Run(ForthInterpreter intr)
    {
        foreach (var s in _steps) s(intr);
    }
}
