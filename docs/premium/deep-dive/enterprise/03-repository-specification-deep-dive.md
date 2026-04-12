# Deep Dive: Repository + Specification -- Data Access Patterns That Scale

The Repository pattern and the Specification pattern are among the most debated topics in the .NET ecosystem. "Should I wrap EF Core?" is a perennial question on every .NET forum. This guide cuts through the noise with clear guidelines on when each pattern earns its keep and when it creates unnecessary indirection.

## Repository Pattern: What It Actually Provides

A repository is an in-memory collection-like interface over your persistence layer. The contract hides whether data comes from SQL Server, MongoDB, an in-memory dictionary, or a flat file.

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

This repository provides three benefits:

1. **Testability.** Swap the real implementation for an `InMemoryProductRepository` in unit tests without touching a database.
2. **Persistence ignorance.** Domain and application layers depend on the interface, not on EF Core, Dapper, or any specific ORM.
3. **Centralized query logic.** Tenant-scoped queries, soft-delete filters, and caching all live in one place instead of being scattered across controllers and services.

## The Anti-Pattern: Wrapping DbContext Needlessly

The most common misuse of Repository is wrapping `DbContext` with a thin pass-through that adds no value:

```csharp
// This is the anti-pattern
public class ProductRepository : IRepository<Product>
{
    private readonly AppDbContext _db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Products.FindAsync(id, ct); // just forwarding

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
        => _db.Products.ToListAsync(ct); // just forwarding
}
```

If every method is a one-line delegation to `DbContext`, the repository is not earning its keep. You have doubled the surface area, made `IQueryable` harder to use, and gained nothing that injecting `DbContext` directly would not provide.

**Repository earns its keep when:**
- You add tenant isolation, soft-delete filtering, or caching inside the repository.
- You want to swap persistence technology (SQL to Cosmos, relational to document).
- Your domain layer must not reference EF Core assemblies.
- You need an in-memory implementation for fast unit tests.

**Use DbContext directly when:**
- You have a simple CRUD application with no domain layer.
- Your queries are complex and benefit from `IQueryable` composition.
- You are prototyping and speed of development matters more than architecture purity.

## This Repository's Product Catalog Example

The `InMemoryProductRepository` demonstrates a repository that goes beyond pass-through CRUD. It provides tenant-scoped queries that enforce data isolation:

```csharp
public Task<IReadOnlyList<Product>> GetByTenantAsync(string tenantId, CancellationToken ct)
{
    IReadOnlyList<Product> result = _store.Values
        .Where(p => p.TenantId == tenantId)
        .ToList().AsReadOnly();
    return Task.FromResult(result);
}
```

Every query method filters by `tenantId`, making it impossible for one tenant to see another's data. This is a meaningful business rule that belongs in the repository, not in every caller.

## Specification Pattern: Composable Business Rules

The Specification pattern encapsulates a boolean predicate as a reusable, composable object. Instead of scattering `Where` clauses across your codebase, you define each rule once:

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}
```

The base class provides composite operators, so subclasses only implement the predicate:

```csharp
public sealed class CategorySpecification(string category) : Specification<Product>
{
    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.Category.Equals(category, StringComparison.OrdinalIgnoreCase);
}

public sealed class PriceRangeSpecification(decimal min, decimal max) : Specification<Product>
{
    public override bool IsSatisfiedBy(Product candidate) =>
        candidate.Price >= min && candidate.Price <= max;
}
```

### Composition in Action

The real power is combining specifications with boolean logic:

```csharp
var spec = new CategorySpecification("Electronics")
    .And(new PriceRangeSpecification(100, 500))
    .And(new InStockSpecification());

var matchingProducts = allProducts.Where(p => spec.IsSatisfiedBy(p)).ToList();
```

This reads like a business rule: "Electronics between $100 and $500 that are in stock." The specification can be reused in repositories, validation services, and UI filter builders.

## Combining Repository + Specification

The natural combination is a repository method that accepts a specification:

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> FindAsync(ISpecification<Product> spec, CancellationToken ct);
}

// Implementation
public Task<IReadOnlyList<Product>> FindAsync(ISpecification<Product> spec, CancellationToken ct)
{
    IReadOnlyList<Product> result = _store.Values
        .Where(p => spec.IsSatisfiedBy(p))
        .ToList().AsReadOnly();
    return Task.FromResult(result);
}
```

The repository handles persistence. The specification handles the business rule. Neither knows the other's internals.

## Adding Unit of Work

When a business operation modifies multiple aggregates, the Unit of Work pattern coordinates the writes into a single commit:

```csharp
public interface IUnitOfWork : IDisposable
{
    void RegisterNew<T>(T entity) where T : class, IEntity;
    void RegisterDirty<T>(T entity) where T : class, IEntity;
    void RegisterDeleted<T>(T entity) where T : class, IEntity;
    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
}
```

In EF Core, `DbContext` is already a Unit of Work and an Identity Map. If you use EF Core, you get Unit of Work behavior for free via `SaveChangesAsync()`. The explicit `IUnitOfWork` interface in this repository is useful when you want to decouple from EF Core entirely or when your persistence layer does not provide built-in change tracking.

## Specification with EF Core: The Expression Problem

The in-memory `Specification<T>` in this repository uses `Func<T, bool>`, which works perfectly for in-memory collections but cannot be translated to SQL by EF Core. For EF Core integration, specifications should expose an `Expression<Func<T, bool>>`:

```csharp
public interface IEfSpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}

// Usage with EF Core
var spec = new CategorySpecification("Electronics");
var products = await _dbContext.Products.Where(spec.ToExpression()).ToListAsync();
```

This allows EF Core to translate the specification into a SQL `WHERE` clause rather than loading all rows into memory. Libraries like Ardalis.Specification provide this out of the box.

## Decision Framework

| Scenario | Recommendation |
|----------|---------------|
| Simple CRUD app, single database | Use `DbContext` directly |
| Domain layer must be ORM-agnostic | Repository + interface |
| Complex filtering across multiple callers | Add Specification |
| Multi-tenant data isolation | Repository with tenant filtering |
| Multiple repositories sharing a transaction | Add Unit of Work |
| Fast unit tests without database | In-memory repository implementation |
| EF Core with complex queries | Specification with `Expression<Func<T, bool>>` |

## Common Mistakes

1. **Generic-only repository.** A `IRepository<T>` that only exposes `GetById`, `GetAll`, `Add`, `Update`, `Delete` forces every query through the same narrow interface. Domain-specific queries (`GetLowStockAsync`, `GetByTenantAsync`) belong on a domain-specific interface like `IProductRepository`.

2. **Leaking IQueryable.** Exposing `IQueryable<T>` from the repository defeats the purpose of abstraction. Callers can compose arbitrary queries that may not translate to SQL, and your repository is no longer a meaningful boundary.

3. **One specification per property.** Specifications represent business rules, not column filters. `InStockSpecification` is a good specification. `StockQuantityGreaterThanSpecification` is just a repackaged comparison operator.

4. **Over-engineering early.** Start with the simplest data access approach that works. Introduce Repository when you need testability or persistence swapping. Introduce Specification when the same filtering logic appears in three or more places.

## Related Patterns

- **CQRS** separates the read and write sides, each of which may use different repository implementations.
- **Domain Events** can be dispatched from within the Unit of Work's `CommitAsync`, ensuring events are raised only when changes are persisted.
- **Decorator** can wrap a repository with caching, logging, or authorization without modifying the repository itself.
