using DesignPatterns.Behavioral.Iterator;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class IteratorTests
{
    [Fact]
    public async Task PagedEnumerable_IteratesAllItems()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var results = new List<int>();
        await foreach (var item in paged.GetAllItemsAsync())
        {
            results.Add(item);
        }

        results.Should().HaveCount(25);
        results.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task PagedEnumerable_FetchesPagesLazily()
    {
        var items = Enumerable.Range(1, 30).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        // Only consume first 5 items (should only need 1 page)
        var count = 0;
        await foreach (var item in paged.GetAllItemsAsync())
        {
            count++;
            if (count >= 5) break;
        }

        source.FetchCount.Should().Be(1);
    }

    [Fact]
    public async Task PagedEnumerable_HandlesEmptySource()
    {
        var source = new InMemoryPagedDataSource<int>([]);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var results = new List<int>();
        await foreach (var item in paged.GetAllItemsAsync())
        {
            results.Add(item);
        }

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task PagedEnumerable_FetchesCorrectNumberOfPages()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        await foreach (var _ in paged.GetAllItemsAsync()) { }

        source.FetchCount.Should().Be(3); // 10 + 10 + 5
    }

    [Fact]
    public async Task PagedEnumerable_GetPagesAsync_ReturnsPageMetadata()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var pages = new List<PagedResult<int>>();
        await foreach (var page in paged.GetPagesAsync())
        {
            pages.Add(page);
        }

        pages.Should().HaveCount(3);
        pages[0].Items.Should().HaveCount(10);
        pages[0].TotalCount.Should().Be(25);
        pages[0].HasNextPage.Should().BeTrue();
        pages[2].Items.Should().HaveCount(5);
        pages[2].HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task PagedResult_CalculatesTotalPages()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var source = new InMemoryPagedDataSource<int>(items);

        var page = await source.GetPageAsync(1, 10);

        page.TotalPages.Should().Be(3);
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task PagedEnumerable_SupportsCancellation()
    {
        var items = Enumerable.Range(1, 100).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        using var cts = new CancellationTokenSource();
        var results = new List<int>();

        await foreach (var item in paged.GetAllItemsAsync(cts.Token))
        {
            results.Add(item);
            if (results.Count >= 15)
                cts.Cancel();
        }

        results.Count.Should().BeGreaterThanOrEqualTo(15);
        results.Count.Should().BeLessThan(100);
    }
}
