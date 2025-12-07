using Forth.Core.Interpreter;
using System;

var tokens = Tokenizer.Tokenize("s\" HELLO\"");
Console.WriteLine($"Token count: {tokens.Count}");
for (int i = 0; i < tokens.Count; i++)
{
    Console.WriteLine($"Token {i}: '{tokens[i]}'");
}
