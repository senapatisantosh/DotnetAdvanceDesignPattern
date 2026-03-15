using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Concrete Decorator — caches GET responses for a configurable TTL.
/// POST requests always pass through (not idempotent).
/// Uses a thread-safe concurrent dictionary for the cache store.
/// </summary>
public sealed class CachingApiClientDecorator : IApiClient
{
    private readonly IApiClient _inner;
    private readonly TimeSpan _ttl;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public CachingApiClientDecorator(IApiClient inner, TimeSpan? ttl = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _ttl = ttl ?? TimeSpan.FromMinutes(5);
    }

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public async Task<ApiResponse> GetAsync(string url)
    {
        // Check cache for unexpired entry
        if (_cache.TryGetValue(url, out var cached) && !cached.IsExpired)
        {
            CacheHits++;
            return cached.Response with { FromCache = true };
        }

        // Cache miss — call downstream
        CacheMisses++;
        var response = await _inner.GetAsync(url);

        // Only cache successful responses
        if (response.IsSuccess)
        {
            _cache[url] = new CacheEntry(response, DateTime.UtcNow.Add(_ttl));
        }

        return response;
    }

    /// <summary>POST requests are never cached — always pass through.</summary>
    public Task<ApiResponse> PostAsync(string url, string payload) =>
        _inner.PostAsync(url, payload);

    /// <summary>Manually evict a cached entry.</summary>
    public void Invalidate(string url) => _cache.TryRemove(url, out _);

    /// <summary>Clear the entire cache.</summary>
    public void InvalidateAll() => _cache.Clear();

    private sealed record CacheEntry(ApiResponse Response, DateTime ExpiresAtUtc)
    {
        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    }
}
