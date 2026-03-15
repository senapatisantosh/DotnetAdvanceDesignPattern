namespace DesignPatterns.Enterprise.DomainEvents.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    string CustomerId,
    IReadOnlyList<OrderLineItem> Items,
    decimal TotalAmount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record OrderLineItem(string Sku, string ProductName, int Quantity, decimal UnitPrice);
