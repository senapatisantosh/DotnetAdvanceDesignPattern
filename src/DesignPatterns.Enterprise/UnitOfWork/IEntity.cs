namespace DesignPatterns.Enterprise.UnitOfWork;

/// <summary>
/// Marker interface for entities managed by the Unit of Work.
/// Every entity must have a stable identity.
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}
