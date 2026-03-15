namespace DesignPatterns.Enterprise.NullObject;

/// <summary>
/// Logs audit-worthy business events (user actions, data changes, security events).
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(string userId, string action, string entityType, string entityId, string? details = null);
    Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(string? userId = null, int limit = 100);
    bool IsEnabled { get; }
}

public sealed record AuditLogEntry(
    Guid Id,
    string UserId,
    string Action,
    string EntityType,
    string EntityId,
    string? Details,
    DateTime Timestamp);
