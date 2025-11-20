namespace Forth.Core.Modules;

/// <summary>
/// Contract for a class that registers one or more Forth words into an interpreter instance.
/// Implementations are typically discovered via reflection and invoked during module loading.
/// </summary>
public interface IForthWordModule
{
    /// <summary>
    /// Registers all provided words into the supplied interpreter.
    /// </summary>
    /// <param name="forth">Interpreter target for registration.</param>
    void Register(Forth.Core.IForthInterpreter forth);
}
