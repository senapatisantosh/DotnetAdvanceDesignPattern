---
title: "Observer vs Domain Events Deep Dive"
contentKey: "observer-domain-events-deep-dive"
section: "deep-dive-behavioral"
accessLevel: "premium"
contentType: "doc"
tags: ["dotnet", "design-patterns", "observer", "domain-events", "behavioral", "deep-dive"]
order: 3
sourceType: "same_repo"
sourcePath: "docs/premium/deep-dive/behavioral/03-observer-domain-events-deep-dive.md"
routePath: "/project/dotnet-advanced-design-patterns/learn/observer-domain-events-deep-dive"
migrationTargetPath: "premium/dotnet-advanced-design-patterns/docs/deep-dive/behavioral/03-observer-domain-events-deep-dive.md"
isPublished: true
---

# Deep Dive: Observer vs Domain Events vs Event Sourcing

These three patterns all involve events, but at different levels of abstraction, persistence, and scope. Understanding the evolution from Observer to Domain Events to Event Sourcing is essential for designing production event-driven architectures.

## The Evolution

```
Observer (in-process, synchronous, ephemeral)
    ↓ needs persistence and decoupling
Domain Events (dispatched via bus, async, cross-boundary)
    ↓ needs full audit trail and temporal queries
Event Sourcing (events ARE the source of truth)
```

## Observer: In-Process Notifications

The GoF Observer pattern provides simple, synchronous, in-process publish/subscribe.

```csharp
public class PatientMonitor
{
    private readonly List<IVitalSignsObserver> _observers = new();

    public void Subscribe(IVitalSignsObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IVitalSignsObserver observer) => _observers.Remove(observer);

    public void RecordVitals(VitalSigns vitals)
    {
        _currentVitals = vitals;
        foreach (var observer in _observers)
            observer.OnVitalsChanged(vitals); // synchronous, immediate
    }
}
```

**Characteristics:**
- Synchronous execution -- observers block the publisher
- Ephemeral -- if no one is listening, the event is lost
- In-process only -- cannot cross service boundaries
- Manual subscribe/unsubscribe -- risk of memory leaks if not cleaned up
- In .NET, `event EventHandler<T>` is the idiomatic implementation

**Best for:** UI updates, real-time monitoring dashboards, in-process reactive logic where immediacy matters and persistence is not needed.

## Domain Events: Business-Level Events

Domain Events elevate the concept from a technical notification mechanism to a business-meaningful occurrence.

```csharp
// The event is a business concept with rich semantics
public record OrderPlacedEvent(
    Guid OrderId,
    string CustomerId,
    decimal TotalAmount,
    DateTimeOffset PlacedAt) : IDomainEvent;

// Handlers are auto-discovered via DI
public class SendConfirmationEmailHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent @event)
    {
        await _emailService.SendOrderConfirmation(@event.CustomerId, @event.OrderId);
    }
}

public class UpdateInventoryProjectionHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent @event)
    {
        await _projection.DeductStock(@event.OrderId);
    }
}
```

**Characteristics:**
- Handlers are decoupled -- publisher has zero knowledge of handlers
- Can be dispatched synchronously (after SaveChanges) or asynchronously (via message bus)
- Events carry rich domain semantics, not just raw state
- Handlers are discovered via DI container, not manual subscription
- Events can cross bounded context boundaries

**Best for:** Side effects from domain operations (email, audit, projections), cross-aggregate communication, decoupled architectures.

## Domain Events + Outbox: Guaranteed Delivery

The Outbox pattern ensures domain events survive crashes and are delivered at least once.

```csharp
public class OrderService
{
    public async Task PlaceOrder(Order order)
    {
        await using var transaction = await _db.BeginTransactionAsync();

        _db.Orders.Add(order);

        // Write event to outbox in the SAME transaction
        var @event = new OrderPlacedEvent(order.Id, order.CustomerId, order.Total, DateTimeOffset.UtcNow);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(OrderPlacedEvent),
            Payload = JsonSerializer.Serialize(@event),
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = null
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        // Background worker publishes outbox messages to the message broker
    }
}
```

**Key guarantee:** The event is saved atomically with the domain data. If the process crashes after SaveChanges but before publishing, the background worker will pick it up on restart. This solves the dual-write problem.

