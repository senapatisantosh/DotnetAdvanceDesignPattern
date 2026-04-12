# Deep Dive: CQRS Implementation Guide for Production .NET

CQRS (Command Query Responsibility Segregation) is one of the most discussed and most misapplied enterprise patterns. This guide covers practical implementation in .NET, from simple separation to full CQRS with separate read stores.

## The Three Levels of CQRS

### Level 1: Logical Separation (Same Database, Different Handlers)

The simplest form. Commands and queries use separate handler classes but share the same database and potentially the same model. This is what this repository implements.

```csharp
// Command side
public record ReserveInventoryCommand(string Sku, int Quantity) : ICommand<ReservationResult>;

public class ReserveInventoryHandler : ICommandHandler<ReserveInventoryCommand, ReservationResult>
{
    public async Task<ReservationResult> HandleAsync(ReserveInventoryCommand command)
    {
        var item = await _store.GetBySku(command.Sku);
        if (item.AvailableQuantity < command.Quantity)
            return ReservationResult.InsufficientStock();

        item.Reserve(command.Quantity);
        await _store.SaveAsync();
        return ReservationResult.Success(item.ReservationId);
    }
}

// Query side
public record GetInventoryLevelQuery(string Sku) : IQuery<InventoryLevel>;

public class GetInventoryLevelHandler : IQueryHandler<GetInventoryLevelQuery, InventoryLevel>
{
    public async Task<InventoryLevel> HandleAsync(GetInventoryLevelQuery query)
    {
        var item = await _store.GetBySku(query.Sku);
        return new InventoryLevel(item.Sku, item.TotalQuantity, item.ReservedQuantity, item.AvailableQuantity);
    }
}
```

**Benefits:** Clean separation of concerns, single responsibility per handler, easy to test, pipeline behaviors (validation, logging) apply differently to commands vs queries.

**Trade-off:** Still the same database and often the same model. You gain organizational clarity but not performance separation.

### Level 2: Separate Read Models (Same Database, Different Models)

Commands use rich domain entities. Queries use lightweight DTOs or database views.

```csharp
// Write model: rich domain entity with behavior
public class InventoryItem
{
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public List<Reservation> Reservations { get; private set; }

    public void Reserve(int qty)
    {
        if (AvailableQuantity < qty) throw new InsufficientStockException();
        Reservations.Add(new Reservation(qty, DateTime.UtcNow));
    }
}

// Read model: flat DTO optimized for queries
public record InventoryLevelDto(string Sku, string ProductName, int Available, int Reserved, DateTime LastUpdated);

// Query uses Dapper or raw SQL for performance
public class GetLowStockHandler : IQueryHandler<GetLowStockQuery, List<InventoryLevelDto>>
{
    public async Task<List<InventoryLevelDto>> HandleAsync(GetLowStockQuery query)
    {
        return await _connection.QueryAsync<InventoryLevelDto>(
            "SELECT Sku, ProductName, Available, Reserved, LastUpdated FROM vw_InventoryLevels WHERE Available < @Threshold",
            new { query.Threshold });
    }
}
```

**Benefits:** Read model is optimized for display (denormalized, pre-computed). Write model is optimized for business rules. Queries bypass the domain model overhead.

### Level 3: Separate Read Store (Different Databases)

Commands write to a normalized relational database. Events project changes to a denormalized read store (Redis, Elasticsearch, dedicated read replicas).

```csharp
// Command handler writes to SQL and raises event
public class ReserveInventoryHandler : ICommandHandler<ReserveInventoryCommand, ReservationResult>
{
    public async Task<ReservationResult> HandleAsync(ReserveInventoryCommand command)
    {
        var item = await _writeDb.GetBySku(command.Sku);
        item.Reserve(command.Quantity);
        await _writeDb.SaveAsync();

        await _eventBus.Publish(new InventoryReservedEvent(item.Sku, command.Quantity));
        return ReservationResult.Success();
    }
}

// Event handler projects to read store
public class InventoryProjectionHandler : IDomainEventHandler<InventoryReservedEvent>
{
    public async Task Handle(InventoryReservedEvent @event)
    {
        await _readStore.UpdateStockLevel(@event.Sku, stockLevel =>
        {
            stockLevel.Reserved += @event.Quantity;
            stockLevel.Available -= @event.Quantity;
            stockLevel.LastUpdated = DateTimeOffset.UtcNow;
        });
    }
}
```

