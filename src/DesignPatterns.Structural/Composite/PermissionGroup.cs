namespace DesignPatterns.Structural.Composite;

/// <summary>
/// Composite — a group of permissions that can contain individual
/// permissions (leaves) and/or nested permission groups (composites).
/// Enables building hierarchical authorization structures like:
///   Admin -> OrderManagement -> [orders:read, orders:write, orders:delete]
///                            -> RefundManagement -> [refunds:create, refunds:approve]
/// </summary>
public sealed class PermissionGroup : IPermissionComponent
{
    private readonly List<IPermissionComponent> _children = [];

    public string Name { get; }
    public string Description { get; }

    /// <summary>Read-only view of immediate children.</summary>
    public IReadOnlyList<IPermissionComponent> Children => _children.AsReadOnly();

    public PermissionGroup(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public PermissionGroup Add(IPermissionComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        if (ContainsCycle(component))
            throw new InvalidOperationException(
                $"Adding '{component.Name}' to '{Name}' would create a circular reference.");

        _children.Add(component);
        return this; // Fluent API for easy tree building
    }

    public PermissionGroup Remove(IPermissionComponent component)
    {
        _children.Remove(component);
        return this;
    }

    /// <summary>
    /// Recursively searches all children (and their children) for the
    /// specified permission. This is the power of the Composite pattern —
    /// the caller doesn't need to know the tree structure.
    /// </summary>
    public bool HasPermission(string permissionName) =>
        _children.Any(child => child.HasPermission(permissionName));

    /// <summary>
    /// Flattens the entire tree into a distinct list of permission names.
    /// </summary>
    public IReadOnlyList<string> GetAllPermissions() =>
        _children
            .SelectMany(child => child.GetAllPermissions())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

    public void Display(int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        Console.WriteLine($"{indent}+ [{Name}] {Description}");
        foreach (var child in _children)
        {
            child.Display(indentLevel + 1);
        }
    }

    /// <summary>Prevents circular references when adding groups to each other.</summary>
    private bool ContainsCycle(IPermissionComponent component)
    {
        if (ReferenceEquals(this, component))
            return true;

        if (component is PermissionGroup group)
        {
            return group._children.Any(ContainsCycle);
        }

        return false;
    }
}
