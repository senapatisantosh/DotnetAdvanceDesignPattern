namespace DesignPatterns.Enterprise.Outbox;

public interface IOutboxStore
{
    Task SaveAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default);
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
}
