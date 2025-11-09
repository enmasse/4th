using System;

namespace Forth;

/// <summary>
/// Optional attribute to declare the Forth module name to scope registered words under.
/// If not present, words go into the core/global dictionary.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ForthModuleAttribute : Attribute
{
    /// <summary>
    /// Gets the declared module name used to scope registered words.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForthModuleAttribute"/> class.
    /// </summary>
    /// <param name="name">The module name under which words will be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    public ForthModuleAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