## Event Sourcing: Events as Source of Truth

Event Sourcing takes the final step: instead of storing current state, you store the sequence of events that produced that state.

```csharp
public class Order : AggregateRoot
{
    private readonly List<IDomainEvent> _events = new();

    public void Place(string customerId, List<LineItem> items)
    {
        // Instead of setting properties, raise events
        Raise(new OrderPlacedEvent(Id, customerId, items.Sum(i => i.Total), DateTimeOffset.UtcNow));
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Cannot ship unconfirmed order");
        Raise(new OrderShippedEvent(Id, trackingNumber, DateTimeOffset.UtcNow));
    }

    // State is rebuilt by replaying events
    private void Apply(OrderPlacedEvent e) { Status = OrderStatus.Placed; CustomerId = e.CustomerId; }
    private void Apply(OrderShippedEvent e) { Status = OrderStatus.Shipped; TrackingNumber = e.TrackingNumber; }
}

// To load: replay all events for this aggregate
var events = await _eventStore.GetEventsAsync(orderId);
var order = new Order();
foreach (var e in events) order.Apply(e);
```

**Characteristics:**
- Events are immutable and append-only
- Current state is derived by replaying events
- Full audit trail -- you can answer "what was the state at time T?"
- Read models are built by projecting events (CQRS is natural fit)
- Complex to implement correctly (snapshots, versioning, eventual consistency)

**Best for:** Financial systems (audit requirements), systems needing temporal queries, systems where the history of changes is as important as the current state.

## Comparison Matrix

| Aspect | Observer | Domain Events | Event Sourcing |
|--------|----------|---------------|----------------|
| Scope | In-process | Cross-boundary | System-wide |
| Persistence | None | Optional (Outbox) | Events ARE the data |
| Delivery | Synchronous | Sync or Async | Append to event store |
| Replay | Not possible | Possible with Outbox | Core feature |
| Audit trail | None | Optional | Complete |
| Complexity | Low | Medium | High |
| Consistency | Immediate | Eventual (with Outbox) | Eventual |
| .NET tooling | `event`, `IObservable<T>` | MediatR, MassTransit, NServiceBus | EventStoreDB, Marten |

## Evolution Path in Practice

Most systems evolve through these stages:

1. **Start with Observer.** Simple in-process events using C# `event` or `IObservable<T>`. Fast to implement, good for prototypes.
2. **Introduce Domain Events.** When side effects need to be decoupled from the triggering action, or when events need to cross aggregate/service boundaries.
3. **Add Outbox.** When event delivery must be guaranteed (no lost events on crash). Essential for production systems with message brokers.
4. **Consider Event Sourcing.** Only when you genuinely need full audit trails, temporal queries, or the ability to rebuild state from history. Most systems do NOT need this.

## Common Mistakes

1. **Jumping to Event Sourcing.** Event Sourcing is powerful but complex. Most applications are well-served by Domain Events + Outbox without full event sourcing.
2. **Synchronous Domain Events blocking the request.** If email sending takes 5 seconds and runs in the event handler synchronously, the user waits 5 seconds. Use async dispatch or fire-and-forget for non-critical side effects.
3. **Forgetting idempotency.** With Outbox and at-least-once delivery, handlers MUST be idempotent. Sending two confirmation emails because an event was delivered twice is a bug.
4. **Observer memory leaks.** In C#, `event` handlers that are not unsubscribed keep the subscriber alive. Use weak events or explicit unsubscription.
5. **Events with too much data.** Events should carry the minimum data needed. Including the entire aggregate state in every event couples consumers to the producer's model.

## Interview-Worthy Insights

- Observer is a **technical mechanism**. Domain Events are a **business concept**. Event Sourcing is a **persistence strategy**.
- In .NET, `IObservable<T>` / `IObserver<T>` (Reactive Extensions) provide a powerful Observer implementation with LINQ-like operators.
- MediatR's `INotification` is a Domain Event dispatcher. MassTransit and NServiceBus handle cross-service Domain Events.
- Event Sourcing pairs naturally with CQRS: commands produce events (write side), events build projections (read side).
- The key evolution question: "Do I need to guarantee delivery?" If yes, move from Observer to Domain Events + Outbox. "Do I need to rebuild state from history?" If yes, consider Event Sourcing.
