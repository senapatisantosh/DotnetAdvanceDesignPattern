# Deep Dive: Decorator vs Proxy

Decorator and Proxy are the two most commonly conflated structural patterns. Both wrap an object behind the same interface, but their intent and usage differ fundamentally. This guide resolves the confusion with production examples from this repository.

## Core Distinction

| Aspect | Decorator | Proxy |
|--------|-----------|-------|
| **Intent** | Add new behavior | Control access to existing behavior |
| **Layers** | Multiple, stackable | Typically single |
| **Lifecycle** | Does not manage wrapped object lifecycle | May manage lifecycle (lazy init, pooling) |
| **Client awareness** | Client often assembles the chain | Client typically unaware |
| **DI registration** | Explicit decoration chain | Transparent substitution |

**The one-sentence test:** If you are adding something the wrapped object does not do (logging, retry, metrics), it is a Decorator. If you are controlling whether/when/how the wrapped object is accessed (auth, caching, lazy loading), it is a Proxy.

## Decorator in Production .NET

### Stacking Cross-Cutting Concerns

The most common production use of Decorator in .NET is stacking cross-cutting concerns around a service:

```csharp
// Registration order defines the execution order (outermost first)
services.AddScoped<IApiClient, HttpApiClient>();
services.Decorate<IApiClient, CachingDecorator>();
services.Decorate<IApiClient, RetryDecorator>();
services.Decorate<IApiClient, LoggingDecorator>();

// Execution flow: Logging -> Retry -> Caching -> HttpApiClient
```

Each decorator implements `IApiClient`, holds a reference to the next `IApiClient` in the chain, and adds its behavior before/after delegating:

```csharp
public class LoggingDecorator : IApiClient
{
    private readonly IApiClient _inner;
    private readonly ILogger<LoggingDecorator> _logger;

    public async Task<Response> SendAsync(Request request)
    {
        _logger.LogInformation("Sending request to {Url}", request.Url);
        var sw = Stopwatch.StartNew();
        var response = await _inner.SendAsync(request);
        _logger.LogInformation("Received {Status} in {Ms}ms", response.Status, sw.ElapsedMilliseconds);
        return response;
    }
}
```

### DI Integration with Scrutor

The `Scrutor` library (or manual registration) enables the `.Decorate<TInterface, TDecorator>()` pattern. Without it, you must wire decorators manually:

```csharp
services.AddScoped<IApiClient>(sp =>
    new LoggingDecorator(
        new RetryDecorator(
            new CachingDecorator(
                new HttpApiClient(sp.GetRequiredService<HttpClient>())),
            maxRetries: 3),
        sp.GetRequiredService<ILogger<LoggingDecorator>>()));
```

This is verbose but makes the decoration chain explicit and testable.

## Proxy in Production .NET

### Virtual Proxy (Lazy Loading)

```csharp
public class LazyServiceProxy : IExpensiveService
{
    private readonly Lazy<IExpensiveService> _inner;

    public LazyServiceProxy(Func<IExpensiveService> factory)
    {
        _inner = new Lazy<IExpensiveService>(factory);
    }

    public Result DoWork(Request request) => _inner.Value.DoWork(request);
}
```

The client has no idea the real service is not created until first use. This is Proxy, not Decorator, because no behavior is added -- access is deferred.

### Protection Proxy (Authorization)

```csharp
public class AuthorizationProxy : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ICurrentUser _user;

    public async Task<Order> GetOrder(int id)
    {
        var order = await _inner.GetOrder(id);
        if (order.CustomerId != _user.Id && !_user.IsAdmin)
            throw new UnauthorizedAccessException();
        return order;
    }
}
```

### Caching Proxy

```csharp
public class CachingInventoryProxy : IInventoryService
{
    private readonly IInventoryService _inner;
    private readonly IMemoryCache _cache;

    public async Task<StockLevel> GetStockLevel(string sku)
    {
        return await _cache.GetOrCreateAsync($"stock:{sku}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.GetStockLevel(sku);
        });
    }
}
```

## The Gray Area: Caching

Caching is the pattern that blurs the Decorator/Proxy line most often. The distinction:

- **Caching Proxy:** The cache is an intrinsic part of how the service operates. The client does not know or care. One layer.
- **Caching Decorator:** Caching is one of several composable behaviors (logging + retry + caching). The client or DI config assembles the chain. Stackable.

**Practical rule:** If caching is the ONLY cross-cutting concern, use a Proxy. If caching is one of SEVERAL concerns being composed, use a Decorator.

## When to Use Which

| Scenario | Pattern | Why |
|----------|---------|-----|
| Add logging + retry + metrics around a service | Decorator | Multiple stackable behaviors |
| Lazy-load an expensive dependency | Proxy (Virtual) | Controlling lifecycle, not adding behavior |
| Enforce authorization before data access | Proxy (Protection) | Controlling access |
| Cache API responses transparently | Proxy (Caching) | Single concern, transparent to client |
| Build a composable middleware pipeline | Decorator | Stackable, orderable behaviors |
| Rate-limit calls to an external API | Could be either | Proxy if it is the sole concern; Decorator if combined with retry/logging |

## Common Mistakes

1. **Calling everything a "wrapper."** Both patterns wrap, but the intent matters for maintainability and communication with your team.
2. **Stacking proxies.** If you find yourself stacking 3+ proxies, you actually have decorators. Proxies are typically single-layer.
3. **Decorator without interface.** Both patterns require the wrapper and the wrapped object to share an interface. Without it, you just have a wrapper class with no polymorphism.
4. **Forgetting disposal.** If the wrapped object is `IDisposable`, the outermost decorator/proxy must propagate `Dispose()`.

## Interview-Worthy Insights

- ASP.NET Core middleware is a **Decorator chain** -- each middleware wraps `RequestDelegate` and adds behavior (auth, logging, exception handling).
- `HttpMessageHandler` in `HttpClient` is another Decorator chain -- `RetryHandler(LoggingHandler(SocketsHttpHandler))`.
- EF Core's lazy loading proxies are **Virtual Proxies** -- navigation properties are not loaded until accessed.
- Polly's resilience pipeline (retry, circuit breaker, timeout) is a **Decorator chain** over `HttpClient`.
- The key interview distinction: Decorator is about **enrichment** (adding value). Proxy is about **control** (gatekeeping access).
