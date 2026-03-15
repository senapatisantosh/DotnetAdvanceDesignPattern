using DesignPatterns.Behavioral.Iterator;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class IteratorTests
{
    [Fact]
    public async Task GetAllItems_ReturnsAllItemsAcrossPages()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var result = new List<int>();
        await foreach (var item in paged.GetAllItemsAsync())
        {
            result.Add(item);
        }

        result.Should().HaveCount(25);
        result.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetAllItems_FetchesPagesLazily()
    {
        var items = Enumerable.Range(1, 30).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var result = new List<int>();
        await foreach (var item in paged.GetAllItemsAsync())
        {
            result.Add(item);
            if (result.Count == 5) break; // Only consume 5 items
        }

        result.Should().HaveCount(5);
        source.FetchCount.Should().Be(1); // Only first page fetched
    }

    [Fact]
    public async Task GetPages_ReturnsPageMetadata()
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
        pages[1].Items.Should().HaveCount(10);
        pages[2].Items.Should().HaveCount(5);
        pages[0].TotalCount.Should().Be(25);
        pages[0].HasNextPage.Should().BeTrue();
        pages[2].HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task EmptySource_ReturnsNoItems()
    {
        var source = new InMemoryPagedDataSource<int>([]);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        var result = new List<int>();
        await foreach (var item in paged.GetAllItemsAsync())
        {
            result.Add(item);
        }

        result.Should().BeEmpty();
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
    public async Task Cancellation_StopsIteration()
    {
        var items = Enumerable.Range(1, 100).ToList();
        var source = new InMemoryPagedDataSource<int>(items);
        var paged = new PagedEnumerable<int>(source, pageSize: 10);

        using var cts = new CancellationTokenSource();
        var result = new List<int>();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in paged.GetAllItemsAsync(cts.Token))
            {
                result.Add(item);
                if (result.Count == 15) cts.Cancel();
            }
        });

        result.Count.Should().BeGreaterThanOrEqualTo(15);
    }
}
