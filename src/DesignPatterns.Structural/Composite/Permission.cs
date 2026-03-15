namespace DesignPatterns.Structural.Composite;

/// <summary>
/// Leaf — represents a single, atomic permission (e.g., "orders:read", "inventory:write").
/// Cannot contain children.
/// </summary>
public sealed class Permission : IPermissionComponent
{
    public string Name { get; }
    public string Description { get; }

    public Permission(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public bool HasPermission(string permissionName) =>
        string.Equals(Name, permissionName, StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> GetAllPermissions() => [Name];

    public void Display(int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        Console.WriteLine($"{indent}- [{Name}] {Description}");
    }
}
