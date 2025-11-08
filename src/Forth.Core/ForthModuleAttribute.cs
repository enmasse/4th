using System;

namespace Forth;

/// <summary>
/// Optional attribute to declare the Forth module name to scope registered words under.
/// If not present, words go into the core/global dictionary.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ForthModuleAttribute : Attribute
{
    public string Name { get; }
    public ForthModuleAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
