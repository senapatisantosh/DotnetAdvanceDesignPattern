using DesignPatterns.Enterprise.Repository;

namespace DesignPatterns.Enterprise.Specification.ProductSpecifications;

/// <summary>
/// Satisfied when a product's price falls within [min, max].
/// </summary>
public sealed class PriceRangeSpecification(decimal min, decimal max) : Specification<Product>
{
    public decimal Min => min;
    public decimal Max => max;

    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.Price >= min && candidate.Price <= max;

    public override string ToString() => $"Price in [{min:F2}, {max:F2}]";
}
