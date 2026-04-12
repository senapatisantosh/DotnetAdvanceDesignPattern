---
title: "Why Design Patterns Still Matter in .NET 10"
contentKey: "blog-why-patterns-still-matter"
section: "blog-public"
accessLevel: "free"
contentType: "blog"
tags: ["dotnet", "design-patterns", "csharp", "software-architecture", "blog"]
order: 1
sourceType: "same_repo"
sourcePath: "blog/public/01-why-design-patterns-still-matter-in-dotnet-10.md"
routePath: "/project/dotnet-advanced-design-patterns/blog/why-design-patterns-still-matter"
isPublished: true
---

# Why Design Patterns Still Matter in .NET 10

"Design patterns are outdated." You hear this every year, and every year it is wrong. Patterns are not rigid templates from a 1994 textbook -- they are a shared vocabulary for solving recurring problems. What changes is how we implement them. Modern C# makes patterns more concise, more type-safe, and easier to compose than ever before.

## Patterns Are a Language, Not a Library

The primary value of design patterns is communication. When a senior developer says "we used Strategy for the pricing engine," every .NET developer on the team immediately understands: there is an interface, multiple implementations, and the client selects which one to use. No one needs to read the implementation to grasp the architecture.

Without this shared vocabulary, code reviews devolve into paragraph-long explanations of what amounts to a well-known structure. Patterns compress communication.

## How Modern C# Transforms Pattern Implementations

C# has evolved dramatically since the GoF book. Features introduced in recent versions make patterns more expressive with less ceremony.

**Records replace boilerplate value objects.** The Value Object pattern once required overriding `Equals`, `GetHashCode`, and `ToString`. Now it is a single line:

```csharp
public sealed record Money(decimal Amount, string Currency);
```

**Pattern matching replaces visitor chains.** The Visitor pattern's double-dispatch mechanism is still useful for extensibility, but simple type-based dispatch is cleaner with `switch` expressions:

```csharp
decimal CalculateTax(Transaction t) => t switch
{
    DomesticSale d => d.Amount * 0.08m,
    InternationalSale i => i.Amount * i.CountryTaxRate,
    Refund r => -r.OriginalTax,
    _ => throw new ArgumentException($"Unknown transaction type: {t.GetType().Name}")
};
```

**Primary constructors reduce Decorator noise.** Decorators wrap an inner component. Primary constructors eliminate the constructor-and-field boilerplate:

```csharp
public sealed class RetryApiClientDecorator(IApiClient inner, int maxRetries = 3) : IApiClient
{
    public async Task<ApiResponse> GetAsync(string url) =>
        await ExecuteWithRetryAsync(() => inner.GetAsync(url));
}
```

**Generic math and static interface members enable type-safe Strategy.** The `IParsable<T>` and `INumber<T>` interfaces allow strategies to operate on generic numeric types without boxing.

## The Anti-Pattern Trap

Knowing patterns is useful. Knowing when NOT to use them is essential. Here are the most common over-applications:

- **Strategy with one implementation.** If there is only one pricing strategy today and no realistic prospect of a second, a direct method call is simpler. Add the pattern when the second variant appears.
- **Factory for trivial construction.** If `new CustomerService()` has no parameters and no variants, a factory is overhead.
- **Repository wrapping DbContext.** If your repository methods are one-line delegations to EF Core, the abstraction provides no value.
- **Mediator for everything.** Using MediatR to mediate between a controller and a single handler adds a layer of indirection without decoupling anything meaningful.

The best engineers apply patterns surgically. They recognize the problem first, then reach for the pattern -- never the other way around.

## Five Patterns Every .NET Developer Should Start With

1. **Strategy** -- Swap algorithms at runtime. Used everywhere from pricing engines to validation pipelines.
2. **Decorator** -- Add behavior without modifying existing code. Logging, caching, retry, and circuit breaking all compose naturally as decorators.
3. **Factory Method** -- Centralize object creation. Useful whenever construction logic involves conditional decisions or configuration.
4. **Observer / Domain Events** -- Decouple producers from consumers. The foundation of event-driven architecture.
5. **Result Pattern** -- Replace exceptions for expected failures. Makes error handling explicit in the type system.

These five patterns cover the vast majority of design decisions you will face in production .NET code.

## How This Repository Helps

This repository provides production-quality implementations of each pattern, not toy examples. The Strategy pattern uses a real pricing engine with five strategies. The Decorator pattern wraps an API client with logging, caching, and retry. The Saga pattern models a complete order fulfillment workflow with compensation logic.

Every implementation includes:
- Interfaces that follow .NET naming conventions
- Async support with `CancellationToken`
- Thread-safe implementations where concurrency matters
- Tests that verify both the happy path and edge cases

Patterns are best learned by reading and modifying real code. Clone the repository, run the tests, and start experimenting.

## Patterns Are a Starting Point, Not a Destination

The GoF patterns are 30 years old. The enterprise patterns in this repository -- CQRS, Saga, Outbox, Domain Events -- emerged from real production systems in the decades since. New patterns will continue to emerge as architecture evolves.

What remains constant is the discipline: identify a recurring problem, name it, document its trade-offs, and share the solution with your team. That is what design patterns are, and that is why they still matter.
