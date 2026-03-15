namespace DesignPatterns.Enterprise.Outbox;

/// <summary>
/// Abstraction over the actual message broker (RabbitMQ, Kafka, Azure Service Bus, etc.).
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(string eventType, string payload, CancellationToken ct = default);
}
