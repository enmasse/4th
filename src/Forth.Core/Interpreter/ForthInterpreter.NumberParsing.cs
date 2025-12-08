using System.Globalization;
using Forth.Core.Binding;

namespace Forth.Core.Interpreter;

// Partial: number parsing
public partial class ForthInterpreter
{
    private bool TryParseNumber(string token, out long value)
    {
        long GetBase(long def)
        {
            MemTryGet(_baseAddr, out var b);
            return b <= 0 ? def : b;
        }

        return NumberParser.TryParse(token, GetBase, out value);
    }

    private bool TryParseDouble(string token, out double value)
    {
        value = 0.0;
        if (string.IsNullOrEmpty(token)) return false;
        
        // ANS Forth floating-point literal support:
        // 1. Decimal point: "1.5", "3.14"
        // 2. Scientific notation: "1.5e2", "3e-1", "1.5E+2"
        // 3. Forth shorthand: "1e" = 1.0, "2e" = 2.0, "1.0E" = 1.0 (e/E as type indicator)
        // 4. Optional trailing 'd'/'D' suffix: "1.5d"
        // 5. Special values: NaN, Infinity, -Infinity
        
        bool hasSuffixD = token.EndsWith('d') || token.EndsWith('D');
        string span = hasSuffixD ? token.Substring(0, token.Length - 1) : token;
        
        // Check for Forth shorthand notation: <number>e or <number>E (without exponent)
        // Examples: 1e = 1.0, 2e = 2.0, -3e = -3.0, 0e = 0.0, 1.0E = 1.0
        // This is when 'E' or 'e' is at the end with no digits after it
        if (span.Length >= 2 && (span.EndsWith('e') || span.EndsWith('E')))
        {
            var mantissa = span.Substring(0, span.Length - 1);
            // Try to parse the mantissa as an integer first (common case: "1e", "2e")
            if (long.TryParse(mantissa, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var intValue))
            {
                value = (double)intValue;
                return true;
            }
            // If that fails, try to parse as a floating-point number (e.g., "1.0E", "3.14E")
            if (double.TryParse(mantissa, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }
        
        // Check if this looks like a floating-point number
        bool looksFloating = span.Contains('.') 
            || span.Contains('e') || span.Contains('E')
            || span.IndexOf("NaN", StringComparison.OrdinalIgnoreCase) >= 0
            || span.IndexOf("Infinity", StringComparison.OrdinalIgnoreCase) >= 0;
        
        if (!looksFloating) return false;
        
        // Try to parse as double - ANS Forth allows simple decimal notation like "1.5"
        return double.TryParse(span, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }
}
