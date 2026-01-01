using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Defining;

public class ForgetWordTests
{
    [Fact]
    public async Task Forget_RemovesWord_AndLaterOnes()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync(": A 1 ; : B 2 ; : C 3 ;"));

        // Forget B -> removes B and C, A remains
        Assert.True(await forth.EvalAsync("FORGET B"));

        // A should still work
        Assert.True(await forth.EvalAsync("A"));
        Assert.Single(forth.Stack);
        Assert.Equal(1L, (long)forth.Stack[0]);

        // B and C should be undefined now
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("B"));
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("C"));
    }

    [Fact]
    public async Task Forget_CoreWord_IsRejected()
    {
        var forth = new ForthInterpreter();
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("FORGET +"));
    }

    [Fact]
    public async Task Forget_ModuleQualified_AffectsGlobalOrder()
    {
        var forth = new ForthInterpreter();
        // Define two words in module M, then a global Z
        Assert.True(await forth.EvalAsync("MODULE M : X 10 ; : Y 20 ; END-MODULE : Z 30 ;"));

        // Forget M:X -> removes M:X, M:Y, and Z (all later definitions by order)
        Assert.True(await forth.EvalAsync("FORGET M:X"));

        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("M:X"));
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("M:Y"));
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("Z"));
    }

    [Fact]
    public async Task Forget_RemovesVALUE_Definition()
    {
        var forth = new ForthInterpreter();
        // ANS Forth: VALUE requires an initial value from stack
        Assert.True(await forth.EvalAsync("0 VALUE X 42 TO X X"));
        Assert.Single(forth.Stack);
        Assert.Equal(42L, (long)forth.Stack[0]);

        // Define a later word Y to ensure order trimming works
        Assert.True(await forth.EvalAsync(": Y 99 ;"));

        // Forget X -> removes X and Y
        Assert.True(await forth.EvalAsync("FORGET X"));

        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("X"));
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync("Y"));
    }
}
