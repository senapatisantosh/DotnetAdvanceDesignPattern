---
title: "Anti-Patterns: Common Mistakes and How to Avoid Them"
contentKey: "anti-patterns"
section: "architecture"
accessLevel: "premium"
contentType: "doc"
tags: ["dotnet", "design-patterns", "anti-patterns", "architecture", "clean-code"]
order: 1
sourceType: "same_repo"
sourcePath: "docs/premium/architecture/01-anti-patterns.md"
routePath: "/project/dotnet-advanced-design-patterns/learn/anti-patterns"
migrationTargetPath: "premium/dotnet-advanced-design-patterns/docs/architecture/01-anti-patterns.md"
isPublished: true
---

# Anti-Patterns: Common Mistakes and How to Avoid Them

Knowing when **not** to use a pattern is just as important as knowing when to use one. This document covers the most common anti-patterns that arise from misapplying design patterns in .NET applications.

---

## Table of Contents

1. [Overengineering with Patterns](#1-overengineering-with-patterns)
2. [Premature Abstraction](#2-premature-abstraction)
3. [Singleton Abuse](#3-singleton-abuse)
4. [Service Locator Anti-Pattern](#4-service-locator-anti-pattern)
5. [God Factory](#5-god-factory)
6. [Inheritance Misuse](#6-inheritance-misuse)
7. [Pattern Obsession](#7-pattern-obsession)
8. [Repository Overuse with EF Core](#8-repository-overuse-with-ef-core)
9. [Leaky Abstraction](#9-leaky-abstraction)
10. [Fake CQRS](#10-fake-cqrs)
11. [Unnecessary Mediator Layers](#11-unnecessary-mediator-layers)
12. [Anemic Domain Model](#12-anemic-domain-model)

---

## 1. Overengineering with Patterns

### Why It Is Harmful

Adding patterns where they are not needed increases complexity, slows development, and makes the codebase harder to understand. Every pattern has a cost — more files, more indirection, more cognitive load.

### Symptoms

- A simple CRUD endpoint has 8+ classes (handler, validator, mapper, repository, specification, result, event, event handler).
- New team members take days to trace a single request through the codebase.
- Adding a simple field requires changes in 5+ files.
- The architecture diagram looks impressive but the app is a basic form-over-data system.

### Code Smells

```csharp
// OVER-ENGINEERED: Factory for two types that will never change
public interface IGreetingFactory { IGreeting Create(string type); }
public class GreetingFactory : IGreetingFactory
{
    public IGreeting Create(string type) => type switch
    {
        "hello" => new HelloGreeting(),
        "goodbye" => new GoodbyeGreeting(),
        _ => throw new ArgumentException()
    };
}

// BETTER: Just use the type directly
var greeting = isMorning ? "Hello" : "Goodbye";
```

### Better Approach

- **Start simple**. Write the obvious solution first.
- **Introduce patterns when pain appears**: duplication, tight coupling, difficult testing.
- **Rule of three**: Wait until you have three concrete cases before abstracting.
- **Measure the cost**: If the pattern adds more code than the problem it solves, skip it.

---

## 2. Premature Abstraction

### Why It Is Harmful

Abstracting too early locks you into an interface before you understand the problem space. You end up with abstractions that do not fit, forcing awkward workarounds later.

### Symptoms

- Interfaces with only one implementation and no realistic prospect of a second.
- Abstract base classes created "just in case" someone needs to extend them.
- Generic type parameters on classes that are only ever used with one type.
- `IService`, `IManager`, `IHelper` interfaces with no clear contract.

### Code Smells

```csharp
// PREMATURE: Interface for a class that will never have another implementation
public interface IEmailSender { Task Send(Email email); }
public class SmtpEmailSender : IEmailSender { /* only impl ever */ }

// The interface was created because "you might need another implementation later."
// But SMTP is the only sender and the interface just adds noise.
```

### Better Approach

- **YAGNI** (You Ain't Gonna Need It) — Do not add abstraction until you have a concrete need.
- Create interfaces when you need **testability** (mocking), **polymorphism** (multiple implementations), or **decoupling** (cross-layer boundaries).
- If you create an interface for testing only, consider whether an integration test would be better.

---

## 3. Singleton Abuse

### Why It Is Harmful

Overusing Singleton creates hidden global state, makes testing difficult, introduces tight coupling, and can cause concurrency bugs. It is the most misused GoF pattern.

### Symptoms

- Multiple `Instance` properties accessed throughout the codebase.
- Difficulty unit testing because singletons carry state between tests.
- Thread-safety bugs from shared mutable state.
- "God singletons" that accumulate responsibilities over time.
- Static access patterns like `Logger.Instance.Log()` scattered everywhere.

### Code Smells

```csharp
// ABUSED: Singleton with mutable state and static access
public class AppState
{
    private static AppState _instance;
    public static AppState Instance => _instance ??= new AppState();

    public User CurrentUser { get; set; }      // Mutable shared state!
    public string ConnectionString { get; set; } // Configuration as global state!
    public List<string> AuditLog { get; } = []; // Growing shared collection!
}

// Used throughout: AppState.Instance.CurrentUser — untestable, tightly coupled
```

### Better Approach

```csharp
// BETTER: DI container manages lifetime; inject interfaces
services.AddSingleton<ITelemetryRegistry, TelemetryRegistry>();
services.AddScoped<ICurrentUserAccessor, HttpContextUserAccessor>();
services.AddSingleton<IConfiguration>(configuration);

// Now injectable, testable, and lifetime is managed centrally
public class OrderService(ITelemetryRegistry telemetry) { }
```

**Guidelines**:
- Use DI singleton lifetime (`AddSingleton`) instead of the GoF pattern.
- Singletons should be **stateless** or have **immutable state** only.
- If a singleton accumulates responsibilities, it is becoming a God Object.
- Ask: "Would this break if two instances existed?" If no, it should not be a singleton.

---

## 4. Service Locator Anti-Pattern

### Why It Is Harmful

Service Locator hides dependencies, making classes harder to understand, test, and maintain. Instead of declaring what a class needs in its constructor, it reaches into a global container at runtime.

### Symptoms

- `IServiceProvider` injected into business logic classes.
- `GetService<T>()` or `GetRequiredService<T>()` called inside methods.
- Constructor has few parameters but the class uses many services.
- Runtime failures instead of compile-time errors when dependencies are missing.

### Code Smells

```csharp
// ANTI-PATTERN: Service Locator
public class OrderService
{
    private readonly IServiceProvider _provider;

    public OrderService(IServiceProvider provider)
    {
        _provider = provider; // Hides real dependencies
    }

    public void ProcessOrder(Order order)
    {
        var repo = _provider.GetRequiredService<IOrderRepository>();
        var emailer = _provider.GetRequiredService<IEmailSender>();
        var logger = _provider.GetRequiredService<ILogger<OrderService>>();
        // Dependencies are invisible from the constructor
    }
}
```

### Better Approach

```csharp
// BETTER: Explicit constructor injection
public class OrderService(
    IOrderRepository repository,
    IEmailSender emailSender,
    ILogger<OrderService> logger)
{
    public void ProcessOrder(Order order)
    {
        // Dependencies are visible, testable, and validated at startup
    }
}
```

**When Service Locator is acceptable**:
- In factory classes that need to resolve types at runtime based on input.
- In framework/infrastructure code (middleware, filters) where DI is limited.
- Never in domain or application logic.

---

## 5. God Factory

### Why It Is Harmful

A single factory that creates everything becomes a maintenance bottleneck, violates the Single Responsibility Principle, and grows without bound.

### Symptoms

- One factory class with dozens of `Create` methods.
- Every new type requires modifying the factory.
- The factory has dependencies on every part of the system.
- Factory class is hundreds or thousands of lines long.

### Code Smells

```csharp
// ANTI-PATTERN: God Factory
public class ServiceFactory
{
    public IPaymentProcessor CreatePaymentProcessor(string type) { /* ... */ }
    public IShippingCarrier CreateShippingCarrier(string type) { /* ... */ }
    public INotificationSender CreateNotificationSender(string type) { /* ... */ }
    public IReportGenerator CreateReportGenerator(string type) { /* ... */ }
    public IValidator CreateValidator(string type) { /* ... */ }
    // ... 20 more Create methods
}
```

### Better Approach

- **One factory per product family** — `PaymentProcessorFactory`, `ShippingCarrierFactory`.
- Use **DI container** for resolution: `IEnumerable<IPaymentProcessor>` with keyed services.
- Use **generic factory interfaces**: `IFactory<TInput, TOutput>`.
- Each factory is small, focused, and follows SRP.

---

## 6. Inheritance Misuse

### Why It Is Harmful

Using inheritance for code reuse (instead of polymorphism) creates fragile hierarchies, violates Liskov Substitution, and makes refactoring painful.

### Symptoms

- Deep inheritance chains (4+ levels).
- Base classes with `virtual` methods that subclasses override to do nothing.
- "Is-a" relationships that are actually "has-a" relationships.
- `base.DoSomething()` calls scattered throughout overrides.
- Inheriting from a class just to reuse two utility methods.

### Code Smells

```csharp
// ANTI-PATTERN: Inheritance for code reuse
public class BaseService
{
    protected void LogInfo(string message) { /* ... */ }
    protected void SendEmail(string to, string body) { /* ... */ }
    protected decimal CalculateTax(decimal amount) { /* ... */ }
}

public class OrderService : BaseService    // Order "is-a" BaseService? No.
{
    public void Process() { LogInfo("..."); SendEmail("...", "..."); }
}

public class InvoiceService : BaseService  // Invoice "is-a" BaseService? No.
{
    public void Generate() { LogInfo("..."); CalculateTax(100); }
}
```

### Better Approach

```csharp
// BETTER: Composition over inheritance
public class OrderService(
    ILogger<OrderService> logger,
    IEmailSender emailSender)
{
    public void Process()
    {
        logger.LogInformation("...");
        emailSender.Send("...", "...");
    }
}
```

**When inheritance IS appropriate**:
- Template Method pattern (well-defined algorithm skeleton).
- True "is-a" relationships with Liskov Substitution compliance.
- Framework extension points designed for inheritance (e.g., `ControllerBase`).

---

## 7. Pattern Obsession

### Why It Is Harmful

Treating patterns as goals rather than tools leads to code that is harder to read, harder to debug, and harder to change. Pattern-obsessed code often has more architecture than logic.

### Symptoms

- Every class, no matter how simple, implements a GoF pattern.
- Team discussions focus on "which pattern should we use?" rather than "what problem are we solving?"
- Code reviews reject simple solutions in favor of pattern-based ones.
- The codebase has more interfaces than implementations.
- Pattern names appear in class names unnecessarily: `OrderFactoryStrategyDecoratorProxy`.

### Code Smells

- Using Strategy for a single algorithm that will never change.
- Using Observer for one subscriber.
- Using Command for operations that never need undo, queuing, or logging.
- Using Decorator to add one behavior that could be a simple method call.

### Better Approach

- Patterns are **solutions to recurring problems**, not architectural decorations.
- Start with the simplest solution that works.
- Introduce a pattern when you experience the specific pain it solves.
- Name classes after what they DO, not what pattern they implement.
- Ask: "If I removed this pattern, would the code be worse?" If no, remove it.

---

## 8. Repository Overuse with EF Core

### Why It Is Harmful

EF Core's `DbContext` already implements Repository (via `DbSet<T>`) and Unit of Work (via `SaveChanges()`). Wrapping it in another Repository layer often adds no value and can actively prevent you from using EF Core's features effectively.

### Symptoms

- `IRepository<T>` that only wraps `DbSet<T>` methods one-to-one.
- Cannot use EF Core features like `Include()`, `ThenInclude()`, projections, or raw SQL.
- Repository methods return `IQueryable<T>` — leaking the abstraction anyway.
- Unit tests mock the repository but never test actual database behavior.
- Every new query requires a new repository method.

### Code Smells

```csharp
// ANTI-PATTERN: Thin wrapper with no added value
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public async Task<Order?> GetByIdAsync(int id)
        => await _context.Orders.FindAsync(id);  // Just forwarding!

    public async Task AddAsync(Order order)
        => await _context.Orders.AddAsync(order); // Just forwarding!

    public IQueryable<Order> GetAll()
        => _context.Orders;                       // Leaking IQueryable!
}
```

### Better Approach

- **Use `DbContext` directly** in application services if you have no real abstraction need.
- Use Repository **only when**: you need to swap ORMs, you have complex query encapsulation, or you need genuinely testable data access.
- If you use Repository, use **Specification pattern** for queries instead of adding methods.
- Write **integration tests** against a real database (TestContainers, SQLite in-memory) instead of mocking repositories.

---

## 9. Leaky Abstraction

### Why It Is Harmful

An abstraction that exposes implementation details defeats its purpose. Consumers become coupled to the concrete implementation even though they program against an interface.

### Symptoms

- Interface methods mirror the underlying technology's API.
- Consumers need to know implementation details to use the interface correctly.
- Changing the implementation requires changing all consumers.
- Exception types from the underlying technology leak through the interface.

### Code Smells

```csharp
// LEAKY: Interface exposes SQL/EF Core concepts
public interface IOrderRepository
{
    IQueryable<Order> Query();                    // Leaks IQueryable (EF Core)
    Task<Order> FromSqlRaw(string sql);           // Leaks SQL
    void Attach(Order order);                     // Leaks EF Core change tracking
    Task<int> ExecuteSqlAsync(string sql);         // Leaks SQL
}

// LEAKY: Adapter that exposes the adaptee's exceptions
public class FedExAdapter : IShippingCarrier
{
    public Task<ShipmentResult> Ship(Shipment s)
    {
        // Throws FedExApiException — consumer must know about FedEx!
        return _fedExClient.CreateShipment(s.ToFedExRequest());
    }
}
```

### Better Approach

```csharp
// BETTER: Domain-level interface with no technology leakage
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id);
    Task<IReadOnlyList<Order>> FindAsync(Specification<Order> spec);
    Task AddAsync(Order order);
}

// BETTER: Adapter catches and translates exceptions
public class FedExAdapter : IShippingCarrier
{
    public async Task<ShipmentResult> Ship(Shipment s)
    {
        try { return await _fedExClient.CreateShipment(s.ToFedExRequest()); }
        catch (FedExApiException ex)
        {
            throw new ShippingException("FedEx shipment failed", ex);
        }
    }
}
```

---

## 10. Fake CQRS

### Why It Is Harmful

Implementing CQRS in name only — where commands and queries use the same model, same database, and same ORM — adds the complexity of CQRS without any of its benefits.

### Symptoms

- Command handlers and query handlers both use the same `DbContext` and same entity models.
- Read models are just the same entities with some properties hidden.
- No separate read store, no denormalized views, no independent scaling.
- The team says "we use CQRS" but reads and writes go through the same pipeline.

### Code Smells

```csharp
// FAKE CQRS: Same model, same DbContext, just different class names
public class GetOrderQuery : IQuery<Order> { public int Id { get; set; } }
public class GetOrderHandler : IQueryHandler<GetOrderQuery, Order>
{
    private readonly AppDbContext _context; // Same context as commands!
    public async Task<Order> Handle(GetOrderQuery query)
        => await _context.Orders.FindAsync(query.Id); // Same model as commands!
}
```

### Better Approach

- **Do not adopt CQRS unless** read and write models genuinely differ.
- If you do adopt CQRS:
  - Use **separate read models** (DTOs, projections, denormalized views).
  - Consider **separate data stores** for reads if scaling requires it.
  - Use **domain events** to project write-side changes to read-side models.
- For simple CRUD, a single service class with methods is fine.

---

## 11. Unnecessary Mediator Layers

### Why It Is Harmful

Using MediatR (or a custom mediator) as a universal dispatch mechanism turns what should be direct method calls into indirect, harder-to-trace communication. The pattern is meant to reduce coupling between *many interconnected objects*, not to replace simple dependency injection.

### Symptoms

- Every controller action sends a request to MediatR that is handled by exactly one handler.
- The handler is in the same project, often in the same folder.
- You cannot "Go to Definition" on a method call — you must search for the handler.
- Pipeline behaviors add cross-cutting concerns that middleware or filters already handle.
- The mediator is used as a service locator disguised as a pattern.

### Code Smells

```csharp
// UNNECESSARY: Mediator for a simple call with one handler
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await _mediator.Send(new CreateOrderCommand(request));
    return Ok(result);
}

// The handler just calls a service:
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderService _service;
    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
        => await _service.CreateOrder(cmd.Request); // Just forwarding!
}
```

### Better Approach

```csharp
// SIMPLER: Direct injection
[HttpPost]
public async Task<IActionResult> CreateOrder(
    [FromBody] CreateOrderRequest request,
    [FromServices] IOrderService orderService)
{
    var result = await orderService.CreateOrder(request);
    return Ok(result);
}
```

**When Mediator IS valuable**:
- Multiple colleagues need to communicate without knowing about each other.
- You want a clean pipeline (validation, logging, authorization) for a large number of command/query types.
- You are implementing CQRS and want a consistent dispatch mechanism.
- You have a complex workflow where the mediator coordinates multiple services.

---

## 12. Anemic Domain Model

### Why It Is Harmful

An anemic domain model is one where domain entities are data bags (getters/setters only) and all business logic lives in service classes. This contradicts the object-oriented principle that objects should encapsulate both data *and* behavior.

### Symptoms

- Entities have only properties with public getters and setters.
- All business logic is in "service" or "manager" classes.
- Entities can be put into any state — no invariants enforced.
- Services are the only classes with behavior; entities are DTOs.
- The domain model is a data structure, not an object model.

### Code Smells

```csharp
// ANEMIC: Entity is just a data bag
public class Order
{
    public int Id { get; set; }
    public string Status { get; set; }       // Can be set to anything!
    public decimal Total { get; set; }        // Can be negative!
    public List<OrderItem> Items { get; set; } // Externally mutable!
}

// ALL logic is in the service
public class OrderService
{
    public void Submit(Order order)
    {
        if (order.Status != "Draft") throw new Exception("...");
        if (order.Items.Count == 0) throw new Exception("...");
        order.Status = "Submitted";
        order.Total = order.Items.Sum(i => i.Price * i.Quantity);
    }
}
```

### Rich Domain Model (Better Approach)

```csharp
// RICH: Entity encapsulates data AND behavior
public class Order
{
    public OrderId Id { get; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public Result Submit()
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Order is not in Draft state");
        if (_items.Count == 0)
            return Result.Failure("Order must have at least one item");

        Status = OrderStatus.Submitted;
        Total = Money.Sum(_items.Select(i => i.LineTotal));
        AddDomainEvent(new OrderSubmittedEvent(Id));
        return Result.Success();
    }

    public Result AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Cannot modify a non-draft order");
        // ... validation and item addition
    }
}
```

### Anemic vs Rich Domain Model Comparison

| Aspect                | Anemic Domain Model                      | Rich Domain Model                        |
|-----------------------|------------------------------------------|------------------------------------------|
| **Entity behavior**   | None — just properties                   | Encapsulates business rules             |
| **Invariants**        | Not enforced                             | Always valid by construction            |
| **Testing**           | Must test services + entity state combo  | Test entity behavior directly           |
| **Discoverability**   | Logic scattered across services          | Logic co-located with data              |
| **Appropriate for**   | Simple CRUD, data pipelines, DTOs        | Complex domains with business rules     |

**Important nuance**: Anemic models are not always wrong. For simple CRUD applications, data transfer, and read models, anemic objects (DTOs, records) are perfectly appropriate. The anti-pattern is using anemic models in a domain where **complex business rules exist** and should be encapsulated.

---

## Anti-Pattern Summary Table

| Anti-Pattern                | Root Cause                        | Key Fix                                      |
|-----------------------------|-----------------------------------|----------------------------------------------|
| Overengineering             | Pattern as goal, not tool         | Start simple; add patterns when pain appears |
| Premature Abstraction       | "Just in case" thinking           | Wait for concrete need (Rule of Three)       |
| Singleton Abuse              | Global state addiction            | Use DI singleton lifetime instead            |
| Service Locator             | Lazy dependency declaration       | Explicit constructor injection               |
| God Factory                 | SRP violation in creation         | One factory per product family               |
| Inheritance Misuse          | Reuse via "is-a" instead of "has-a"| Composition over inheritance                |
| Pattern Obsession           | Patterns as decorations           | Solve problems, not apply patterns           |
| Repository Overuse          | Cargo-cult architecture           | Use DbContext directly when appropriate      |
| Leaky Abstraction           | Technology details in contracts   | Domain-level interfaces only                 |
| Fake CQRS                   | CQRS cargo cult                   | Only separate when shapes differ             |
| Unnecessary Mediator        | MediatR as universal dispatch     | Direct injection when only one handler       |
| Anemic Domain Model         | OOP neglect                       | Co-locate behavior with data                 |

---

## Decision Checklist: Should I Use This Pattern?

Before introducing a pattern, ask yourself:

- [ ] **Is there a real problem?** Not a theoretical future problem, but pain you feel today.
- [ ] **Does the pattern solve THIS problem?** Not a similar problem from a blog post.
- [ ] **Is the cost justified?** More files, more indirection, more cognitive load — worth it?
- [ ] **Would a simpler solution work?** A method, a `switch`, a direct call?
- [ ] **Can my team understand it?** If you have to explain the pattern every code review, reconsider.
- [ ] **Can I remove it later?** Good abstractions are easy to remove. Bad ones are not.

> **The best pattern is the one you do not notice.** It should make the code clearer, not more impressive.

---

## Next Steps

- [Pattern Selection Guide](pattern-selection-guide.md) — Choose the right pattern for the right problem
- [Interview Cheat Sheet](interview-revision-cheatsheet.md) — Know when to argue AGAINST a pattern in interviews
- [Comparison Maps](comparison-maps.md) — Understand the subtle differences that prevent misuse
