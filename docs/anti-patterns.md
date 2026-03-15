# Design Pattern Anti-Patterns Guide

> A comprehensive guide to common misuses and abuses of design patterns in .NET applications. Recognizing these anti-patterns is just as important as knowing the patterns themselves.

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
12. [Anemic Domain Model vs Rich Domain Model](#12-anemic-domain-model-vs-rich-domain-model)

---

## 1. Overengineering with Patterns

### Description

Overengineering with patterns occurs when developers apply design patterns to problems that could be solved with straightforward, simple code. A three-line method becomes a Strategy pattern with an interface, two concrete classes, a factory, and a DI registration -- all to choose between two options.

### Why It's Harmful

- Increases codebase complexity disproportionately to the problem being solved.
- Makes onboarding new developers harder; they must learn the pattern before understanding simple logic.
- Adds maintenance burden: every change requires touching multiple files.
- Slows development velocity for zero architectural benefit.
- Creates indirection that makes debugging significantly harder.

### Symptoms / Code Smells

- A pattern implementation has only one concrete class and is unlikely to ever have more.
- You spend more time wiring up the pattern than writing the actual business logic.
- Simple `if/else` logic is replaced by a full Strategy or Chain of Responsibility.
- The codebase has more interfaces than classes that contain real logic.
- A colleague asks "why is this so complicated?" and you cannot give a concrete, present-tense reason.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Strategy pattern for a simple tax calculation with only two cases
public interface ITaxCalculationStrategy
{
    decimal Calculate(decimal amount);
}

public class StandardTaxStrategy : ITaxCalculationStrategy
{
    public decimal Calculate(decimal amount) => amount * 0.2m;
}

public class ReducedTaxStrategy : ITaxCalculationStrategy
{
    public decimal Calculate(decimal amount) => amount * 0.05m;
}

public class TaxCalculationStrategyFactory
{
    public ITaxCalculationStrategy Create(string productType)
    {
        return productType switch
        {
            "food" => new ReducedTaxStrategy(),
            _ => new StandardTaxStrategy()
        };
    }
}

// Usage: 4 classes and an interface for two multiplications
var strategy = _factory.Create(product.Type);
var tax = strategy.Calculate(product.Price);
```

### The Better Alternative

```csharp
// SIMPLE: Just use a method
public static decimal CalculateTax(string productType, decimal amount)
{
    var rate = productType == "food" ? 0.05m : 0.2m;
    return amount * rate;
}

// If rates grow to 10+, THEN consider a pattern
```

### Takeaway

> **If your pattern has one implementation and no realistic prospect of more, delete the interface and write a method.**

---

## 2. Premature Abstraction

### Description

Premature abstraction is the practice of creating interfaces, abstract classes, and abstraction layers before you have a concrete, proven need for them. It is driven by "what if we need to swap this later" thinking rather than present requirements.

### Why It's Harmful

- You cannot design a good abstraction until you understand the problem space; early abstractions are almost always wrong.
- Wrong abstractions are harder to fix than no abstractions -- they calcify into the codebase.
- Every interface adds a navigation hop during debugging (F12 goes to the interface, not the implementation).
- Violates YAGNI (You Aren't Gonna Need It).
- Creates a false sense of flexibility that rarely gets exercised.

### Symptoms / Code Smells

- Every class has a matching `I{ClassName}` interface with an identical method set.
- Interfaces have exactly one implementation and no tests mock them.
- You hear "we might need to swap the database someday" as justification, but it has never happened in the project's lifetime.
- Abstract base classes exist with a single derived class.
- Generic type parameters used where a concrete type would suffice.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Interface created "just in case"
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public interface IEmailSenderFactory
{
    IEmailSender Create();
}

public interface IEmailTemplateRenderer
{
    string Render(string templateName, object model);
}

// Only one implementation exists for each, registered in DI
public class SmtpEmailSender : IEmailSender { /* ... */ }
public class RazorEmailTemplateRenderer : IEmailTemplateRenderer { /* ... */ }
public class EmailSenderFactory : IEmailSenderFactory { /* ... */ }

// 3 interfaces, 3 classes, for sending an email
// Nobody has ever swapped any of these implementations
```

### The Better Alternative

```csharp
// START CONCRETE: Extract an interface when you actually need a second implementation
public class EmailSender
{
    public async Task SendAsync(string to, string subject, string body)
    {
        // SMTP logic here
    }
}

// When (and IF) you need a SendGrid implementation, THEN extract IEmailSender.
// The refactoring takes 5 minutes with modern IDEs.
```

### Takeaway

> **Extract an interface when you have two implementations, not when you imagine you might.**

---

## 3. Singleton Abuse

### Description

Singleton abuse occurs when the Singleton pattern is used as a mechanism to create globally accessible mutable state, rather than for its intended purpose of ensuring a single instance of a resource that genuinely requires exclusivity (e.g., a thread pool, a hardware interface).

### Why It's Harmful

- Introduces hidden global state that makes behavior unpredictable.
- Makes unit testing extremely difficult: tests share state and cannot run in parallel.
- Creates tight coupling: every consumer is coupled to the singleton's concrete class.
- Violates the Single Responsibility Principle by combining "ensure one instance" with business logic.
- Hides dependencies -- callers use `MyService.Instance` instead of receiving the dependency explicitly.

### Symptoms / Code Smells

- `static Instance` properties scattered throughout the codebase.
- Test failures that only happen when tests run in a specific order.
- "Reset" methods on singletons used to clean up between tests.
- Singletons holding mutable collections, configuration, or session state.
- You cannot instantiate a class in isolation because it depends on a singleton.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Singleton as global mutable state
public class UserSession
{
    private static readonly Lazy<UserSession> _instance =
        new(() => new UserSession());

    public static UserSession Instance => _instance.Value;

    public string CurrentUserId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> SessionData { get; } = new();

    private UserSession() { }
}

// Usage: hidden dependency, untestable
public class OrderService
{
    public void PlaceOrder(Order order)
    {
        // Where does this dependency come from? Nobody knows from the constructor.
        if (!UserSession.Instance.Permissions.Contains("place_orders"))
            throw new UnauthorizedException();

        order.PlacedBy = UserSession.Instance.CurrentUserId;
    }
}
```

### The Better Alternative

```csharp
// BETTER: Use DI with a scoped lifetime
public interface IUserContext
{
    string UserId { get; }
    IReadOnlyList<string> Permissions { get; }
}

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;
    public HttpUserContext(IHttpContextAccessor accessor) => _accessor = accessor;
    public string UserId => _accessor.HttpContext?.User?.FindFirst("sub")?.Value;
    public IReadOnlyList<string> Permissions => /* extract from claims */;
}

