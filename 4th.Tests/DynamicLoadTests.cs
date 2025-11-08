using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Forth;
using Xunit;

namespace Forth.Tests;

public class DynamicLoadTests
{
    [Fact]
    public async Task LoadAssemblyWords_FromCSharp()
    {
        var f = new ForthInterpreter();
        var asm = typeof(DynamicModules.SampleDynamicModule).Assembly;
        var count = f.LoadAssemblyWords(asm);
        Assert.True(count >= 1);
        Assert.True(await f.EvalAsync("USING DynMod 41 INC"));
        Assert.Equal(42L, (long)f.Stack[^1]);
    }

    [Fact]
    public async Task LoadAssemblyWords_FromForth()
    {
        var f = new ForthInterpreter();
        // Use type-based loading to avoid path/escaping issues
        Assert.True(await f.EvalAsync("LOAD-ASM-TYPE Forth.Tests.DynamicModules.SampleDynamicModule"));
        Assert.True(await f.EvalAsync("USING DynMod 9 INCASYNC"));
        Assert.Equal(10L, (long)f.Stack[^1]);
    }
}
