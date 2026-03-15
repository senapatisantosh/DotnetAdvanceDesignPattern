using System.Collections.Concurrent;

namespace DesignPatterns.Enterprise.Outbox;

public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages = new();

    public Task SaveAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _messages[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        IReadOnlyList<OutboxMessage> result = _messages.Values
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(messageId, out var msg))
            msg.ProcessedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(Guid messageId, string error, CancellationToken ct = default)
    {
        if (_messages.TryGetValue(messageId, out var msg))
        {
            msg.RetryCount++;
            msg.LastError = error;
        }
        return Task.CompletedTask;
    }

    public Task<int> GetPendingCountAsync(CancellationToken ct = default) =>
        Task.FromResult(_messages.Values.Count(m => !m.IsProcessed));
}
