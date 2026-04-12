# Deep Dive: Saga and Outbox -- Distributed Transaction Patterns

When business operations span multiple services or databases, traditional ACID transactions are not available. The Saga pattern coordinates multi-step processes with compensating actions. The Outbox pattern guarantees reliable event delivery. Together, they form the backbone of distributed consistency in microservices.

## The Problem: Distributed Transactions

Consider order fulfillment: validate the order, reserve inventory, charge payment, arrange shipping. Each step may involve a different service or database. If payment fails after inventory is reserved, you need to release the reservation. If the process crashes between charging payment and arranging shipping, you need to recover.

Two-Phase Commit (2PC) solves this with distributed locks, but it requires all participants to support the protocol, introduces latency from lock coordination, and creates a single point of failure in the transaction coordinator. Most cloud-native architectures reject 2PC in favor of sagas.

## Saga Pattern: Orchestration vs Choreography

### Orchestration (Central Coordinator)

This repository implements the orchestration approach. A central `SagaOrchestrator` controls the flow:

```csharp
public class OrderFulfillmentSaga
{
    private readonly SagaOrchestrator _orchestrator = new();

    public OrderFulfillmentSaga()
    {
        _orchestrator.AddStep(new ValidateOrderStep());
        _orchestrator.AddStep(new ReserveInventoryStep());
        _orchestrator.AddStep(new ProcessPaymentStep());
        _orchestrator.AddStep(new ArrangeShippingStep());
    }

    public async Task<SagaResult> ExecuteAsync(SagaContext context)
    {
        return await _orchestrator.ExecuteAsync(context);
        // On failure at step N, automatically compensates steps N-1 through 0
    }
}
```

Each step implements `ISagaStep` with `ExecuteAsync` and `CompensateAsync`:

```csharp
public class ReserveInventoryStep : ISagaStep
{
    public string Name => "Reserve Inventory";

    public async Task<SagaStepResult> ExecuteAsync(SagaContext context)
    {
        var orderId = context.Get<string>("OrderId");
        var reservationId = await _inventory.ReserveAsync(orderId);
        context.Set("ReservationId", reservationId);
        return SagaStepResult.Success();
    }

    public async Task<SagaStepResult> CompensateAsync(SagaContext context)
    {
        var reservationId = context.Get<string>("ReservationId");
        await _inventory.ReleaseAsync(reservationId);
        return SagaStepResult.Success();
    }
}
```

**Strengths:** Easy to understand the flow (it is linear). Easy to debug (the orchestrator has full visibility). Easy to add/remove/reorder steps.

**Weaknesses:** Central point of coordination. The orchestrator must be reliable and persistent for long-running sagas.

### Choreography (Event-Driven)

No central coordinator. Each service publishes events and listens for events from other services:

```
OrderService --[OrderPlaced]--> InventoryService
InventoryService --[InventoryReserved]--> PaymentService
PaymentService --[PaymentCharged]--> ShippingService
PaymentService --[PaymentFailed]--> InventoryService (compensate)
```

**Strengths:** No central coordinator. Services are fully independent. Scales naturally.

**Weaknesses:** Hard to understand the overall flow (it is scattered across services). Hard to debug (which service has the problem?). Hard to add compensations (each service must know its compensating trigger events).

### Decision: Orchestration vs Choreography

| Factor | Orchestration | Choreography |
|--------|--------------|--------------|
| Visibility | High (one place to see the flow) | Low (flow is implicit in events) |
| Coupling | Orchestrator knows all steps | Services know their own events |
| Complexity | Simple with few steps | Simpler with many independent services |
| Debugging | Easy (check orchestrator state) | Hard (trace events across services) |
| Recovery | Orchestrator manages compensation | Each service handles its own compensation |

**Rule of thumb:** Use orchestration when you have 3-7 sequential steps that must be coordinated. Use choreography when services are truly independent and the "saga" is really just eventual consistency between autonomous services.

## Outbox Pattern: Guaranteed Event Delivery

The Outbox pattern solves the dual-write problem: you need to both save data to the database AND publish an event to a message broker. If either fails independently, the system is inconsistent.

