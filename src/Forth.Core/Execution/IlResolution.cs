using System;
using System.Reflection;

namespace Forth.Core.Execution;

internal static class IlResolution
{
    public static Type? ResolveType(string name)
    {
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

    public static MethodInfo? ResolveMethod(string sig)
    {
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
            if (!string.IsNullOrWhiteSpace(inner))
                paramTypeNames = inner.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            methodName = rest;
        }

        var t = ResolveType(typeName);
        if (t is null) return null;

        if (paramTypeNames.Length == 0)
        {
            var cands = t.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (cands.Length == 1) return (MethodInfo)cands[0];

            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (m.Name != methodName) continue;
                return m;
            }

            return null;
        }

        var pts = new Type[paramTypeNames.Length];
        for (int k = 0; k < pts.Length; k++)
        {
            var pt = ResolveType(paramTypeNames[k]);
            if (pt is null) return null;
            pts[k] = pt;
        }

        return t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, binder: null, types: pts, modifiers: null);
    }

    public static FieldInfo? ResolveField(string sig)
    {
        var typeSep = sig.IndexOf("::", StringComparison.Ordinal);
        if (typeSep <= 0) return null;

        var typeName = sig[..typeSep];
        var fieldName = sig[(typeSep + 2)..];

        var t = ResolveType(typeName);
        if (t is null) return null;

        return t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    }
}
