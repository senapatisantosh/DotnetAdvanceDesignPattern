# Deep Dive: Repository and Specification -- Data Access Patterns in .NET

The Repository and Specification patterns are among the most debated in the .NET ecosystem, largely because EF Core already implements both. This guide clarifies when custom implementations add value and when they are unnecessary overhead.

## The EF Core Debate

EF Core's `DbContext` already IS a Unit of Work (`SaveChanges()` commits atomically) and each `DbSet<T>` IS a Repository (`Add()`, `Find()`, `Remove()`). So why add custom Repository and Specification layers?

**Arguments for wrapping EF Core:**
- Testability: mock `IRepository<T>` without `DbContext` or in-memory database
- Swappability: switch from EF Core to Dapper, MongoDB, or an external API
- Encapsulation: complex queries live in Specifications, not scattered in services
- Consistency: all data access follows the same patterns

**Arguments against wrapping EF Core:**
- Leaky abstraction: EF Core features (eager loading, change tracking, raw SQL) are hard to expose through a generic repository
- Duplication: the repository methods often just delegate to `DbSet<T>` methods
- Testing: EF Core's in-memory provider or SQLite in-memory mode can replace mocks
- Overhead: extra interfaces and classes for simple CRUD

**The pragmatic answer:** Use a custom Repository when you need testability without a database, when persistence technology might change, or when you have complex query logic worth encapsulating. Skip it for simple CRUD applications where `DbContext` injection is sufficient.

## Repository Implementation

### Generic Repository

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(ISpecification<T> spec);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);
    void Add(T entity);
    void Remove(T entity);
}

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> FindAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        var query = _dbSet.AsQueryable();
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        query = spec.Includes.Aggregate(query, (q, include) => q.Include(include));
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);
        return query;
    }
}
```

### When Generic Repository Falls Short

The generic repository struggles with:
- **Complex joins:** Multi-table queries that do not map to a single entity
- **Projections:** `Select()` to a DTO rather than loading full entities
- **Raw SQL:** Performance-critical queries that need hand-tuned SQL
- **Batch operations:** `UPDATE ... WHERE` without loading entities

For these cases, either extend the generic repository with specific methods or use a separate query service:

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<OrderSummaryDto> GetOrderSummaryAsync(Guid orderId);
    Task<List<OrderListItemDto>> GetRecentOrdersAsync(int count);
}
```

## Specification Implementation

### The Specification Base Class

```csharp
public abstract class Specification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; IsPagingEnabled = true; }
}
```

### Concrete Specifications

```csharp
public class ActiveCustomerSpec : Specification<Customer>
{
    public ActiveCustomerSpec()
    {
        Criteria = c => c.IsActive && !c.IsDeleted;
    }
}

public class PremiumCustomerSpec : Specification<Customer>
{
    public PremiumCustomerSpec()
    {
        Criteria = c => c.Tier == CustomerTier.Premium;
    }
}

public class CustomerWithOrdersSpec : Specification<Customer>
{
    public CustomerWithOrdersSpec(bool activeOnly = true)
    {
        if (activeOnly)
            Criteria = c => c.IsActive;
        AddInclude(c => c.Orders);
        OrderBy = c => c.LastName;
    }
}
```

### Composing Specifications

The power of Specification is composition:

```csharp
public static class SpecificationExtensions
{
    public static Specification<T> And<T>(this Specification<T> left, Specification<T> right)
        => new AndSpecification<T>(left, right);

    public static Specification<T> Or<T>(this Specification<T> left, Specification<T> right)
        => new OrSpecification<T>(left, right);

    public static Specification<T> Not<T>(this Specification<T> spec)
        => new NotSpecification<T>(spec);
}

// Usage
var spec = new ActiveCustomerSpec()
    .And(new PremiumCustomerSpec());

var premiumActiveCustomers = await _repository.FindAsync(spec);
```

This avoids the anti-pattern of adding a new repository method for every query variation (`GetActiveCustomers()`, `GetPremiumCustomers()`, `GetActivePremiumCustomers()`, `GetActivePremiumCustomersWithOrders()`...).

## Repository + Specification + Unit of Work

In the full pattern stack:

```csharp
public class OrderService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Customer> _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<OrderId>> PlaceOrder(PlaceOrderCommand command)
    {
        var customer = await _customerRepo.FirstOrDefaultAsync(
            new ActiveCustomerSpec().And(new CustomerByIdSpec(command.CustomerId)));

        if (customer == null)
            return Result<OrderId>.Failure("Customer not found or inactive");

        var order = Order.Create(customer, command.Items);
        _orderRepo.Add(order);

        await _unitOfWork.SaveChangesAsync(); // commits both repos atomically
        return Result<OrderId>.Success(order.Id);
    }
}
```

## When to Use Which Level

| Scenario | Recommendation |
|----------|---------------|
| Simple CRUD, small app | Inject `DbContext` directly. No custom repository. |
| Medium app, testability needed | Generic `IRepository<T>` + Unit of Work |
| Complex queries, dynamic filtering | Add Specification pattern |
| Multiple persistence technologies | Repository per technology behind a common interface |
| Read-heavy with complex projections | Separate query service (Dapper/raw SQL) alongside repository |

## Ardalis.Specification

The [Ardalis.Specification](https://github.com/ardalis/Specification) NuGet package provides a production-ready Specification implementation for EF Core with:
- `Specification<T>` base class with criteria, includes, ordering, paging
- `IRepository<T>` that applies specifications to EF Core queries
- Integration with `AutoMapper` for projections
- Specification evaluator for both `IQueryable<T>` and `IEnumerable<T>`

For production applications, consider using this library rather than hand-rolling specifications.

## Common Mistakes

1. **Generic repository with no specifications.** A repository that only has `GetById()`, `GetAll()`, and `Add()` forces all filtering into the service layer, defeating the purpose of encapsulation.
2. **Specification that returns IQueryable.** Specifications should return expressions, not queryables. The repository translates expressions into database queries.
3. **Repository per table.** Create repositories per aggregate root, not per table. An `OrderRepository` manages `Order` and its `LineItems`; you do not need a separate `LineItemRepository`.
4. **Ignoring EF Core change tracking.** If you use a repository wrapper, ensure you are not accidentally defeating EF Core's change tracking by returning detached entities.
5. **Over-specifying.** If a query is used exactly once and is a simple `WHERE` clause, a specification adds overhead. Reserve specifications for queries that are reused or composed.

## Interview-Worthy Insights

- Repository is about **how** (CRUD mechanics). Specification is about **what** (query criteria). They are complementary, not competing.
- EF Core's `DbContext` already implements Repository + Unit of Work. Custom wrappers add value only when you need testability without a database or persistence swappability.
- The Specification pattern maps to SQL's `WHERE` clause. `And()` is `AND`, `Or()` is `OR`, `Not()` is `NOT`. The composition is the power.
- Aggregate root boundaries determine repository boundaries. `Order` is an aggregate root (has its own repository). `LineItem` is an entity within the `Order` aggregate (no separate repository).
- The question "Should you wrap EF Core in a repository?" is one of the most common .NET architecture interview questions. The answer is nuanced: "It depends on testability requirements, complexity of queries, and likelihood of changing persistence technology."
