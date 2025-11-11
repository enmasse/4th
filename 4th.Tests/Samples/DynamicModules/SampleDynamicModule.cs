using Forth.Core;
using Forth.Core.Modules;
using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Tests.Samples.DynamicModules;

[ForthModule("DynMod")]
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
