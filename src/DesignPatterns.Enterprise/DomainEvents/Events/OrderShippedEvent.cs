namespace DesignPatterns.Enterprise.DomainEvents.Events;

public sealed record OrderShippedEvent(
    Guid OrderId,
    string TrackingNumber,
    string Carrier,
    DateTime EstimatedDelivery) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
