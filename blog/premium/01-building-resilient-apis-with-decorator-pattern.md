---
title: "Building Resilient API Clients with the Decorator Pattern"
contentKey: "blog-resilient-apis-decorator"
section: "blog-premium"
accessLevel: "premium"
contentType: "blog"
tags: ["dotnet", "design-patterns", "decorator", "api-client", "resilience", "blog"]
order: 1
sourceType: "same_repo"
sourcePath: "blog/premium/01-building-resilient-apis-with-decorator-pattern.md"
routePath: "/project/dotnet-advanced-design-patterns/blog/building-resilient-apis-decorator"
migrationTargetPath: "premium/dotnet-advanced-design-patterns/blog/01-building-resilient-apis-with-decorator-pattern.md"
isPublished: true
---

# Building Resilient API Clients with the Decorator Pattern

Every production application talks to external APIs. Payment gateways, shipping providers, notification services, third-party data feeds -- they all share the same reality: networks are unreliable, services go down, and latency varies wildly. The Decorator pattern provides an elegant way to add retry logic, caching, and logging to any API client without touching the original implementation.

## The Problem: Cross-Cutting Concerns Everywhere

You start with a clean API client:

```csharp
public interface IApiClient
{
    Task<ApiResponse> GetAsync(string url);
    Task<ApiResponse> PostAsync(string url, string payload);
}
```

Then production requirements arrive: "Add logging." "Add retry with exponential backoff." "Cache GET responses." "Add circuit breaking." Each requirement is orthogonal to the actual HTTP call. Embedding them all into the base client creates a tangled mess that violates the Single Responsibility Principle and makes testing painful.

## Decorators: One Concern Per Layer

Each decorator implements the same `IApiClient` interface and wraps an inner client. It adds exactly one behavior and delegates everything else.

**Logging** captures timing and status for every call:

```csharp
public sealed class LoggingApiClientDecorator(IApiClient inner) : IApiClient
{
    public async Task<ApiResponse> GetAsync(string url)
    {
        Log($"GET {url} -- Starting request");
        var response = await inner.GetAsync(url);
        Log($"GET {url} -- Completed: {response.StatusCode} in {response.Duration.TotalMilliseconds:F1}ms");
        return response;
    }
}
```

**Caching** short-circuits GET requests when a valid cached response exists:

```csharp
public sealed class CachingApiClientDecorator(IApiClient inner, TimeSpan? ttl = null) : IApiClient
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public async Task<ApiResponse> GetAsync(string url)
    {
        if (_cache.TryGetValue(url, out var cached) && !cached.IsExpired)
            return cached.Response with { FromCache = true };

        var response = await inner.GetAsync(url);
        if (response.IsSuccess)
            _cache[url] = new CacheEntry(response, DateTime.UtcNow.Add(_ttl));
        return response;
    }

    // POST requests are never cached -- always pass through
    public Task<ApiResponse> PostAsync(string url, string payload) =>
        inner.PostAsync(url, payload);
}
```

**Retry** wraps failed calls with exponential backoff:

```csharp
public sealed class RetryApiClientDecorator(IApiClient inner, int maxRetries = 3) : IApiClient
{
    public async Task<ApiResponse> GetAsync(string url) =>
        await ExecuteWithRetryAsync(() => inner.GetAsync(url));

    private async Task<ApiResponse> ExecuteWithRetryAsync(Func<Task<ApiResponse>> action)
    {
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            var response = await action();
            if (response.IsSuccess || !IsRetriableStatusCode(response.StatusCode))
                return response;

            await Task.Delay(TimeSpan.FromMilliseconds(100) * Math.Pow(2, attempt));
        }
        // exhausted retries...
    }
}
```

## Stacking Order Matters

The order you compose decorators determines the behavior:

```csharp
IApiClient client = new BaseApiClient();
client = new RetryApiClientDecorator(client, maxRetries: 3);
client = new CachingApiClientDecorator(client, TimeSpan.FromMinutes(5));
client = new LoggingApiClientDecorator(client);
```

The call chain is: **Logging -> Caching -> Retry -> BaseClient**. This means:

1. **Logging sees everything**, including cache hits. You get full observability.
2. **Caching short-circuits before retry.** A cached response never triggers retries, saving network calls and latency.
3. **Retry wraps only the actual HTTP call.** Transient failures are retried transparently.

If you swap Caching and Retry (Logging -> Retry -> Caching -> Base), retries would re-check the cache on each attempt -- wasteful, since the cache does not change between retries.

This repository's `ApiClientFactory` encapsulates the correct composition order:

```csharp
public static IApiClient CreateResilientClient(
    double failureRate = 0.0, int maxRetries = 3,
    TimeSpan? cacheTtl = null, TimeSpan? retryBaseDelay = null)
{
    IApiClient client = new BaseApiClient(failureRate);
    client = new RetryApiClientDecorator(client, maxRetries, retryBaseDelay);
    client = new CachingApiClientDecorator(client, cacheTtl);
    client = new LoggingApiClientDecorator(client);
    return client;
}
```

## Integration with ASP.NET Core DI

In production, decorators are registered through the DI container. The Scrutor library provides `Decorate<TInterface, TDecorator>()` for clean registration:

```csharp
services.AddHttpClient<IApiClient, HttpApiClient>();
services.Decorate<IApiClient, RetryApiClientDecorator>();
services.Decorate<IApiClient, CachingApiClientDecorator>();
services.Decorate<IApiClient, LoggingApiClientDecorator>();
```

Without Scrutor, you can use factory registrations:

```csharp
services.AddSingleton<IApiClient>(sp =>
{
    IApiClient client = new HttpApiClient(sp.GetRequiredService<HttpClient>());
    client = new RetryApiClientDecorator(client, maxRetries: 3);
    client = new CachingApiClientDecorator(client, TimeSpan.FromMinutes(5));
    client = new LoggingApiClientDecorator(client);
    return client;
});
```

## Testing Decorators in Isolation

Each decorator is independently testable. Pass a mock `IApiClient` as the inner component and verify the decorator's behavior:

```csharp
[Fact]
public async Task CachingDecorator_ReturnsCachedResponse_OnSecondCall()
{
    var mockInner = new Mock<IApiClient>();
    mockInner.Setup(c => c.GetAsync("https://api.example.com/data"))
        .ReturnsAsync(new ApiResponse { StatusCode = 200, Body = "data" });

    var decorator = new CachingApiClientDecorator(mockInner.Object, TimeSpan.FromMinutes(5));

    await decorator.GetAsync("https://api.example.com/data"); // cache miss
    await decorator.GetAsync("https://api.example.com/data"); // cache hit

    mockInner.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
}
```

The inner client is called exactly once. The second call is served from cache. No HTTP, no retry, no latency.

## When to Use Decorator vs Polly vs HttpClientFactory

**Use Decorator** when you want full control over the behavior, need to compose concerns beyond resilience (caching, authorization, transformation), or want to test each concern in isolation.

**Use Polly** (or the .NET 8+ `Microsoft.Extensions.Resilience` package) when you primarily need resilience policies (retry, circuit breaker, timeout, hedging) and want battle-tested implementations with advanced features like bulkhead isolation.

**Use HttpClientFactory** with `DelegatingHandler` when your decorators are HTTP-specific and you want integration with .NET's `HttpClient` lifecycle management.

These approaches are not mutually exclusive. A decorator can use Polly internally for its retry logic while still providing the clean interface composition that the Decorator pattern offers.
