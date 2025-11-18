using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Forth.Core.SourceGenerators;

[Generator]
public class CorePrimitivesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization steps required for this simple generator
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;

        var primitiveAttr = compilation.GetTypeByMetadataName("Forth.Core.Execution.PrimitiveAttribute");
        var corePrimitives = compilation.GetTypeByMetadataName("Forth.Core.Execution.CorePrimitives");
        if (primitiveAttr is null || corePrimitives is null) return;

        // Find all method symbols named in CorePrimitives type with PrimitiveAttribute
        var methods = corePrimitives.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary && m.DeclaredAccessibility == Accessibility.Private && m.IsStatic)
            .ToArray();

        string ToLiteral(string s)
        {
            if (s is null) return "null";
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < 32 || ch > 0x7f)
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }
            return "\"" + sb.ToString() + "\"";
        }

        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Collections.Immutable;");
        sb.AppendLine("using Forth.Core.Interpreter;");
        sb.AppendLine("namespace Forth.Core.Execution;");
        sb.AppendLine("internal static partial class CorePrimitives");
        sb.AppendLine("{");
        sb.AppendLine("    private static ImmutableDictionary<(string? Module, string Name), Word> CreateWords()");
        sb.AppendLine("    {");
        sb.AppendLine("        var d = new System.Collections.Generic.Dictionary<(string? Module, string Name), Word>(new KeyComparer());");

        int idx = 0;
        foreach (var m in methods)
        {
            foreach (var attr in m.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, primitiveAttr)) continue;

                var name = attr.ConstructorArguments.Length > 0 ? attr.ConstructorArguments[0].Value as string : null;
                if (name is null) continue;
                var isImmediate = false;
                var module = (string?)null;
                var help = (string?)null;

                foreach (var named in attr.NamedArguments)
                {
                    if (named.Key == "IsImmediate" && named.Value.Value is bool b) isImmediate = b;
                    if (named.Key == "Module" && named.Value.Value is string s) module = s;
                    if (named.Key == "HelpString" && named.Value.Value is string hs) help = hs;
                }

                // Method name
                var methodName = m.Name;
                var delName = $"del{idx}";
                var wName = $"w{idx}";

                sb.AppendLine($"        // Generated for {methodName}");
                sb.AppendLine("        {");
                sb.AppendLine($"            Func<ForthInterpreter, System.Threading.Tasks.Task> {delName} = {methodName};");
                var helpLiteral = help is null ? "null" : ToLiteral(help);
                sb.AppendLine($"            var {wName} = new Word({delName}) {{ Name = {ToLiteral(name)}, IsImmediate = {isImmediate.ToString().ToLowerInvariant()}, HelpString = {helpLiteral} }};");
                var moduleLiteral = module is null ? "null" : ToLiteral(module);
                sb.AppendLine($"            d[({moduleLiteral}, {ToLiteral(name)})] = {wName};");
                sb.AppendLine("        }");
                idx++;
            }
        }

        sb.AppendLine("        return d.ToImmutableDictionary(new KeyComparer());");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("CorePrimitives.g.cs", sb.ToString());
    }
}
