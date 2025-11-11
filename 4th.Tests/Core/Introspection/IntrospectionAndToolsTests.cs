using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Introspection;

public class IntrospectionAndToolsTests
{
    /// <summary>
    /// Intention: Validate .S prints current stack contents for debugging without altering stack state.
    /// Expected: After 1 2 3 .S output contains representation like "<3> 1 2 3".
    /// </summary>
    [Fact(Skip = ".S (stack display) not implemented yet")] 
    public void DotS_StackDisplay()
    {
        var forth = new ForthInterpreter();
        // 1 2 3 .S  should show <3> 1 2 3
    }

    /// <summary>
    /// Intention: Ensure DEPTH pushes current stack depth count without modifying existing items.
    /// Expected: 1 2 3 DEPTH -> 1 2 3 3.
    /// </summary>
    [Fact] 
    public void Depth_Word()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 3 DEPTH"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        Assert.Equal(3L, (long)forth.Stack[2]);
        Assert.Equal(3L, (long)forth.Stack[3]);
    }

    /// <summary>
    /// Intention: Confirm SEE decompiles a word showing its definition for inspection.
    /// Expected: Output includes colon definition for previously defined word.
    /// </summary>
    [Fact(Skip = "SEE (decompiler) not implemented yet")] 
    public void See_Decompile()
    {
        var forth = new ForthInterpreter();
        // : X 1 2 + ; SEE X
    }

    /// <summary>
    /// Intention: Validate DUMP outputs raw memory contents for a given address range.
    /// Expected: Output shows hex/character pairs for requested cell span.
    /// </summary>
    [Fact(Skip = "DUMP (memory dump) not implemented yet")] 
    public void Dump_Memory()
    {
        var forth = new ForthInterpreter();
        // HERE 16 DUMP
    }
}
