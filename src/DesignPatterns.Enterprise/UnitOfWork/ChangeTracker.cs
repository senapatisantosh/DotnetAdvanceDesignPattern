namespace DesignPatterns.Enterprise.UnitOfWork;

/// <summary>
/// Tracks pending changes (inserts, updates, deletes) before they are committed.
/// </summary>
public sealed class ChangeTracker
{
    private readonly List<ChangeRecord> _changes = new();

    public IReadOnlyList<ChangeRecord> Changes => _changes.AsReadOnly();

    public void TrackNew<T>(T entity) where T : class, IEntity =>
        _changes.Add(new ChangeRecord(entity.Id, typeof(T).Name, ChangeType.Insert, entity));

    public void TrackDirty<T>(T entity) where T : class, IEntity =>
        _changes.Add(new ChangeRecord(entity.Id, typeof(T).Name, ChangeType.Update, entity));

    public void TrackDeleted<T>(T entity) where T : class, IEntity =>
        _changes.Add(new ChangeRecord(entity.Id, typeof(T).Name, ChangeType.Delete, entity));

    public void Clear() => _changes.Clear();

    public bool HasChanges => _changes.Count > 0;
}

public enum ChangeType { Insert, Update, Delete }

/// <summary>
/// Immutable record of a single change within a Unit of Work transaction.
/// </summary>
public sealed record ChangeRecord(
    Guid EntityId,
    string EntityType,
    ChangeType ChangeType,
    object Entity,
    DateTime Timestamp = default)
{
    public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.UtcNow : Timestamp;
}
