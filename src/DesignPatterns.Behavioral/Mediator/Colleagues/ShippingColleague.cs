namespace DesignPatterns.Behavioral.Mediator.Colleagues;

/// <summary>
/// Handles shipping label creation and scheduling.
/// </summary>
public sealed class ShippingColleague
{
    private readonly List<ShipmentRecord> _shipments = [];

    public IReadOnlyList<ShipmentRecord> Shipments => _shipments.AsReadOnly();

    public Task<(bool Success, string TrackingNumber, string Message)> CreateShipmentAsync(
        Guid orderId, string shippingAddress, List<CheckoutItem> items)
    {
        var trackingNumber = $"SHIP-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        var totalItems = items.Sum(i => i.Quantity);

        _shipments.Add(new ShipmentRecord(orderId, shippingAddress, trackingNumber, totalItems));

        return Task.FromResult((true, trackingNumber,
            $"Shipment created with tracking {trackingNumber} for {totalItems} item(s)."));
    }
}

public sealed record ShipmentRecord(
    Guid OrderId,
    string Address,
    string TrackingNumber,
    int ItemCount);
