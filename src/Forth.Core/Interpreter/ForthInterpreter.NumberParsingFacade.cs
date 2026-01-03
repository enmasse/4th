namespace Forth.Core.Interpreter;

// Facade: keep `ForthInterpreter.TryParseNumber/TryParseDouble` available for other partial files,
// but delegate implementation to `ForthInterpreterNumberParsing`.
public partial class ForthInterpreter
{
    private bool TryParseNumber(string token, out long value) =>
        _numberParsing.TryParseNumber(token, out value);

    private bool TryParseDouble(string token, out double value) =>
        _numberParsing.TryParseDouble(token, out value);
}
