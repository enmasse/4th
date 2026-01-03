using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class IlEmitter
{
    public static Task EmitInlineBlock(ForthInterpreter i)
    {
        var tokens = new List<string>();
        while (i.TryParseNextWord(out var tok))
        {
            if (tok == "}IL") break;
            tokens.Add(tok);
        }

        if (tokens.Count == 0)
            throw new ForthException(ForthErrorCode.CompileError, "Empty IL block");

        var dm = new DynamicMethod(
            "IL$" + Guid.NewGuid().ToString("N"),
            typeof(void),
            new[] { typeof(ForthInterpreter), typeof(ForthStack) },
            typeof(ForthInterpreter).Module,
            true)
        {
            InitLocals = true
        };

        var il = dm.GetILGenerator();

        var opMap = BuildOpcodeMap();
        var locals = new List<LocalBuilder>();

        LocalBuilder EnsureLocal(int index, Type? t = null)
        {
            while (locals.Count <= index)
                locals.Add(il.DeclareLocal(typeof(long)));

            var lb = locals[index];
            if (t != null && lb.LocalType != t)
            {
                locals[index] = lb = il.DeclareLocal(t);
            }
            return lb;
        }

        var labels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        Label GetLabel(string name)
        {
            if (!labels.TryGetValue(name, out var l))
            {
                l = il.DefineLabel();
                labels[name] = l;
            }
            return l;
        }

        Type? lastPushedType = null;
        bool emittedRet = false;

        int idx = 0;
        while (idx < tokens.Count)
        {
            var t = tokens[idx++];

            if (t.Length > 1 && t[^1] == ':')
            {
                var labelName = t[..^1];
                il.MarkLabel(GetLabel(labelName));
                continue;
            }

            var norm = NormalizeOpcodeToken(t);
            if (!opMap.TryGetValue(norm, out var op))
                throw new ForthException(ForthErrorCode.CompileError, $"Unknown IL token: {t}");

            if (op == OpCodes.Ldloc_0 || op == OpCodes.Ldloc_1 || op == OpCodes.Ldloc_2 || op == OpCodes.Ldloc_3)
            {
                int lidx = op == OpCodes.Ldloc_0 ? 0 : op == OpCodes.Ldloc_1 ? 1 : op == OpCodes.Ldloc_2 ? 2 : 3;
                var lb0 = EnsureLocal(lidx);
                il.Emit(OpCodes.Ldloc, lb0);
                lastPushedType = lb0.LocalType;
                continue;
            }

            if (op == OpCodes.Stloc_0 || op == OpCodes.Stloc_1 || op == OpCodes.Stloc_2 || op == OpCodes.Stloc_3)
            {
                int sidx = op == OpCodes.Stloc_0 ? 0 : op == OpCodes.Stloc_1 ? 1 : op == OpCodes.Stloc_2 ? 2 : 3;
                var lb1 = EnsureLocal(sidx, lastPushedType ?? typeof(long));
                il.Emit(OpCodes.Stloc, lb1);
                lastPushedType = null;
                continue;
            }

            switch (op.OperandType)
            {
                case OperandType.InlineNone:
                    il.Emit(op);
                    if (op == OpCodes.Ldc_I4_0 || op == OpCodes.Ldc_I4_1 || op == OpCodes.Ldc_I4_2 || op == OpCodes.Ldc_I4_3 || op == OpCodes.Ldc_I4_4 || op == OpCodes.Ldc_I4_5 || op == OpCodes.Ldc_I4_6 || op == OpCodes.Ldc_I4_7 || op == OpCodes.Ldc_I4_8 || op == OpCodes.Ldc_I4_M1)
                        lastPushedType = typeof(int);
                    else if (op == OpCodes.Conv_I8)
                        lastPushedType = typeof(long);
                    else if (op == OpCodes.Box)
                        lastPushedType = typeof(object);
                    else if (op == OpCodes.Add)
                        lastPushedType = typeof(long);
                    else if (op == OpCodes.Ret)
                        emittedRet = true;
                    else
                        lastPushedType = null;
                    break;

                case OperandType.ShortInlineI:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, (sbyte)ParseInt(tokens[idx++]));
                    lastPushedType = typeof(int);
                    break;

                case OperandType.InlineI:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, ParseInt(tokens[idx++]));
                    lastPushedType = typeof(int);
                    break;

                case OperandType.InlineI8:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, ParseLong(tokens[idx++]));
                    lastPushedType = typeof(long);
                    break;

                case OperandType.ShortInlineR:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, float.Parse(tokens[idx++], CultureInfo.InvariantCulture));
                    lastPushedType = typeof(float);
                    break;

                case OperandType.InlineR:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, double.Parse(tokens[idx++], CultureInfo.InvariantCulture));
                    lastPushedType = typeof(double);
                    break;

                case OperandType.InlineString:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, Unquote(tokens[idx++]));
                    lastPushedType = typeof(string);
                    break;

                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                    EnsureHasNext(tokens, idx, op);
                    il.Emit(op, GetLabel(tokens[idx++]));
                    lastPushedType = null;
                    break;

                case OperandType.InlineSwitch:
                    EnsureHasNext(tokens, idx, op);
                    var count = ParseInt(tokens[idx++]);
                    if (count < 0 || idx + count > tokens.Count)
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid switch label count");

                    var arr = new Label[count];
                    for (int k = 0; k < count; k++)
                        arr[k] = GetLabel(tokens[idx++]);

                    il.Emit(op, arr);
                    lastPushedType = null;
                    break;

                case OperandType.InlineType:
                    EnsureHasNext(tokens, idx, op);
                    {
                        var typeName = Unquote(tokens[idx++]);
                        var tinfo = IlResolution.ResolveType(typeName) ?? throw new ForthException(ForthErrorCode.CompileError, $"Type not found: {typeName}");
                        il.Emit(op, tinfo);
                        lastPushedType = typeof(Type);
                    }
                    break;

                case OperandType.InlineField:
                    EnsureHasNext(tokens, idx, op);
                    {
                        var fieldSig = Unquote(tokens[idx++]);
                        var finfo = IlResolution.ResolveField(fieldSig) ?? throw new ForthException(ForthErrorCode.CompileError, $"Field not found: {fieldSig}");
                        il.Emit(op, finfo);
                        lastPushedType = finfo.FieldType;
                    }
                    break;

                case OperandType.InlineMethod:
                    EnsureHasNext(tokens, idx, op);
                    {
                        var methodSig = Unquote(tokens[idx++]);
                        var minfo = IlResolution.ResolveMethod(methodSig) ?? throw new ForthException(ForthErrorCode.CompileError, $"Method not found: {methodSig}");
                        var m = (MethodInfo)minfo;
                        if (op == OpCodes.Callvirt && !m.IsVirtual)
                            il.Emit(OpCodes.Call, m);
                        else
                            il.Emit(op, m);
                        lastPushedType = m.ReturnType == typeof(void) ? null : m.ReturnType;
                    }
                    break;

                case OperandType.InlineTok:
                    EnsureHasNext(tokens, idx, op);
                    {
                        var tokSig = Unquote(tokens[idx++]);
                        var mi = IlResolution.ResolveMethod(tokSig);
                        if (mi is not null)
                        {
                            var mm = (MethodInfo)mi;
                            if (op == OpCodes.Callvirt && !mm.IsVirtual)
                                il.Emit(OpCodes.Call, mm);
                            else
                                il.Emit(op, mm);
                            lastPushedType = mm.ReturnType == typeof(void) ? null : mm.ReturnType;
                            break;
                        }

                        var fi = IlResolution.ResolveField(tokSig);
                        if (fi is not null)
                        {
                            il.Emit(op, fi);
                            lastPushedType = fi.FieldType;
                            break;
                        }

                        var tt = IlResolution.ResolveType(tokSig);
                        if (tt is not null)
                        {
                            il.Emit(op, tt);
                            lastPushedType = typeof(Type);
                            break;
                        }

                        throw new ForthException(ForthErrorCode.CompileError, $"Unable to resolve token: {tokSig}");
                    }

                case OperandType.InlineVar:
                case OperandType.ShortInlineVar:
                    EnsureHasNext(tokens, idx, op);
                    {
                        var varIdx = ParseInt(tokens[idx++]);
                        var lb = EnsureLocal(varIdx, (op == OpCodes.Stloc || op == OpCodes.Stloc_S) ? (lastPushedType ?? typeof(long)) : null);
                        if (op == OpCodes.Ldloc_S || op == OpCodes.Ldloc)
                        {
                            il.Emit(OpCodes.Ldloc, lb);
                            lastPushedType = lb.LocalType;
                        }
                        else if (op == OpCodes.Stloc_S || op == OpCodes.Stloc)
                        {
                            il.Emit(OpCodes.Stloc, lb);
                            lastPushedType = null;
                        }
                        else if (op == OpCodes.Ldloca_S || op == OpCodes.Ldloca)
                        {
                            il.Emit(OpCodes.Ldloca, lb);
                            lastPushedType = lb.LocalType.MakeByRefType();
                        }
                        else
                        {
                            il.Emit(op, (short)varIdx);
                            lastPushedType = null;
                        }
                    }
                    break;

                default:
                    throw new ForthException(ForthErrorCode.CompileError, $"Unsupported operand type: {op.OperandType}");
            }
        }

        if (!emittedRet)
            il.Emit(OpCodes.Ret);

        var action = (Action<ForthInterpreter, ForthStack>)dm.CreateDelegate(typeof(Action<ForthInterpreter, ForthStack>));
        if (!i._isCompiling)
            action(i, i._stack);
        else
            i.CurrentList().Add(ii => { action(ii, ii._stack); return Task.CompletedTask; });

        return Task.CompletedTask;
    }

    private static Dictionary<string, OpCode> BuildOpcodeMap()
    {
        var map = new Dictionary<string, OpCode>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (f.FieldType != typeof(OpCode))
                continue;

            var op = (OpCode)f.GetValue(null)!;
            var key = NormalizeOpcodeToken(f.Name);
            map[key] = op;

            if (f.Name.Contains("_", StringComparison.Ordinal))
            {
                var dotted = f.Name.Replace('_', '.');
                map[NormalizeOpcodeToken(dotted)] = op;
            }
        }
        return map;
    }

    private static string NormalizeOpcodeToken(string t)
    {
        var s = t.Trim();
        s = s.Replace('-', '_');
        s = s.Replace('.', '_');
        return s.ToUpperInvariant();
    }

    private static void EnsureHasNext(List<string> tokens, int idx, OpCode op)
    {
        if (idx >= tokens.Count)
            throw new ForthException(ForthErrorCode.CompileError, $"Missing operand for {op.Name}");
    }

    private static string Unquote(string t)
    {
        if (t.Length >= 2 && t[0] == '"' && t[^1] == '"')
            return t[1..^1];
        return t;
    }

    private static int ParseInt(string t)
    {
        if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(t.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return int.Parse(t, CultureInfo.InvariantCulture);
    }

    private static long ParseLong(string t)
    {
        if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return long.Parse(t.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return long.Parse(t, CultureInfo.InvariantCulture);
    }
}
