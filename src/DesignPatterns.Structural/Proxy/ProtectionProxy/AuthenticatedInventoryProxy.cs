namespace DesignPatterns.Structural.Proxy.ProtectionProxy;

/// <summary>
/// Protection Proxy — controls access to inventory operations based on
/// user roles and permissions. Read operations require "inventory:read",
/// write operations (reserve/release) require "inventory:write".
/// </summary>
public sealed class AuthenticatedInventoryProxy : IInventoryService
{
    private readonly IInventoryService _inner;
    private readonly UserContext _userContext;

    public AuthenticatedInventoryProxy(IInventoryService inner, UserContext userContext)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    public Task<InventoryItem?> GetItemAsync(string sku)
    {
        EnsurePermission("inventory:read");
        return _inner.GetItemAsync(sku);
    }

    public Task<IReadOnlyList<InventoryItem>> GetAllItemsAsync()
    {
        EnsurePermission("inventory:read");
        return _inner.GetAllItemsAsync();
    }

    public Task<ReservationResult> ReserveAsync(string sku, int quantity)
    {
        EnsurePermission("inventory:write");
        return _inner.ReserveAsync(sku, quantity);
    }

    public Task<bool> ReleaseReservationAsync(string reservationId)
    {
        EnsurePermission("inventory:write");
        return _inner.ReleaseReservationAsync(reservationId);
    }

    private void EnsurePermission(string requiredPermission)
    {
        if (!_userContext.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        if (!_userContext.HasPermission(requiredPermission))
            throw new UnauthorizedAccessException(
                $"User '{_userContext.Username}' lacks required permission: '{requiredPermission}'.");
    }
}

/// <summary>
/// Represents the current user's identity and permissions.
/// </summary>
public sealed class UserContext
{
    public string Username { get; }
    public bool IsAuthenticated { get; }
    private readonly HashSet<string> _permissions;

    public UserContext(string username, bool isAuthenticated, IEnumerable<string> permissions)
    {
        Username = username;
        IsAuthenticated = isAuthenticated;
        _permissions = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission) => _permissions.Contains(permission);

    public static UserContext Anonymous => new("anonymous", false, []);

    public static UserContext ReadOnlyUser(string username) =>
        new(username, true, ["inventory:read"]);

    public static UserContext FullAccessUser(string username) =>
        new(username, true, ["inventory:read", "inventory:write"]);
}
