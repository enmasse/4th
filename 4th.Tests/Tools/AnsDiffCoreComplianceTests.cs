using System.Diagnostics;
using System.IO;
using Xunit;

namespace FourTh.Tests.Tools
{
    public class AnsDiffCoreComplianceTests
    {
        [Fact]
        public void AnsDiffReportsNoMissingCoreWords()
        {
            // Locate repo root
            var start = Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(start);
            while (dir != null && dir.GetFiles("*.sln").Length == 0)
            {
                dir = dir.Parent;
            }
            var root = dir?.FullName ?? start;
            var projPath = Path.Combine(root, "tools", "ans-diff", "ans-diff.csproj");
            Assert.True(File.Exists(projPath), $"ans-diff project not found at {projPath}");

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projPath}\" -- --sets=core",
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)!;
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                var reportPath = Path.Combine(root, "tools", "ans-diff", "report.md");
                var report = File.Exists(reportPath) ? File.ReadAllText(reportPath) : "<no report>";
                Assert.True(false, $"ans-diff failed (code {proc.ExitCode}).\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}\nREPORT:\n{report}");
            }
        }
    }
}
