using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class FileIOTests
{
    [Fact(Skip = "Template: implement file I/O words OPEN-FILE CLOSE-FILE READ-FILE WRITE-FILE FILE-SIZE FILE-POSITION SET-FILE-POSITION")]
    public async Task File_IO_Words()
    {
        var forth = new ForthInterpreter();
        // OPEN-FILE should push a file handle or a flag; when implemented ensure it returns a handle
        Assert.True(await forth.EvalAsync("\" test.txt\" OPEN-FILE"));
        Assert.True(forth.Stack.Any());

        // After implementation: READ-FILE will push data and a status
        // and CLOSE-FILE should remove the file handle
        // These asserts are placeholders for the intended behavior
        Assert.True(await forth.EvalAsync("\" test.txt\" OPEN-FILE READ-FILE"));
        Assert.True(await forth.EvalAsync("CLOSE-FILE"));
    }
}
