using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Api;

public class PublicStackApiTests
{
    [Fact]
    public void PushPop_ReadsAndWrites()
    {
        var f = new ForthInterpreter();
        f.Push(5L);
        f.Push(7L);
        Assert.Equal(7L, (long)f.Pop());
        Assert.Equal(5L, (long)f.Pop());
    }
}
