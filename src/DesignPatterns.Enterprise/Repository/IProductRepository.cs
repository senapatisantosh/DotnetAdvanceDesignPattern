namespace DesignPatterns.Enterprise.Repository;

/// <summary>
/// Product-specific repository that extends the generic interface with
/// domain-aware queries for multi-tenant catalog management.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetByTenantAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string tenantId, string category, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetInPriceRangeAsync(string tenantId, decimal min, decimal max, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(string tenantId, int threshold, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(string tenantId, CancellationToken ct = default);
}