// Register as scoped -- one instance per request, not per application
services.AddScoped<IUserContext, HttpUserContext>();

public class OrderService
{
    private readonly IUserContext _userContext; // Explicit dependency

    public OrderService(IUserContext userContext) => _userContext = userContext;

    public void PlaceOrder(Order order)
    {
        if (!_userContext.Permissions.Contains("place_orders"))
            throw new UnauthorizedException();

        order.PlacedBy = _userContext.UserId;
    }
}
```

### Takeaway

> **If your singleton holds mutable state, it is global state in disguise -- use scoped DI registration instead.**

---

## 4. Service Locator Anti-Pattern

### Description

The Service Locator pattern provides a centralized registry from which any class can request any dependency at runtime. While it solves the dependency resolution problem, it does so by hiding the dependency graph from consumers and making it impossible to know what a class needs without reading its entire implementation.

### Why It's Harmful

- **Hides dependencies**: The constructor does not reveal what the class needs. You must read every method to discover calls to `serviceLocator.GetService<T>()`.
- **Breaks compile-time safety**: Missing registrations only surface at runtime, often in production.
- **Makes testing harder**: You must set up the entire service locator with all transitive dependencies, rather than passing in focused mocks.
- **Violates the Dependency Inversion Principle**: Classes depend on the locator (a concrete mechanism) rather than on abstractions passed to them.
- **Defeats static analysis**: Tools cannot trace the dependency graph.

### Symptoms / Code Smells

- `IServiceProvider` or a custom `ServiceLocator` injected into business logic classes.
- `GetService<T>()` or `GetRequiredService<T>()` calls outside of composition roots, factories, or middleware.
- NullReferenceExceptions in production caused by unregistered services.
- Test setup that involves configuring a full DI container.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Service Locator injected into business logic
public class OrderProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public OrderProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessAsync(Order order)
    {
        // What does this class depend on? You can't tell from the constructor.
        var validator = _serviceProvider.GetRequiredService<IOrderValidator>();
        var repository = _serviceProvider.GetRequiredService<IOrderRepository>();
        var emailSender = _serviceProvider.GetRequiredService<IEmailSender>();
        var logger = _serviceProvider.GetRequiredService<ILogger<OrderProcessor>>();

        if (!validator.Validate(order))
            return;

        await repository.SaveAsync(order);
        await emailSender.SendConfirmationAsync(order);
        logger.LogInformation("Order {OrderId} processed", order.Id);
    }
}
```

