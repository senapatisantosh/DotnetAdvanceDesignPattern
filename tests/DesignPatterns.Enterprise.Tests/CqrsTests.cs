using DesignPatterns.Enterprise.Cqrs;
using DesignPatterns.Enterprise.Cqrs.Commands;
using DesignPatterns.Enterprise.Cqrs.Models;
using DesignPatterns.Enterprise.Cqrs.Queries;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class CqrsTests
{
    private readonly InMemoryInventoryStore _store = new();

    public CqrsTests()
    {
        _store.Seed(
            new InventoryItem { Sku = "SKU-001", ProductName = "Laptop", AvailableQuantity = 50, ReorderThreshold = 10 },
            new InventoryItem { Sku = "SKU-002", ProductName = "Mouse", AvailableQuantity = 200, ReorderThreshold = 20 },
            new InventoryItem { Sku = "SKU-003", ProductName = "Keyboard", AvailableQuantity = 5, ReorderThreshold = 10 }
        );
    }

    [Fact]
    public async Task ReserveInventory_decreases_available_increases_reserved()
    {
        var handler = new ReserveInventoryHandler(_store);

        await handler.HandleAsync(new ReserveInventoryCommand("SKU-001", 10, "ORD-1"));

        var item = _store.Get("SKU-001")!;
        item.AvailableQuantity.Should().Be(40);
        item.ReservedQuantity.Should().Be(10);
    }

    [Fact]
    public async Task ReserveInventory_insufficient_stock_throws()
    {
        var handler = new ReserveInventoryHandler(_store);

        var act = () => handler.HandleAsync(new ReserveInventoryCommand("SKU-001", 999, "ORD-1"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task ReserveInventory_unknown_sku_throws()
    {
        var handler = new ReserveInventoryHandler(_store);

        var act = () => handler.HandleAsync(new ReserveInventoryCommand("NOPE", 1, "ORD-1"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ReleaseInventory_reverses_reservation()
    {
        var reserveHandler = new ReserveInventoryHandler(_store);
        var releaseHandler = new ReleaseInventoryHandler(_store);

        await reserveHandler.HandleAsync(new ReserveInventoryCommand("SKU-001", 10, "ORD-1"));
        await releaseHandler.HandleAsync(new ReleaseInventoryCommand("SKU-001", 10, "Cancelled"));

        var item = _store.Get("SKU-001")!;
        item.AvailableQuantity.Should().Be(50);
        item.ReservedQuantity.Should().Be(0);
    }

    [Fact]
    public async Task ReleaseInventory_more_than_reserved_throws()
    {
        var handler = new ReleaseInventoryHandler(_store);

        var act = () => handler.HandleAsync(new ReleaseInventoryCommand("SKU-001", 100, "Error"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot release*");
    }

    [Fact]
    public async Task GetInventoryLevel_returns_item()
    {
        var handler = new GetInventoryLevelHandler(_store);

        var result = await handler.HandleAsync(new GetInventoryLevelQuery("SKU-001"));

        result.Should().NotBeNull();
        result!.ProductName.Should().Be("Laptop");
        result.AvailableQuantity.Should().Be(50);
    }

    [Fact]
    public async Task GetInventoryLevel_unknown_sku_returns_null()
    {
        var handler = new GetInventoryLevelHandler(_store);

        var result = await handler.HandleAsync(new GetInventoryLevelQuery("NOPE"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLowStockItems_uses_item_threshold()
    {
        var handler = new GetLowStockItemsHandler(_store);

        var result = await handler.HandleAsync(new GetLowStockItemsQuery());

        // SKU-003 has 5 available with threshold 10
        result.Should().HaveCount(1);
        result[0].Sku.Should().Be("SKU-003");
    }

    [Fact]
    public async Task GetLowStockItems_with_override_threshold()
    {
        var handler = new GetLowStockItemsHandler(_store);

        var result = await handler.HandleAsync(new GetLowStockItemsQuery(ThresholdOverride: 100));

        // All items have <= 200 available; with threshold 100, SKU-001 (50) and SKU-003 (5) qualify
        result.Should().HaveCount(2);
    }

    [Fact]
    public void InventoryItem_total_quantity_includes_reserved()
    {
        var item = new InventoryItem
        {
            Sku = "X", ProductName = "X", AvailableQuantity = 30, ReservedQuantity = 20
        };

        item.TotalQuantity.Should().Be(50);
    }
}
