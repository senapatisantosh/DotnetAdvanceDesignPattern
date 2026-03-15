namespace DesignPatterns.Enterprise.DomainEvents.Events;

public sealed record PaymentReceivedEvent(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string TransactionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