### The Better Alternative

```csharp
// BETTER: Constructor injection -- dependencies are explicit
public class OrderProcessor
{
    private readonly IOrderValidator _validator;
    private readonly IOrderRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(
        IOrderValidator validator,
        IOrderRepository repository,
        IEmailSender emailSender,
        ILogger<OrderProcessor> logger)
    {
        _validator = validator;
        _repository = repository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessAsync(Order order)
    {
        if (!_validator.Validate(order))
            return;

        await _repository.SaveAsync(order);
        await _emailSender.SendConfirmationAsync(order);
        _logger.LogInformation("Order {OrderId} processed", order.Id);
    }
}
```

### Takeaway

> **`IServiceProvider` belongs in your composition root, not in your business logic -- inject the dependency, not the container.**

---

## 5. God Factory

### Description

A God Factory is a single factory class that is responsible for creating many unrelated types. It grows over time as developers add "just one more" creation method, eventually becoming a monolithic class that violates the Single Responsibility Principle and becomes a bottleneck for changes.

### Why It's Harmful

- Violates SRP: one class has reasons to change for every type it creates.
- Creates a coupling hub: every consumer depends on the factory, and the factory depends on everything.
- Makes the DI container redundant -- the factory *becomes* a hand-rolled container.
- Merge conflicts are frequent because many developers touch the same file.
- Testing requires mocking a massive interface.

### Symptoms / Code Smells

- A factory with 10+ `Create` methods.
- The factory's constructor has 15+ dependencies (one for each type it can create).
- New feature requests always require modifying the factory.
- The factory class is over 200 lines.
- Method names like `CreateUserService`, `CreateOrderValidator`, `CreateEmailSender` in a single class.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: One factory to rule them all
public class ServiceFactory
{
    private readonly IConfiguration _config;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDbConnectionFactory _dbFactory;
    // ... 12 more dependencies

    public ServiceFactory(IConfiguration config, ILoggerFactory loggerFactory,
        IDbConnectionFactory dbFactory /* ... */)
    {
        _config = config;
        _loggerFactory = loggerFactory;
        _dbFactory = dbFactory;
    }

    public IOrderService CreateOrderService() => new OrderService(/* ... */);
    public IUserService CreateUserService() => new UserService(/* ... */);
    public IPaymentProcessor CreatePaymentProcessor(string provider) => /* ... */;
    public IEmailSender CreateEmailSender() => /* ... */;
    public IReportGenerator CreateReportGenerator(ReportType type) => /* ... */;
    public INotificationService CreateNotificationService() => /* ... */;
    // ... 10 more methods, growing every sprint
}
```

### The Better Alternative

```csharp
// BETTER: Focused factories for each family, or just use DI
// Factory only where runtime decisions are needed:
public class PaymentProcessorFactory
{
    private readonly IServiceProvider _sp;

