namespace DesignPatterns.Enterprise.DomainEvents;

/// <summary>
/// Something meaningful that happened in the domain.
/// Events are immutable facts recorded with a timestamp.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
