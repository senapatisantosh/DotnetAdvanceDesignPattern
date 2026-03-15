namespace DesignPatterns.Structural.Composite;

/// <summary>
/// Component interface — declares the common contract for both individual
/// permissions (leaves) and permission groups (composites).
/// Enables treating single permissions and groups uniformly.
/// </summary>
public interface IPermissionComponent
{
    /// <summary>Unique identifier for this permission or group.</summary>
    string Name { get; }

    /// <summary>Human-readable description.</summary>
    string Description { get; }

    /// <summary>
    /// Checks whether this component grants the specified permission.
    /// For leaves: exact match check.
    /// For composites: recursive search through children.
    /// </summary>
    bool HasPermission(string permissionName);

    /// <summary>
    /// Returns a flat list of all individual permission names contained
    /// in this component (recursively for composites).
    /// </summary>
    IReadOnlyList<string> GetAllPermissions();

    /// <summary>
    /// Displays the permission tree structure with indentation.
    /// </summary>
    void Display(int indentLevel = 0);
}
