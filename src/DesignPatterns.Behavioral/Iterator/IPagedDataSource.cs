namespace DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Abstraction for a paged data source (API, database, etc.).
/// </summary>
public interface IPagedDataSource<T>
{
    Task<PagedResult<T>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct = default);
}
