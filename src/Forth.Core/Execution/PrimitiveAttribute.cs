using System;

namespace Forth.Core.Execution
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class PrimitiveAttribute : Attribute
    {
        public string Name { get; }
        public bool IsImmediate { get; set; }
        public string? Module { get; set; }
        public bool IsAsync { get; set; }

        public PrimitiveAttribute(string name)
        {
            Name = name;
        }
    }
}