    public PaymentProcessorFactory(IServiceProvider sp) => _sp = sp;

    public IPaymentProcessor Create(string provider) => provider switch
    {
        "stripe" => _sp.GetRequiredService<StripeProcessor>(),
        "paypal" => _sp.GetRequiredService<PayPalProcessor>(),
        _ => throw new ArgumentException($"Unknown provider: {provider}")
    };
}

// For services that don't need runtime selection, just inject them directly:
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IUserService, UserService>();
```

### Takeaway

> **A factory should create one family of related objects -- if it creates everything, it's a disguised service locator.**

---

## 6. Inheritance Misuse

### Description

Inheritance misuse manifests as deep inheritance hierarchies (3+ levels) used to share code rather than to model genuine "is-a" relationships. Developers use inheritance for code reuse, creating fragile base classes and hierarchies that are difficult to modify without cascading side effects.

### Why It's Harmful

- **Fragile Base Class Problem**: Changes to a base class can break all derived classes in unexpected ways.
- **Tight coupling**: Derived classes are intimately coupled to the base class's implementation details.
- **Inflexible**: A class can only inherit from one base (in C#), so the hierarchy decision is permanent.
- **Liskov Substitution Violations**: Derived classes often override methods in ways that break the base contract.
- **Combinatorial explosion**: If you have N dimensions of variation, inheritance requires N! classes.

### Symptoms / Code Smells

- Inheritance hierarchies deeper than 2 levels.
- Base classes with `virtual` methods that most derived classes override to do nothing.
- `protected` fields and methods used extensively for "sharing" state.
- Derived classes that throw `NotSupportedException` for inherited methods.
- You need a combination of behaviors from two branches of the hierarchy.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Deep hierarchy for code reuse
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    protected virtual void Validate() { }
}

public abstract class AuditableEntity : BaseEntity
{
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    protected virtual void AuditChange(string userId) { /* ... */ }
}

public abstract class SoftDeletableAuditableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public virtual void SoftDelete() { IsDeleted = true; DeletedAt = DateTime.UtcNow; }
}

public abstract class TenantScopedSoftDeletableAuditableEntity : SoftDeletableAuditableEntity
{
    public Guid TenantId { get; set; }
    protected virtual void ValidateTenant() { /* ... */ }
}

// Now every entity must decide where in this 4-level hierarchy it belongs.
// What if you need tenant-scoped but NOT soft-deletable? You can't.
public class Order : TenantScopedSoftDeletableAuditableEntity
{
    // Inherits 4 levels of behavior whether it wants it or not
}
```

### The Better Alternative

```csharp
// BETTER: Composition with interfaces and mixins
public interface IAuditable
{
    string CreatedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}

// Compose only what you need:
public class Order : IAuditable, ITenantScoped
{
    public Guid Id { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid TenantId { get; set; }
    // No soft-delete: we don't need it for orders
}

// Shared behavior via extension methods or EF Core interceptors, not base classes
public static class SoftDeleteExtensions
{
    public static void SoftDelete(this ISoftDeletable entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
    }
}
```

### Takeaway

> **Favor composition over inheritance -- if you are inheriting to share code rather than to model an "is-a" relationship, use interfaces and composition instead.**

---

## 7. Pattern Obsession

### Description

Pattern obsession is the compulsion to use a named design pattern for every piece of code, regardless of whether the problem warrants it. It often stems from recently learning patterns and wanting to apply them everywhere, or from a belief that "patterned code" is inherently superior.

### Why It's Harmful

- Introduces unnecessary abstraction layers that obscure simple logic.
- Inflates the codebase: what should be 50 lines becomes 500 lines across 12 files.
- Creates a vocabulary barrier: new team members must learn the pattern taxonomy before reading code.
- Shifts focus from solving the business problem to satisfying the pattern's structure.
- Every pattern has a cost (indirection, complexity); without a benefit, you only get the cost.

### Symptoms / Code Smells

