using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core;

public class PreludeLoadingTests
{
    [Fact]
    public async Task Prelude_IsLoaded()
    {
        var forth = new ForthInterpreter();
        // Try to use a prelude word
        await forth.EvalAsync("TRUE");
        Assert.Single(forth.Stack);
    }

    [Fact]
    public void Prelude_ResourceExists()
    {
        var asm = typeof(ForthInterpreter).Assembly;
        var resources = asm.GetManifestResourceNames();
        Assert.Contains("Forth.Core.prelude.4th", resources);
    }

    [Fact]
    public async Task Prelude_ResourceIsReadable()
    {
        var asm = typeof(ForthInterpreter).Assembly;
        using var stream = asm.GetManifestResourceStream("Forth.Core.prelude.4th");
        Assert.NotNull(stream);
        using var reader = new System.IO.StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        Assert.Contains("TRUE", content);
        Assert.Contains("FALSE", content);
    }
}
