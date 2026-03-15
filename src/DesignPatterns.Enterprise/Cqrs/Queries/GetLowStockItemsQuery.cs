using DesignPatterns.Enterprise.Cqrs.Models;

namespace DesignPatterns.Enterprise.Cqrs.Queries;

public sealed record GetLowStockItemsQuery(int? ThresholdOverride = null) : IQuery<IReadOnlyList<InventoryItem>>;
