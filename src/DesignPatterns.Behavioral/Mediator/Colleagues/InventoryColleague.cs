namespace DesignPatterns.Behavioral.Mediator.Colleagues;

/// <summary>
/// Handles inventory reservation and validation during checkout.
/// Does not know about payment, shipping, or notification — only talks via mediator.
/// </summary>
public sealed class InventoryColleague
{
    private readonly Dictionary<string, int> _stock = new();
    private readonly List<(string ProductId, int Quantity)> _reservations = [];

    public InventoryColleague(Dictionary<string, int>? initialStock = null)
    {
        if (initialStock is not null)
            _stock = new Dictionary<string, int>(initialStock);
    }

    public IReadOnlyList<(string ProductId, int Quantity)> Reservations => _reservations.AsReadOnly();

    public Task<(bool Success, string Message)> ReserveItemsAsync(List<CheckoutItem> items)
    {
        foreach (var item in items)
        {
            var available = _stock.GetValueOrDefault(item.ProductId, 0);
            if (available < item.Quantity)
            {
                return Task.FromResult((false,
                    $"Insufficient stock for '{item.ProductName}': requested {item.Quantity}, available {available}."));
            }
        }

        // Reserve all items
        foreach (var item in items)
        {
            _stock[item.ProductId] -= item.Quantity;
            _reservations.Add((item.ProductId, item.Quantity));
        }

        return Task.FromResult((true, "All items reserved successfully."));
    }

    public Task ReleaseReservationAsync(List<CheckoutItem> items)
    {
        foreach (var item in items)
        {
            _stock[item.ProductId] = _stock.GetValueOrDefault(item.ProductId, 0) + item.Quantity;
        }
        return Task.CompletedTask;
    }
}
