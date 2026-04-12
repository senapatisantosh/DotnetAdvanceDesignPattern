# Implementing the Saga Pattern in Microservices

When a single business operation spans multiple services -- placing an order that requires inventory reservation, payment processing, and shipping arrangement -- you cannot wrap everything in a database transaction. The Saga pattern provides the coordination you need through a sequence of local transactions with compensating actions.

## Why Not Distributed Transactions?

Two-Phase Commit (2PC) requires all participants to hold locks while the coordinator decides to commit or rollback. In a microservices architecture:

- Services use different databases (PostgreSQL, MongoDB, DynamoDB) that may not support 2PC
- Lock duration increases with network latency and service count
- A single unavailable service blocks the entire transaction
- Cloud-managed databases often do not expose 2PC interfaces

Sagas accept eventual consistency in exchange for availability and independence.

## Orchestration: The Practical Approach

An orchestrator coordinates the saga. It knows the steps, their order, and what to do when one fails.

```csharp
public class OrderFulfillmentSaga
{
    private readonly SagaOrchestrator _orchestrator;

    public OrderFulfillmentSaga(
        IOrderValidator validator,
        IInventoryService inventory,
        IPaymentService payment,
        IShippingService shipping)
    {
        _orchestrator = new SagaOrchestrator();
        _orchestrator.AddStep(new ValidateOrderStep(validator));
        _orchestrator.AddStep(new ReserveInventoryStep(inventory));
        _orchestrator.AddStep(new ProcessPaymentStep(payment));
        _orchestrator.AddStep(new ArrangeShippingStep(shipping));
    }
}
```

Each step has an execute and compensate action. The orchestrator runs steps in order. If step 3 fails, it calls compensate on step 2, then step 1. Step 0 (validation) has no side effects, so its compensation is a no-op.

## Designing Compensations

Compensations are NOT rollbacks. They are new forward actions that semantically undo the effect:

| Step | Execute | Compensate |
|------|---------|------------|
| Validate order | Check order details | No-op (no side effects) |
| Reserve inventory | `POST /reservations` | `DELETE /reservations/{id}` |
| Process payment | `POST /charges` | `POST /refunds` |
| Arrange shipping | `POST /shipments` | `PUT /shipments/{id}/cancel` |

Notice: you cannot "un-charge" a credit card. You issue a refund -- a new transaction. You cannot "un-send" a confirmation email. You send a correction email. Design compensations as new business actions.

## The Idempotency Requirement

Compensations (and executions) may run more than once due to retries. Every step MUST be idempotent:

```csharp
public class ReserveInventoryStep : ISagaStep
{
    public async Task<SagaStepResult> ExecuteAsync(SagaContext context)
    {
        // Check if already reserved (idempotent)
        var existingReservation = context.Get<string>("ReservationId");
        if (existingReservation != null)
            return SagaStepResult.Success(); // already done

        var reservationId = await _inventory.ReserveAsync(context.Get<string>("OrderId"));
        context.Set("ReservationId", reservationId);
        return SagaStepResult.Success();
    }
}
```

Without idempotency, a retry after a network timeout could reserve inventory twice or charge a customer twice.

## Persistent Sagas for Long-Running Processes

In-memory sagas work for operations that complete in seconds. For long-running sagas (travel booking, insurance claims, order fulfillment spanning days), the saga state must survive process restarts:

```csharp
public class SagaState
{
    public Guid SagaId { get; set; }
    public string SagaType { get; set; }
    public int CurrentStep { get; set; }
    public SagaStatus Status { get; set; } // Running, Compensating, Completed, Failed
    public string ContextJson { get; set; } // Serialized SagaContext
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

A background worker polls for incomplete sagas and resumes them. This is where frameworks like MassTransit and NServiceBus provide significant value -- they handle saga persistence, message correlation, and timeout management.

## Pairing Sagas with the Outbox

Each saga step that communicates with an external service should use the Outbox pattern for reliable messaging. When the orchestrator tells a step to execute, the step writes its command to an outbox table. A background worker publishes it to the message broker. The response comes back as an event that advances the saga.

This guarantees that no messages are lost between saga steps, even if the orchestrator process crashes.

## Monitoring and Observability

Production sagas require monitoring:

- **Saga duration:** How long does the average fulfillment saga take?
- **Failure rate:** What percentage of sagas require compensation?
- **Compensation success rate:** Are compensations succeeding on first attempt?
- **Stuck sagas:** Any saga running longer than the expected maximum?
- **Step-level timing:** Which step is the bottleneck?

Build a saga dashboard that shows active sagas, their current step, and any that are stuck in compensation. This visibility is essential for production support.

## When NOT to Use Sagas

- All operations can run in a single database transaction
- The process has only 1-2 steps where a simple try/catch is clearer
- Steps cannot be compensated (if step 3 is "launch the missile," there is no compensation)
- Strict isolation is required (other operations must not see intermediate state)
- The team lacks the infrastructure for reliable messaging and idempotency

The Saga pattern adds meaningful complexity. Use it when distributed consistency is genuinely required, not as a default architecture for every multi-step process.
