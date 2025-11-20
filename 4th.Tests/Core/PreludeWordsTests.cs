using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core;

/// <summary>
/// Tests for words defined in prelude.4th (pure Forth implementations)
/// </summary>
public class PreludeWordsTests
{
    [Fact]
    public async Task TRUE_PushesMinusOne()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("TRUE"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task FALSE_PushesZero()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("FALSE"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task NOT_LogicalNegation()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0 NOT"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]);
        
        Assert.True(await forth.EvalAsync("1 NOT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task TwoDROP_DropsTwo()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 3 2DROP"));
        Assert.Single(forth.Stack);
        Assert.Equal(1L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task QuestionDUP_DuplicatesNonZero()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("5 ?DUP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[0]);
        Assert.Equal(5L, (long)forth.Stack[1]);
        
        // Zero case
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("0 ?DUP"));
        Assert.Single(forth2.Stack);
        Assert.Equal(0L, (long)forth2.Stack[0]);
    }

    [Fact]
    public async Task NIP_RemovesSecond()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 NIP"));
        Assert.Single(forth.Stack);
        Assert.Equal(2L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task TUCK_CopiesTopBelowSecond()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 TUCK"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[0]);  // bottom: 2
        Assert.Equal(1L, (long)forth.Stack[1]);  // middle: 1
        Assert.Equal(2L, (long)forth.Stack[2]);  // top: 2
    }

    [Fact]
    public async Task OnePlus_Increments()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("5 1+"));
        Assert.Single(forth.Stack);
        Assert.Equal(6L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task OneMinus_Decrements()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("5 1-"));
        Assert.Single(forth.Stack);
        Assert.Equal(4L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task TwoStar_Doubles()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("7 2*"));
        Assert.Single(forth.Stack);
        Assert.Equal(14L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task TwoSlash_Halves()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("8 2/"));
        Assert.Single(forth.Stack);
        Assert.Equal(4L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task ABS_AbsoluteValue()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-5 ABS"));
        Assert.Single(forth.Stack);
        Assert.Equal(5L, (long)forth.Stack[0]);
        
        Assert.True(await forth.EvalAsync("3 ABS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task SPACE_EmitsSingleSpace()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("SPACE"));
        Assert.Single(io.Outputs);
        Assert.Equal(" ", io.Outputs[0]);
    }

    [Fact]
    public async Task SPACES_EmitsMultipleSpaces()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("3 SPACES"));
        Assert.Equal(3, io.Outputs.Count);
        Assert.All(io.Outputs, s => Assert.Equal(" ", s));
    }

    [Fact]
    public async Task TwoFetch_FetchesDoubleCell()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE V1 VARIABLE V2"));
        Assert.True(await forth.EvalAsync("10 V1 ! 20 V2 !"));
        Assert.True(await forth.EvalAsync("V1 2@"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[0]);
        Assert.Equal(20L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task TwoStore_StoresDoubleCell()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE V1 VARIABLE V2"));
        Assert.True(await forth.EvalAsync("99 88 V1 2!"));
        Assert.True(await forth.EvalAsync("V1 @ V2 @"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(99L, (long)forth.Stack[0]);
        Assert.Equal(88L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task UDot_UnsignedOutput()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("42 U."));
        // U. outputs the number as a string via TYPE, then a space
        Assert.True(io.Outputs.Count >= 1);
        // The first output should contain "42"
        var allOutput = string.Join("", io.Outputs);
        Assert.Contains("42", allOutput);
    }

    [Fact]
    public async Task WITHIN_TestsRange()
    {
        var forth = new ForthInterpreter();
        // 5 is within [3, 10)
        Assert.True(await forth.EvalAsync("5 3 10 WITHIN"));
        Assert.Single(forth.Stack);
        Assert.True((long)forth.Stack[0] != 0);
        
        // 10 is NOT within [3, 10)
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("10 3 10 WITHIN"));
        Assert.Single(forth2.Stack);
        Assert.Equal(0L, (long)forth2.Stack[0]);
        
        // 2 is NOT within [3, 10)
        var forth3 = new ForthInterpreter();
        Assert.True(await forth3.EvalAsync("2 3 10 WITHIN"));
        Assert.Single(forth3.Stack);
        Assert.Equal(0L, (long)forth3.Stack[0]);
    }

    [Fact]
    public async Task StarSlashMOD_MultiplyThenDivide()
    {
        var forth = new ForthInterpreter();
        // 10 * 7 = 70, 70 / 3 = 23 rem 1
        Assert.True(await forth.EvalAsync("10 7 3 */MOD"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]); // remainder
        Assert.Equal(23L, (long)forth.Stack[1]); // quotient
    }

    [Fact]
    public async Task CELLS_IdentityOperation()
    {
        var forth = new ForthInterpreter();
        // CELLS is identity on our platform (1 cell = 1 address unit)
        Assert.True(await forth.EvalAsync("5 CELLS"));
        Assert.Single(forth.Stack);
        Assert.Equal(5L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task PICK_CopiesNthItem()
    {
        var forth = new ForthInterpreter();
        // Stack: 10 20 30, pick 0 should copy 30 (top)
        Assert.True(await forth.EvalAsync("10 20 30 0 PICK"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(30L, (long)forth.Stack[3]);
        
        // Pick 1 should copy 20 (second from top)
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("10 20 30 1 PICK"));
        Assert.Equal(4, forth2.Stack.Count);
        Assert.Equal(20L, (long)forth2.Stack[3]);
        
        // Pick 2 should copy 10 (third from top)
        var forth3 = new ForthInterpreter();
        Assert.True(await forth3.EvalAsync("10 20 30 2 PICK"));
        Assert.Equal(4, forth3.Stack.Count);
        Assert.Equal(10L, (long)forth3.Stack[3]);
    }

    [Fact]
    public async Task PICK_NegativeIndexThrows()
    {
        var forth = new ForthInterpreter();
        var ex = await Assert.ThrowsAsync<Forth.Core.ForthException>(
            () => forth.EvalAsync("10 20 30 -1 PICK"));
        Assert.Equal(Forth.Core.ForthErrorCode.StackUnderflow, ex.Code);
    }

    [Fact]
    public async Task PICK_OutOfRangeThrows()
    {
        var forth = new ForthInterpreter();
        var ex = await Assert.ThrowsAsync<Forth.Core.ForthException>(
            () => forth.EvalAsync("10 20 30 10 PICK"));
        Assert.Equal(Forth.Core.ForthErrorCode.StackUnderflow, ex.Code);
    }

    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }
}
