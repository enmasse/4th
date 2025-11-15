using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ParsingAndStringsTests
{
    [Fact(Skip = "Template: implement parsing helpers WORD PARSE COUNT PLACE")]
    public async Task Parsing_And_Strings()
    {
        var forth = new ForthInterpreter();
        // WORD should parse next word from input/source and leave it on stack
        Assert.True(await forth.EvalAsync("\" hello world\" S"));
        // COUNT should push the length and address for counted strings
    }
}
