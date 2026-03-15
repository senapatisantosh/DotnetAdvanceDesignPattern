namespace DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Simulates a paged API by serving slices of an in-memory collection.
/// Tracks how many pages were fetched (useful for verifying lazy behavior in tests).
/// </summary>
public sealed class InMemoryPagedDataSource<T> : IPagedDataSource<T>
{
    private readonly IReadOnlyList<T> _allItems;
    private int _fetchCount;

    public InMemoryPagedDataSource(IEnumerable<T> items)
    {
        _allItems = items.ToList().AsReadOnly();
    }

    /// <summary>Number of times GetPageAsync has been called.</summary>
    public int FetchCount => _fetchCount;

    public Task<PagedResult<T>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _fetchCount);

        var items = _allItems
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .AsReadOnly();

        var result = new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = _allItems.Count
        };

        return Task.FromResult(result);
    }
}
