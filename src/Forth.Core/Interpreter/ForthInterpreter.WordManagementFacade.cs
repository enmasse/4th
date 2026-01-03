using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Facade: preserve existing word-management APIs on `ForthInterpreter`.
public partial class ForthInterpreter
{
    internal void RegisterDefinition(string name) => _wordManagement.RegisterDefinition(name);

    internal void ForgetWord(string token) => _wordManagement.ForgetWord(token);

    /// <summary>
    /// Adds a new synchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Delegate executed when the word runs.</param>
    public void AddWord(string name, Action<IForthInterpreter> body) => _wordManagement.AddWord(name, body);

    /// <summary>
    /// Adds a new asynchronous word to the dictionary.
    /// </summary>
    /// <param name="name">Word name (case-insensitive).</param>
    /// <param name="body">Async delegate executed when the word runs.</param>
    public void AddWordAsync(string name, Func<IForthInterpreter, Task> body) => _wordManagement.AddWordAsync(name, body);

    internal bool TryResolveWord(string token, out Word? word) => _wordManagement.TryResolveWord(token, out word);

    /// <summary>
    /// Gets all word names in the dictionary.
    /// </summary>
    public IEnumerable<string> GetAllWordNames() => _wordManagement.GetAllWordNames();
}