- Developers discuss solutions in terms of which pattern to apply rather than what the code should do.
- Code reviews insist on pattern names rather than evaluating whether the code is clear and correct.
- A simple CRUD controller has a mediator, a command, a handler, a validator, a repository, a unit of work, a specification, and a mapper -- for saving a single record.
- The word "pattern" appears in commit messages more than the business domain.
- Architectural diagrams show pattern names but not data flow.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Pattern salad for updating a user's email address
// 1. Command
public record UpdateEmailCommand(Guid UserId, string NewEmail) : IRequest<Result>;

// 2. Validator
public class UpdateEmailCommandValidator : AbstractValidator<UpdateEmailCommand>
{
    public UpdateEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail).EmailAddress();
    }
}

// 3. Handler
public class UpdateEmailCommandHandler : IRequestHandler<UpdateEmailCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ISpecification<User> _spec;

    public UpdateEmailCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateEmailCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.FindBySpecAsync(new UserByIdSpec(request.UserId));
        user.UpdateEmail(request.NewEmail);    // Rich domain method
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

// 4. Specification
public class UserByIdSpec : Specification<User>
{
    public UserByIdSpec(Guid id) => Query.Where(u => u.Id == id);
}

// 5. Domain Event
public record EmailUpdatedEvent(Guid UserId, string OldEmail, string NewEmail) : IDomainEvent;

// Total: 5 files, ~80 lines for changing one column in one table
```

### The Better Alternative

```csharp
// BETTER: Appropriate complexity for the problem
public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<bool> UpdateEmailAsync(Guid userId, string newEmail)
    {
        if (!IsValidEmail(newEmail)) return false;

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        user.Email = newEmail;
        await _db.SaveChangesAsync();
        return true;
    }

    private static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@');
}
```

### Takeaway

> **Patterns are tools, not goals -- if you cannot articulate the specific problem a pattern solves in your current code, do not use it.**

---

## 8. Repository Overuse with EF Core

### Description

Repository overuse with EF Core is the practice of wrapping `DbContext` in a custom Repository and Unit of Work layer that adds no meaningful abstraction. Since EF Core's `DbSet<T>` already implements the Repository pattern and `DbContext` already implements Unit of Work, the custom layer simply proxies calls with zero added value.

### Why It's Harmful

- **Double abstraction**: You wrap a repository (DbSet) in another repository, and a unit of work (DbContext) in another unit of work.
- **Feature loss**: Custom repositories often expose only a subset of EF Core's capabilities, leading developers to bypass the repository to access features like `Include()`, projections, or raw SQL.
- **False portability promise**: The justification is "we might switch ORMs" -- this almost never happens, and when it does, the repository abstraction is never sufficient anyway.
- **Maintenance tax**: Every new query requires adding a method to the repository interface and implementation.
- **Leaky abstraction**: The repository inevitably starts returning `IQueryable<T>`, at which point it is just a passthrough to DbSet.

### Symptoms / Code Smells

- `IRepository<T>` with `GetAll()`, `GetById()`, `Add()`, `Update()`, `Delete()` that directly call DbSet methods.
- Repository methods returning `IQueryable<T>` (defeats the purpose of the abstraction).
- Developers bypassing the repository to use `DbContext` directly because the repository does not expose a needed feature.
- A `IUnitOfWork` interface whose only method is `SaveChangesAsync()` -- identical to what DbContext already provides.
- Every new database query requires modifying the repository class.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Wrapper that adds nothing
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query(); // Leaks EF Core abstraction
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
    public IQueryable<T> Query() => _dbSet.AsQueryable(); // Just returns DbSet!
}

public interface IUnitOfWork
{
    IRepository<Order> Orders { get; }
    IRepository<Product> Products { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
// This is just DbContext with extra steps.
```

### The Better Alternative

```csharp
// BETTER: Use DbContext directly for simple CRUD
public class OrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<Order?> GetOrderAsync(int id) =>
        await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
}

// Use the Specification pattern ONLY when you have complex, reusable query logic:
public class ActiveOrdersSpec : Specification<Order>
{
    public ActiveOrdersSpec(Guid customerId)
    {
        Query
            .Where(o => o.CustomerId == customerId && o.Status != OrderStatus.Cancelled)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);
    }
}
```

