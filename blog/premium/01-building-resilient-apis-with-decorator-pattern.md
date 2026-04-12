# Building Resilient APIs with the Decorator Pattern

Production APIs face failures that never appear in development: network timeouts, rate limiting, transient database errors, downstream service outages. The Decorator pattern provides a clean, composable approach to handling all of these without polluting your business logic.

## The Problem: Cross-Cutting Concerns Everywhere

Consider a product catalog service that calls an external inventory API. In development, it is a simple HTTP call. In production, you need:

- **Retry** for transient failures (503, timeout)
- **Circuit breaker** to stop calling a dead service
- **Caching** to reduce load and improve latency
- **Logging** for debugging and monitoring
- **Metrics** for dashboards and alerting
- **Timeout** to prevent hanging requests

Without Decorator, these concerns invade the business logic. With Decorator, each concern is an independent layer.

## Building the Decorator Chain

### Step 1: Define the Interface

```csharp
public interface IInventoryClient
{
    Task<StockLevel> GetStockAsync(string sku, CancellationToken ct = default);
}
```

### Step 2: Implement the Real Client

```csharp
public class HttpInventoryClient(HttpClient http) : IInventoryClient
{
    public async Task<StockLevel> GetStockAsync(string sku, CancellationToken ct)
    {
        var response = await http.GetFromJsonAsync<StockLevel>($"/api/stock/{sku}", ct);
        return response ?? throw new InventoryNotFoundException(sku);
    }
}
```

### Step 3: Stack Decorators

```csharp
public class CachingInventoryDecorator(IInventoryClient inner, IMemoryCache cache) : IInventoryClient
{
    public async Task<StockLevel> GetStockAsync(string sku, CancellationToken ct)
    {
        return await cache.GetOrCreateAsync($"stock:{sku}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await inner.GetStockAsync(sku, ct);
        }) ?? await inner.GetStockAsync(sku, ct);
    }
}

public class LoggingInventoryDecorator(IInventoryClient inner, ILogger<LoggingInventoryDecorator> logger) : IInventoryClient
{
    public async Task<StockLevel> GetStockAsync(string sku, CancellationToken ct)
    {
        logger.LogInformation("Fetching stock for {Sku}", sku);
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await inner.GetStockAsync(sku, ct);
            logger.LogInformation("Stock for {Sku}: {Quantity} in {Ms}ms", sku, result.Available, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch stock for {Sku} after {Ms}ms", sku, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### Step 4: Wire It Up in DI

```csharp
services.AddScoped<IInventoryClient>(sp =>
    new LoggingInventoryDecorator(
        new CachingInventoryDecorator(
            new HttpInventoryClient(sp.GetRequiredService<HttpClient>()),
            sp.GetRequiredService<IMemoryCache>()),
        sp.GetRequiredService<ILogger<LoggingInventoryDecorator>>()));
```

Or with Scrutor: `services.Decorate<IInventoryClient, CachingInventoryDecorator>()`.

## Combining with Polly for Resilience

Polly's resilience pipeline IS a decorator chain over `HttpMessageHandler`:

```csharp
services.AddHttpClient<IInventoryClient, HttpInventoryClient>()
    .AddResilienceHandler("inventory", builder =>
    {
        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
        });
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15)
        });
        builder.AddTimeout(TimeSpan.FromSeconds(10));
    });
```

Each `.Add*()` call wraps the previous handler -- classic Decorator.

## Production Lessons

**Order matters.** Logging should be outermost (logs everything including retries). Caching should be before retry (cache hits skip the network entirely). Retry should be before the real client.

**Test each decorator independently.** Mock the inner `IInventoryClient` and verify that the caching decorator returns cached values, the logging decorator logs correctly, and the retry decorator retries on specific exceptions.

**Monitor the decoration.** Add metrics at each layer. A high cache miss rate means your TTL is too short. A high retry count means the downstream service is unhealthy. These metrics come naturally from having separate decorators.

**Know when to stop.** If you have 7 decorators stacked, debugging a failure requires tracing through 7 layers. Consider whether some concerns (metrics, logging) can be handled by middleware or interceptors at a higher level rather than per-service decorators.

The Decorator pattern turns resilience from a monolithic concern into a composable, testable, configurable set of behaviors. Each behavior is a single class with one responsibility, and the composition is explicit in your DI registration.
