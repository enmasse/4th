using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: control frame types for compilation structures
public partial class ForthInterpreter
{
    internal abstract class CompileFrame
    {
        public abstract List<Func<ForthInterpreter, Task>> GetCurrentList();
    }

    internal sealed class IfFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> ThenPart { get; } = new();
        public List<Func<ForthInterpreter, Task>>? ElsePart { get; set; }
        public bool InElse { get; set; }
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InElse ? (ElsePart ??= new()) : ThenPart;
    }

    internal sealed class BeginFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> PrePart { get; } = new();
        public List<Func<ForthInterpreter, Task>> MidPart { get; } = new();
        public bool InWhile { get; set; }
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InWhile ? MidPart : PrePart;
    }

    internal sealed class DoFrame : CompileFrame
    {
        public List<Func<ForthInterpreter, Task>> Body { get; } = new();
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => Body;
    }

    internal sealed class LoopLeaveException : Exception { }
}