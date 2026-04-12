# Why Design Patterns Still Matter in .NET 10

Design patterns have been around since the Gang of Four published their book in 1994. Thirty years later, some developers argue they are outdated. With .NET 10 bringing features like improved keyed DI services, enhanced minimal APIs, and native AOT compilation, it is tempting to think that modern frameworks have made patterns obsolete. They have not -- but the way we apply them has changed dramatically.

## Patterns Are Not About the Code

The most common misconception is that design patterns are code templates you copy and paste. They are not. Patterns are **vocabulary for design decisions**. When a senior engineer says "we need a Strategy here," the entire team immediately understands the intent, the structure, and the trade-offs. That shared vocabulary is as valuable in 2026 as it was in 1994.

## What Has Changed in Modern .NET

### Dependency Injection Replaced Manual Wiring

The Singleton pattern no longer requires a static `Instance` property. You write `services.AddSingleton<T>()` and the DI container handles lifetime, thread safety, and disposal. Factory Method no longer requires inheritance hierarchies -- keyed services in .NET 8+ let you register multiple implementations and resolve by key.

### LINQ and Records Absorbed Several Patterns

Iterator is built into `IEnumerable<T>`. Value Object semantics come nearly free with `record` types. Builder patterns are less necessary when records with `with` expressions provide immutable copying with modifications.

### Middleware IS Chain of Responsibility

ASP.NET Core's middleware pipeline is Chain of Responsibility. You do not need to implement the pattern from scratch -- you need to understand the pattern to use middleware effectively. Knowing that middleware can short-circuit, that order matters, and that each layer wraps the next comes from understanding CoR.

## The Patterns That Matter Most in 2026

### For API Development
- **Decorator** for composable cross-cutting concerns (logging, retry, caching around HTTP clients)
- **Strategy** for swappable business logic (pricing, validation, notification channels)
- **Result Pattern** for explicit error handling without exceptions

### For Domain-Driven Design
- **Repository + Specification** for testable, composable data access
- **Domain Events + Outbox** for reliable side effects in distributed systems
- **Value Object** (now easier than ever with records) for eliminating primitive obsession

### For Microservices
- **CQRS** for separating read/write concerns at scale
- **Saga** for distributed transaction coordination
- **Facade** for API gateway aggregation

## The Anti-Pattern: Pattern Obsession

The most important thing patterns teach is **when NOT to use them**. A simple CRUD endpoint does not need a Factory, Repository, Specification, Unit of Work, Domain Event, and Outbox. It needs a controller that calls `DbContext.SaveChanges()`. Pattern obsession -- applying patterns where they add no value -- is the single biggest source of over-engineering in .NET codebases.

## The Bottom Line

Design patterns in .NET 10 are not about implementing classic GoF code structures. They are about understanding the **principles** behind those structures -- separation of concerns, programming to interfaces, favoring composition over inheritance -- and applying them using modern .NET idioms. The developer who understands why Decorator exists will use Polly's resilience pipeline effectively. The developer who understands CQRS will design better APIs even without a formal CQRS framework.

Patterns are not outdated. Our implementations of them have simply matured.
