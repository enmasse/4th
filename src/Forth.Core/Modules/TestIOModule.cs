using Forth.Core.Execution;
using Forth.Core.Interpreter;
using System.Text;

namespace Forth.Core.Modules
{
    /// <summary>
    /// Provides Forth words for testing input/output operations,
    /// including setting simulated input lines and bytes.
    /// </summary>
    [ForthModule("TEST-IO")]
    public class TestIOModule : IForthWordModule
    {
        /// <summary>
        /// Registers the words provided by this module into the specified Forth interpreter.
        /// </summary>
        /// <param name="interp">The Forth interpreter instance to register words with.</param>
        public void Register(IForthInterpreter interp)
        {
            if (interp is not ForthInterpreter impl) return;
            var w = new Word(Prim_AddInputLine) { Name = "ADD-INPUT-LINE", Module = "TEST-IO" };
            impl._dict = impl._dict.SetItem(("TEST-IO", "ADD-INPUT-LINE"), w);
            impl.RegisterDefinition("ADD-INPUT-LINE");
        }

        /// <summary>
        /// Enqueue a simulated input line for subsequent ACCEPT / READ-LINE processing.
        /// Supports forms:
        /// ( addr u ) – address/length pair
        /// counted-addr – address returned by S" (length cell followed by chars)
        /// string – direct string literal pushed by S
        /// Additional items below the top of stack are ignored.
        /// </summary>
        [Primitive("ADD-INPUT-LINE", Module = "TEST-IO", HelpString = "ADD-INPUT-LINE ( addr u | counted-addr | string -- ) - enqueue simulated input line")]
        public static Task Prim_AddInputLine(ForthInterpreter i)
        {
            if (i.Stack.Count == 0)
                throw new ForthException(ForthErrorCode.StackUnderflow, "ADD-INPUT-LINE stack empty");

            static bool IsNumeric(object o) => o is long || o is int || o is short || o is byte;

            var top = i.Stack[^1];

            // Form 1: direct string literal ( string -- )
            if (top is string sLine)
            {
                i.PopInternal();
                if (i.IO is TestIO tioStr) { tioStr.AddInputLine(sLine); return Task.CompletedTask; }
                throw new ForthException(ForthErrorCode.TypeError, "ADD-INPUT-LINE requires TestIO");
            }

            // Prefer (addr u) pair when top is numeric and second is address or string.
            if (i.Stack.Count >= 2 && IsNumeric(top) && (IsNumeric(i.Stack[^2]) || i.Stack[^2] is string))
            {
                var uObj = i.PopInternal();
                var addrOrStrObj = i.PopInternal();
                long u = ForthInterpreter.ToLong(uObj);
                if (u < 0) throw new ForthException(ForthErrorCode.TypeError, "Negative length for ADD-INPUT-LINE");
                string line;
                if (addrOrStrObj is string direct)
                {
                    line = u <= direct.Length ? direct.Substring(0, (int)u) : direct;
                }
                else
                {
                    long addr = ForthInterpreter.ToLong(addrOrStrObj);
                    var sb = new StringBuilder();
                    for (long k = 0; k < u; k++) { i.MemTryGet(addr + k, out var v); sb.Append((char)(ForthInterpreter.ToLong(v) & 0xFF)); }
                    line = sb.ToString();
                }
                if (i.IO is TestIO tioPair) { tioPair.AddInputLine(line); return Task.CompletedTask; }
                throw new ForthException(ForthErrorCode.TypeError, "ADD-INPUT-LINE requires TestIO");
            }

            // Prioritize counted string detection: numeric top whose cell holds a plausible length
            if (IsNumeric(top))
            {
                long addrCandidate = ForthInterpreter.ToLong(top);
                i.MemTryGet(addrCandidate, out var lenCell);
                long len = ForthInterpreter.ToLong(lenCell);
                if (len >= 0 && len < 1000)
                {
                    var sbc = new StringBuilder();
                    for (long k = 0; k < len; k++) { i.MemTryGet(addrCandidate + 1 + k, out var v); sbc.Append((char)(ForthInterpreter.ToLong(v) & 0xFF)); }
                    i.PopInternal();
                    if (i.IO is TestIO tioCounted) { tioCounted.AddInputLine(sbc.ToString()); return Task.CompletedTask; }
                    throw new ForthException(ForthErrorCode.TypeError, "ADD-INPUT-LINE requires TestIO");
                }
            }

            throw new ForthException(ForthErrorCode.TypeError, "ADD-INPUT-LINE unsupported stack pattern");
        }
    }

    /// <summary>
    /// Provides a test implementation of <see cref="IForthIO"/> for simulating input and output operations
    /// in Forth interpreter unit tests. Allows setting input lines and captures output to the console.
    /// </summary>
    public class TestIO : IForthIO
    {
        private string _input = "";
        /// <summary>
        /// Initializes a new instance of the <see cref="TestIO"/> class with optional input lines.
        /// </summary>
        /// <param name="keys">Optional collection of key codes to simulate input.</param>
        /// <param name="lines">Optional collection of input lines to simulate.</param>
        /// <param name="keyAvailable">Whether a key is available.</param>
        public TestIO(IEnumerable<int>? keys = null, IEnumerable<string?>? lines = null, bool keyAvailable = false)
        {
            if (keys != null) foreach (var k in keys) _input += (char)k;
            if (lines != null) foreach (var l in lines) if (l != null) AddInputLine(l);
        }
        /// <summary>
        /// Prints the specified text to the console output.
        /// </summary>
        /// <param name="text">The text to print.</param>
        public void Print(string text) { Console.Write(text); }
        /// <summary>
        /// Prints the specified number to the console output.
        /// </summary>
        public void PrintNumber(long number) { Console.Write(number.ToString()); }
        /// <summary>
        /// Writes a newline character to the console output.
        /// </summary>
        public void NewLine() { Console.WriteLine(); }
        /// <summary>
        /// Reads the next input line from the simulated input queue.
        /// Returns an empty string if no lines are available.
        /// </summary>
        public string? ReadLine()
        {
            if (string.IsNullOrEmpty(_input)) return null;
            var idx = _input.IndexOf('\n');
            if (idx == -1)
            {
                var res = _input;
                _input = "";
                return res;
            }
            else
            {
                var res = _input[..idx];
                _input = _input[(idx + 1)..];
                return res;
            }
        }
        /// <summary>
        /// Reads a key from the input. Returns -1 to indicate no key is available in test mode.
        /// </summary>
        public int ReadKey()
        {
            if (string.IsNullOrEmpty(_input)) return -1;
            var ch = _input[0];
            _input = _input[1..];
            return ch;
        }
        /// <summary>
        /// Indicates whether a key is available for reading in test mode.
        /// Always returns false in this test implementation.
        /// </summary>
        public bool KeyAvailable() => !string.IsNullOrEmpty(_input);
        /// <summary>
        /// Sets the input lines for the simulated input queue.
        /// Replaces any existing lines with the provided collection.
        /// </summary>
        /// <param name="line">The collection of input lines to set.</param>
        public void AddInputLine(string line)
        {
            _input += line + "\n";
        }
    }
}
