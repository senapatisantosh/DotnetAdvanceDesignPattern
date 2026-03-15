using DesignPatterns.Structural.Proxy;
using DesignPatterns.Structural.Proxy.CachingProxy;
using DesignPatterns.Structural.Proxy.ProtectionProxy;
using DesignPatterns.Structural.Proxy.VirtualProxy;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class ProxyTests
{
    // --- Protection Proxy Tests ---

    [Fact]
    public async Task ProtectionProxy_AuthenticatedUser_CanReadInventory()
    {
        var realService = new RealInventoryService();
        var proxy = new AuthenticatedInventoryProxy(
            realService, UserContext.ReadOnlyUser("alice"));

        var item = await proxy.GetItemAsync("LAPTOP-001");

        item.Should().NotBeNull();
        item!.ProductName.Should().Be("ThinkPad X1 Carbon");
    }

    [Fact]
    public async Task ProtectionProxy_ReadOnlyUser_CannotReserve()
    {
        var realService = new RealInventoryService();
        var proxy = new AuthenticatedInventoryProxy(
            realService, UserContext.ReadOnlyUser("alice"));

        var act = () => proxy.ReserveAsync("LAPTOP-001", 1);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inventory:write*");
    }

    [Fact]
    public async Task ProtectionProxy_AnonymousUser_CannotRead()
    {
        var realService = new RealInventoryService();
        var proxy = new AuthenticatedInventoryProxy(realService, UserContext.Anonymous);

        var act = () => proxy.GetItemAsync("LAPTOP-001");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task ProtectionProxy_FullAccessUser_CanReserve()
    {
        var realService = new RealInventoryService();
        var proxy = new AuthenticatedInventoryProxy(
            realService, UserContext.FullAccessUser("admin"));

        var result = await proxy.ReserveAsync("KEYBOARD-001", 5);

        result.Success.Should().BeTrue();
        result.QuantityReserved.Should().Be(5);
    }

    [Fact]
    public async Task ProtectionProxy_FullAccessUser_CanReleaseReservation()
    {
        var realService = new RealInventoryService();
        var proxy = new AuthenticatedInventoryProxy(
            realService, UserContext.FullAccessUser("admin"));

        var reservation = await proxy.ReserveAsync("MONITOR-001", 2);
        var released = await proxy.ReleaseReservationAsync(reservation.ReservationId);

        released.Should().BeTrue();
    }

    // --- Virtual Proxy Tests ---

    [Fact]
    public void VirtualProxy_DoesNotInitialize_UntilFirstCall()
    {
        var initCount = 0;
        var proxy = new LazyInventoryProxy(() =>
        {
            initCount++;
            return new RealInventoryService();
        });

        proxy.IsInitialized.Should().BeFalse();
        initCount.Should().Be(0);
    }

    [Fact]
    public async Task VirtualProxy_InitializesOnFirstCall()
    {
        var initCount = 0;
        var proxy = new LazyInventoryProxy(() =>
        {
            initCount++;
            return new RealInventoryService();
        });

        await proxy.GetItemAsync("LAPTOP-001");

        proxy.IsInitialized.Should().BeTrue();
        initCount.Should().Be(1);
    }

    [Fact]
    public async Task VirtualProxy_InitializesOnlyOnce()
    {
        var initCount = 0;
        var proxy = new LazyInventoryProxy(() =>
        {
            initCount++;
            return new RealInventoryService();
        });

        await proxy.GetItemAsync("LAPTOP-001");
        await proxy.GetAllItemsAsync();
        await proxy.GetItemAsync("MONITOR-001");

        initCount.Should().Be(1);
    }

    // --- Caching Proxy Tests ---

    [Fact]
    public async Task CachingProxy_CachesGetItemResults()
    {
        var realService = new RealInventoryService();
        var proxy = new CachedInventoryProxy(realService);

        await proxy.GetItemAsync("LAPTOP-001");
        await proxy.GetItemAsync("LAPTOP-001");

        proxy.CacheHits.Should().Be(1);
        proxy.CacheMisses.Should().Be(1);
        realService.OperationCount.Should().Be(1);
    }

    [Fact]
    public async Task CachingProxy_CachesGetAllItemsResults()
    {
        var realService = new RealInventoryService();
        var proxy = new CachedInventoryProxy(realService);

        await proxy.GetAllItemsAsync();
        await proxy.GetAllItemsAsync();

        proxy.CacheHits.Should().Be(1);
        proxy.CacheMisses.Should().Be(1);
        realService.OperationCount.Should().Be(1);
    }

    [Fact]
    public async Task CachingProxy_InvalidatesOnReserve()
    {
        var realService = new RealInventoryService();
        var proxy = new CachedInventoryProxy(realService);

        // Populate cache
        await proxy.GetItemAsync("KEYBOARD-001");
        proxy.CacheMisses.Should().Be(1);

        // Reserve should invalidate
        await proxy.ReserveAsync("KEYBOARD-001", 1);

        // Next read should miss cache
        await proxy.GetItemAsync("KEYBOARD-001");
        proxy.CacheMisses.Should().Be(2);
    }

    [Fact]
    public async Task CachingProxy_ManualInvalidation_ForcesRefresh()
    {
        var realService = new RealInventoryService();
        var proxy = new CachedInventoryProxy(realService);

        await proxy.GetItemAsync("MOUSE-001");
        proxy.InvalidateItem("MOUSE-001");
        await proxy.GetItemAsync("MOUSE-001");

        proxy.CacheMisses.Should().Be(2);
    }

    [Fact]
    public async Task CachingProxy_InvalidateAll_ClearsEntireCache()
    {
        var realService = new RealInventoryService();
        var proxy = new CachedInventoryProxy(realService);

        await proxy.GetItemAsync("LAPTOP-001");
        await proxy.GetItemAsync("MONITOR-001");
        proxy.InvalidateAll();
        await proxy.GetItemAsync("LAPTOP-001");
        await proxy.GetItemAsync("MONITOR-001");

        proxy.CacheMisses.Should().Be(4);
    }

    // --- Real Service Tests ---

    [Fact]
    public async Task RealService_GetItem_ReturnsSeededData()
    {
        var service = new RealInventoryService();

        var item = await service.GetItemAsync("LAPTOP-001");

        item.Should().NotBeNull();
        item!.Sku.Should().Be("LAPTOP-001");
        item.QuantityOnHand.Should().Be(50);
    }

    [Fact]
    public async Task RealService_GetItem_ReturnsNullForUnknownSku()
    {
        var service = new RealInventoryService();

        var item = await service.GetItemAsync("NONEXISTENT");

        item.Should().BeNull();
    }

    [Fact]
    public async Task RealService_Reserve_ReducesAvailableQuantity()
    {
        var service = new RealInventoryService();

        var before = await service.GetItemAsync("KEYBOARD-001");
        var result = await service.ReserveAsync("KEYBOARD-001", 5);
        var after = await service.GetItemAsync("KEYBOARD-001");

        result.Success.Should().BeTrue();
        after!.QuantityAvailable.Should().Be(before!.QuantityAvailable - 5);
    }

    [Fact]
    public async Task RealService_Reserve_FailsWhenInsufficientStock()
    {
        var service = new RealInventoryService();

        var result = await service.ReserveAsync("HEADSET-001", 100);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task RealService_Reserve_FailsForUnknownSku()
    {
        var service = new RealInventoryService();

        var result = await service.ReserveAsync("FAKE-SKU", 1);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("not found");
    }

    [Fact]
    public async Task RealService_ReleaseReservation_RestoresQuantity()
    {
        var service = new RealInventoryService();

        var reservation = await service.ReserveAsync("MOUSE-001", 10);
        var before = await service.GetItemAsync("MOUSE-001");

        await service.ReleaseReservationAsync(reservation.ReservationId);
        var after = await service.GetItemAsync("MOUSE-001");

        after!.QuantityAvailable.Should().Be(before!.QuantityAvailable + 10);
    }

    [Fact]
    public async Task RealService_GetAllItems_ReturnsAllSeededItems()
    {
        var service = new RealInventoryService();

        var items = await service.GetAllItemsAsync();

        items.Should().HaveCount(5);
        items.Select(i => i.Sku).Should().Contain("LAPTOP-001");
    }

    [Fact]
    public void InventoryItem_QuantityAvailable_IsComputedCorrectly()
    {
        var item = new InventoryItem
        {
            Sku = "TEST",
            ProductName = "Test",
            QuantityOnHand = 100,
            QuantityReserved = 30,
            WarehouseLocation = "A-1",
            LastUpdatedUtc = DateTime.UtcNow
        };

        item.QuantityAvailable.Should().Be(70);
        item.IsInStock.Should().BeTrue();
    }

    [Fact]
    public void InventoryItem_IsInStock_FalseWhenFullyReserved()
    {
        var item = new InventoryItem
        {
            Sku = "TEST",
            ProductName = "Test",
            QuantityOnHand = 10,
            QuantityReserved = 10,
            WarehouseLocation = "A-1",
            LastUpdatedUtc = DateTime.UtcNow
        };

        item.IsInStock.Should().BeFalse();
    }
}
