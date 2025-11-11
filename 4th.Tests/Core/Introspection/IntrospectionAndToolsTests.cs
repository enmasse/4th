using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Introspection;

public class IntrospectionAndToolsTests
{
    [Fact(Skip = ".S (stack display) not implemented yet")] 
    public void DotS_StackDisplay()
    {
        var forth = new ForthInterpreter();
        // 1 2 3 .S  should show <3> 1 2 3
    }

    [Fact(Skip = "DEPTH not implemented yet")] 
    public void Depth_Word()
    {
        var forth = new ForthInterpreter();
        // 1 2 3 DEPTH -> 1 2 3 3
    }

    [Fact(Skip = "SEE (decompiler) not implemented yet")] 
    public void See_Decompile()
    {
        var forth = new ForthInterpreter();
        // : X 1 2 + ; SEE X
    }

    [Fact(Skip = "DUMP (memory dump) not implemented yet")] 
    public void Dump_Memory()
    {
        var forth = new ForthInterpreter();
        // HERE 16 DUMP
    }
}
