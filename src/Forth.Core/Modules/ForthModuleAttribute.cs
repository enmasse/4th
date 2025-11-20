namespace Forth.Core.Modules;

/// <summary>
/// Marks a class as providing Forth words under a specific module name, enabling
/// module-qualified lookups (e.g. MODULE:WORD) and search order management.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ForthModuleAttribute : Attribute
{
    /// <summary>
    /// Gets the declared module name for the attributed class.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes the attribute with a required module <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the module.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
    public ForthModuleAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
}
