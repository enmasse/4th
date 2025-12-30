using System;
using System.IO;
using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Diagnostics;

public class TtesterQuoteTokenizationDiagnosticTests
{
    [Fact]
    public async Task LoadingFpTtester_ThrowsUndefinedQuoteToken()
    {
        var f = new ForthInterpreter { EnableTrace = true };
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var fpTtester = Path.Combine(root, "tests", "forth2012-test-suite", "src", "fp", "ttester.fs");

        var ex = await Record.ExceptionAsync(() => f.EvalAsync($"\\\"{fpTtester}\\\" INCLUDE"));
        if (ex is null)
            return;

        var trace = f.GetTraceDump();
        throw new Xunit.Sdk.XunitException($"{ex}\n\nTRACE:\n{trace}");
    }
}
