using System.Collections.Concurrent;

namespace DesignPatterns.Enterprise.UnitOfWork;

/// <summary>
/// In-memory Unit of Work that tracks inserts, updates, and deletes across
/// entity types and applies them atomically on <see cref="CommitAsync"/>.
/// A real implementation would wrap a database transaction.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, object> _store;
    private readonly ChangeTracker _changeTracker = new();
    private readonly List<ChangeRecord> _committedLog = new();
    private bool _disposed;

    public InMemoryUnitOfWork() : this(new ConcurrentDictionary<Guid, object>()) { }

    /// <summary>
    /// Accepts a shared store so that multiple repositories can read committed data.
    /// </summary>
    public InMemoryUnitOfWork(ConcurrentDictionary<Guid, object> sharedStore)
    {
        _store = sharedStore;
    }

    public void RegisterNew<T>(T entity) where T : class, IEntity
    {
        ThrowIfDisposed();
        _changeTracker.TrackNew(entity);
    }

    public void RegisterDirty<T>(T entity) where T : class, IEntity
    {
        ThrowIfDisposed();
        _changeTracker.TrackDirty(entity);
    }

    public void RegisterDeleted<T>(T entity) where T : class, IEntity
    {
        ThrowIfDisposed();
        _changeTracker.TrackDeleted(entity);
    }

    public Task<int> CommitAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (!_changeTracker.HasChanges)
            return Task.FromResult(0);

        // Snapshot current state for rollback if any step fails
        var snapshot = new Dictionary<Guid, object?>(_store.Count);
        var changes = _changeTracker.Changes.ToList();

        try
        {
            foreach (var change in changes)
            {
                // Save rollback snapshot
                snapshot.TryAdd(change.EntityId, _store.TryGetValue(change.EntityId, out var prev) ? prev : null);

                switch (change.ChangeType)
                {
                    case ChangeType.Insert:
                        if (!_store.TryAdd(change.EntityId, change.Entity))
                            throw new InvalidOperationException($"Entity {change.EntityId} already exists.");
                        break;

                    case ChangeType.Update:
                        if (!_store.ContainsKey(change.EntityId))
                            throw new KeyNotFoundException($"Entity {change.EntityId} not found for update.");
                        _store[change.EntityId] = change.Entity;
                        break;

                    case ChangeType.Delete:
                        if (!_store.TryRemove(change.EntityId, out _))
                            throw new KeyNotFoundException($"Entity {change.EntityId} not found for delete.");
                        break;
                }
            }

            _committedLog.AddRange(changes);
            var count = changes.Count;
            _changeTracker.Clear();
            return Task.FromResult(count);
        }
        catch
        {
            // Rollback: restore snapshot
            foreach (var (id, original) in snapshot)
            {
                if (original is null)
                    _store.TryRemove(id, out _);
                else
                    _store[id] = original;
            }

            throw;
        }
    }

    public void Rollback()
    {
        ThrowIfDisposed();
        _changeTracker.Clear();
    }

    public IReadOnlyList<ChangeRecord> GetChangeLog() => _committedLog.AsReadOnly();

    /// <summary>Provides read access to committed entities for testing.</summary>
    public T? TryGet<T>(Guid id) where T : class =>
        _store.TryGetValue(id, out var obj) ? obj as T : null;

    public void Dispose()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
