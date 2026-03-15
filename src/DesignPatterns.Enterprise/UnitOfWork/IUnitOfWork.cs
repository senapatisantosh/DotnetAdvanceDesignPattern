namespace DesignPatterns.Enterprise.UnitOfWork;

/// <summary>
/// Coordinates writes across multiple repositories so that either all changes
/// commit together or none of them do — the classic "all or nothing" guarantee.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    void RegisterNew<T>(T entity) where T : class, IEntity;
    void RegisterDirty<T>(T entity) where T : class, IEntity;
    void RegisterDeleted<T>(T entity) where T : class, IEntity;
    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
    IReadOnlyList<ChangeRecord> GetChangeLog();
}
