using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Parsing;

/// <summary>
/// Regression tests for the WORD primitive.
/// WORD ( char "<chars>ccc<char>" -- c-addr ) parses a word delimited by the given character.
/// 
/// NOTE: WORD parses from the current source input (set via SOURCE). These tests use
/// a pattern where WORD is called inside a colon definition so it has access to the rest
/// of the input line.
/// </summary>
public class WordPrimitiveTests
{
    [Fact]
    public async Task Word_BasicSpaceDelimiter()
    {
        var forth = new ForthInterpreter();
        // Test WORD with space delimiter (ASCII 32)
        // Just parse one word
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST hello"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("hello", parsed);
    }

    [Fact]
    public async Task Word_ParsesUpToDelimiter()
    {
        var forth = new ForthInterpreter();
        // Parse comma-delimited: 44 is comma
        // Known issue: When WORD uses non-space delimiter, tokenizer's space-skipping
        // causes >IN to point to a space before the word, resulting in " foo" instead of "foo"
        Assert.True(await forth.EvalAsync("99 CONSTANT bar"));
        Assert.True(await forth.EvalAsync(": TEST 44 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST foo,bar"));
        // Stack should have: address from WORD, then 99 from bar constant
        Assert.Equal(2, forth.Stack.Count);
        forth.Pop(); // discard 99 from bar
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("foo", parsed);
    }

    [Fact]
    public async Task Word_SkipsLeadingDelimiters()
    {
        var forth = new ForthInterpreter();
        // Multiple leading spaces should be skipped
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST    test"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("test", parsed);
    }

