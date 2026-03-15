using System.Runtime.CompilerServices;

namespace DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Transparently iterates over all pages from a paged data source.
/// Callers use "await foreach" and pages are fetched lazily under the hood.
/// </summary>
public sealed class PagedEnumerable<T>
{
    private readonly IPagedDataSource<T> _dataSource;
    private readonly int _pageSize;

    public PagedEnumerable(IPagedDataSource<T> dataSource, int pageSize = 20)
    {
        _dataSource = dataSource;
        _pageSize = pageSize;
    }

    /// <summary>
    /// Lazily fetches pages and yields individual items.
    /// </summary>
    public async IAsyncEnumerable<T> GetAllItemsAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int currentPage = 1;
        bool hasMore = true;

        while (hasMore)
        {
            var page = await _dataSource.GetPageAsync(currentPage, _pageSize, ct);

            foreach (var item in page.Items)
            {
                yield return item;
            }

            hasMore = page.HasNextPage;
            currentPage++;
        }
    }

    /// <summary>
    /// Returns pages as async enumerable (useful when you want page metadata).
    /// </summary>
    public async IAsyncEnumerable<PagedResult<T>> GetPagesAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int currentPage = 1;
        bool hasMore = true;

        while (hasMore)
        {
            var page = await _dataSource.GetPageAsync(currentPage, _pageSize, ct);
            yield return page;

            hasMore = page.HasNextPage;
            currentPage++;
        }
    }
}
