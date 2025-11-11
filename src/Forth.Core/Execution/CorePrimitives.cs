using System.Text;
using System.Threading.Tasks;
using Forth.Core.Interpreter;

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
        dict["SPAWN"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"SPAWN"); var obj=i.PopInternal(); if (obj is not Task t) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"SPAWN expects a Task"); i.Push(t); i.Push(t); });
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
    }

    private static long ToLong(object v) => ForthInterpreter.ToLongPublic(v);
}
