namespace DesignPatterns.Structural.Proxy.VirtualProxy;

/// <summary>
/// Virtual Proxy — delays creation of the expensive <see cref="RealInventoryService"/>
/// until the first operation is actually invoked. Useful when the service requires
/// heavy initialization (database connections, API handshakes) that should be deferred.
/// </summary>
public sealed class LazyInventoryProxy : IInventoryService
{
    private readonly Lazy<IInventoryService> _lazyService;

    /// <summary>
    /// Creates a virtual proxy that defers real service creation.
    /// </summary>
    /// <param name="serviceFactory">Factory function invoked on first use.</param>
    public LazyInventoryProxy(Func<IInventoryService> serviceFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceFactory);
        _lazyService = new Lazy<IInventoryService>(serviceFactory);
    }

    /// <summary>Whether the real service has been instantiated yet.</summary>
    public bool IsInitialized => _lazyService.IsValueCreated;

    public Task<InventoryItem?> GetItemAsync(string sku) =>
        _lazyService.Value.GetItemAsync(sku);

    public Task<IReadOnlyList<InventoryItem>> GetAllItemsAsync() =>
        _lazyService.Value.GetAllItemsAsync();

    public Task<ReservationResult> ReserveAsync(string sku, int quantity) =>
        _lazyService.Value.ReserveAsync(sku, quantity);

    public Task<bool> ReleaseReservationAsync(string reservationId) =>
        _lazyService.Value.ReleaseReservationAsync(reservationId);
}
