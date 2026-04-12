# Top 5 Patterns Every .NET Developer Should Know

If you are a .NET developer building production applications, these five patterns will appear in nearly every project you work on. They are not theoretical exercises -- they are practical tools you will use weekly.

## 1. Strategy Pattern

**What it solves:** Hard-coded algorithms that need to vary based on context.

**Where you see it daily:** Payment processing (Stripe vs PayPal), notification delivery (email vs SMS vs push), pricing calculation (regular vs premium vs bulk), validation rules.

**The .NET way:**

```csharp
services.AddScoped<IPricingStrategy, RegularPricing>();
services.AddScoped<IPricingStrategy, PremiumPricing>();

// In the service, inject all strategies and select
public class PricingEngine(IEnumerable<IPricingStrategy> strategies)
{
    public decimal Calculate(Order order, string tier)
        => strategies.First(s => s.CanHandle(tier)).Calculate(order);
}
```

**Why it matters:** Without Strategy, you get switch statements that grow every time you add a new algorithm. With Strategy, adding a new pricing tier means adding one class and one DI registration.

## 2. Decorator Pattern

**What it solves:** Adding cross-cutting behavior (logging, retry, caching, metrics) without modifying the original service.

**Where you see it daily:** HttpClient handler pipelines, Polly resilience policies, MediatR pipeline behaviors, any layered service.

**The .NET way:**

```csharp
services.AddHttpClient<IWeatherApi, WeatherApiClient>()
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10))
    .AddPolicyHandler(Policy.Handle<HttpRequestException>().RetryAsync(3));
```

Every `.AddPolicyHandler()` wraps the previous handler -- that is Decorator. The `WeatherApiClient` has no idea it is being retried or timed out.

**Why it matters:** Decorator lets you add behavior to any service without modifying it. This is the Open/Closed Principle in action.

## 3. Repository Pattern (with Specification)

**What it solves:** Coupling between business logic and data access technology.

**Where you see it daily:** Any application with non-trivial data access that needs unit testing.

**When to use it:** When you need testability without a database, when persistence might change, or when query logic is complex enough to encapsulate.

**When to skip it:** For simple CRUD where `DbContext` is sufficient. Do not wrap EF Core just because "you should always have a repository." That is cargo-cult architecture.

**Why it matters:** The debate around Repository is one of the most frequent .NET architecture interview questions. Understanding when it adds value (and when it does not) demonstrates architectural maturity.

## 4. Result Pattern

**What it solves:** Using exceptions for expected failures (validation errors, not-found, conflicts).

**Where you see it daily:** API responses, domain service returns, validation pipelines.

```csharp
public async Task<Result<OrderId>> PlaceOrder(PlaceOrderCommand cmd)
{
    var customer = await _repo.FindAsync(cmd.CustomerId);
    if (customer is null) return Result<OrderId>.NotFound("Customer not found");
    if (!customer.IsActive) return Result<OrderId>.Forbidden("Customer is inactive");

    var order = Order.Create(customer, cmd.Items);
    _repo.Add(order);
    await _unitOfWork.SaveAsync();
    return Result<OrderId>.Success(order.Id);
}
```

**Why it matters:** Exceptions should be exceptional. Returning `Result<T>` makes error paths explicit in the method signature, enables railway-oriented chaining, and avoids the performance cost of exception throwing for expected failures.

## 5. CQRS (Even the Simple Version)

**What it solves:** Commands and queries that have fundamentally different requirements.

**Where you see it daily:** Any API where the read model (what the user sees) differs from the write model (what the domain enforces).

**The simple version:** Separate command handlers from query handlers. Commands return `Result<T>`. Queries return DTOs. Different pipeline behaviors apply to each (validation for commands, caching for queries). Same database, different handler classes.

**Why it matters:** Even Level 1 CQRS (same database, separate handlers) provides cleaner code organization, single-responsibility handlers, and a natural path to scale when you need it. You do not need separate databases to benefit from the mental model of "reads and writes are different."

## Honorable Mentions

- **Observer / Domain Events** for decoupled side effects
- **Builder** for constructing complex objects (especially configuration)
- **Facade** for simplifying subsystem APIs
- **Chain of Responsibility** for understanding ASP.NET Core middleware

## The Common Thread

All five patterns share one principle: **program to an interface, not an implementation.** Strategy swaps algorithms behind an interface. Decorator wraps an interface. Repository abstracts data access behind an interface. Result Pattern makes the contract explicit. CQRS separates concerns behind dedicated interfaces.

Master this principle and the patterns follow naturally.