### Takeaway

> **EF Core's DbContext is already a Repository + Unit of Work -- wrapping it in another one only hides its features behind an inferior API.**

---

## 9. Leaky Abstraction

### Description

A leaky abstraction is an abstraction that exposes implementation details to its consumers, forcing them to understand the underlying mechanism to use the abstraction correctly. The abstraction promises to hide complexity but fails to contain it, and consumers end up coupled to the implementation anyway.

### Why It's Harmful

- Defeats the purpose of the abstraction: consumers still need to know the internals.
- Creates hidden coupling: code appears decoupled but is functionally coupled.
- Breaking changes in the implementation break consumers, even though there is an "abstraction" layer.
- Makes testing unreliable: mocks may behave differently from the real implementation in subtle ways the leaky interface cannot capture.
- Gives a false sense of encapsulation.

### Symptoms / Code Smells

- Interfaces that expose `IQueryable<T>` (leaks the ORM's query engine).
- Abstractions whose method signatures include implementation-specific types (e.g., `SqlParameter`, `HttpRequestMessage`).
- Consumers that must call methods in a specific order because the abstraction does not manage its own state.
- Exception types from the underlying library leaking through the abstraction.
- Documentation that says "Note: this behaves differently when the underlying implementation is X."

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Abstraction that leaks IQueryable and EF Core specifics
public interface IOrderRepository
{
    IQueryable<Order> GetOrders(); // Leaks: consumers build EF Core queries
    Task SaveAsync(Order order, bool useTransaction = false); // Leaks: exposes DB concept
}

public class OrderController
{
    private readonly IOrderRepository _repo;

    public async Task<IActionResult> GetActiveOrders()
    {
        // Consumer must know EF Core's Include/Where/Select syntax
        var orders = await _repo.GetOrders()
            .Include(o => o.Items)          // EF Core specific
            .ThenInclude(i => i.Product)    // EF Core specific
            .Where(o => o.Status == Status.Active)
            .Select(o => new OrderDto       // EF Core projection
            {
                Id = o.Id,
                Total = o.Items.Sum(i => i.Price)
            })
            .ToListAsync();                 // EF Core specific

        return Ok(orders);
    }
}
// The "abstraction" provides zero encapsulation.
```

### The Better Alternative

```csharp
// BETTER: Abstraction that fully encapsulates the query
public interface IOrderRepository
{
    Task<IReadOnlyList<OrderSummary>> GetActiveOrdersAsync(Guid customerId);
    Task<Order?> GetByIdWithItemsAsync(Guid orderId);
    Task SaveAsync(Order order);
}

// The implementation handles EF Core details internally
public class EfOrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public async Task<IReadOnlyList<OrderSummary>> GetActiveOrdersAsync(Guid customerId)
    {
        return await _db.Orders
            .Where(o => o.CustomerId == customerId && o.Status == Status.Active)
            .Select(o => new OrderSummary(o.Id, o.Items.Sum(i => i.Price)))
            .ToListAsync();
    }
}
// Consumers call a domain-meaningful method and get a DTO. No EF knowledge needed.
```

### Takeaway

> **If consumers need to understand the implementation to use the abstraction, the abstraction does not exist -- it is just an extra layer of indirection.**

---

## 10. Fake CQRS

### Description

Fake CQRS is the practice of separating code into Command and Query handlers (often with MediatR) while still using the same database, the same models, and the same data access logic for both reads and writes. It applies the structural ceremony of CQRS without any of its architectural benefits.

### Why It's Harmful

- Adds the complexity cost of CQRS (separate handler classes, separate models, message dispatch) with none of the benefits (independent scaling, optimized read models, event sourcing).
- Doubles the number of classes: every operation now has a request, a handler, and often a validator and a mapper.
- Creates the illusion of architectural sophistication while the system is functionally identical to a simple service layer.
- Makes simple CRUD operations take 4x as long to implement.
- Misleads architects into thinking the system is "CQRS-ready" when it is not.

### Symptoms / Code Smells

- `GetOrderByIdQuery` and `CreateOrderCommand` both inject the same `DbContext` and hit the same table.
- Read and write models are identical or trivially different (one has an extra property).
- No separate read database, no projections, no event sourcing.
- The only reason for CQRS is "we use MediatR and it's the recommended pattern."
- Command handlers return data (e.g., `Task<OrderDto>`) -- real commands return void or an acknowledgment.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: CQRS ceremony with CRUD reality
public record GetOrderQuery(Guid Id) : IRequest<OrderDto>;
public record CreateOrderCommand(string Product, int Qty) : IRequest<OrderDto>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly AppDbContext _db; // Same DbContext

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync(request.Id); // Same table
        return new OrderDto(order.Id, order.Product, order.Qty); // Same model, mapped
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _db; // Same DbContext, same table, same model

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order { Product = request.Product, Qty = request.Qty };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return new OrderDto(order.Id, order.Product, order.Qty); // Command returns data!
    }
}
// Two handler classes, two request classes, same database, same model. This is just CRUD.
```

