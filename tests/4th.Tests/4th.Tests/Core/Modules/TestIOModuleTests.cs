using Xunit;
using Forth.Core.Interpreter;
using Forth.Core.Modules;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Modules;

public class TestIOModuleTests
{
    [Fact]
    public async Task AddInputLine_DirectString_ReadLine()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Ensure module words from this assembly are registered
        var cnt = forth.LoadAssemblyWords(typeof(TestIOModule).Assembly);
        Assert.True(cnt >= 1);
        Assert.True(await forth.EvalAsync("USING TEST-IO"));

        Assert.True(await forth.EvalAsync("S\"HELLO\" ADD-INPUT-LINE"));
        Assert.True(await forth.EvalAsync("CREATE B 16 ALLOT"));
        Assert.True(await forth.EvalAsync("B 10 READ-LINE"));
        // drop returned length
        Assert.True(await forth.EvalAsync("DROP"));

        // Verify memory at B..B+4
        Assert.True(await forth.EvalAsync("B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(72L, (long)forth.Stack[0]); // 'H'
        Assert.Equal(69L, (long)forth.Stack[1]); // 'E'
        Assert.Equal(76L, (long)forth.Stack[2]); // 'L'
        Assert.Equal(76L, (long)forth.Stack[3]); // 'L'
        Assert.Equal(79L, (long)forth.Stack[4]); // 'O'
    }

    [Fact]
    public async Task AddInputLine_CountedAddress_ReadLine()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var cnt = forth.LoadAssemblyWords(typeof(TestIOModule).Assembly);
        Assert.True(cnt >= 1);
        Assert.True(await forth.EvalAsync("USING TEST-IO"));

        // Create counted string at C: cell length at C, bytes at C+1..C+5
        Assert.True(await forth.EvalAsync("CREATE C 16 ALLOT"));
        // store length 5 in cell
        Assert.True(await forth.EvalAsync("5 C !"));
        // store bytes "HELLO" starting at C+1
        Assert.True(await forth.EvalAsync("72 C 1 + C! 69 C 2 + C! 76 C 3 + C! 76 C 4 + C! 79 C 5 + C!"));

        // Enqueue counted-addr form
        Assert.True(await forth.EvalAsync("C ADD-INPUT-LINE"));

        // Read into buffer B
        Assert.True(await forth.EvalAsync("CREATE B 16 ALLOT"));
        Assert.True(await forth.EvalAsync("B 10 READ-LINE"));
        Assert.True(await forth.EvalAsync("DROP"));

        Assert.True(await forth.EvalAsync("B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(72L, (long)forth.Stack[0]);
        Assert.Equal(69L, (long)forth.Stack[1]);
        Assert.Equal(76L, (long)forth.Stack[2]);
        Assert.Equal(76L, (long)forth.Stack[3]);
        Assert.Equal(79L, (long)forth.Stack[4]);
    }

    [Fact]
    public async Task AddInputLine_AddrLengthPair_ReadLine()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var cnt = forth.LoadAssemblyWords(typeof(TestIOModule).Assembly);
        Assert.True(cnt >= 1);
        Assert.True(await forth.EvalAsync("USING TEST-IO"));

        // Create raw bytes at D..D+4 (no length cell)
        Assert.True(await forth.EvalAsync("CREATE D 16 ALLOT"));
        Assert.True(await forth.EvalAsync("72 D C! 69 D 1 + C! 76 D 2 + C! 76 D 3 + C! 79 D 4 + C!"));

        // Enqueue using (addr u) form: push addr then u
        Assert.True(await forth.EvalAsync("D 5 ADD-INPUT-LINE"));

        Assert.True(await forth.EvalAsync("CREATE B 16 ALLOT"));
        Assert.True(await forth.EvalAsync("B 10 READ-LINE"));
        Assert.True(await forth.EvalAsync("DROP"));

        Assert.True(await forth.EvalAsync("B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(72L, (long)forth.Stack[0]);
        Assert.Equal(69L, (long)forth.Stack[1]);
        Assert.Equal(76L, (long)forth.Stack[2]);
        Assert.Equal(76L, (long)forth.Stack[3]);
        Assert.Equal(79L, (long)forth.Stack[4]);
    }
}
