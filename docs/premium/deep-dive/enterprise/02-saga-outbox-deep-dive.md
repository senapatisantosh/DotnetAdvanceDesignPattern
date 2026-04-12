# Deep Dive: Saga + Outbox -- Reliable Distributed Workflows in .NET

Microservices break the single-database transaction boundary that monoliths take for granted. When an order placement must reserve inventory, charge payment, and arrange shipping across separate services, there is no distributed `BEGIN TRANSACTION`. The Saga pattern provides application-level consistency, and the Outbox pattern guarantees that events reach their destinations even when the message broker is temporarily down.

## The Distributed Transaction Problem

In a monolith, a single database transaction guarantees atomicity:

```csharp
using var tx = await db.BeginTransactionAsync();
await reserveInventory(order);
await chargePayment(order);
await createShipment(order);
await tx.CommitAsync(); // all or nothing
```

In microservices, each of these operations lives in a different service with its own database. Two-phase commit (2PC) requires all participants to lock resources and wait for a coordinator -- it is slow, fragile, and not supported by most cloud-native databases. The Saga pattern replaces the distributed lock with a sequence of local transactions and compensating actions.

## Orchestration vs Choreography

There are two ways to coordinate saga steps:

| Aspect | Orchestrator (centralized) | Choreography (event-driven) |
|--------|---------------------------|----------------------------|
| **Coordination** | Central orchestrator drives the flow | Each service listens and reacts to events |
| **Visibility** | Easy to trace the full workflow | Flow is implicit, scattered across services |
| **Coupling** | Orchestrator knows all participants | Services only know the events they consume |
| **Error handling** | Orchestrator triggers compensation | Each service must handle compensation independently |
| **Complexity growth** | Linear with step count | Exponential with event interactions |

**This repository implements the orchestrator approach.** It is easier to reason about, debug, and extend.

## Saga Orchestrator in This Repository

The `SagaOrchestrator` defines a linear sequence of steps. Each step implements `ISagaStep`, providing both an `ExecuteAsync` and a `CompensateAsync` method:

```csharp
public interface ISagaStep
{
    string Name { get; }
    Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default);
    Task CompensateAsync(SagaContext context, CancellationToken ct = default);
}
```

Steps share a mutable `SagaContext` dictionary to pass data between them. The orchestrator runs steps in order. If step N fails, it calls `CompensateAsync` on steps N-1 through 0 in reverse:

```csharp
var saga = new SagaOrchestrator()
    .AddStep(new ValidateOrderStep())
    .AddStep(new ReserveInventoryStep())
    .AddStep(new ProcessPaymentStep())
    .AddStep(new ArrangeShippingStep());

var result = await saga.ExecuteAsync(context);
// result.Success, result.CompletedSteps, result.CompensatedSteps
```

### The Order Fulfillment Saga

The four steps in this repository model a real e-commerce flow:

1. **ValidateOrder** -- Read-only validation. Compensation is a no-op because there are no side effects.
2. **ReserveInventory** -- Decrements available stock. Compensation releases the reserved quantity.
3. **ProcessPayment** -- Charges the customer. Compensation issues a refund.
4. **ArrangeShipping** -- Creates a shipment record. Compensation cancels the shipment.

If `ProcessPayment` fails, the orchestrator automatically calls `ReserveInventory.CompensateAsync` to release stock, then `ValidateOrder.CompensateAsync` (which is a no-op). The `SagaResult` records exactly which steps succeeded and which were compensated, giving full traceability.

## Compensation Design Guidelines

1. **Compensations must be idempotent.** A compensation may be retried if the first attempt fails partway through. Releasing inventory twice should not produce a negative reservation count.

2. **Compensations should never fail silently.** The orchestrator catches exceptions during compensation and records them, but continues compensating remaining steps. Log compensation failures aggressively -- they represent a partial rollback.

3. **Some steps have no compensation.** Validation, logging, and notification steps are either read-only or fire-and-forget. Return `Task.CompletedTask` from `CompensateAsync` and document why.