### The Better Alternative

```csharp
// OPTION A: If it's CRUD, just do CRUD
public class OrderService
{
    private readonly AppDbContext _db;

    public async Task<OrderDto> GetAsync(Guid id) { /* ... */ }
    public async Task<Guid> CreateAsync(CreateOrderRequest request) { /* ... */ }
}

// OPTION B: If you genuinely need CQRS, commit to it
// - Separate read database (e.g., denormalized SQL views, Redis, Elasticsearch)
// - Write side publishes events
// - Read side subscribes and maintains its own projections
// - Commands return void or an ID, never the full entity
```

### Takeaway

> **If your commands and queries hit the same database with the same models, you have a service layer with extra steps -- not CQRS.**

---

## 11. Unnecessary Mediator Layers

### Description

Unnecessary mediator layers involve using a library like MediatR to dispatch every request from a controller to a handler, even when the handler simply calls a service method. The mediator becomes an indirection layer that adds hop count and complexity without providing cross-cutting concern benefits or decoupling benefits.

### Why It's Harmful

- **Indirection without benefit**: Controller calls mediator, mediator calls handler, handler calls service. The mediator hop adds nothing.
- **Hides the dependency graph**: You cannot tell from a controller what services it depends on -- you must trace through the mediator.
- **F12 navigation is broken**: "Go to definition" on `Send()` takes you to MediatR internals, not your handler.
- **Testing becomes indirect**: You must either test through the mediator or test the handler in isolation, but not naturally test the controller-to-handler flow.
- **Performance cost**: Reflection-based dispatch, pipeline behaviors, and DI resolution add latency for no architectural gain.

### Symptoms / Code Smells

- Controllers have a single dependency: `IMediator`.
- Handlers have a single line that delegates to a service.
- No pipeline behaviors (logging, validation, caching) are configured -- the mediator is just a message router.
- The team cannot explain why they use MediatR beyond "it's best practice."
- Removing MediatR and calling the service directly would not change any behavior.

### Code Example: The Anti-Pattern

```csharp
// ANTI-PATTERN: Mediator as a passthrough layer
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await _mediator.Send(new GetProductQuery(id)));
}

public record GetProductQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IProductService _service;

    public GetProductQueryHandler(IProductService service) => _service = service;

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken ct)
    {
        return await _service.GetByIdAsync(request.Id); // Just a passthrough!
    }
}
// Controller -> MediatR -> Handler -> Service. The middle two layers add nothing.
```

### The Better Alternative

```csharp
// BETTER: Inject the service directly
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
        => _productService = productService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await _productService.GetByIdAsync(id));
}

// USE MediatR when you actually benefit from it:
// - Pipeline behaviors for cross-cutting concerns (validation, logging, caching)
// - Decoupling between modules in a modular monolith
// - Domain event dispatch where the publisher should not know the subscribers
```

### Takeaway

