using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: word management, dictionary operations
public partial class ForthInterpreter
{
    // definition order tracking for classic FORGET
    internal readonly List<DefinitionRecord> _definitions = new();
    internal int _baselineCount;

    internal readonly struct DefinitionRecord
    {
        public readonly string Name;
        public readonly string? Module;
        public DefinitionRecord(string name, string? module)
        {
            Name = name;
            Module = module;
        }
    }
}