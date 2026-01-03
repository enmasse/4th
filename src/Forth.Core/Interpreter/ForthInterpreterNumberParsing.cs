using System;
using System.Globalization;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterNumberParsing
{
    private readonly ForthInterpreter _i;

    public ForthInterpreterNumberParsing(ForthInterpreter i)
    {
        _i = i;
    }

    internal bool TryParseNumber(string token, out long value)
    {
        long GetBase(long def)
        {
            _i.MemTryGet(_i.BaseAddr, out var b);
            return b <= 0 ? def : b;
        }

        return NumberParser.TryParse(token, GetBase, out value);
    }

    internal bool TryParseDouble(string token, out double value)
    {
        value = 0.0;
        if (string.IsNullOrEmpty(token)) return false;

        bool hasSuffixD = token.EndsWith('d') || token.EndsWith('D');
        string span = hasSuffixD ? token.Substring(0, token.Length - 1) : token;

        if (span.Length >= 2 && (span.EndsWith('e') || span.EndsWith('E')))
        {
            var mantissa = span.Substring(0, span.Length - 1);
            if (long.TryParse(mantissa, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var intValue))
            {
                value = (double)intValue;
                return true;
            }

            if (double.TryParse(mantissa, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        bool looksFloating = span.Contains('.')
            || span.Contains('e') || span.Contains('E')
            || span.IndexOf("NaN", StringComparison.OrdinalIgnoreCase) >= 0
            || span.IndexOf("Infinity", StringComparison.OrdinalIgnoreCase) >= 0;

        if (!looksFloating) return false;

        return double.TryParse(span, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }
}
