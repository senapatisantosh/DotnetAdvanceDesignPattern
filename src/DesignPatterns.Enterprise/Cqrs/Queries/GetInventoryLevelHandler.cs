using DesignPatterns.Enterprise.Cqrs.Models;

namespace DesignPatterns.Enterprise.Cqrs.Queries;

public sealed class GetInventoryLevelHandler(InMemoryInventoryStore store)
    : IQueryHandler<GetInventoryLevelQuery, InventoryItem?>
{
    public Task<InventoryItem?> HandleAsync(GetInventoryLevelQuery query, CancellationToken ct = default)
    {
        return Task.FromResult(store.Get(query.Sku));
    }
}
