namespace DesignPatterns.Enterprise.DomainEvents;

/// <summary>
/// Reacts to a specific domain event. Multiple handlers can subscribe to the same event type
/// to implement cross-cutting concerns (notifications, projections, auditing).
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
