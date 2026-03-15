namespace DesignPatterns.Behavioral.Command;

/// <summary>
/// The Receiver — an order entity that commands act upon.
/// </summary>
public sealed class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string CustomerName { get; init; }
    public required List<string> Items { get; init; }
    public OrderStatus Status { get; private set; } = OrderStatus.None;
    public string ShippingAddress { get; private set; } = string.Empty;
    public decimal TotalAmount { get; init; }

    private readonly List<string> _auditLog = [];
    public IReadOnlyList<string> AuditLog => _auditLog.AsReadOnly();

    public void Place()
    {
        if (Status != OrderStatus.None)
            throw new InvalidOperationException($"Cannot place order in '{Status}' status.");

        Status = OrderStatus.Placed;
        _auditLog.Add($"Order placed for {CustomerName} with {Items.Count} item(s).");
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Placed or OrderStatus.Processing))
            throw new InvalidOperationException($"Cannot cancel order in '{Status}' status.");

        Status = OrderStatus.Cancelled;
        _auditLog.Add("Order cancelled.");
    }

    public void Restore(OrderStatus previousStatus)
    {
        Status = previousStatus;
        _auditLog.Add($"Order restored to '{previousStatus}' status.");
    }

    public string UpdateShipping(string newAddress)
    {
        var previousAddress = ShippingAddress;
        ShippingAddress = newAddress;
        _auditLog.Add($"Shipping address updated to '{newAddress}'.");
        return previousAddress;
    }
}

public enum OrderStatus
{
    None,
    Placed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
