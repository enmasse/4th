using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class IOAndFormattingTests
{
    [Fact(Skip = "Template: implement EMIT KEY . CR .S and SPACE helpers")]
    public async Task IO_Words_Emit_Key_Dot()
    {
        var forth = new ForthInterpreter();
        // EMIT should write a character (observable via IForthIO in integration tests)
        // . should print the top number and remove it
        Assert.True(await forth.EvalAsync("65 EMIT")); // prints 'A'
        Assert.True(await forth.EvalAsync("42 .")); // prints '42' and removes it
        Assert.Empty(forth.Stack);
    }
}
