using DesignPatterns.Enterprise.Repository;

namespace DesignPatterns.Enterprise.Specification.ProductSpecifications;

/// <summary>
/// Satisfied when the search term appears in the product's name or category.
/// </summary>
public sealed class SearchTermSpecification(string searchTerm) : Specification<Product>
{
    public string SearchTerm => searchTerm;

    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        candidate.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => $"Search('{searchTerm}')";
}
