using System.Text;
using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Forth.Core.Binding;
using System.Globalization;
using System.Reflection;

namespace Forth.Core.Execution;

internal static class CorePrimitives
{
    public static void Install(ForthInterpreter interp, Dictionary<string, ForthInterpreter.Word> dict)
    {
        dict["+"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"+"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a+b); });
        dict["-"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"-"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a-b); });
        dict["*"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"*"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a*b); });
        dict["/"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"/"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); i.Push(a/b); });
        dict["/MOD"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"/MOD"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); var quot=a/b; var rem=a % b; i.Push(rem); i.Push(quot); });
        dict["MOD"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"MOD"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); i.Push(a % b); });
        dict["<"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"<"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a < b ? 1L : 0L); });
        dict["="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"="); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a == b ? 1L : 0L); });
        dict[">"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,">"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a > b ? 1L : 0L); });
        dict["0="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"0="); var a=ToLong(i.PopInternal()); i.Push(a==0 ? 1L : 0L); });
        dict["0<>"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"0<>"); var a=ToLong(i.PopInternal()); i.Push(a!=0 ? 1L : 0L); });
        dict["<>"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"<>"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a!=b ? 1L : 0L); });
        dict["<="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"<="); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a<=b ? 1L : 0L); });
        dict[">="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,">="); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a>=b ? 1L : 0L); });
        dict["MIN"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"MIN"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a<b ? a : b); });
        dict["MAX"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"MAX"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a>b ? a : b); });
        dict["ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); });
        dict["-ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"-ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); });
        dict["@"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"@"); var addr=ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); });
        dict["!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"!"); var addr=ToLong(i.PopInternal()); var val=ToLong(i.PopInternal()); i.MemSet(addr,val); });
        dict["+!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"+!"); var addr=ToLong(i.PopInternal()); var add=ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, cur + add); });
        // Byte memory operations
        dict["C!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"C!"); var addr=ToLong(i.PopInternal()); var val=ToLong(i.PopInternal()); var b=(long)((byte)val); i.MemSet(addr, b); });
        dict["C@"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"C@"); var addr=ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push((long)((byte)v)); });
        // Block memory ops: MOVE (src dst u --), FILL (addr u char --), ERASE (addr u --)
        dict["MOVE"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,3,"MOVE");
            var u = ToLong(i.PopInternal());
            var dst = ToLong(i.PopInternal());
            var src = ToLong(i.PopInternal());
            if (u < 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Negative MOVE length");
            if (u == 0) return;
            // Handle overlap: copy direction based on addresses
            if (src < dst && src + u > dst)
            {
                for (long k = u - 1; k >= 0; k--)
                {
                    i.MemTryGet(src + k, out var v);
                    i.MemSet(dst + k, (long)((byte)v));
                }
            }
            else
            {
                for (long k = 0; k < u; k++)
                {
                    i.MemTryGet(src + k, out var v);
                    i.MemSet(dst + k, (long)((byte)v));
                }
            }
        });
        dict["FILL"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,3,"FILL");
            var ch = ToLong(i.PopInternal());
            var u = ToLong(i.PopInternal());
            var addr = ToLong(i.PopInternal());
            if (u < 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Negative FILL length");
            var b = (long)((byte)ch);
            for (long k = 0; k < u; k++) i.MemSet(addr + k, b);
        });
        dict["ERASE"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,2,"ERASE");
            var u = ToLong(i.PopInternal());
            var addr = ToLong(i.PopInternal());
            if (u < 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Negative ERASE length");
            for (long k = 0; k < u; k++) i.MemSet(addr + k, 0);
        });
        // Introspection: DUMP (addr u --) prints u bytes starting at addr as two-digit hex separated by spaces
        dict["DUMP"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,2,"DUMP");
            var u = ToLong(i.PopInternal());
            var addr = ToLong(i.PopInternal());
            if (u < 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "Negative DUMP length");
            var sb = new StringBuilder();
            for (long k = 0; k < u; k++)
            {
                if (k > 0) sb.Append(' ');
                i.MemTryGet(addr + k, out var v);
                var b = (byte)v;
                sb.Append(b.ToString("X2"));
            }
            i.WriteText(sb.ToString());
        });
        // Number parsing: >NUMBER ( str start consumed -- value remainderLen totalConsumed )
        dict[">NUMBER"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,3,">NUMBER");
            var consumed = (int)ToLong(i.PopInternal());
            var start = (int)ToLong(i.PopInternal());
            var obj = i.PopInternal();
            if (obj is not string s)
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, ">NUMBER expects a string and two integers");
            // skip leading whitespace from start
            int idx = Math.Clamp(start, 0, s.Length);
            while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
            // current BASE
            i.MemTryGet(i.BaseAddr, out var baseVal);
            int b = baseVal <= 0 ? 10 : (int)baseVal;
            long value = 0;
            int digits = 0;
            while (idx < s.Length)
            {
                int d;
                char ch = s[idx];
                if (ch >= '0' && ch <= '9') d = ch - '0';
                else if (ch >= 'A' && ch <= 'Z') d = ch - 'A' + 10;
                else if (ch >= 'a' && ch <= 'z') d = ch - 'a' + 10;
                else break;
                if (d >= b) break;
                value = value * b + d;
                idx++;
                digits++;
            }
            int remainder = s.Length - idx;
            i.Push(value);
            i.Push((long)remainder);
            i.Push((long)(consumed + digits));
        });
        // Pictured numeric output primitives (single-cell variant)
        dict["<#"] = new ForthInterpreter.Word(i => { i.PicturedBegin(); });
        dict["HOLD"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"HOLD"); var n=ToLong(i.PopInternal()); i.PicturedHold((char)(n & 0xFFFF)); });
        dict["#"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"#");
            var n = ToLong(i.PopInternal());
            // For this implementation, pictured numeric output is decimal-only regardless of BASE
            const long b = 10;
            long u = n < 0 ? -n : n;
            long rem = u % b;
            long q = u / b;
            i.PicturedHoldDigit(rem);
            i.Push(q);
        });
        dict["#S"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"#S");
            var n = ToLong(i.PopInternal());
            // Decimal-only pictured numeric output regardless of BASE
            const long b = 10;
            long u = n < 0 ? -n : n;
            if (u == 0)
            {
                i.PicturedHoldDigit(0);
                i.Push(0L);
                return;
            }
            while (u > 0)
            {
                long rem = u % b;
                i.PicturedHoldDigit(rem);
                u /= b;
            }
            i.Push(0L);
        });
        dict["SIGN"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"SIGN"); var n=ToLong(i.PopInternal()); if (n < 0) i.PicturedHold('-'); });
        dict["#>"] = new ForthInterpreter.Word(i => { var s=i.PicturedEnd(); i.Push(s); });
        dict[">R"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,">R"); var a=i.PopInternal(); i.RPush(a); });
        dict["R>"] = new ForthInterpreter.Word(i => { if (i.RCount==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow,"Return stack underflow in R>"); var a=i.RPop(); i.Push(a); });
        dict["2>R"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"2>R"); var b=i.PopInternal(); var a=i.PopInternal(); i.RPush(a); i.RPush(b); });
        dict["2R>"] = new ForthInterpreter.Word(i => { if (i.RCount<2) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow,"Return stack underflow in 2R>"); var b=i.RPop(); var a=i.RPop(); i.Push(a); i.Push(b); });
        dict["DUP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DUP"); i.Push(i.StackTop()); });
        dict["2DUP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"2DUP"); var a=i.StackNthFromTop(2); var b=i.StackNthFromTop(1); i.Push(a); i.Push(b); });
        dict["DROP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DROP"); i.DropTop(); });
        dict["SWAP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"SWAP"); i.SwapTop2(); });
        dict["2SWAP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,4,"2SWAP"); var d=i.PopInternal(); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(c); i.Push(d); i.Push(a); i.Push(b); });
        dict["OVER"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"OVER"); i.Push(i.StackNthFromTop(2)); });
        dict["NEGATE"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"NEGATE"); var a=ToLong(i.PopInternal()); i.Push(-a); });
        // Note: Additional convenience words (TRUE, FALSE, NOT, 2DROP, ?DUP, NIP, TUCK, 1+, 1-, 2*, 2/, ABS, SPACE, SPACES, 2@, 2!, U.) 
        // are defined in prelude.4th and loaded automatically after core initialization

        dict["YIELD"] = new ForthInterpreter.Word(async i => { await Task.Yield(); });
        dict["BYE"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["QUIT"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["ABORT"] = new ForthInterpreter.Word(i => { throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.Unknown, "ABORT"); });
        dict["."] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"."); var n=ToLong(i.PopInternal()); i.WriteNumber(n); });
        dict[".S"] = new ForthInterpreter.Word(i => {
            var items = i.Stack;
            var sb = new StringBuilder();
            sb.Append('<').Append(items.Count).Append("> ");
            for (int idx = 0; idx < items.Count; idx++)
            {
                if (idx > 0) sb.Append(' ');
                var o = items[idx];
                switch (o)
                {
                    case long l: sb.Append(l); break;
                    case int ii: sb.Append(ii); break;
                    case short s: sb.Append((long)s); break;
                    case byte b: sb.Append((long)b); break;
                    case char ch: sb.Append((int)ch); break;
                    case bool bo: sb.Append(bo ? 1 : 0); break;
                    default:
                        sb.Append(o?.ToString() ?? "null");
                        break;
                }
            }
            i.WriteText(sb.ToString());
        });
        dict["CR"] = new ForthInterpreter.Word(i => { i.NewLine(); });
        dict["EMIT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"EMIT"); var n=ToLong(i.PopInternal()); char ch=(char)(n & 0xFFFF); i.WriteText(ch.ToString()); });
        dict["TYPE"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"TYPE"); var obj=i.PopInternal(); if (obj is string s) { i.WriteText(s); } else throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError,"TYPE expects a string"); });
        dict["AND"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"AND"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a & b); });
        dict["OR"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"OR"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a | b); });
        dict["XOR"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"XOR"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a ^ b); });
        dict["INVERT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"INVERT"); var a=ToLong(i.PopInternal()); i.Push(~a); });
        dict["LSHIFT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"LSHIFT"); var u=ToLong(i.PopInternal()); var x=ToLong(i.PopInternal()); i.Push((long)((ulong)x << (int)u)); });
        dict["RSHIFT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"RSHIFT"); var u=ToLong(i.PopInternal()); var x=ToLong(i.PopInternal()); i.Push((long)((ulong)x >> (int)u)); });
        dict["EXIT"] = new ForthInterpreter.Word(i => { i.ThrowExit(); });
        dict["UNLOOP"] = new ForthInterpreter.Word(i => { i.Unloop(); });
        dict["DEPTH"] = new ForthInterpreter.Word(i => { i.Push((long)i.Stack.Count); });
        dict["RP@"] = new ForthInterpreter.Word(i => { i.Push((long)i.RCount); });
        dict["STATE"] = new ForthInterpreter.Word(i => { i.Push(i.StateAddr); });
        dict["BASE"] = new ForthInterpreter.Word(i => { i.Push(i.BaseAddr); });
        dict["DECIMAL"] = new ForthInterpreter.Word(i => { i.MemSet(i.BaseAddr, 10); });
        dict["HEX"] = new ForthInterpreter.Word(i => { i.MemSet(i.BaseAddr, 16); });
        dict["I"] = new ForthInterpreter.Word(i => { i.Push(i.CurrentLoopIndex()); });
        dict["EXECUTE"] = new ForthInterpreter.Word(async i => {
            ForthInterpreter.EnsureStack(i,1,"EXECUTE");
            var top = i.StackTop();
            if (top is ForthInterpreter.Word wTop)
            {
                i.PopInternal();
                await wTop.ExecuteAsync(i).ConfigureAwait(false);
                return;
            }
            if (i.Stack.Count >= 2 && i.StackNthFromTop(2) is ForthInterpreter.Word wBelow)
            {
                var data = i.PopInternal(); // value
                i.PopInternal();            // xt
                await wBelow.ExecuteAsync(i).ConfigureAwait(false);
                i.Push(data);
                return;
            }
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "EXECUTE expects an execution token");
        });
        // Exception model: CATCH ( xt -- 0 | err ) and THROW ( err -- )
        dict["CATCH"] = new ForthInterpreter.Word(async i => {
            ForthInterpreter.EnsureStack(i,1,"CATCH");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "CATCH expects an execution token");
            try
            {
                await xt.ExecuteAsync(i).ConfigureAwait(false);
                i.Push(0L);
            }
            catch (Forth.Core.ForthException ex)
            {
                var codeVal = (long)ex.Code;
                if (codeVal == 0) codeVal = 1;
                i.Push(codeVal);
            }
            catch (Exception)
            {
                // Push a non-zero error code for non-Forth exceptions (1 since Unknown is 0)
                i.Push(1L);
            }
        });
        dict["THROW"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"THROW");
            var err = ToLong(i.PopInternal());
            if (err != 0)
                throw new Forth.Core.ForthException((Forth.Core.ForthErrorCode)err, $"THROW {err}");
        });

        // WORDS: list all available word names
        dict["WORDS"] = new ForthInterpreter.Word(i => {
            var names = i.GetAllWordNames();
            var sb = new StringBuilder();
            bool first = true;
            foreach (var n in names)
            {
                if (!first) sb.Append(' ');
                first = false;
                sb.Append(n);
            }
            i.WriteText(sb.ToString());
        });

        // Multiply then divide: ( n1 n2 n3 -- n ) -> (n1 * n2) / n3
        dict["*/"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,3,"*/");
            var d = ToLong(i.PopInternal());
            var n2 = ToLong(i.PopInternal());
            var n1 = ToLong(i.PopInternal());
            if (d == 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero, "Divide by zero");
            var prod = n1 * n2;
            i.Push(prod / d);
        });
    }

    public static void InstallCompilerWords(ForthInterpreter intr)
    {
        var builder = new Dictionary<string, ForthInterpreter.Word>(StringComparer.OrdinalIgnoreCase);
        builder[":"] = new ForthInterpreter.Word(i => { var name = i.ReadNextTokenOrThrow("Expected name after ':'"); i.BeginDefinition(name); }) { IsImmediate = true, Name = ":" };
        builder[";"] = new ForthInterpreter.Word(i => { i.FinishDefinition(); }) { IsImmediate = true, Name = ";" };
        builder["IMMEDIATE"] = new ForthInterpreter.Word(i => { if (i._lastDefinedWord is null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "No recent definition to mark IMMEDIATE"); i._lastDefinedWord.IsImmediate = true; }) { IsImmediate = true, Name = "IMMEDIATE" };
        builder["POSTPONE"] = new ForthInterpreter.Word(async i => {
            var name = i.ReadNextTokenOrThrow("POSTPONE expects a name");
            if (i.TryResolveWord(name, out var wpost) && wpost is not null)
            {
                if (wpost.IsImmediate) await wpost.ExecuteAsync(i); else i.CurrentList().Add(async ii => await wpost.ExecuteAsync(ii));
                return;
            }
            switch (name.ToUpperInvariant())
            {
                case "IF": case "ELSE": case "THEN": case "BEGIN": case "WHILE": case "REPEAT": case "UNTIL":
                case "DO": case "LOOP": case "LEAVE": case "LITERAL": case "[": case "]": case "'":
                    i._tokens!.Insert(i._tokenIndex, name); return;
            }
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
        }) { IsImmediate = true, Name = "POSTPONE" };
        builder["'"] = new ForthInterpreter.Word(i => { var name = i.ReadNextTokenOrThrow("Expected word after '"); if (!i.TryResolveWord(name, out var wt) || wt is null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, $"Undefined word: {name}"); if (!i._isCompiling) i.Push(wt); else i.CurrentList().Add(ii => { ii.Push(wt); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "'" };
        builder["LITERAL"] = new ForthInterpreter.Word(i => { if (!i._isCompiling) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "LITERAL outside compilation"); ForthInterpreter.EnsureStack(i,1,"LITERAL"); var val=i.PopInternal(); i.CurrentList().Add(ii=> { ii.Push(val); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "LITERAL" };
        builder["["] = new ForthInterpreter.Word(i => { i._isCompiling=false; i._mem[i.StateAddr]=0; }) { IsImmediate = true, Name = "[" };
        builder["]"] = new ForthInterpreter.Word(i => { i._isCompiling=true; i._mem[i.StateAddr]=1; }) { IsImmediate = true, Name = "]" };
        builder["IF"] = new ForthInterpreter.Word(i => { i._controlStack.Push(new ForthInterpreter.IfFrame()); }) { IsImmediate = true, Name = "IF" };
        builder["ELSE"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.IfFrame ifr) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"ELSE without IF"); if(ifr.ElsePart is not null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Multiple ELSE"); ifr.ElsePart=new(); ifr.InElse=true; }) { IsImmediate = true, Name = "ELSE" };
        builder["THEN"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.IfFrame ifr) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"THEN without IF"); i._controlStack.Pop(); var thenPart=ifr.ThenPart; var elsePart=ifr.ElsePart; i.CurrentList().Add(async ii=> { ForthInterpreter.EnsureStack(ii,1,"IF"); var flag=ii.PopInternal(); if(ToBool(flag)) foreach(var a in thenPart) await a(ii); else if(elsePart is not null) foreach(var a in elsePart) await a(ii); }); }) { IsImmediate = true, Name = "THEN" };
        builder["BEGIN"] = new ForthInterpreter.Word(i => { i._controlStack.Push(new ForthInterpreter.BeginFrame()); }) { IsImmediate = true, Name = "BEGIN" };
        builder["WHILE"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.BeginFrame bf) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"WHILE without BEGIN"); if(bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Multiple WHILE"); bf.InWhile=true; }) { IsImmediate = true, Name = "WHILE" };
        builder["REPEAT"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.BeginFrame bf) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"REPEAT without BEGIN"); if(!bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"REPEAT requires WHILE"); i._controlStack.Pop(); var pre=bf.PrePart; var mid=bf.MidPart; i.CurrentList().Add(async ii=> { while(true){ foreach(var a in pre) await a(ii); ForthInterpreter.EnsureStack(ii,1,"WHILE"); var flag=ii.PopInternal(); if(!ToBool(flag)) break; foreach(var b in mid) await b(ii);} }); }) { IsImmediate = true, Name = "REPEAT" };
        builder["UNTIL"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.BeginFrame bf) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"UNTIL without BEGIN"); if(bf.InWhile) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"UNTIL after WHILE use REPEAT"); i._controlStack.Pop(); var body=bf.PrePart; i.CurrentList().Add(async ii=> { while(true){ foreach(var a in body) await a(ii); ForthInterpreter.EnsureStack(ii,1,"UNTIL"); var flag=ii.PopInternal(); if(ToBool(flag)) break; } }); }) { IsImmediate = true, Name = "UNTIL" };
        builder["DO"] = new ForthInterpreter.Word(i => { i._controlStack.Push(new ForthInterpreter.DoFrame()); }) { IsImmediate = true, Name = "DO" };
        builder["LOOP"] = new ForthInterpreter.Word(i => { if(i._controlStack.Count==0 || i._controlStack.Peek() is not ForthInterpreter.DoFrame df) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"LOOP without DO"); i._controlStack.Pop(); var body=df.Body; i.CurrentList().Add(async ii=> { ForthInterpreter.EnsureStack(ii,2,"DO"); var start=ToLong(ii.PopInternal()); var limit=ToLong(ii.PopInternal()); long step=start<=limit?1L:-1L; for(long idx=start; idx!=limit; idx+=step){ ii.PushLoopIndex(idx); try { foreach(var a in body) await a(ii);} catch(ForthInterpreter.LoopLeaveException){ break; } finally { ii.PopLoopIndexMaybe(); } } }); }) { IsImmediate = true, Name = "LOOP" };
        builder["LEAVE"] = new ForthInterpreter.Word(i => { bool inside=false; foreach(var f in i._controlStack) if(f is ForthInterpreter.DoFrame){ inside=true; break; } if(!inside) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"LEAVE outside DO...LOOP"); i.CurrentList().Add(ii=> { throw new ForthInterpreter.LoopLeaveException(); }); }) { IsImmediate = true, Name = "LEAVE" };
        builder["MODULE"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after MODULE"); i._currentModule=name; if(string.IsNullOrWhiteSpace(i._currentModule)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Invalid module name"); if(!i._modules.ContainsKey(i._currentModule)) i._modules[i._currentModule]= new(StringComparer.OrdinalIgnoreCase); }) { IsImmediate = true, Name = "MODULE" };
        builder["END-MODULE"] = new ForthInterpreter.Word(i => { i._currentModule=null; }) { IsImmediate = true, Name = "END-MODULE" };
        builder["USING"] = new ForthInterpreter.Word(i => { var m=i.ReadNextTokenOrThrow("Expected name after USING"); if(!i._usingModules.Contains(m)) i._usingModules.Add(m); }) { IsImmediate = true, Name = "USING" };
        builder["LOAD-ASM"] = new ForthInterpreter.Word(i => { var path=i.ReadNextTokenOrThrow("Expected path after LOAD-ASM"); var count=AssemblyWordLoader.Load(i,path); i.Push((long)count); }) { IsImmediate = true, Name = "LOAD-ASM" };
        builder["LOAD-ASM-TYPE"] = new ForthInterpreter.Word(i => { var tn=i.ReadNextTokenOrThrow("Expected type after LOAD-ASM-TYPE"); Type? t=Type.GetType(tn,false,false); if(t==null) foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){ t=asm.GetType(tn,false,false); if(t!=null) break; } if(t==null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,$"Type not found: {tn}"); var count=i.LoadAssemblyWords(t.Assembly); i.Push((long)count); }) { IsImmediate = true, Name = "LOAD-ASM-TYPE" };
        builder["CREATE"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after CREATE"); if(string.IsNullOrWhiteSpace(name)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Invalid name for CREATE"); var addr=i._nextAddr; i._lastCreatedName=name; i._lastCreatedAddr=addr; i.TargetDict()[name]= new ForthInterpreter.Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "CREATE" };
        builder[","] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,","); var v=ToLong(i.PopInternal()); i._mem[i._nextAddr++]=v; }) { IsImmediate = true, Name = "," };
        builder["DOES>"] = new ForthInterpreter.Word(i => { if(string.IsNullOrEmpty(i._lastCreatedName)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"DOES> without CREATE"); i._doesCollecting=true; i._doesTokens=new List<string>(); }) { IsImmediate = true, Name = "DOES>" };
        builder["ALLOT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"ALLOT"); var cells=ToLong(i.PopInternal()); if(cells<0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Negative ALLOT size"); for(long k=0;k<cells;k++) i._mem[i._nextAddr++]=0; }) { IsImmediate = true, Name = "ALLOT" };
        builder["VARIABLE"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after VARIABLE"); var addr=i._nextAddr++; i._mem[addr]=0; i.TargetDict()[name]= new ForthInterpreter.Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "VARIABLE" };
        builder["CONSTANT"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after CONSTANT"); ForthInterpreter.EnsureStack(i,1,"CONSTANT"); var val=i.PopInternal(); i.TargetDict()[name]= new ForthInterpreter.Word(ii=> ii.Push(val)) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "CONSTANT" };
        builder["VALUE"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after VALUE"); if(!i._values.ContainsKey(name)) i._values[name]=0; i.TargetDict()[name]= new ForthInterpreter.Word(ii=> ii.Push(ii.ValueGet(name))) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "VALUE" };
        builder["TO"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after TO"); ForthInterpreter.EnsureStack(i,1,"TO"); var vv=ToLong(i.PopInternal()); i.ValueSet(name,vv); }) { IsImmediate = true, Name = "TO" };
        builder["DEFER"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after DEFER"); i._deferred[name]=null; i.TargetDict()[name]= new ForthInterpreter.Word(async ii=> { if(!ii._deferred.TryGetValue(name,out var target) || target is null) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord,$"Deferred word not set: {name}"); await target.ExecuteAsync(ii); }) { Name = name, Module = i._currentModule }; i.RegisterDefinition(name); }) { IsImmediate = true, Name = "DEFER" };
        builder["IS"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected deferred name after IS"); ForthInterpreter.EnsureStack(i,1,"IS"); var xtObj=i.PopInternal(); if(xtObj is not ForthInterpreter.Word xt) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError,"IS expects an execution token"); if(!i._deferred.ContainsKey(name)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord,$"No such deferred: {name}"); i._deferred[name]=xt; }) { IsImmediate = true, Name = "IS" };
        builder["SEE"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after SEE"); var plain=name; var cidx=name.IndexOf(':'); if(cidx>0) plain=name[(cidx+1)..]; var text=i._decompile.TryGetValue(plain,out var s) ? s : $": {plain} ;"; i.WriteText(text); }) { IsImmediate = true, Name = "SEE" };
        builder["CHAR"] = new ForthInterpreter.Word(i => { var s=i.ReadNextTokenOrThrow("Expected char after CHAR"); if(!i._isCompiling) i.Push(s.Length>0?(long)s[0]:0L); else i.CurrentList().Add(ii=> { ii.Push(s.Length>0?(long)s[0]:0L); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "CHAR" };
        builder["S\""] = new ForthInterpreter.Word(i => { var next=i.ReadNextTokenOrThrow("Expected text after S\""); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"S\" expects quoted token"); var str=next[1..^1]; if(!i._isCompiling) i.Push(str); else i.CurrentList().Add(ii=> { ii.Push(str); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "S\"" };
        builder["S"] = new ForthInterpreter.Word(i => { var next=i.ReadNextTokenOrThrow("Expected text after S"); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"S expects quoted token"); var str=next[1..^1]; if(!i._isCompiling) i.Push(str); else i.CurrentList().Add(ii=> { ii.Push(str); return Task.CompletedTask; }); }) { IsImmediate = true, Name = "S" };
        builder[".\""] = new ForthInterpreter.Word(i => { var next=i.ReadNextTokenOrThrow("Expected text after .\""); if(next.Length<2 || next[0] != '"' || next[^1] != '"') throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,".\" expects quoted token"); var str=next[1..^1].TrimStart(); if(!i._isCompiling) { i.WriteText(str);} else { i.CurrentList().Add(ii=> { ii.WriteText(str); return Task.CompletedTask; }); } }) { IsImmediate = true, Name = ".\"" };
        builder["BIND"] = new ForthInterpreter.Word(i => { var typeName=i.ReadNextTokenOrThrow("type after BIND"); var methodName=i.ReadNextTokenOrThrow("method after BIND"); var argToken=i.ReadNextTokenOrThrow("arg count after BIND"); if(!int.TryParse(argToken,NumberStyles.Integer,CultureInfo.InvariantCulture,out var argCount)) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"Invalid arg count"); var forthName=i.ReadNextTokenOrThrow("forth name after BIND"); i.TargetDict()[forthName]= ClrBinder.CreateBoundWord(typeName,methodName,argCount); i.RegisterDefinition(forthName); }) { IsImmediate = true, Name = "BIND" };
        builder["FORGET"] = new ForthInterpreter.Word(i => { var name=i.ReadNextTokenOrThrow("Expected name after FORGET"); i.ForgetWord(name); }) { IsImmediate = true, Name = "FORGET" };
        builder["TASK?"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"TASK?"); var obj=i.PopInternal(); long flag = obj is Task t && t.IsCompleted ? 1L : 0L; i.Push(flag); }) { Name = "TASK?" };
        builder["AWAIT"] = new ForthInterpreter.Word(async i => {
            ForthInterpreter.EnsureStack(i,1,"AWAIT");
            var obj = i.PopInternal();
            switch(obj)
            {
                case Task t:
                    // This will throw if the task is faulted
                    await t.ConfigureAwait(false);
                    
                    var taskType = t.GetType();
                    if (taskType.IsGenericType)
                    {
                        var resultProperty = taskType.GetProperty("Result");
                        if (resultProperty != null && resultProperty.CanRead)
                        {
                            var result = resultProperty.GetValue(t);
                            if (result != null)
                            {
                                var resultType = result.GetType();
                                // Skip VoidTaskResult - it's the internal marker for Task (non-generic)
                                if (resultType.Name == "VoidTaskResult")
                                    break;
                                    
                                // Convert result to Forth-compatible type (same as ClrBinder.ForthInterpreterPush)
                                switch (result)
                                {
                                    case int iv: i.Push((long)iv); break;
                                    case long lv: i.Push(lv); break;
                                    case short sv: i.Push((long)sv); break;
                                    case byte bv: i.Push((long)bv); break;
                                    case char cv: i.Push((long)cv); break;
                                    case bool bov: i.Push(bov ? 1L : 0L); break;
                                    default: i.Push(result); break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "AWAIT expects a Task or ValueTask");
            }
        }) { Name = "AWAIT" };

        // JOIN is an idiomatic alias for AWAIT in Forth concurrency
        builder["JOIN"] = new ForthInterpreter.Word(async i => {
            // Delegate to AWAIT behavior
            ForthInterpreter.EnsureStack(i,1,"JOIN");
            i._dict.TryGetValue("AWAIT", out var awaitWord);
            if (awaitWord is null)
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.UndefinedWord, "AWAIT not found for JOIN");
            await awaitWord.ExecuteAsync(i);
        }) { Name = "JOIN" };

        // SPAWN: ( xt -- task ) start xt on a background Task without result
        builder["SPAWN"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"SPAWN");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt)
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "SPAWN expects an execution token");
            var task = Task.Run(async () =>
            {
                // Run on a fresh interpreter to avoid concurrent access to the same instance
                var child = new ForthInterpreter();
                try { await xt.ExecuteAsync(child).ConfigureAwait(false); }
                catch { /* fault task; let exception propagate */ throw; }
            });
            i.Push(task);
        }) { Name = "SPAWN" };

        // FUTURE: ( xt -- task ) run xt on a fresh interpreter and return its top-of-stack as the task result
        builder["FUTURE"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"FUTURE");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt)
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "FUTURE expects an execution token");
            var task = Task.Run(async () =>
            {
                var child = new ForthInterpreter();
                await xt.ExecuteAsync(child).ConfigureAwait(false);
                // If child left values, take top as result
                return child.Stack.Count > 0 ? child.Pop() : null;
            });
            i.Push(task);
        }) { Name = "FUTURE" };

        // TASK is a synonym for FUTURE for brevity, with same semantics
        builder["TASK"] = new ForthInterpreter.Word(i => {
            ForthInterpreter.EnsureStack(i,1,"TASK");
            var obj = i.PopInternal();
            if (obj is not ForthInterpreter.Word xt)
                throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, "TASK expects an execution token");
            var task = Task.Run(async () =>
            {
                var child = new ForthInterpreter();
                await xt.ExecuteAsync(child).ConfigureAwait(false);
                return child.Stack.Count > 0 ? child.Pop() : null;
            });
            i.Push(task);
        }) { Name = "TASK" };

        foreach (var kv in builder) intr._dict[kv.Key] = kv.Value;
        intr.SnapshotWords();
    }

    private static long ToLong(object v) => ForthInterpreter.ToLongPublic(v);
    private static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        _ => throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };
}
