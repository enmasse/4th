using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Forth.Core.Binding;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("IL{", IsImmediate = true, HelpString = "IL{ ... }IL - inline IL block (raw IL opcodes supported)")]
    private static Task Prim_IL_BEGIN(ForthInterpreter i)
    {
        var tokens = new List<string>();
        while (i.TryReadNextToken(out var tok)) { if (tok == "}IL") break; tokens.Add(tok); }
        if (tokens.Count == 0) throw new ForthException(ForthErrorCode.CompileError, "Empty IL block");

        var dm = new DynamicMethod("IL$" + Guid.NewGuid().ToString("N"), typeof(void), new[] { typeof(ForthStack), typeof(ForthInterpreter) }, typeof(ForthInterpreter).Module, true);
        var il = dm.GetILGenerator();

        // Build opcode map (normalized): e.g., ldarg.0 -> Ldarg_0
        var opMap = BuildOpcodeMap();

        // Locals cache: index -> LocalBuilder (default long)
        var locals = new Dictionary<int, LocalBuilder>();
        LocalBuilder EnsureLocal(int index, Type? t = null)
        {
            if (!locals.TryGetValue(index, out var lb))
            {
                lb = il.DeclareLocal(t ?? typeof(long));
                locals[index] = lb;
            }
            return lb;
        }

        // Labels
        var labels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        Label GetLabel(string name)
        {
            if (!labels.TryGetValue(name, out var l)) { l = il.DefineLabel(); labels[name] = l; }
            return l;
        }

        int idx = 0;
        while (idx < tokens.Count)
        {
            var t = tokens[idx++];
            if (t.Length > 1 && t[^1] == ':') { var labelName = t[..^1]; il.MarkLabel(GetLabel(labelName)); continue; }
            var norm = NormalizeOpcodeToken(t);
            if (opMap.TryGetValue(norm, out var op))
            {
                // Ensure locals when using ldloc/stloc short forms
                if (op.Name.StartsWith("Ldloc_", StringComparison.Ordinal))
                {
                    if (int.TryParse(op.Name.AsSpan("Ldloc_".Length), out var lidx)) EnsureLocal(lidx);
                }
                else if (op.Name.StartsWith("Stloc_", StringComparison.Ordinal))
                {
                    if (int.TryParse(op.Name.AsSpan("Stloc_".Length), out var sidx)) EnsureLocal(sidx);
                }

                switch (op.OperandType)
                {
                    case OperandType.InlineNone:
                        il.Emit(op);
                        break;
                    case OperandType.ShortInlineI:
                        EnsureHasNext(tokens, idx, op);
                        var i8 = ParseSByte(tokens[idx++]);
                        il.Emit(op, i8);
                        break;
                    case OperandType.InlineI:
                        EnsureHasNext(tokens, idx, op);
                        var i32 = ParseInt(tokens[idx++]);
                        il.Emit(op, i32);
                        break;
                    case OperandType.InlineI8:
                        EnsureHasNext(tokens, idx, op);
                        var i64 = ParseLong(tokens[idx++]);
                        il.Emit(op, i64);
                        break;
                    case OperandType.ShortInlineR:
                        EnsureHasNext(tokens, idx, op);
                        var r4 = ParseFloat(tokens[idx++]);
                        il.Emit(op, r4);
                        break;
                    case OperandType.InlineR:
                        EnsureHasNext(tokens, idx, op);
                        var r8 = ParseDouble(tokens[idx++]);
                        il.Emit(op, r8);
                        break;
                    case OperandType.InlineString:
                        EnsureHasNext(tokens, idx, op);
                        var s = Unquote(tokens[idx++]);
                        il.Emit(op, s);
                        break;
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        EnsureHasNext(tokens, idx, op);
                        var labName = tokens[idx++];
                        il.Emit(op, GetLabel(labName));
                        break;
                    case OperandType.InlineSwitch:
                        EnsureHasNext(tokens, idx, op);
                        var count = ParseInt(tokens[idx++]);
                        if (count < 0 || idx + count > tokens.Count) throw new ForthException(ForthErrorCode.CompileError, "Invalid switch label count");
                        var arr = new Label[count];
                        for (int k = 0; k < count; k++) arr[k] = GetLabel(tokens[idx++]);
                        il.Emit(op, arr);
                        break;
                    case OperandType.InlineType:
                        EnsureHasNext(tokens, idx, op);
                        var typeName = Unquote(tokens[idx++]);
                        var tinfo = ResolveType(typeName) ?? throw new ForthException(ForthErrorCode.CompileError, $"Type not found: {typeName}");
                        il.Emit(op, tinfo);
                        break;
                    case OperandType.InlineField:
                        EnsureHasNext(tokens, idx, op);
                        var fieldSig = Unquote(tokens[idx++]);
                        var finfo = ResolveField(fieldSig) ?? throw new ForthException(ForthErrorCode.CompileError, $"Field not found: {fieldSig}");
                        il.Emit(op, finfo);
                        break;
                    case OperandType.InlineMethod:
                        EnsureHasNext(tokens, idx, op);
                        var methodSig = Unquote(tokens[idx++]);
                        var minfo = ResolveMethod(methodSig) ?? throw new ForthException(ForthErrorCode.CompileError, $"Method not found: {methodSig}");
                        il.Emit(op, minfo);
                        break;
                    case OperandType.InlineTok:
                        EnsureHasNext(tokens, idx, op);
                        var tokSig = Unquote(tokens[idx++]);
                        var mi = ResolveMethod(tokSig);
                        if (mi is not null) { il.Emit(op, mi); break; }
                        var fi = ResolveField(tokSig);
                        if (fi is not null) { il.Emit(op, fi); break; }
                        var tt = ResolveType(tokSig);
                        if (tt is not null) { il.Emit(op, tt); break; }
                        throw new ForthException(ForthErrorCode.CompileError, $"Unable to resolve token: {tokSig}");
                    case OperandType.InlineVar:
                    case OperandType.ShortInlineVar:
                        EnsureHasNext(tokens, idx, op);
                        var varIdx = ParseInt(tokens[idx++]);
                        EnsureLocal(varIdx);
                        if (op.OperandType == OperandType.ShortInlineVar) il.Emit(op, (byte)varIdx); else il.Emit(op, (short)varIdx);
                        break;
                    default:
                        throw new ForthException(ForthErrorCode.CompileError, $"Unsupported operand type: {op.OperandType}");
                }
                continue;
            }

            throw new ForthException(ForthErrorCode.CompileError, $"Unknown IL token: {t}");
        }

        il.Emit(OpCodes.Ret);
        var action = (Action<ForthStack, ForthInterpreter>)dm.CreateDelegate(typeof(Action<ForthStack, ForthInterpreter>));
        if (!i._isCompiling) action(i.DataStack, i); else i.CurrentList().Add(ii => { action(ii.DataStack, ii); return Task.CompletedTask; });
        return Task.CompletedTask;
    }

    private static Dictionary<string, OpCode> BuildOpcodeMap()
    {
        var map = new Dictionary<string, OpCode>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (f.FieldType != typeof(OpCode)) continue;
            var op = (OpCode)f.GetValue(null)!;
            var key = NormalizeOpcodeToken(f.Name);
            map[key] = op;
            // also map lower dotted form e.g., ldarg.0 for Ldarg_0
            if (f.Name.Contains("_"))
            {
                var dotted = f.Name.Replace('_', '.');
                map[NormalizeOpcodeToken(dotted)] = op;
            }
        }
        return map;
    }

    private static string NormalizeOpcodeToken(string t)
    {
        // Accept forms: ldarg.0, ldarg_0, LDARG.0, br.s
        var s = t.Trim();
        // strip optional trailing semicolons etc.
        s = s.Replace('-', '_');
        s = s.Replace('.', '_');
        return s.ToUpperInvariant();
    }

    private static void EnsureHasNext(List<string> tokens, int idx, OpCode op)
    {
        if (idx >= tokens.Count) throw new ForthException(ForthErrorCode.CompileError, $"Missing operand for {op.Name}");
    }

    private static bool TryReadNextLiteral(List<string> tokens, ref int idx, out string? lit)
    {
        if (idx >= tokens.Count) { lit = null; return false; }
        lit = tokens[idx++];
        return true;
    }

    private static string Unquote(string t)
    {
        if (t.Length >= 2 && t[0] == '"' && t[^1] == '"') return t[1..^1];
        return t;
    }

    private static sbyte ParseSByte(string t) => (sbyte)ParseInt(t);
    private static int ParseInt(string t)
    { if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return int.Parse(t.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture); return int.Parse(t, CultureInfo.InvariantCulture); }
    private static long ParseLong(string t)
    { if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return long.Parse(t.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture); return long.Parse(t, CultureInfo.InvariantCulture); }
    private static float ParseFloat(string t) => float.Parse(t, CultureInfo.InvariantCulture);
    private static double ParseDouble(string t) => double.Parse(t, CultureInfo.InvariantCulture);

    private static Type? ResolveType(string name)
    {
        // Accept C# aliases
        name = name switch
        {
            "int" => "System.Int32",
            "long" => "System.Int64",
            "short" => "System.Int16",
            "byte" => "System.Byte",
            "bool" => "System.Boolean",
            "string" => "System.String",
            "object" => "System.Object",
            "float" => "System.Single",
            "double" => "System.Double",
            _ => name
        };
        var t = Type.GetType(name, false, true);
        if (t != null) return t;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(name, false, true);
            if (t != null) return t;
        }
        return null;
    }

    private static MethodInfo? ResolveMethod(string sig)
    {
        // Expect "Namespace.Type::MethodName(type1,type2,...)" or without params
        var typeSep = sig.IndexOf("::", StringComparison.Ordinal);
        if (typeSep <= 0) return null;
        var typeName = sig[..typeSep];
        var rest = sig[(typeSep + 2)..];
        var parenIdx = rest.IndexOf('(');
        string methodName;
        string[] paramTypeNames = Array.Empty<string>();
        if (parenIdx >= 0 && rest.EndsWith(")", StringComparison.Ordinal))
        {
            methodName = rest[..parenIdx];
            var inner = rest[(parenIdx + 1)..^1];
            if (!string.IsNullOrWhiteSpace(inner)) paramTypeNames = inner.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
        else methodName = rest;
        var t = ResolveType(typeName);
        if (t is null) return null;
        if (paramTypeNames.Length == 0)
        {
            // Try unique method by name
            var cands = t.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (cands.Length == 1) return (MethodInfo)cands[0];
            // Fallback: prefer object param when ambiguous common helpers
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (m.Name != methodName) continue;
                return m; // first match
            }
            return null;
        }
        var pts = new Type[paramTypeNames.Length];
        for (int k = 0; k < pts.Length; k++) { var pt = ResolveType(paramTypeNames[k]); if (pt is null) return null; pts[k] = pt; }
        return t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, binder: null, types: pts, modifiers: null);
    }

    private static FieldInfo? ResolveField(string sig)
    {
        // Expect "Namespace.Type::FieldName"
        var typeSep = sig.IndexOf("::", StringComparison.Ordinal);
        if (typeSep <= 0) return null;
        var typeName = sig[..typeSep];
        var fieldName = sig[(typeSep + 2)..];
        var t = ResolveType(typeName);
        if (t is null) return null;
        return t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    }

    private static void EmitPushLiteral(ILGenerator il, MethodInfo mPush, string lit)
    {
        object value;
        if (lit.StartsWith("\"") && lit.EndsWith("\"")) value = lit.Length >= 2 ? lit[1..^1] : string.Empty;
        else if (long.TryParse(lit, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ln)) value = ln;
        else if (double.TryParse(lit, NumberStyles.Float, CultureInfo.InvariantCulture, out var dn)) value = dn;
        else value = lit;
        il.Emit(OpCodes.Ldarg_0);
        if (value is string s) { il.Emit(OpCodes.Ldstr, s); }
        else if (value is long l) { if (l >= int.MinValue && l <= int.MaxValue) { il.Emit(OpCodes.Ldc_I4, (int)l); il.Emit(OpCodes.Conv_I8); } else il.Emit(OpCodes.Ldc_I8, l); il.Emit(OpCodes.Box, typeof(long)); }
        else if (value is double d) { il.Emit(OpCodes.Ldc_R8, d); il.Emit(OpCodes.Box, typeof(double)); }
        else { il.Emit(OpCodes.Ldstr, value.ToString()!); }
        il.Emit(OpCodes.Callvirt, mPush);
    }

    private static void EmitBinary(ILGenerator il, MethodInfo mPop, MethodInfo mToLong, MethodInfo mPush, OpCode math)
    {
        var locA = il.DeclareLocal(typeof(long));
        var locB = il.DeclareLocal(typeof(long));
        var locRes = il.DeclareLocal(typeof(long));
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, mPop); il.Emit(OpCodes.Call, mToLong); il.Emit(OpCodes.Stloc, locA);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, mPop); il.Emit(OpCodes.Call, mToLong); il.Emit(OpCodes.Stloc, locB);
        il.Emit(OpCodes.Ldloc, locB); il.Emit(OpCodes.Ldloc, locA); il.Emit(math); il.Emit(OpCodes.Stloc, locRes);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldloc, locRes); il.Emit(OpCodes.Box, typeof(long)); il.Emit(OpCodes.Callvirt, mPush);
    }
}
