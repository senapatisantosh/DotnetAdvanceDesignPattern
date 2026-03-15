namespace DesignPatterns.Enterprise.NullObject;

/// <summary>
/// Null Object implementation — does nothing but satisfies the interface contract.
/// Eliminates null checks throughout the consuming code.
/// Use this in environments where auditing is disabled (dev, unit tests, lightweight deployments).
/// </summary>
public sealed class NullAuditLogger : IAuditLogger
{
    public static readonly NullAuditLogger Instance = new();

    public bool IsEnabled => false;

    public Task LogAsync(string userId, string action, string entityType, string entityId, string? details = null)
        => Task.CompletedTask;

    public Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(string? userId = null, int limit = 100)
    {
        IReadOnlyList<AuditLogEntry> empty = Array.Empty<AuditLogEntry>();
        return Task.FromResult(empty);
    }
}
