using DesignPatterns.Enterprise.DomainEvents.Events;

namespace DesignPatterns.Enterprise.DomainEvents.Handlers;

/// <summary>
/// Sends a confirmation email when an order is placed.
/// In production this would integrate with an email service.
/// </summary>
public sealed class OrderPlacedEmailHandler : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly List<string> _sentEmails = new();

    public IReadOnlyList<string> SentEmails => _sentEmails.AsReadOnly();

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
    {
        var message = $"Order confirmation for {domainEvent.OrderId} sent to customer {domainEvent.CustomerId}. " +
                      $"Total: ${domainEvent.TotalAmount:F2}, Items: {domainEvent.Items.Count}";
        _sentEmails.Add(message);
        return Task.CompletedTask;
    }
}
