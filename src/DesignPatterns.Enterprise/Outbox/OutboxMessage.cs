namespace DesignPatterns.Enterprise.Outbox;

/// <summary>
/// A message stored in the outbox table. It remains in "Pending" state until the
/// background processor publishes it and marks it as "Processed".
/// This guarantees at-least-once delivery even if the message broker is temporarily down.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;

    public override string ToString() => $"[{Id}] {EventType} — {(IsProcessed ? "Processed" : "Pending")}";
}
