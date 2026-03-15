namespace DesignPatterns.Enterprise.NullObject;

/// <summary>
/// Business service that uses IAuditLogger. It never checks for null —
/// when auditing is disabled, the NullAuditLogger silently absorbs calls.
/// </summary>
public sealed class OrderService(IAuditLogger auditLogger)
{
    private readonly Dictionary<Guid, Order> _orders = new();

    public async Task<Guid> PlaceOrderAsync(string customerId, decimal total)
    {
        var orderId = Guid.NewGuid();
        _orders[orderId] = new Order(orderId, customerId, total, OrderStatus.Placed);

        // No null check needed — NullAuditLogger handles the "off" case
        await auditLogger.LogAsync(customerId, "PlaceOrder", "Order", orderId.ToString(),
            $"Total: ${total:F2}");

        return orderId;
    }

    public async Task CancelOrderAsync(Guid orderId, string userId)
    {
        if (!_orders.TryGetValue(orderId, out var order))
            throw new KeyNotFoundException($"Order {orderId} not found.");

        _orders[orderId] = order with { Status = OrderStatus.Cancelled };

        await auditLogger.LogAsync(userId, "CancelOrder", "Order", orderId.ToString());
    }

    public Order? GetOrder(Guid orderId) =>
        _orders.TryGetValue(orderId, out var order) ? order : null;
}

public sealed record Order(Guid Id, string CustomerId, decimal Total, OrderStatus Status);

public enum OrderStatus { Placed, Paid, Shipped, Cancelled }
