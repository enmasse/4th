using Forth.Core.Interpreter;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static class IlBlockPrimitives
{
    [Primitive("IL{", IsImmediate = true, HelpString = "IL{ ... }IL - inline IL block (raw IL opcodes supported)")]
    private static Task Prim_IL_BEGIN(ForthInterpreter i) => IlEmitter.EmitInlineBlock(i);
}
