namespace Forth;

/// <summary>
/// Contract for dynamically loadable word modules. Implementations register words on the provided interpreter.
/// </summary>
public interface IForthWordModule
{
    /// <summary>
    /// Register words on the provided interpreter. Implementers may call AddWord/AddWordAsync to define words.
    /// The loader may scope registration into a Forth module based on <see cref="ForthModuleAttribute"/>.
    /// </summary>
    /// <param name="forth">Target interpreter to register words into.</param>
    void Register(IForthInterpreter forth);
}
