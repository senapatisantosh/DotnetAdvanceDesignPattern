using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Proxy.CachingProxy;

/// <summary>
/// Caching Proxy — caches read operations (GetItem, GetAll) to avoid
/// repeated calls to the real service. Write operations (Reserve, Release)
/// always pass through and invalidate relevant cache entries.
/// </summary>
public sealed class CachedInventoryProxy : IInventoryService
{
    private readonly IInventoryService _inner;
    private readonly TimeSpan _cacheTtl;
    private readonly ConcurrentDictionary<string, CacheEntry<InventoryItem?>> _itemCache = new();
    private CacheEntry<IReadOnlyList<InventoryItem>>? _allItemsCache;

    public CachedInventoryProxy(IInventoryService inner, TimeSpan? cacheTtl = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cacheTtl = cacheTtl ?? TimeSpan.FromSeconds(30);
    }

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public async Task<InventoryItem?> GetItemAsync(string sku)
    {
        if (_itemCache.TryGetValue(sku, out var cached) && !cached.IsExpired)
        {
            CacheHits++;
            return cached.Value;
        }

        CacheMisses++;
        var item = await _inner.GetItemAsync(sku);
        _itemCache[sku] = new CacheEntry<InventoryItem?>(item, DateTime.UtcNow.Add(_cacheTtl));
        return item;
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllItemsAsync()
    {
        if (_allItemsCache is not null && !_allItemsCache.IsExpired)
        {
            CacheHits++;
            return _allItemsCache.Value;
        }

        CacheMisses++;
        var items = await _inner.GetAllItemsAsync();
        _allItemsCache = new CacheEntry<IReadOnlyList<InventoryItem>>(items, DateTime.UtcNow.Add(_cacheTtl));
        return items;
    }

    public async Task<ReservationResult> ReserveAsync(string sku, int quantity)
    {
        // Write operation: pass through and invalidate cache
        var result = await _inner.ReserveAsync(sku, quantity);
        if (result.Success)
        {
            InvalidateItem(sku);
        }
        return result;
    }

    public async Task<bool> ReleaseReservationAsync(string reservationId)
    {
        var result = await _inner.ReleaseReservationAsync(reservationId);
        if (result)
        {
            // Invalidate all items cache since we don't know which SKU was affected
            _allItemsCache = null;
        }
        return result;
    }

    /// <summary>Evicts a specific SKU from the cache.</summary>
    public void InvalidateItem(string sku)
    {
        _itemCache.TryRemove(sku, out _);
        _allItemsCache = null; // Also invalidate the "all items" cache
    }

    /// <summary>Evicts all cached entries.</summary>
    public void InvalidateAll()
    {
        _itemCache.Clear();
        _allItemsCache = null;
    }

    private sealed record CacheEntry<T>(T Value, DateTime ExpiresAtUtc)
    {
        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    }
}
