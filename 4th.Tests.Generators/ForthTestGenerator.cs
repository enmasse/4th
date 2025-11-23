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
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var includeIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("INCLUDE"))
                {
                    includeIndex = i;
                    break;
                }
            }
            if (includeIndex == -1) continue; // No INCLUDE, skip
            var common = string.Join("\n", lines.Take(includeIndex + 1)) + "\n"; // Include the INCLUDE line
            var rest = string.Join("\n", lines.Skip(includeIndex + 1)) + "\n";
            var testGroups = ParseTestGroups(rest);
            if (testGroups.Count == 0)
            {
                // Fallback to whole file
                var displayName = System.IO.Path.GetFileNameWithoutExtension(af.Path);
                var methodName = "Test_" + SanitizeName(displayName);
                var codeToEval = content.Replace("\"", "\"\"");
                sb.AppendLine("    [Fact(DisplayName=\"" + displayName.Replace("\"", "\\\"") + "\")]");
                sb.AppendLine("    public static async Task " + methodName + "() {");
                sb.AppendLine("        var interpreter = new ForthInterpreter(new Forth.Core.Modules.TestIO());");
                sb.AppendLine("        var dir = System.IO.Path.GetDirectoryName(\"" + af.Path.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                sb.AppendLine("        var prev = System.IO.Directory.GetCurrentDirectory();");
                sb.AppendLine("        var code = @\"" + codeToEval + "\";");
                sb.AppendLine("        try {");
                sb.AppendLine("            System.IO.Directory.SetCurrentDirectory(dir!);");
                sb.AppendLine("            var ok = await interpreter.EvalAsync(code);");
                sb.AppendLine("            Assert.True(ok, \"Forth test failed: " + displayName.Replace("\"", "\\\"") + "\");");
                sb.AppendLine("        } finally {");
                sb.AppendLine("            System.IO.Directory.SetCurrentDirectory(prev);");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
            }
            else
            {
                foreach (var group in testGroups)
                {
                    var displayName = group.Name;
                    var methodName = "Test_" + SanitizeName(displayName);
                    var codeToEval = (common + group.Code).Replace("\"", "\"\"");
                    sb.AppendLine("    [Fact(DisplayName=\"" + displayName.Replace("\"", "\\\"") + "\")]");
                    sb.AppendLine("    public static async Task " + methodName + "() {");
                    sb.AppendLine("        var interpreter = new ForthInterpreter(new Forth.Core.Modules.TestIO());");
                    sb.AppendLine("        var dir = System.IO.Path.GetDirectoryName(\"" + af.Path.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                    sb.AppendLine("        var prev = System.IO.Directory.GetCurrentDirectory();");
                    sb.AppendLine("        var code = @\"" + codeToEval + "\";");
                    sb.AppendLine("        try {");
                    sb.AppendLine("            System.IO.Directory.SetCurrentDirectory(dir!);");
                    sb.AppendLine("            var ok = await interpreter.EvalAsync(code);");
                    sb.AppendLine("            Assert.True(ok, \"Forth test failed: " + displayName.Replace("\"", "\\\"") + "\");");
                    sb.AppendLine("        } finally {");
                    sb.AppendLine("            System.IO.Directory.SetCurrentDirectory(prev);");
                    sb.AppendLine("        }");
                    sb.AppendLine("    }");
                }
            }
        }

        sb.AppendLine("}} ");
        context.AddSource("ForthGeneratedTests.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static List<(string Name, string Code)> ParseTestGroups(string rest)
    {
        var groups = new List<(string, string)>();
        var lines = rest.Split(new[] { '\n' }, StringSplitOptions.None);
        int start = -1;
        string currentName = "";
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("TESTING"))
            {
                if (start != -1)
                {
                    // End previous group
                    var code = string.Join("\n", lines.Skip(start).Take(i - start));
                    groups.Add((currentName, code));
                }
                // Start new group
                var match = Regex.Match(trimmed, @"TESTING\s+(.+)");
                currentName = match.Success ? match.Groups[1].Value.Trim('\"') : "Unknown";
                start = i;
            }
        }
        if (start != -1)
        {
            var code = string.Join("\n", lines.Skip(start));
            groups.Add((currentName, code));
        }
        return groups;
    }

    private static string SanitizeName(string name)
    {
        var baseName = new string(name.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        if (string.IsNullOrEmpty(baseName)) baseName = "Test" + Guid.NewGuid().ToString("N");
        if (!char.IsLetter(baseName[0])) baseName = "T_" + baseName;
        return baseName;
    }
}
