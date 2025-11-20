using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: control frame types for compilation structures
public partial class ForthInterpreter
{
    /// <summary>
    /// Abstract base for compile-time control flow frames used while assembling word instruction lists.
    /// </summary>
    internal abstract class CompileFrame
    {
        /// <summary>
        /// Gets the current instruction list segment to which emitted operations should be appended.
        /// </summary>
        public abstract List<Func<ForthInterpreter, Task>> GetCurrentList();
    }

    /// <summary>
    /// Frame representing an IF/ELSE conditional under construction.
    /// </summary>
    internal sealed class IfFrame : CompileFrame
    {
        /// <summary>Instructions for the THEN branch.</summary>
        public List<Func<ForthInterpreter, Task>> ThenPart { get; } = new();
        /// <summary>Instructions for the ELSE branch (allocated lazily when first needed).</summary>
        public List<Func<ForthInterpreter, Task>>? ElsePart { get; set; }
        /// <summary>Indicates that emission is currently targeting the ELSE branch.</summary>
        public bool InElse { get; set; }
        /// <inheritdoc />
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InElse ? (ElsePart ??= new()) : ThenPart;
    }

    /// <summary>
    /// Frame representing a BEGIN ... WHILE ... REPEAT structure under compilation.
    /// </summary>
    internal sealed class BeginFrame : CompileFrame
    {
        /// <summary>Instructions emitted before the WHILE condition (loop body preamble).</summary>
        public List<Func<ForthInterpreter, Task>> PrePart { get; } = new();
        /// <summary>Instructions emitted while inside the WHILE clause.</summary>
        public List<Func<ForthInterpreter, Task>> MidPart { get; } = new();
        /// <summary>Indicates whether emission targets the WHILE clause section.</summary>
        public bool InWhile { get; set; }
        /// <inheritdoc />
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() =>
            InWhile ? MidPart : PrePart;
    }

    /// <summary>
    /// Frame representing a DO ... LOOP structure under compilation.
    /// </summary>
    internal sealed class DoFrame : CompileFrame
    {
        /// <summary>Instructions for the loop body.</summary>
        public List<Func<ForthInterpreter, Task>> Body { get; } = new();
        /// <inheritdoc />
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => Body;
    }

    /// <summary>
    /// Exception thrown internally to signal early termination (LEAVE) from a loop during execution.
    /// </summary>
    internal sealed class LoopLeaveException : Exception { }
}