### The Dual-Write Problem

```csharp
// DANGEROUS: dual-write
await _db.SaveChangesAsync();    // Success
await _messageBroker.PublishAsync(event);  // Fails! Event is lost.
```

Or the reverse:
```csharp
await _messageBroker.PublishAsync(event);  // Success
await _db.SaveChangesAsync();    // Fails! Event published for nonexistent data.
```

### The Outbox Solution

Write the event to an outbox table in the SAME database transaction as the domain data:

```csharp
public class OrderService
{
    public async Task PlaceOrder(Order order)
    {
        await using var transaction = await _db.BeginTransactionAsync();

        _db.Orders.Add(order);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(OrderPlacedEvent),
            Payload = JsonSerializer.Serialize(new OrderPlacedEvent(order.Id, order.CustomerId)),
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = null
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
```

A background worker polls the outbox and publishes unpublished messages:

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var messages = await _db.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(100)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                await _messageBroker.PublishAsync(message.Type, message.Payload);
                message.ProcessedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

### Outbox Guarantees

- **At-least-once delivery:** If the worker crashes after publishing but before marking as processed, the message will be published again on restart.
- **Ordering:** Messages are processed in `CreatedAt` order within a single outbox table.
- **Idempotency required:** Consumers MUST be idempotent because messages may arrive more than once.

## Saga + Outbox: The Complete Picture

In a distributed system, the saga orchestrator uses the outbox to communicate with remote services:

1. Saga step calls local service and writes result + outbox event in one transaction.
2. Background worker publishes the outbox event to the message broker.
3. Remote service processes the event and writes its result + response event to its own outbox.
4. Saga orchestrator receives the response and proceeds to the next step.

If any step fails, compensating events flow back through the same outbox mechanism.

## Production Considerations

### Idempotency Strategies

Since at-least-once delivery means duplicate messages, every handler needs idempotency:

1. **Idempotency key:** Store processed message IDs in a `ProcessedMessages` table. Check before handling.
2. **Natural idempotency:** `SET stock = 50` is idempotent. `SET stock = stock - 5` is NOT.
3. **Upsert:** Use `INSERT ... ON CONFLICT DO UPDATE` for naturally idempotent writes.

### Compensation Design

Compensations are NOT rollbacks. They are new forward-looking actions:
- Reserve inventory -> **Release** inventory (not "un-reserve")
- Charge payment -> **Refund** payment (not "un-charge")
- Send email -> **Send correction email** (you cannot un-send)

### Monitoring

- Track saga state: which step is active, how long has it been running?
- Alert on compensation: a compensation means something went wrong.
- Monitor outbox lag: how many unpublished messages are queued?
- Track duplicate deliveries: how often are consumers rejecting duplicates?

## Common Mistakes

1. **Compensations that can fail.** What if the refund API is down when you need to compensate? Compensations must be retried until successful. Use the outbox for compensation events too.
2. **No idempotency.** Charging a customer twice because a payment event was delivered twice is a critical bug.
3. **Mixing saga and two-phase commit.** If you are using a saga, embrace eventual consistency. Do not try to make it look like a distributed transaction.
4. **Outbox without cleanup.** The outbox table grows forever unless you purge processed messages. Add a retention policy (e.g., delete processed messages older than 7 days).
5. **Long-running sagas without persistence.** If the saga takes hours or days (e.g., a travel booking workflow), the saga state must be persisted to survive process restarts.

## Interview-Worthy Insights

- Saga is the microservice replacement for distributed transactions. It trades **strong consistency** for **eventual consistency** with **compensation**.
- The Outbox pattern guarantees **at-least-once** delivery. **Exactly-once** delivery is impossible in distributed systems; design consumers for idempotency instead.
- In .NET, MassTransit and NServiceBus provide built-in saga orchestration and outbox support. For simple cases, hand-rolling (as in this repository) is educational and sufficient.
- Compensations run in **reverse order** -- this is critical for correctness. You must release inventory before refunding payment, because the refund amount might depend on what was reserved.
- The key trade-off: sagas and outbox add infrastructure complexity (background workers, message brokers, idempotency tables) in exchange for resilience in distributed systems.
