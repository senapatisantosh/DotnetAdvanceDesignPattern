using System.Text.Json;

namespace DesignPatterns.Enterprise.Outbox;

/// <summary>
/// Instead of publishing directly to the message broker, this publisher writes to the outbox.
/// The business transaction and the outbox write share the same database transaction,
/// guaranteeing that if the business operation succeeds, the message will eventually be published.
/// </summary>
public sealed class OutboxPublisher(IOutboxStore outboxStore)
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : class
    {
        var message = new OutboxMessage
        {
            EventType = typeof(TEvent).FullName ?? typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(domainEvent)
        };

        await outboxStore.SaveAsync(message, ct);
    }
}
