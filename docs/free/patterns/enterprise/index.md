---
title: "Enterprise Patterns Index"
contentKey: "patterns-enterprise-index"
section: "patterns-index"
accessLevel: "free"
contentType: "index"
tags: ["dotnet", "design-patterns", "enterprise"]
order: 4
sourceType: "same_repo"
sourcePath: "docs/free/patterns/enterprise/index.md"
routePath: "/project/dotnet-advanced-design-patterns/preview/patterns-enterprise-index"
isPublished: true
---

# Enterprise Patterns

Enterprise patterns address the challenges of building production-grade .NET applications: data access abstraction, domain modeling, event-driven architecture, distributed systems coordination, and application configuration. These patterns are commonly found in real-world systems and are frequent topics in senior developer interviews.

---

## Repository

The Repository pattern mediates between the domain and data mapping layers, providing a collection-like interface for accessing domain objects. It abstracts data access so that business logic is independent of the persistence mechanism -- enabling swaps between EF Core and Dapper, SQL and NoSQL, or real and mock implementations. Methods like `Add()`, `Find()`, and `Remove()` make data access feel like working with an in-memory collection.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/Repository/README.md)

---

## Unit of Work

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates writing out changes as a single atomic operation. When multiple repositories participate in a business operation, Unit of Work ensures they all succeed or fail together with a single `SaveChanges()` call. In .NET, EF Core's `DbContext` already implements this pattern, but an explicit Unit of Work is useful when coordinating multiple data stores or repositories.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/UnitOfWork/README.md)

---

## Specification

The Specification pattern encapsulates query logic into composable, reusable objects. Instead of adding a new repository method for every query variant, specifications define filtering criteria that can be combined with `And()`, `Or()`, and `Not()` operators. For example, `ActiveCustomerSpec.And(PremiumTierSpec)` builds a reusable, testable predicate that translates to an efficient SQL WHERE clause through EF Core's expression tree support.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/Specification/README.md)

---

## Result Pattern

The Result Pattern returns success or failure explicitly in method signatures rather than throwing exceptions for expected failures. A `Result<T>` type carries either the successful value or error information, making error handling visible in the API contract. This avoids using exceptions for flow control (validation failures, not-found, conflicts) and enables railway-oriented programming where operations chain together, short-circuiting on the first failure.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/ResultPattern/README.md)

---

## CQRS

Command Query Responsibility Segregation (CQRS) separates the read model from the write model, allowing each to be optimized, scaled, and evolved independently. Commands mutate state through rich domain models with validation and business rules. Queries return data through lightweight DTOs or projections optimized for read performance. This separation is valuable when read and write workloads have fundamentally different shapes, frequencies, or scaling requirements.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/Cqrs/README.md)

---

## Domain Events

Domain Events represent something meaningful that happened in the domain -- `OrderPlacedEvent`, `PaymentReceivedEvent`, `InventoryReservedEvent`. They decouple the action that triggered the event from the side effects that respond to it (sending emails, updating projections, auditing). Handlers are auto-discovered through the DI container, and events can be dispatched synchronously after `SaveChanges()` or asynchronously through a message bus.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/DomainEvents/README.md)

---

## Outbox

The Outbox pattern solves the dual-write problem by writing domain events to an outbox table in the same database transaction as the domain data. A background worker then reads the outbox and publishes events to the message broker. This guarantees at-least-once delivery: if the process crashes after saving but before publishing, the worker picks up the event on restart. Consumers must handle idempotency since events may be delivered more than once.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/Outbox/README.md)

---

## Null Object

The Null Object pattern provides a no-op implementation of an interface as a safe default, eliminating null checks throughout the codebase. Instead of returning `null` and forcing callers to check `if (logger != null)`, return a `NullLogger` that implements `ILogger` with empty method bodies. The absence of behavior becomes an explicit, polymorphic design choice rather than a source of `NullReferenceException`.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/NullObject/README.md)

---

## Value Object

A Value Object is defined entirely by its attributes, not by an identity. Two `Money` objects with the same amount and currency are equal, regardless of which instance you hold. Value Objects are immutable, self-validating on construction, and provide value-based equality. They model domain concepts like Email, Address, Money, DateRange, and Temperature -- preventing primitive obsession and encapsulating validation rules within the type itself.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/ValueObject/README.md)

---

## Saga

The Saga pattern coordinates a multi-step distributed business process where each step has a compensating action. If step 3 fails, the orchestrator runs compensations for steps 2 and 1 in reverse order. Unlike distributed transactions (2PC), sagas do not hold locks -- they achieve eventual consistency through forward actions and compensations. This is essential for microservice architectures where a single business operation spans multiple services and databases.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/Saga/README.md)

---

## Options Pattern

The Options Pattern binds configuration sections from `appsettings.json` to strongly-typed C# classes using `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>`. It provides compile-time type safety, startup validation with `ValidateDataAnnotations()` or `ValidateOnStart()`, and support for hot-reload of configuration without restarting the application. This replaces scattered `Configuration["key"]` string lookups with clean, testable, injected configuration objects.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/OptionsPattern/README.md)

---

## Policy Pattern

The Policy Pattern encapsulates business rules or resilience policies as composable objects that can be combined with `And()` and `Or()` operators. Each policy evaluates to a yes/no decision with optional violation details. For example, `MinOrderPolicy.And(CreditCheckPolicy).And(InventoryPolicy)` evaluates whether an order can proceed. Policies are independently testable, and new rules can be added without modifying existing evaluation logic.

[View source and full documentation](../../../../src/DesignPatterns.Enterprise/PolicyPattern/README.md)
