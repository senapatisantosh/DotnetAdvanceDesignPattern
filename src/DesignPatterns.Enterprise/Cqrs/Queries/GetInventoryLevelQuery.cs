using DesignPatterns.Enterprise.Cqrs.Models;

namespace DesignPatterns.Enterprise.Cqrs.Queries;

public sealed record GetInventoryLevelQuery(string Sku) : IQuery<InventoryItem?>;
