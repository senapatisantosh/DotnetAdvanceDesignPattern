namespace DesignPatterns.Enterprise.Outbox;

/// <summary>
/// Background processor that polls the outbox for pending messages and publishes them
/// to the real message broker. On success, messages are marked as processed; on failure,
/// the retry count is incremented and the error is logged.
/// In production, this would run as a hosted service or scheduled job.
/// </summary>
public sealed class OutboxProcessor(IOutboxStore outboxStore, IMessagePublisher publisher, int maxRetries = 3)
{
    /// <summary>
    /// Processes a batch of pending outbox messages.
    /// Returns the number of successfully published messages.
    /// </summary>
    public async Task<int> ProcessBatchAsync(int batchSize = 10, CancellationToken ct = default)
    {
        var pending = await outboxStore.GetPendingAsync(batchSize, ct);
        var successCount = 0;

        foreach (var message in pending)
        {
            if (message.RetryCount >= maxRetries)
                continue; // Dead-letter; skip for now

            try
            {
                await publisher.PublishAsync(message.EventType, message.Payload, ct);
                await outboxStore.MarkProcessedAsync(message.Id, ct);
                successCount++;
            }
            catch (Exception ex)
            {
                await outboxStore.MarkFailedAsync(message.Id, ex.Message, ct);
            }
        }

        return successCount;
    }
}
