using DesignPatterns.Enterprise.Repository;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class RepositoryTests
{
    private readonly InMemoryProductRepository _repo = new();

    private static Product CreateProduct(string tenant = "tenant-a", string name = "Widget",
        string category = "Electronics", decimal price = 29.99m, int stock = 100) =>
        new()
        {
            TenantId = tenant,
            Name = name,
            Category = category,
            Price = price,
            StockQuantity = stock
        };

    [Fact]
    public async Task Add_and_retrieve_product_by_id()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        var found = await _repo.GetByIdAsync(product.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task Add_duplicate_id_throws()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        var act = () => _repo.AddAsync(product);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Update_sets_updated_timestamp()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        product.Price = 39.99m;
        await _repo.UpdateAsync(product);

        var found = await _repo.GetByIdAsync(product.Id);
        found!.Price.Should().Be(39.99m);
        found.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_removes_product()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        await _repo.DeleteAsync(product.Id);

        (await _repo.ExistsAsync(product.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_nonexistent_throws()
    {
        var act = () => _repo.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Tenant_isolation_works()
    {
        await _repo.AddAsync(CreateProduct("store-1", "Laptop"));
        await _repo.AddAsync(CreateProduct("store-2", "Phone"));
        await _repo.AddAsync(CreateProduct("store-1", "Tablet"));

        var store1Products = await _repo.GetByTenantAsync("store-1");
        var store2Products = await _repo.GetByTenantAsync("store-2");

        store1Products.Should().HaveCount(2);
        store2Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task Filter_by_category()
    {
        await _repo.AddAsync(CreateProduct(category: "Electronics"));
        await _repo.AddAsync(CreateProduct(category: "Books"));
        await _repo.AddAsync(CreateProduct(category: "Electronics"));

        var electronics = await _repo.GetByCategoryAsync("tenant-a", "Electronics");

        electronics.Should().HaveCount(2);
    }

    [Fact]
    public async Task Price_range_filtering()
    {
        await _repo.AddAsync(CreateProduct(price: 10m));
        await _repo.AddAsync(CreateProduct(price: 50m));
        await _repo.AddAsync(CreateProduct(price: 100m));

        var midRange = await _repo.GetInPriceRangeAsync("tenant-a", 20m, 80m);

        midRange.Should().HaveCount(1);
        midRange[0].Price.Should().Be(50m);
    }

    [Fact]
    public async Task Low_stock_items()
    {
        await _repo.AddAsync(CreateProduct(stock: 5));
        await _repo.AddAsync(CreateProduct(stock: 50));
        await _repo.AddAsync(CreateProduct(stock: 3));

        var lowStock = await _repo.GetLowStockAsync("tenant-a", 10);

        lowStock.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_categories_returns_distinct_values()
    {
        await _repo.AddAsync(CreateProduct(category: "Electronics"));
        await _repo.AddAsync(CreateProduct(category: "Books"));
        await _repo.AddAsync(CreateProduct(category: "Electronics"));

        var categories = await _repo.GetCategoriesAsync("tenant-a");

        categories.Should().HaveCount(2);
        categories.Should().Contain("Electronics");
        categories.Should().Contain("Books");
    }

    [Fact]
    public async Task Count_returns_total()
    {
        await _repo.AddAsync(CreateProduct());
        await _repo.AddAsync(CreateProduct());

        (await _repo.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Find_with_predicate()
    {
        await _repo.AddAsync(CreateProduct(name: "Expensive Widget", price: 999m));
        await _repo.AddAsync(CreateProduct(name: "Cheap Widget", price: 5m));

        var expensive = await _repo.FindAsync(p => p.Price > 100m);

        expensive.Should().HaveCount(1);
        expensive[0].Name.Should().Be("Expensive Widget");
    }
}
