using DesignPatterns.Enterprise.Repository;

namespace DesignPatterns.Enterprise.Specification.ProductSpecifications;

/// <summary>
/// Satisfied when a product belongs to the specified category (case-insensitive).
/// </summary>
public sealed class CategorySpecification(string category) : Specification<Product>
{
    public string Category => category;

    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.Category.Equals(category, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => $"Category = '{category}'";
}
