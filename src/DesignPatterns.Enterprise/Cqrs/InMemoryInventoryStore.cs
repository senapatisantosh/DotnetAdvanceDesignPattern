using System.Collections.Concurrent;
using DesignPatterns.Enterprise.Cqrs.Models;

namespace DesignPatterns.Enterprise.Cqrs;

/// <summary>
/// Shared in-memory store used by both command and query handlers.
/// In a real CQRS system this would be two separate stores (write DB + read projection).
/// </summary>
public sealed class InMemoryInventoryStore
{
    private readonly ConcurrentDictionary<string, InventoryItem> _items = new(StringComparer.OrdinalIgnoreCase);

    public void Seed(params InventoryItem[] items)
    {
        foreach (var item in items)
            _items[item.Sku] = item;
    }

    public InventoryItem? Get(string sku) =>
        _items.TryGetValue(sku, out var item) ? item : null;

    public IReadOnlyList<InventoryItem> GetAll() =>
        _items.Values.ToList().AsReadOnly();

    public void AddOrUpdate(InventoryItem item) =>
        _items[item.Sku] = item;
}
