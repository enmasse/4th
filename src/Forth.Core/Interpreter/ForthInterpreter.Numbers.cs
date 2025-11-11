using System.Globalization;
using System;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter
{
    private static object Normalize(object? val) => val switch
    {
        null => 0L,
        int i => (long)i,
        long l => l,
        short s => (long)s,
        byte b => (long)b,
        char c => (long)c,
        bool bo => bo ? 1L : 0L,
        _ => val!
    };

    private bool TryParseNumber(string token, out long value)
    {
        value = 0;
        if (string.IsNullOrEmpty(token)) return false;
        bool neg = false;
        int pos = 0;
        if (token[0] == '+' || token[0] == '-')
        {
            neg = token[0] == '-';
            pos = 1;
            if (token.Length == 1) return false;
        }
        MemTryGet(_baseAddr, out var b);
        int @base = (int)(b <= 0 ? 10 : b);
        long acc = 0;
        for (; pos < token.Length; pos++)
        {
            char c = token[pos];
            int digit = c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => 10 + (c - 'A'),
                >= 'a' and <= 'z' => 10 + (c - 'a'),
                _ => -1
            };
            if (digit < 0 || digit >= @base)
                return false;
            try
            {
                checked { acc = acc * @base + digit; }
            }
            catch (OverflowException)
            {
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "Number out of range");
            }
        }
        value = neg ? -acc : acc;
        return true;
    }
}
