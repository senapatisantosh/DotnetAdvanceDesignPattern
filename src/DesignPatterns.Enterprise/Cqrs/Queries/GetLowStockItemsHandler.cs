using DesignPatterns.Enterprise.Cqrs.Models;

namespace DesignPatterns.Enterprise.Cqrs.Queries;

public sealed class GetLowStockItemsHandler(InMemoryInventoryStore store)
    : IQueryHandler<GetLowStockItemsQuery, IReadOnlyList<InventoryItem>>
{
    public Task<IReadOnlyList<InventoryItem>> HandleAsync(GetLowStockItemsQuery query, CancellationToken ct = default)
    {
        IReadOnlyList<InventoryItem> result = store.GetAll()
            .Where(item => query.ThresholdOverride.HasValue
                ? item.AvailableQuantity <= query.ThresholdOverride.Value
                : item.IsLowStock)
            .ToList().AsReadOnly();

        return Task.FromResult(result);
    }
}
