namespace Forth.Core.Interpreter;

// Facade: preserve parsing APIs on `ForthInterpreter`.
public partial class ForthInterpreter
{
    internal bool TryParseNextWord(out string word)
    {
        if (_isCompiling && _currentDefName is null && (_currentDefTokens is null || _currentDefTokens.Count == 0) && _compilationStack.Count == 0)
        {
            _isCompiling = false;
            _mem[_stateAddr] = 0;
        }

        return _parsing.TryParseNextWord(out word);
    }

    internal string ParseNextWordOrThrow(string message) => _parsing.ParseNextWordOrThrow(message);

    internal string ReadNextTokenOrThrow(string message) => _parsing.ReadNextTokenOrThrow(message);

    /// <summary>
    /// Refills the current input source with the specified line, resetting the input index.
    /// </summary>
    /// <param name="line">The new input line.</param>
    public void RefillSource(string line) => _parsing.RefillSource(line);
}
