---
title: "Top 5 Patterns Every .NET Developer Should Know"
contentKey: "blog-top-5-patterns"
section: "blog-public"
accessLevel: "free"
contentType: "blog"
tags: ["dotnet", "design-patterns", "csharp", "strategy", "decorator", "factory-method", "observer", "result-pattern", "blog"]
order: 2
sourceType: "same_repo"
sourcePath: "blog/public/02-top-5-patterns-every-dotnet-developer-should-know.md"
routePath: "/project/dotnet-advanced-design-patterns/blog/top-5-patterns-every-dotnet-developer"
isPublished: true
---

# Top 5 Patterns Every .NET Developer Should Know

After years of building production .NET systems, five patterns consistently prove their value across domains -- from e-commerce platforms to healthcare monitoring systems. These are not theoretical exercises. Each one solves a concrete problem you will encounter repeatedly.

## 1. Strategy: The Most Versatile Pattern

**The problem:** Your application needs to perform the same operation in different ways depending on context -- pricing, validation, serialization, sorting, notification delivery.

**The solution:** Define an interface for the algorithm. Create implementations for each variant. Let the caller (or DI container) select which one to use.

```csharp
public interface IPricingStrategy
{
    string Name { get; }
    decimal CalculatePrice(OrderDetails order);
}

public sealed class VolumeDiscountStrategy : IPricingStrategy
{
    public string Name => "Volume Discount";
    public decimal CalculatePrice(OrderDetails order) =>
        order.Quantity > 100 ? order.Subtotal * 0.85m : order.Subtotal;
}
```

**Why it matters:** Strategy is the gateway pattern to understanding interface-based design. Once you see how Strategy decouples the "what" from the "how," you start recognizing opportunities for it everywhere. In ASP.NET Core, the entire authentication system is built on Strategy -- `IAuthenticationHandler` with implementations for cookies, JWT, OAuth, and more.

**Watch out for:** Creating a Strategy interface when you only have one implementation and no realistic second. Start with a direct method call. Refactor to Strategy when the second variant appears.

## 2. Decorator: Composition Over Inheritance in Action

**The problem:** You need to add cross-cutting concerns -- logging, caching, retry logic, authorization -- to existing services without modifying them.

**The solution:** Wrap the original service with a decorator that implements the same interface. Each decorator adds one concern and delegates to the inner service.

```csharp
public sealed class LoggingApiClientDecorator(IApiClient inner) : IApiClient
{
    public async Task<ApiResponse> GetAsync(string url)
    {
        Log($"GET {url} -- Starting request");
        var response = await inner.GetAsync(url);
        Log($"GET {url} -- Completed: {response.StatusCode}");
        return response;
    }
}
```

**Stacking decorators** is where the pattern shines. The order matters:

```csharp
IApiClient client = new BaseApiClient();
client = new RetryApiClientDecorator(client, maxRetries: 3);
client = new CachingApiClientDecorator(client, TimeSpan.FromMinutes(5));
client = new LoggingApiClientDecorator(client);
// Logging sees everything, Caching short-circuits before retry, Retry wraps HTTP calls
```

**Why it matters:** Decorator is the single most practical structural pattern. ASP.NET Core middleware is a decorator chain. HttpClient message handlers are decorators. Polly resilience pipelines are decorators. Mastering this pattern unlocks fluent composition of behaviors.

## 3. Factory Method: Object Creation Done Right

**The problem:** Object creation involves conditional logic, configuration, or dependencies that callers should not know about.

**The solution:** Centralize creation behind a method or class that encapsulates the decision logic.

```csharp
public IPaymentProcessor Create(string provider) => provider switch
{
    "stripe" => new StripeProcessor(apiKey: _config["Stripe:Key"]),
    "paypal" => new PayPalProcessor(clientId: _config["PayPal:ClientId"]),
    _ => throw new ArgumentException($"Unknown provider: {provider}")
};
```

**Why it matters:** Factory Method prevents `new` from scattering across your codebase. When the construction logic changes (a new dependency, a different default, a new variant), you change it in one place. In .NET, keyed services (`services.AddKeyedTransient`) often replace hand-rolled factories, but understanding the pattern helps you recognize when the DI container is not enough.

**Levels of factory:** Simple Factory (one method, one switch) is an idiom for small, stable sets. Factory Method (GoF) uses inheritance to defer creation to subclasses. Abstract Factory creates families of related objects. Start simple and promote only when complexity demands it.

## 4. Observer / Domain Events: Event-Driven .NET

**The problem:** When something happens in your system (an order is placed, a payment is received, a vital sign spikes), multiple components need to react -- and you do not want the publisher to know about all of them.

**The solution:** The publisher emits an event. Interested subscribers register and react independently.

The classic Observer uses interfaces:

```csharp
public interface IVitalSignObserver
{
    void OnVitalSignReceived(VitalSignReading reading);
}

// Monitor notifies all observers
foreach (var observer in _observers)
    observer.OnVitalSignReceived(reading);
```

Domain Events take this further with typed, immutable event records and an event dispatcher:

```csharp
public sealed record OrderPlacedEvent(
    Guid OrderId, string CustomerId, decimal TotalAmount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

**Why it matters:** Event-driven design is the foundation of scalable architectures. Observer handles in-process notifications (UI updates, alert services, audit logging). Domain Events handle cross-boundary communication (inventory adjustment after order placement, email notifications after payment). The progression from Observer to Domain Events to Event Sourcing is a natural evolution as your system grows.

## 5. Result Pattern: Error Handling Without Exceptions

**The problem:** Exceptions are expensive, and using them for expected failures (validation errors, not-found conditions, business rule violations) makes control flow hard to follow.

**The solution:** Return a `Result` object that explicitly represents success or failure.

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }      // accessible only on success
    public Error Error { get; }  // accessible only on failure

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
```

Usage becomes explicit and composable:

```csharp
public Result<User> Register(RegisterCommand command)
{
    if (string.IsNullOrEmpty(command.Email))
        return Error.Validation("Email is required.");

    if (await _repo.ExistsAsync(command.Email))
        return Error.Conflict("Email already registered.");

    var user = new User(command.Email, command.Name);
    await _repo.AddAsync(user);
    return user; // implicit conversion to Result<User>
}
```

**Why it matters:** Result forces every caller to handle the failure path. There is no way to accidentally ignore an error because the compiler requires you to check `IsSuccess` before accessing `Value`. This eliminates an entire class of bugs that exception-based code is prone to: swallowed exceptions, missing try-catch blocks, and unclear failure semantics.

## Putting It All Together

These five patterns compose naturally. A pricing service might use **Strategy** for algorithm selection, wrapped in a **Decorator** for caching and logging, created by a **Factory** based on configuration, publishing price-change **Domain Events**, and returning a **Result** to handle business rule violations.

The key insight is that patterns are composable building blocks, not isolated techniques. Learning them in combination -- how Strategy feeds into Factory, how Decorator wraps any interface, how Domain Events decouple producers from consumers -- is what transforms a developer from someone who writes code into someone who designs systems.
