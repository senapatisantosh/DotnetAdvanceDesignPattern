using DesignPatterns.Enterprise.Repository;
using DesignPatterns.Enterprise.Specification;
using DesignPatterns.Enterprise.Specification.ProductSpecifications;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class SpecificationTests
{
    private static Product CreateProduct(string name = "Widget", string category = "Electronics",
        decimal price = 29.99m, int stock = 10) =>
        new() { TenantId = "t1", Name = name, Category = category, Price = price, StockQuantity = stock };

    [Fact]
    public void PriceRange_satisfied_when_price_within_range()
    {
        var spec = new PriceRangeSpecification(10m, 50m);

        spec.IsSatisfiedBy(CreateProduct(price: 29.99m)).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(price: 5m)).Should().BeFalse();
        spec.IsSatisfiedBy(CreateProduct(price: 100m)).Should().BeFalse();
    }

    [Fact]
    public void PriceRange_includes_boundaries()
    {
        var spec = new PriceRangeSpecification(10m, 50m);

        spec.IsSatisfiedBy(CreateProduct(price: 10m)).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(price: 50m)).Should().BeTrue();
    }

    [Fact]
    public void Category_is_case_insensitive()
    {
        var spec = new CategorySpecification("electronics");

        spec.IsSatisfiedBy(CreateProduct(category: "Electronics")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(category: "ELECTRONICS")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(category: "Books")).Should().BeFalse();
    }

    [Fact]
    public void InStock_checks_quantity_greater_than_zero()
    {
        var spec = new InStockSpecification();

        spec.IsSatisfiedBy(CreateProduct(stock: 10)).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(stock: 0)).Should().BeFalse();
    }

    [Fact]
    public void SearchTerm_matches_name_or_category()
    {
        var spec = new SearchTermSpecification("Widget");

        spec.IsSatisfiedBy(CreateProduct(name: "Super Widget")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(name: "Gadget", category: "Widget Parts")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(name: "Phone", category: "Electronics")).Should().BeFalse();
    }

    [Fact]
    public void And_composition_requires_both()
    {
        var affordable = new PriceRangeSpecification(0m, 30m);
        var inStock = new InStockSpecification();
        var combined = affordable.And(inStock);

        combined.IsSatisfiedBy(CreateProduct(price: 20m, stock: 5)).Should().BeTrue();
        combined.IsSatisfiedBy(CreateProduct(price: 20m, stock: 0)).Should().BeFalse();
        combined.IsSatisfiedBy(CreateProduct(price: 50m, stock: 5)).Should().BeFalse();
    }

    [Fact]
    public void Or_composition_requires_either()
    {
        var electronics = new CategorySpecification("Electronics");
        var books = new CategorySpecification("Books");
        var combined = electronics.Or(books);

        combined.IsSatisfiedBy(CreateProduct(category: "Electronics")).Should().BeTrue();
        combined.IsSatisfiedBy(CreateProduct(category: "Books")).Should().BeTrue();
        combined.IsSatisfiedBy(CreateProduct(category: "Clothing")).Should().BeFalse();
    }

    [Fact]
    public void Not_inverts_result()
    {
        var inStock = new InStockSpecification();
        var outOfStock = inStock.Not();

        outOfStock.IsSatisfiedBy(CreateProduct(stock: 0)).Should().BeTrue();
        outOfStock.IsSatisfiedBy(CreateProduct(stock: 5)).Should().BeFalse();
    }

    [Fact]
    public void Complex_composition()
    {
        // Affordable electronics that are in stock
        var spec = new CategorySpecification("Electronics")
            .And(new PriceRangeSpecification(0m, 50m))
            .And(new InStockSpecification());

        spec.IsSatisfiedBy(CreateProduct(category: "Electronics", price: 30m, stock: 5)).Should().BeTrue();
        spec.IsSatisfiedBy(CreateProduct(category: "Books", price: 30m, stock: 5)).Should().BeFalse();
        spec.IsSatisfiedBy(CreateProduct(category: "Electronics", price: 100m, stock: 5)).Should().BeFalse();
        spec.IsSatisfiedBy(CreateProduct(category: "Electronics", price: 30m, stock: 0)).Should().BeFalse();
    }

    [Fact]
    public void Specification_can_be_used_as_predicate()
    {
        var products = new[]
        {
            CreateProduct(price: 10m),
            CreateProduct(price: 30m),
            CreateProduct(price: 60m)
        };

        Specification<Product> spec = new PriceRangeSpecification(0m, 50m);
        Func<Product, bool> predicate = spec;

        products.Where(predicate).Should().HaveCount(2);
    }
}
