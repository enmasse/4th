using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests;

public class DynamicLoadTests
{
    [Fact]
    public async Task LoadAssemblyWords_FromCSharp()
    {
        var f = new ForthInterpreter();
        var asm = typeof(Forth.Tests.Samples.DynamicModules.SampleDynamicModule).Assembly;
        var count = f.LoadAssemblyWords(asm);
        Assert.True(count >= 1);
        Assert.True(await f.EvalAsync("USING DynMod 41 INC"));
        Assert.Equal(42L, (long)f.Stack[^1]);
    }

    [Fact]
    public async Task LoadAssemblyWords_FromForth()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("LOAD-ASM-TYPE Forth.Tests.DynamicModules.SampleDynamicModule"));
        Assert.True(await f.EvalAsync("USING DynMod 9 INCASYNC"));
        Assert.Equal(10L, (long)f.Stack[^1]);
    }
}
