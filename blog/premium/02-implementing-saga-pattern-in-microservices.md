---
title: "Implementing the Saga Pattern for .NET Microservices"
contentKey: "blog-saga-microservices"
section: "blog-premium"
accessLevel: "premium"
contentType: "blog"
tags: ["dotnet", "design-patterns", "saga", "microservices", "distributed-systems", "blog"]
order: 2
sourceType: "same_repo"
sourcePath: "blog/premium/02-implementing-saga-pattern-in-microservices.md"
routePath: "/project/dotnet-advanced-design-patterns/blog/implementing-saga-microservices"
migrationTargetPath: "premium/dotnet-advanced-design-patterns/blog/02-implementing-saga-pattern-in-microservices.md"
isPublished: true
---

# Implementing the Saga Pattern for .NET Microservices

When you break a monolith into microservices, you lose the one thing that made data consistency easy: database transactions. An order placement that once lived inside a single `BEGIN TRANSACTION ... COMMIT` now spans inventory, payment, and shipping services -- each with its own database. The Saga pattern provides a disciplined way to maintain consistency without distributed transactions.

## Why Distributed Transactions Are Not the Answer

Two-phase commit (2PC) requires every participant to lock resources and wait for a global coordinator. In practice, this means:

- **Increased latency.** Every participant must acknowledge before any can commit.
- **Reduced availability.** If the coordinator or any participant is unreachable, the entire transaction blocks.
- **Vendor lock-in.** Most cloud-native databases (DynamoDB, Cosmos DB, Cloud Spanner) do not support 2PC across service boundaries.

Sagas replace the global lock with a sequence of local transactions. Each step commits independently. If a later step fails, compensating transactions undo the effects of earlier steps.

## Orchestrator Pattern: Centralized Control

The orchestrator approach uses a central coordinator that knows the full workflow and drives each step in sequence. This is what this repository implements.

```csharp
public interface ISagaStep
{
    string Name { get; }
    Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default);
    Task CompensateAsync(SagaContext context, CancellationToken ct = default);
}
```

Every step has two methods: `ExecuteAsync` for the forward operation and `CompensateAsync` for the rollback. The orchestrator runs steps in order and reverses on failure:

```csharp
var saga = new SagaOrchestrator()
    .AddStep(new ValidateOrderStep())
    .AddStep(new ReserveInventoryStep())
    .AddStep(new ProcessPaymentStep())
    .AddStep(new ArrangeShippingStep());

var result = await saga.ExecuteAsync(context);
```

If `ProcessPayment` fails, the orchestrator calls `ReserveInventory.CompensateAsync()` to release the reserved stock, then `ValidateOrder.CompensateAsync()` (a no-op, since validation has no side effects). The result object records every completed step, every compensated step, and the failure reason.

## Designing Compensation Logic

Compensation is the hardest part of the Saga pattern. It requires careful thinking about what "undo" means for each operation.

**Reversible operations** have straightforward compensations:
- Reserve inventory -> Release inventory
- Charge payment -> Issue refund
- Create shipment -> Cancel shipment

**Irreversible operations** require corrective actions instead of true reversals:
- Send confirmation email -> Send cancellation email
- Publish analytics event -> Publish correction event
- Generate invoice -> Issue credit memo

**Read-only operations** need no compensation at all:
- Validate order data -> No side effects to undo
- Check credit score -> No state change

### Compensation Rules

1. **Idempotency is mandatory.** A compensation may be retried if the first attempt fails partway. Refunding a payment twice must not double-charge the customer's account. Use idempotency keys.

2. **Compensations must not throw.** If a compensation fails, the orchestrator logs the failure and continues compensating remaining steps. A thrown exception in compensation could leave the system in a worse state than doing nothing.

3. **Order is reverse.** Compensations run in reverse order of execution. This ensures that dependencies are unwound correctly (you release inventory before un-validating the order, not the other way around).

## Sharing State Between Steps

Steps communicate through a `SagaContext` -- a key-value dictionary that flows through the entire saga:

```csharp
// Step 1 writes
context.Set("OrderId", orderId);
context.Set("TotalAmount", 299.99m);

// Step 2 reads
var orderId = context.Get<string>("OrderId");
```

This approach avoids coupling between steps. `ProcessPaymentStep` does not depend on `ReserveInventoryStep` directly -- it only depends on the keys that a prior step has set. This makes steps reusable across different saga compositions.

## Combining Saga with the Outbox Pattern

A saga step that commits to its local database and then publishes an event has a reliability gap: if the process crashes between the commit and the publish, the event is lost. The Outbox pattern closes this gap.

Instead of publishing directly, each step writes the event to an outbox table within the same database transaction. A background processor polls the outbox and publishes pending messages to the broker:

```csharp
// Inside a saga step
await _db.SaveChangesAsync(); // commits business data + outbox message atomically

// Background processor (separate concern)
var pending = await _outboxStore.GetPendingAsync(batchSize: 10);
foreach (var message in pending)
{
    await _broker.PublishAsync(message.EventType, message.Payload);
    await _outboxStore.MarkProcessedAsync(message.Id);
}
```

This guarantees at-least-once delivery. Consumers must be idempotent because a message may be delivered more than once if the processor crashes after publishing but before marking the message as processed.

## Monitoring and Observability

Production sagas need observability beyond simple logging:

- **Saga duration metrics.** Track how long each saga takes end-to-end. A spike indicates a slow downstream service.
- **Step-level timing.** Identify which step is the bottleneck.
- **Compensation rate.** A high rate of compensations signals a systemic issue (payment provider degradation, inventory exhaustion).
- **Dead-letter monitoring.** Outbox messages that exceed retry limits need alerting and manual resolution.

The `SagaResult` in this repository provides the raw data for these metrics: `CompletedSteps`, `CompensatedSteps`, `FailedStep`, and `ErrorMessage`.

## When to Use Saga vs Simpler Alternatives

| Scenario | Recommendation |
|----------|---------------|
| Single database, single service | Database transaction (`BEGIN ... COMMIT`) |
| Two services, one can be eventually consistent | Domain Events with Outbox |
| Multiple services, all must succeed or compensate | Saga Orchestrator |
| Loose coupling, no central coordination needed | Saga Choreography (event-driven) |
| Mission-critical with audit requirements | Saga Orchestrator + persistent state + Outbox |

The Saga pattern introduces real complexity: compensation logic, state management, failure handling, and idempotency requirements. Do not reach for it in a system where a simple database transaction would suffice. But when you genuinely need cross-service consistency, a well-implemented saga is far more reliable and maintainable than ad-hoc compensation scattered across services.
