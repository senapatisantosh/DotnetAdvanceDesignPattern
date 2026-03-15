namespace DesignPatterns.Structural.Proxy;

/// <summary>
/// Subject interface — defines the contract for inventory operations.
/// Both the real service and all proxy types implement this interface,
/// so proxies can be used interchangeably with the real service.
/// </summary>
public interface IInventoryService
{
    Task<InventoryItem?> GetItemAsync(string sku);
    Task<IReadOnlyList<InventoryItem>> GetAllItemsAsync();
    Task<ReservationResult> ReserveAsync(string sku, int quantity);
    Task<bool> ReleaseReservationAsync(string reservationId);
}