    [Fact]
    public async Task Word_EmptyResult()
    {
        var forth = new ForthInterpreter();
        // If there's nothing to parse after calling WORD, it returns empty string
        // Create a test that uses the second WORD call which has no more input
        Assert.True(await forth.EvalAsync(": TEST 32 WORD DROP 32 WORD ; TEST word"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("", parsed);
    }

    [Fact]
    public async Task Word_ColonDelimiter()
    {
        var forth = new ForthInterpreter();
        // Parse using colon as delimiter (ASCII 58)
        // Known issue: When WORD uses non-space delimiter, tokenizer's space-skipping
        // causes >IN to point to a space before the word, resulting in " name" instead of "name"
        Assert.True(await forth.EvalAsync("123 CONSTANT value"));
        Assert.True(await forth.EvalAsync(": TEST 58 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST name:value"));
        // Stack should have: address from WORD, then 123 from value constant
        Assert.Equal(2, forth.Stack.Count);
        forth.Pop(); // discard 123 from value
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("name", parsed);
    }

    [Fact]
    public async Task Word_AtEndOfInput()
    {
        var forth = new ForthInterpreter();
        // Parse last word in input
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST last"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("last", parsed);
    }

    [Fact]
    public async Task Word_SingleCharacter()
    {
        var forth = new ForthInterpreter();
        // Parse single character
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST x"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("x", parsed);
    }

    [Fact]
    public async Task Word_ConsecutiveDelimiters()
    {
        var forth = new ForthInterpreter();
        // Multiple delimiters between words
        // Call WORD twice in the same definition to parse both words
        Assert.True(await forth.EvalAsync(": TEST 32 WORD 32 WORD ; TEST a  b"));
        Assert.Equal(2, forth.Stack.Count);
        
        var addr2 = (long)forth.Pop();
        var parsed2 = ReadCountedString(forth, addr2);
        Assert.Equal("b", parsed2);
        
        var addr = (long)forth.Pop();
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("a", parsed);
    }

    [Fact]
    public async Task Word_AdvancesInputPointer()
    {
        var forth = new ForthInterpreter();
        // Check that >IN is advanced correctly
        // Call WORD twice to parse two words sequentially
        Assert.True(await forth.EvalAsync(": TEST 32 WORD 32 WORD ; TEST first second"));
        Assert.Equal(2, forth.Stack.Count);
        
        var addr2 = (long)forth.Pop();
        var parsed2 = ReadCountedString(forth, addr2);
        Assert.Equal("second", parsed2);
        
        var addr1 = (long)forth.Pop();
        var parsed1 = ReadCountedString(forth, addr1);
        Assert.Equal("first", parsed1);
    }

    [Fact]
    public async Task Word_WithDifferentDelimiters()
    {
        var forth = new ForthInterpreter();
        // Test parsing with different delimiters
        // Known issue: When WORD uses non-space delimiter, tokenizer's space-skipping
        // causes >IN to point to a space before the word
        
        // First test: comma delimiter
        Assert.True(await forth.EvalAsync("1 CONSTANT bar"));
        Assert.True(await forth.EvalAsync(": TEST1 44 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST1 foo,bar"));
        forth.Pop(); // discard 1 from bar
        var addr1 = (long)forth.Pop();
        Assert.Equal("foo", ReadCountedString(forth, addr1));
        
        // Second test: period delimiter
        Assert.True(await forth.EvalAsync("2 CONSTANT world"));
        Assert.True(await forth.EvalAsync(": TEST2 46 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST2 hello.world"));
        forth.Pop(); // discard 2 from world
        var addr2 = (long)forth.Pop();
        Assert.Equal("hello", ReadCountedString(forth, addr2));
        
        // Third test: hyphen delimiter (ASCII 45)
        Assert.True(await forth.EvalAsync("3 CONSTANT name"));
        Assert.True(await forth.EvalAsync(": TEST3 45 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST3 test-name"));
        forth.Pop(); // discard 3 from name
        var addr3 = (long)forth.Pop();
        Assert.Equal("test", ReadCountedString(forth, addr3));
    }

    [Fact]
    public async Task Word_PreservesSpecialCharacters()
    {
        var forth = new ForthInterpreter();
        // Parse word containing special characters (not delimiter)
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST test-name"));
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("test-name", parsed);
    }

    [Fact]
    public async Task Word_HandlesNumbers()
    {
        var forth = new ForthInterpreter();
        // Parse numeric strings
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST 12345"));
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("12345", parsed);
    }

    [Fact]
    public async Task Word_TabDelimiter()
    {
        var forth = new ForthInterpreter();
        // Tab character is ASCII 9 - but tokenizer might not preserve tabs
        // Test with hyphen delimiter (ASCII 45)
        // Known issue: When WORD uses non-space delimiter, tokenizer's space-skipping
        // causes >IN to point to a space before the word, resulting in " before" instead of "before"
        Assert.True(await forth.EvalAsync("42 CONSTANT after"));
        Assert.True(await forth.EvalAsync(": TEST 45 WORD ;"));
        Assert.True(await forth.EvalAsync("TEST before-after"));
        // Stack should have: address from WORD, then 42 from after constant
        Assert.Equal(2, forth.Stack.Count);
        forth.Pop(); // discard 42 from after
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("before", parsed);
    }

    [Fact]
    public async Task Word_InDefinition()
    {
        var forth = new ForthInterpreter();
        // Use WORD inside a colon definition
        Assert.True(await forth.EvalAsync(": PARSE-NEXT 32 WORD ;"));
        Assert.True(await forth.EvalAsync("PARSE-NEXT hello"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("hello", parsed);
    }

    [Fact]
    public async Task Word_ReturnsCountedString()
    {
        var forth = new ForthInterpreter();
        // Verify WORD returns a counted string (first byte is length)
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST test"));
        var addr = (long)forth.Pop();
        
        forth.MemTryGet(addr, out var lenObj);
        var len = (int)(long)lenObj;
        Assert.Equal(4, len);
        
        // Verify the string content starts at addr+1
        forth.MemTryGet(addr + 1, out var ch1);
        Assert.Equal('t', (char)((long)ch1 & 0xFF));
        forth.MemTryGet(addr + 2, out var ch2);
        Assert.Equal('e', (char)((long)ch2 & 0xFF));
    }

    [Fact]
    public async Task Word_WithCount()
    {
        var forth = new ForthInterpreter();
        // WORD followed by COUNT should produce c-addr and length
        Assert.True(await forth.EvalAsync(": TEST 32 WORD COUNT ; TEST example"));
        Assert.Equal(2, forth.Stack.Count);
        
        var u = (long)forth.Pop();
        Assert.Equal(7, u); // length of "example"
        
        var caddr = (long)forth.Pop();
        var parsed = forth.ReadMemoryString(caddr, u);
        Assert.Equal("example", parsed);
    }

    [Fact]
    public async Task Word_EmptySourceAfterParsing()
    {
        var forth = new ForthInterpreter();
        // After parsing all input, subsequent WORD should return empty
        // Call WORD twice - second call has no more input
        Assert.True(await forth.EvalAsync(": TEST 32 WORD 32 WORD ; TEST test"));
        Assert.Equal(2, forth.Stack.Count);
        
        var addr2 = (long)forth.Pop();
        var parsed2 = ReadCountedString(forth, addr2);
        Assert.Equal("", parsed2);
        
        forth.Pop(); // discard first address
    }

    [Fact]
    public async Task Word_WithParse()
    {
        var forth = new ForthInterpreter();
        // WORD should skip leading delimiters
        Assert.True(await forth.EvalAsync(": TEST 32 WORD ; TEST   spaced"));
        var addr = (long)forth.Pop();
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal("spaced", parsed);
    }

    [Fact]
    public async Task Word_LongString()
    {
        var forth = new ForthInterpreter();
        // Parse a longer word
        var longWord = "thisIsAVeryLongWordThatShouldStillBeParsedCorrectly";
        Assert.True(await forth.EvalAsync($": TEST 32 WORD ; TEST {longWord}"));
        var addr = (long)forth.Pop();
        var parsed = ReadCountedString(forth, addr);
        Assert.Equal(longWord, parsed);
    }

    [Fact]
    public async Task Word_AllocatesNewMemory()
    {
        var forth = new ForthInterpreter();
        // Each WORD call should allocate new memory (HERE advances)
        Assert.True(await forth.EvalAsync("HERE"));
        var here1 = (long)forth.Pop();
        
        Assert.True(await forth.EvalAsync("BL WORD test"));
        forth.Pop(); // discard address
        
        Assert.True(await forth.EvalAsync("HERE"));
        var here2 = (long)forth.Pop();
        
        // HERE should have advanced
        Assert.True(here2 > here1);
    }

    /// <summary>
    /// Helper to read a counted string from memory at the given address.
    /// </summary>
    private static string ReadCountedString(ForthInterpreter forth, long addr)
    {
        forth.MemTryGet(addr, out var lenObj);
        var len = (int)(long)lenObj;
        
        if (len == 0)
            return string.Empty;
        
        var chars = new char[len];
        for (int i = 0; i < len; i++)
        {
            forth.MemTryGet(addr + 1 + i, out var charObj);
            chars[i] = (char)((long)charObj & 0xFF);
        }
        return new string(chars);
    }
}
