using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    /// <summary>
    /// Regression tests for CASE/OF/ENDOF/ENDCASE control structure.
    /// 
    /// These tests verify the fix for a critical compilation bug where test values
    /// before OF were compiled into the wrong instruction list, causing them to be
    /// left on the stack during execution.
    /// 
    /// THE BUG:
    /// When compiling `: TEST CASE 5 OF 100 ENDOF 10 OF 200 ENDOF 300 ENDCASE ;`
    /// the test values (5, 10) were being compiled into CaseFrame.DefaultPart instead
    /// of being part of their respective branches. This caused them to all be pushed
    /// onto the stack at runtime, resulting in incorrect stack contents.
    /// 
    /// THE FIX:
    /// 1. OF now moves the last DefaultPart action (the test value) into the new branch
    /// 2. OfFrame now references the parent CaseFrame's CurrentBranch so code between
    ///    OF and ENDOF goes into the correct instruction list
    /// 3. ENDCASE correctly drops the selector and executes only matching branches
    /// 
    /// These tests ensure the fix works correctly and prevent regression.
    /// </summary>
    public class CaseRegressionTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task RegressionTest_TestValuesNotLeftOnStack_MatchFirst()
        {
            // Before fix: stack would be [5, 10, 300, 100] 
            // After fix: stack should be [100]
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 5 OF 100 ENDOF 10 OF 200 ENDOF 300 ENDCASE ; 5 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(100L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task RegressionTest_TestValuesNotLeftOnStack_MatchSecond()
        {
            // Before fix: stack would be [5, 10, 300, 200]
            // After fix: stack should be [200]
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 5 OF 100 ENDOF 10 OF 200 ENDOF 300 ENDCASE ; 10 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(200L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task RegressionTest_TestValuesNotLeftOnStack_NoMatch()
        {
            // Before fix: stack would be [5, 10, 300]
            // After fix: stack should be [300]
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 5 OF 100 ENDOF 10 OF 200 ENDOF 300 ENDCASE ; 15 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(300L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_CanExecuteMultipleTimes()
        {
            // Verify the fix doesn't break multiple executions
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF 111 ENDOF 2 OF 222 ENDOF 333 ENDCASE ;"));
            
            Assert.True(await f.EvalAsync("1 TEST"));
            Assert.Equal(111L, (long)f.Stack[0]);
            
            var f2 = New();
            Assert.True(await f2.EvalAsync(": TEST CASE 1 OF 111 ENDOF 2 OF 222 ENDOF 333 ENDCASE ;"));
            Assert.True(await f2.EvalAsync("2 TEST"));
            Assert.Equal(222L, (long)f2.Stack[0]);
        }

        [Fact]
        public async Task Case_EmptyBranch()
        {
            // Empty branch should still work
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF ENDOF 200 ENDCASE ; 1 TEST"));
            Assert.Empty(f.Stack); // Empty branch, default not executed
        }

        [Fact]
        public async Task Case_EmptyDefault()
        {
            // Empty default should work
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF 100 ENDOF ENDCASE ; 1 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(100L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_EmptyDefaultNoMatch()
        {
            // Empty default, no match
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF 100 ENDOF ENDCASE ; 99 TEST"));
            Assert.Empty(f.Stack); // Selector dropped, no default
        }

        [Fact]
        public async Task Case_MultipleBranchesWithValues()
        {
            // Each branch can push multiple values
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF 10 20 ENDOF 2 OF 30 40 50 ENDOF 60 ENDCASE ; 2 TEST"));
            Assert.Equal(3, f.Stack.Count);
            Assert.Equal(30L, (long)f.Stack[0]);
            Assert.Equal(40L, (long)f.Stack[1]);
            Assert.Equal(50L, (long)f.Stack[2]);
        }

        [Fact]
        public async Task Case_DefaultWithMultipleValues()
        {
            // Default can push multiple values
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 1 OF 10 ENDOF 70 80 90 ENDCASE ; 99 TEST"));
            Assert.Equal(3, f.Stack.Count);
            Assert.Equal(70L, (long)f.Stack[0]);
            Assert.Equal(80L, (long)f.Stack[1]);
            Assert.Equal(90L, (long)f.Stack[2]);
        }

        [Fact]
        public async Task Case_ANSExample()
        {
            // Simplified ANS Forth test example
            var f = New();
            var code = @": CS1 
                CASE 
                    1 OF 111 ENDOF
                    2 OF 222 ENDOF
                    3 OF 333 ENDOF
                    999
                ENDCASE ;";
            
            Assert.True(await f.EvalAsync(code));
            
            var f1 = New();
            Assert.True(await f1.EvalAsync(code));
            Assert.True(await f1.EvalAsync("1 CS1"));
            Assert.Equal(111L, (long)f1.Stack[0]);
            
            var f4 = New();
            Assert.True(await f4.EvalAsync(code));
            Assert.True(await f4.EvalAsync("4 CS1"));
            Assert.Equal(999L, (long)f4.Stack[0]);
        }
    }
}