> **MediatR earns its place through pipeline behaviors and decoupling -- if you are just routing calls, inject the service directly.**

---

## 12. Anemic Domain Model vs Rich Domain Model

### Description

This is not strictly an anti-pattern in all contexts, but a design tension that must be understood. An **Anemic Domain Model** has entities that are pure data containers (getters/setters) with all behavior in external services. A **Rich Domain Model** encapsulates behavior within the entity itself, enforcing invariants through its methods.

### When Anemic Is Appropriate

- Simple CRUD applications where domain logic is minimal.
- Applications dominated by data transformation rather than business rules.
- Teams with strong service-layer conventions and limited DDD experience.
- Prototypes and MVPs where speed of delivery matters more than model purity.
- Reporting and read-heavy applications.

### When Anemic Is Harmful

- Complex business domains with many invariants (e.g., financial systems, booking systems).
- When business rules are duplicated across multiple services because entities cannot enforce them.
- When developers must remember to call validation before saving -- the entity cannot protect itself.
- When the domain model is shared with external systems and must be self-consistent.

### When Rich Is Appropriate

- Domains with complex invariants that must always be enforced (e.g., "an order cannot be shipped unless it is paid").
- Applications following Domain-Driven Design.
- Long-lived projects where the cost of bugs from unenforced invariants exceeds the cost of richer models.
- When multiple entry points (API, message handler, background job) all must enforce the same rules.

### When Rich Is Harmful (Over-Applied)

- Simple CRUD where adding behavior to entities is ceremony without benefit.
- When it leads to entities depending on infrastructure services (repositories, HTTP clients).
- When the team lacks DDD experience and produces "smart entities" that mix concerns.

### Code Example: Anemic Model

```csharp
// ANEMIC: Entity is a data bag, service enforces rules
public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class OrderService
{
    public void AddItem(Order order, Product product, int qty)
    {
        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a non-draft order");

        order.Items.Add(new OrderItem { Product = product, Quantity = qty });
        order.Total = order.Items.Sum(i => i.Product.Price * i.Quantity);
    }

    public void Submit(Order order)
    {
        if (!order.Items.Any())
            throw new InvalidOperationException("Cannot submit empty order");

        order.Status = OrderStatus.Submitted;
    }
}
// Problem: Nothing stops someone from doing order.Status = OrderStatus.Submitted directly,
// bypassing the validation.
```

### Code Example: Rich Domain Model

```csharp
// RICH: Entity protects its own invariants
public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(i => i.Price * i.Quantity);

    public Order()
    {
        Id = Guid.NewGuid();
        Status = OrderStatus.Draft;
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a non-draft order");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        _items.Add(new OrderItem(product.Id, product.Price, quantity));
    }

    public void Submit()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot submit empty order");

        Status = OrderStatus.Submitted;
        // Optionally raise a domain event:
        // AddDomainEvent(new OrderSubmittedEvent(Id));
    }
}
// Invariants are always enforced. No external code can put the order in an invalid state.
```

### Decision Guide

| Factor | Anemic | Rich |
|--------|--------|------|
| Domain complexity | Low | High |
| Number of invariants | Few | Many |
| Entry points enforcing same rules | 1 (API only) | Multiple (API, queue, jobs) |
| Team DDD experience | Low | High |
| Application lifespan | Short / MVP | Long-lived |
| Primary operation | CRUD / reporting | Business workflows |

### Takeaway

> **Anemic models work for simple CRUD; rich models earn their cost when you have business invariants that must be enforced regardless of how the entity is accessed.**

---

## Summary: The Meta-Takeaway

Before applying any design pattern, ask yourself three questions:

1. **What specific problem does this pattern solve in my current code?** (Not hypothetically, not in the future -- right now.)
2. **Is the pattern's cost (indirection, complexity, files) justified by the problem's severity?**
3. **Would a new team member understand this code faster with or without the pattern?**

If you cannot give concrete, present-tense answers to all three, write the simple version first. You can always refactor toward a pattern when the need is real. You can rarely refactor away from a prematurely applied pattern without significant effort.
