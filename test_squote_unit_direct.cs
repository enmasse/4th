using Forth.Core.Interpreter;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var forth = new ForthInterpreter();
        
        Console.WriteLine("Testing: S\" test\"");
        
        try
        {
            var result = await forth.EvalAsync("S\" test\"");
            Console.WriteLine($"EvalAsync returned: {result}");
            Console.WriteLine($"Stack count: {forth.Stack.Count}");
            
            if (forth.Stack.Count >= 2)
            {
                var addr = (long)forth.Stack[0];
                var len = (long)forth.Stack[1];
                Console.WriteLine($"Address: {addr}, Length: {len}");
                
                var str = forth.ReadMemoryString(addr, len);
                Console.WriteLine($"String: {str}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
        }
    }
}
