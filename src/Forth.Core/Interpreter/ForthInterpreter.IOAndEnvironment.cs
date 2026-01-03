using System.Threading.Tasks;
using Forth.Core.Execution;

namespace Forth.Core.Interpreter;

// Partial: IO operations and environment words
public partial class ForthInterpreter
{
    /// <summary>
    /// Gets the IO implementation used by the interpreter.
    /// </summary>
    public IForthIO IO => _io;

    [Primitive("ENV", HelpString = "ENV ( -- wid ) - environment wordlist id")]
    private static Task Prim_ENV(ForthInterpreter i)
    {
        i.Push("ENV");
        return Task.CompletedTask;
    }
}