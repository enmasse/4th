using System;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterParsing
{
    private readonly ForthInterpreter _i;

    public ForthInterpreterParsing(ForthInterpreter i)
    {
        _i = i;
    }

    internal bool TryParseNextWord(out string word)
    {
        // Compile-state recovery is handled by the facade on `ForthInterpreter`.

        if (_i._parseBuffer != null && _i._parseBuffer.Count > 0)
        {
            word = _i._parseBuffer.Dequeue();
            return true;
        }

        if (_i._parser == null)
        {
            word = string.Empty;
            return false;
        }

        _i.MemTryGet(_i._inAddr, out var inVal);
        var inPos = (int)ForthInterpreter.ToLong(inVal);
        var currentPos = _i._parser.Position;

        if (inPos > currentPos || currentPos == 0)
        {
            _i._parser.SetPosition(Math.Max(0, inPos));
        }

        var result = _i._parser.ParseNext();
        if (result == null)
        {
            word = string.Empty;
            return false;
        }

        _i._mem[_i._inAddr] = (long)_i._parser.Position;

        word = result;
        return true;
    }

    internal string ParseNextWordOrThrow(string message)
    {
        if (!TryParseNextWord(out var word) || string.IsNullOrEmpty(word))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return word;
    }

    internal string ReadNextTokenOrThrow(string message)
    {
        if (!TryParseNextWord(out var word) || string.IsNullOrEmpty(word))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return word;
    }

    internal void RefillSource(string line)
    {
        _i._refillSource = line;
        _i._mem[_i._inAddr] = 0;
    }
}
