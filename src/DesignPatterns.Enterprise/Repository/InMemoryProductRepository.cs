using System.Collections.Concurrent;

namespace DesignPatterns.Enterprise.Repository;

/// <summary>
/// Thread-safe in-memory implementation of IProductRepository.
/// Demonstrates tenant isolation — every query is scoped to a tenant so that
/// one store can never see another store's catalog.
/// </summary>
public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _store = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Product>> FindAsync(Func<Product, bool> predicate, CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values.Where(predicate).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(Product entity, CancellationToken ct = default)
    {
        if (!_store.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"Product with ID {entity.Id} already exists.");

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product entity, CancellationToken ct = default)
    {
        if (!_store.ContainsKey(entity.Id))
            throw new KeyNotFoundException($"Product with ID {entity.Id} not found.");

        entity.UpdatedAt = DateTime.UtcNow;
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryRemove(id, out _))
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.ContainsKey(id));

    public Task<int> CountAsync(CancellationToken ct = default) =>
        Task.FromResult(_store.Count);

    // ── Tenant-scoped queries ──────────────────────────────────────────

    public Task<IReadOnlyList<Product>> GetByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values
            .Where(p => p.TenantId == tenantId)
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Product>> GetByCategoryAsync(string tenantId, string category, CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values
            .Where(p => p.TenantId == tenantId && p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Product>> GetInPriceRangeAsync(string tenantId, decimal min, decimal max, CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values
            .Where(p => p.TenantId == tenantId && p.Price >= min && p.Price <= max)
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Product>> GetLowStockAsync(string tenantId, int threshold, CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _store.Values
            .Where(p => p.TenantId == tenantId && p.StockQuantity <= threshold)
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(string tenantId, CancellationToken ct = default)
    {
        IReadOnlyList<string> result = _store.Values
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.Category)
            .Distinct()
            .ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
