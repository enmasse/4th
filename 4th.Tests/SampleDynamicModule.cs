using Forth;
using System.Threading.Tasks;

namespace Forth.Tests.DynamicModules;

[ForthModule("DynMod")] // will scope words under DynMod module
public sealed class SampleDynamicModule : IForthWordModule
{
    public void Register(IForthInterpreter forth)
    {
        forth.AddWord("INC", i => {
            var v = (long)i.Pop();
            i.Push(v + 1);
        });
        forth.AddWordAsync("INCASYNC", async i => {
            var v = (long)i.Pop();
            await Task.Delay(1);
            i.Push(v + 1);
        });
    }
}
