using DesignPatterns.Enterprise.DomainEvents.Events;

namespace DesignPatterns.Enterprise.DomainEvents.Handlers;

/// <summary>
/// Records an audit trail entry when a payment is received.
/// </summary>
public sealed class PaymentReceivedAuditHandler : IDomainEventHandler<PaymentReceivedEvent>
{
    private readonly List<AuditEntry> _auditLog = new();

    public IReadOnlyList<AuditEntry> AuditLog => _auditLog.AsReadOnly();

    public Task HandleAsync(PaymentReceivedEvent domainEvent, CancellationToken ct = default)
    {
        _auditLog.Add(new AuditEntry(
            domainEvent.EventId,
            $"Payment of {domainEvent.Amount:F2} {domainEvent.Currency} received for order {domainEvent.OrderId} " +
            $"via {domainEvent.PaymentMethod} (txn: {domainEvent.TransactionId})",
            domainEvent.OccurredAt));

        return Task.CompletedTask;
    }
}

public sealed record AuditEntry(Guid EventId, string Description, DateTime Timestamp);
