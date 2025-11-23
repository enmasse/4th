using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace Forth.Tests.Generators;

[Generator]
public sealed class ForthTestGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        // Collect .4th AdditionalFiles
        var forthFiles = context.AdditionalFiles.Where(f => f.Path.EndsWith(".4th", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (forthFiles.Length == 0) return;

        var nameRegex = new Regex("S\"(?<name>[^\"]+)\"\\s+TEST-CASE", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var sb = new StringBuilder();
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine("using Forth.Core.Interpreter;");
        sb.AppendLine("namespace Forth.Tests.Generated { public static class ForthGeneratedTests {");

        foreach (var af in forthFiles)
        {
            var text = af.GetText(context.CancellationToken);
            if (text is null) continue;
            var content = text.ToString();
            // Extract display name
            var match = nameRegex.Match(content);
            var displayName = match.Success ? match.Groups["name"].Value.Trim() : System.IO.Path.GetFileNameWithoutExtension(af.Path);
            // Sanitize method name (letters, digits, underscore)
            var baseName = new string(displayName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            if (string.IsNullOrEmpty(baseName)) baseName = "Test" + Guid.NewGuid().ToString("N");
            // Ensure starts with letter
            if (!char.IsLetter(baseName[0])) baseName = "T_" + baseName;
            var methodName = "Test_" + baseName;
            var fileNameLiteral = af.Path.Replace("\\", "\\\\").Replace("\"", "\\\"");
            var dispLiteral = displayName.Replace("\"", "\\\"");
            var includeCmd = "INCLUDE \"" + fileNameLiteral + "\"";

            sb.AppendLine("    [Fact(DisplayName=\"" + dispLiteral + "\")]");
            sb.AppendLine("    public static async Task " + methodName + "() {");
            sb.AppendLine("        var interpreter = new ForthInterpreter(new Forth.Core.Modules.TestIO());");
            sb.AppendLine("        var dir = System.IO.Path.GetDirectoryName(\"" + fileNameLiteral + "\");");
            sb.AppendLine("        var prev = System.IO.Directory.GetCurrentDirectory();");
            sb.AppendLine("        try { System.IO.Directory.SetCurrentDirectory(dir!); var ok = await interpreter.EvalAsync(\"" + includeCmd.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"); Assert.True(ok, \"Forth test failed: " + dispLiteral.Replace("\"", "\\\"") + "\"); } finally { System.IO.Directory.SetCurrentDirectory(prev); }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}} ");
        context.AddSource("ForthGeneratedTests.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
