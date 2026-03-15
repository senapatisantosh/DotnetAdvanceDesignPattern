namespace DesignPatterns.Enterprise.NullObject;

/// <summary>
/// Real audit logger that records entries (in-memory for demo, would be a database in production).
/// </summary>
public sealed class DatabaseAuditLogger : IAuditLogger
{
    private readonly List<AuditLogEntry> _logs = new();

    public bool IsEnabled => true;

    public Task LogAsync(string userId, string action, string entityType, string entityId, string? details = null)
    {
        _logs.Add(new AuditLogEntry(
            Guid.NewGuid(), userId, action, entityType, entityId, details, DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(string? userId = null, int limit = 100)
    {
        IReadOnlyList<AuditLogEntry> result = _logs
            .Where(l => userId is null || l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
