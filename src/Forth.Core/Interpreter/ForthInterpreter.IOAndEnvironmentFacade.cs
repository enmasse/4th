using System.Reflection;

namespace Forth.Core.Interpreter;

// Facade: preserve existing engine-facing IO helper methods while delegating to composed helper.
public partial class ForthInterpreter
{
    internal void WriteNumber(long n) => _ioAndEnvironment.WriteNumber(n);
    internal void NewLine() => _ioAndEnvironment.NewLine();
    internal void WriteText(string s) => _ioAndEnvironment.WriteText(s);

    internal int ReadKey() => _ioAndEnvironment.ReadKey();
    internal bool KeyAvailable() => _ioAndEnvironment.KeyAvailable();
    internal string? ReadLineFromIO() => _ioAndEnvironment.ReadLineFromIO();

    /// <summary>
    /// Registers all words marked by <see cref="Forth.Core.Execution.PrimitiveAttribute"/> in a given assembly.
    /// </summary>
    /// <param name="asm">Assembly to scan.</param>
    /// <returns>Number of words registered.</returns>
    public int LoadAssemblyWords(Assembly asm) => _ioAndEnvironment.LoadAssemblyWords(asm);
}
