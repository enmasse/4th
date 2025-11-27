using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

// Numeric parsing related primitives
internal static partial class CorePrimitives
{
    // >NUMBER ( c-addr u acc start -- acc' remflag digits )
    [Primitive(">NUMBER", HelpString = ">NUMBER ( c-addr u acc start -- acc' remflag digits ) - accumulate digits per BASE, report remainder")]
    private static Task Prim_ToNumber(ForthInterpreter i)
    {
        i.EnsureStack(4, ">NUMBER");
        var startObj = i.PopInternal();
        var accObj = i.PopInternal();
        var uObj = i.PopInternal();
        var addrObj = i.PopInternal();

        long start = ForthInterpreter.ToLong(startObj);
        long acc = ForthInterpreter.ToLong(accObj);
        long u = ForthInterpreter.ToLong(uObj);
        if (u < 0) throw new ForthException(ForthErrorCode.TypeError, ">NUMBER negative length");

        i.MemTryGet(i.BaseAddr, out var baseVal);
        int @base = (int)(baseVal <= 1 ? 10 : baseVal);
        if (@base < 2) @base = 10;

        ParseDigits(i, addrObj, (int)u, @base, ref acc, (int)start, out var consumed, out var remainderFlag);
        i.Push(acc);
        i.Push(remainderFlag ? 1L : 0L);
        i.Push((long)consumed);
        return Task.CompletedTask;
    }

    private static void ParseDigits(ForthInterpreter i, object addrOrStr, int length, int @base, ref long acc, int startDigits, out int consumed, out bool remainderFlag)
    {
        consumed = startDigits;

        // Materialize the input slice into a string
        string slice;
        
        if (addrOrStr is string s)
        {
            slice = length <= s.Length ? s.Substring(0, length) : s;
        }
        else if (addrOrStr is long addr)
        {
            if (length == 0)
            {
                // counted string form: addr points to count byte
                i.MemTryGet(addr, out var countVal);
                int count = (int)((byte)countVal);
                slice = i.ReadMemoryString(addr + 1, count);
            }
            else
            {
                // (c-addr u) form: addr already points to actual string data
                slice = i.ReadMemoryString(addr, length);
            }
        }
        else
        {
            throw new ForthException(ForthErrorCode.TypeError, 
                $"ParseDigits received unexpected type: {addrOrStr?.GetType().Name ?? "null"}, value={addrOrStr}");
        }

        int idx = 0;
        // Don't skip leading space - tokenizer already handled it for S" literals

        // Accumulate digits
        while (idx < slice.Length)
        {
            char c = slice[idx];
            int digit = c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => 10 + (c - 'A'),
                >= 'a' and <= 'z' => 10 + (c - 'a'),
                _ => -1
            };
            if (digit < 0 || digit >= @base) break;
            acc = acc * @base + digit;
            consumed++;
            idx++;
        }

        remainderFlag = idx < slice.Length;
    }
}