4. **Compensation is not undo.** You cannot un-send an email. Design compensations as corrective actions (send a cancellation email), not as true reversals.

## The Outbox Pattern

A common failure scenario: the saga completes successfully, but the event notifying downstream services is lost because the message broker was unreachable at the moment of publishing. The Outbox pattern solves this with a two-step approach:

1. **Write the event to an outbox table in the same database transaction as the business operation.** If the transaction commits, the message is guaranteed to be in the outbox.
2. **A background processor polls the outbox and publishes pending messages to the broker.** Once published, the message is marked as processed.

```csharp
public sealed class OutboxPublisher(IOutboxStore outboxStore)
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : class
    {
        var message = new OutboxMessage
        {
            EventType = typeof(TEvent).FullName ?? typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(domainEvent)
        };
        await outboxStore.SaveAsync(message, ct);
    }
}
```

The `OutboxProcessor` runs as a background service, picking up pending messages in batches, publishing them, and marking them as processed. Failed messages increment a retry counter and are skipped after exceeding `maxRetries` (dead-letter behavior).

### Outbox Guarantees

- **At-least-once delivery.** If the processor crashes after publishing but before marking the message as processed, it will re-publish on the next run. Consumers must be idempotent.
- **Ordering within an aggregate.** Messages for the same aggregate are written in order within the same transaction. The processor reads them in insertion order.
- **No distributed transactions required.** The business write and the outbox write share the same local database transaction.

## Combining Saga + Outbox + Domain Events

The most robust architecture layers all three patterns:

```
Order Placed (Domain Event)
  -> Outbox captures event in same DB transaction
    -> OutboxProcessor publishes to message broker
      -> Saga Orchestrator receives event and starts saga
        -> Each saga step publishes its own domain events via Outbox
```

Each saga step writes its result and any outgoing events to the outbox within its local transaction. The outbox processor delivers those events to downstream services. This provides end-to-end reliability without distributed transactions.

## Failure Scenarios and Recovery

| Failure | What Happens | Recovery |
|---------|-------------|----------|
| Step fails during execution | Orchestrator compensates completed steps in reverse | Saga returns `Failed` with full audit trail |
| Compensation fails | Orchestrator logs the failure but continues compensating remaining steps | Manual intervention based on `CompensatedSteps` log |
| App crashes mid-saga | Incomplete saga has no record of completion | Use saga state table to detect orphaned sagas on startup |
| Outbox processor crashes | Messages remain in `Pending` state | Processor picks them up on next restart |
| Broker unreachable | `OutboxProcessor` increments retry count, skips after max retries | Fix broker, reset retry count or reprocess dead letters |

## Production Recommendations

1. **Persist saga state.** The in-memory orchestrator in this repo is excellent for learning. In production, persist the saga state (current step, context, status) to a database so that sagas survive process restarts.

2. **Add timeouts.** A saga step waiting on an external service should have a timeout. A `CancellationToken` with a deadline prevents indefinite hangs.

3. **Monitor compensation rates.** A high compensation rate indicates a systemic problem (payment provider down, inventory consistently depleted). Alert on this metric.

4. **Use the Outbox for all cross-service communication**, not just saga events. Any time you write to a database and need to notify another service, write the notification to the outbox within the same transaction.

5. **Consider eventual consistency.** Sagas do not provide the same guarantees as ACID transactions. Between the time a step completes and its compensation runs (if needed), the system is in an inconsistent state. Design your domain model and UI to handle this gracefully.

## When Not to Use These Patterns

- **Single database, single service.** Use a regular database transaction. Sagas and outbox add complexity that is unnecessary when a simple `COMMIT` suffices.
- **Read-only operations.** No compensation is needed, so no saga is needed.
- **Fire-and-forget notifications.** If losing an occasional email is acceptable, direct publishing without an outbox is simpler.

## Related Patterns

- **Command** encapsulates each saga step as an undoable action.
- **Domain Events** provide the event types that flow through the outbox.
- **Unit of Work** coordinates the business write and outbox write within a single transaction boundary.