**Benefits:** Read and write databases scale independently. Read store can be Redis (fast lookups), Elasticsearch (full-text search), or a read replica. Write store can be optimized for transactional integrity.

**Trade-off:** Eventual consistency. The read store lags behind writes. Your UI must handle stale data gracefully.

## When NOT to Use CQRS

This is as important as knowing when to use it.

1. **Simple CRUD applications.** If read and write models have the same shape, CQRS adds ceremony without benefit.
2. **Small team unfamiliar with eventual consistency.** The operational complexity of separate read stores, projections, and consistency is significant.
3. **Strong consistency required everywhere.** If users must see their writes immediately and stale reads are unacceptable, Level 3 CQRS creates more problems than it solves.
4. **Low data volume.** If a single PostgreSQL instance handles both reads and writes comfortably, splitting introduces unnecessary infrastructure.

## Production Checklist

### For Level 1 (Logical Separation)
- [ ] Commands return `Result<T>`, not void (callers need to know if it worked)
- [ ] Queries are side-effect free (no writes in query handlers)
- [ ] Pipeline behaviors: validation on commands, caching on queries
- [ ] Commands use domain entities with validation; queries use DTOs

### For Level 2 (Separate Models)
- [ ] Database views or Dapper queries for read models
- [ ] Read DTOs do NOT reference domain entities
- [ ] EF Core tracked entities only in command handlers

### For Level 3 (Separate Stores)
- [ ] Outbox pattern for reliable event delivery
- [ ] Idempotent projections (replaying events produces the same read state)
- [ ] Monitoring for projection lag (how stale is the read store?)
- [ ] Rebuild strategy (how to rebuild read store from scratch)
- [ ] Fallback: if read store is down, can you query the write store?

## MediatR Integration

MediatR is the most common CQRS library in .NET. The pattern maps directly:

```csharp
// Commands
public record CreateOrderCommand(string CustomerId, List<LineItem> Items) : IRequest<Result<OrderId>>;

// Queries
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;

// Pipeline behaviors (Chain of Responsibility)
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> { }
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> { }

// In the controller
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    var result = await _mediator.Send(new CreateOrderCommand(request.CustomerId, request.Items));
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}
```

## Common Mistakes

1. **"Fake CQRS."** Same model, same database, same handler, just routed through MediatR. This adds indirection without benefit. If you are doing this, stop and use a simple service class.
2. **Commands returning large read models.** Commands should return minimal data (the new ID, a success/failure result). If the caller needs the full object, issue a separate query.
3. **Putting business logic in query handlers.** Queries should ONLY read and transform data. If you find validation or state mutation in a query handler, it is a command in disguise.
4. **Skipping the Outbox at Level 3.** Without the Outbox pattern, events can be lost between writing to the database and publishing to the message broker.

## Interview-Worthy Insights

- CQRS does NOT require separate databases. Level 1 (same DB, separate handlers) provides 80% of the benefit with 20% of the complexity.
- CQRS pairs naturally with Event Sourcing but does NOT require it. Most production CQRS systems use a relational write store, not an event store.
- The read model can be ANY technology: SQL views, Redis, Elasticsearch, GraphQL, materialized views. This is the scaling superpower.
- "Eventual consistency" means your UI must design for it: optimistic updates, loading indicators, "your changes are being processed" messages.
- The migration path: start with Level 1. Promote to Level 2 when read performance suffers. Promote to Level 3 only when you need independent scaling.
