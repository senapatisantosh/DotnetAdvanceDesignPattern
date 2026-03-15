namespace DesignPatterns.Structural.Composite;

/// <summary>
/// Client code that works with the permission tree through the
/// <see cref="IPermissionComponent"/> interface — it doesn't need to
/// know whether it's dealing with a single permission or a deeply nested group.
/// </summary>
public sealed class PermissionEvaluator
{
    private readonly IPermissionComponent _rootPermissions;

    public PermissionEvaluator(IPermissionComponent rootPermissions)
    {
        _rootPermissions = rootPermissions ?? throw new ArgumentNullException(nameof(rootPermissions));
    }

    /// <summary>
    /// Checks if the user (represented by their root permission component)
    /// has the specified permission anywhere in their permission tree.
    /// </summary>
    public bool IsAuthorized(string requiredPermission) =>
        _rootPermissions.HasPermission(requiredPermission);

    /// <summary>
    /// Checks if the user has ALL of the specified permissions.
    /// </summary>
    public bool IsAuthorizedForAll(params string[] requiredPermissions) =>
        requiredPermissions.All(_rootPermissions.HasPermission);

    /// <summary>
    /// Checks if the user has ANY of the specified permissions.
    /// </summary>
    public bool IsAuthorizedForAny(params string[] requiredPermissions) =>
        requiredPermissions.Any(_rootPermissions.HasPermission);

    /// <summary>
    /// Returns all permissions the user is missing from the required set.
    /// Useful for generating meaningful "access denied" messages.
    /// </summary>
    public IReadOnlyList<string> GetMissingPermissions(params string[] requiredPermissions)
    {
        var granted = new HashSet<string>(
            _rootPermissions.GetAllPermissions(),
            StringComparer.OrdinalIgnoreCase);

        return requiredPermissions
            .Where(p => !granted.Contains(p))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns a flat, distinct list of every permission name available.
    /// </summary>
    public IReadOnlyList<string> GetAllGrantedPermissions() =>
        _rootPermissions.GetAllPermissions();

    /// <summary>
    /// Builds a common e-commerce role hierarchy for demonstration.
    /// </summary>
    public static IPermissionComponent BuildSampleHierarchy()
    {
        // Individual permissions (leaves)
        var ordersRead = new Permission("orders:read", "View orders");
        var ordersWrite = new Permission("orders:write", "Create/update orders");
        var ordersDelete = new Permission("orders:delete", "Cancel orders");
        var inventoryRead = new Permission("inventory:read", "View inventory levels");
        var inventoryWrite = new Permission("inventory:write", "Adjust inventory");
        var refundsCreate = new Permission("refunds:create", "Initiate refunds");
        var refundsApprove = new Permission("refunds:approve", "Approve refund requests");
        var reportsRead = new Permission("reports:read", "View reports");
        var reportsExport = new Permission("reports:export", "Export report data");
        var usersManage = new Permission("users:manage", "Create/modify user accounts");
        var settingsManage = new Permission("settings:manage", "Modify system settings");

        // Compose groups (composites)
        var orderManagement = new PermissionGroup("OrderManagement", "Full order lifecycle")
            .Add(ordersRead).Add(ordersWrite).Add(ordersDelete);

        var refundManagement = new PermissionGroup("RefundManagement", "Refund processing")
            .Add(refundsCreate).Add(refundsApprove);

        var inventoryManagement = new PermissionGroup("InventoryManagement", "Inventory control")
            .Add(inventoryRead).Add(inventoryWrite);

        var reporting = new PermissionGroup("Reporting", "Business intelligence")
            .Add(reportsRead).Add(reportsExport);

        var systemAdmin = new PermissionGroup("SystemAdmin", "System administration")
            .Add(usersManage).Add(settingsManage);

        // Higher-level role groups
        var customerServiceRole = new PermissionGroup("CustomerServiceRole", "Customer service team permissions")
            .Add(ordersRead).Add(refundsCreate).Add(inventoryRead);

        var managerRole = new PermissionGroup("ManagerRole", "Department manager permissions")
            .Add(orderManagement).Add(refundManagement).Add(inventoryManagement).Add(reporting);

        var adminRole = new PermissionGroup("AdminRole", "Full system administrator")
            .Add(managerRole).Add(systemAdmin);

        return adminRole;
    }
}
