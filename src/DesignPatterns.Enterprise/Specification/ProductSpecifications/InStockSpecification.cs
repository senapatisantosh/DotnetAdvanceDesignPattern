using DesignPatterns.Enterprise.Repository;

namespace DesignPatterns.Enterprise.Specification.ProductSpecifications;

/// <summary>
/// Satisfied when a product has at least one unit in stock.
/// </summary>
public sealed class InStockSpecification : Specification<Product>
{
    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.StockQuantity > 0;

    public override string ToString() => "InStock";
}
