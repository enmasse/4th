// Quick diagnostic for REFILL SOURCE length issue
using Forth.Core;
using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;

var io = new RefillTestIO();
io.AddLine("hello world");
var forth = new ForthInterpreter(io);

// REFILL then inspect SOURCE
await forth.EvalAsync("REFILL DROP");
await forth.EvalAsync("SOURCE");

Console.WriteLine($"Stack count: {forth.Stack.Count}");
var addr = (long)forth.Stack[0];
var len = (long)forth.Stack[1];

Console.WriteLine($"Address: {addr}");
Console.WriteLine($"Length: {len}");

// Read the actual string from memory
var str = forth.ReadMemoryString(addr, len);
Console.WriteLine($"String: '{str}'");
Console.WriteLine($"String.Length: {str.Length}");

// Check each character
Console.WriteLine("Characters:");
for (int i = 0; i < len; i++)
{
    forth.MemTryGet(addr + i, out var ch);
    Console.WriteLine($"  [{i}]: {(int)ch} = '{(char)ch}' (0x{(int)ch:X2})");
}

// Helper IO class
class RefillTestIO : IForthIO
{
    private readonly Queue<string> _lines = new();
    public void AddLine(string line) => _lines.Enqueue(line);
    public void Print(string text) { }
    public void PrintNumber(long number) { }
    public void NewLine() { }
    public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
    public int ReadKey() => -1;
    public bool KeyAvailable() => false;
}
