namespace Forth.Core.Modules;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ForthModuleAttribute : Attribute
{
    public string Name { get; }
    public ForthModuleAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
}
