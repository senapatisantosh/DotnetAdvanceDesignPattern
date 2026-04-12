# Deep Dive: Command vs Chain of Responsibility vs Mediator -- Request Handling Patterns Compared

These three behavioral patterns all deal with requests and their handlers, but they solve different problems. Command encapsulates WHAT to do. Chain of Responsibility decides WHO handles it. Mediator coordinates HOW multiple components interact.

## Three Patterns at a Glance

| Aspect | Command | Chain of Responsibility | Mediator |
|--------|---------|------------------------|----------|
| **Focus** | Encapsulate an action as an object | Route a request through a pipeline | Coordinate complex interactions |
| **Handler count** | Exactly one receiver | Zero, one, or many | Central hub routes to appropriate colleague |
| **Undo support** | Built-in | Not typical | Not typical |
| **Queuing** | Yes (commands are objects) | No (immediate flow) | Possible (mediator controls timing) |
| **ASP.NET analog** | `IRequest<T>` in MediatR | Middleware pipeline | MediatR itself |

## Command: Action as Object

Command turns a method call into an object with state, enabling undo, queuing, logging, and replay.

```csharp
public interface ICommand
{
    Task ExecuteAsync();
    Task UndoAsync();
}

public class ReserveInventoryCommand : ICommand
{
    private readonly IInventoryService _inventory;
    private readonly string _sku;
    private readonly int _quantity;
    private string _reservationId;

    public async Task ExecuteAsync()
    {
        _reservationId = await _inventory.ReserveAsync(_sku, _quantity);
    }

    public async Task UndoAsync()
    {
        if (_reservationId != null)
            await _inventory.ReleaseAsync(_reservationId);
    }
}
```

**Key characteristic:** The command stores the state needed to both execute and reverse the action. The invoker (caller) does not know what the command does -- it just calls `Execute()`.

**Production uses:** Undo/redo in editors, transaction logging, job queues (`Hangfire`, `MassTransit`), saga steps (each step IS a command with a compensating action).

## Chain of Responsibility: Pipeline Processing

CoR passes a request through a sequence of handlers. Each handler decides whether to process the request, modify it, or pass it along.

```csharp
public abstract class ApprovalHandler
{
    private ApprovalHandler _next;

    public ApprovalHandler SetNext(ApprovalHandler next)
    {
        _next = next;
        return next;
    }

    public virtual async Task<ApprovalResult> HandleAsync(ExpenseRequest request)
    {
        if (_next != null)
            return await _next.HandleAsync(request);
        return ApprovalResult.Rejected("No handler could approve this amount");
    }
}

public class TeamLeadHandler : ApprovalHandler
{
    public override async Task<ApprovalResult> HandleAsync(ExpenseRequest request)
    {
        if (request.Amount <= 1000)
            return ApprovalResult.Approved("Team Lead");
        return await base.HandleAsync(request); // pass to next
    }
}
```

**Key characteristic:** The chain can short-circuit (stop processing), or each handler can contribute to the result. Handlers are unaware of their position in the chain.

**Production uses:** ASP.NET Core middleware, MediatR pipeline behaviors, validation pipelines, approval workflows, exception handling chains.

## Mediator: Central Coordination Hub

Mediator centralizes complex interactions between multiple components that would otherwise form a tangled web of direct references.

```csharp
public interface ICheckoutMediator
{
    Task<CheckoutResult> ProcessCheckout(CheckoutRequest request);
}

public class CheckoutMediator : ICheckoutMediator
{
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IShippingService _shipping;
    private readonly INotificationService _notification;

    public async Task<CheckoutResult> ProcessCheckout(CheckoutRequest request)
    {
        var stockResult = await _inventory.CheckStockAsync(request.Items);
        if (!stockResult.AllAvailable)
            return CheckoutResult.Failed("Items out of stock");

        var paymentResult = await _payment.ChargeAsync(request.PaymentMethod, stockResult.Total);
        if (!paymentResult.Success)
        {
            await _inventory.ReleaseReservation(stockResult.ReservationId);
            return CheckoutResult.Failed("Payment failed");
        }

        var shipment = await _shipping.ArrangeAsync(request.Address, request.Items);
        await _notification.SendConfirmationAsync(request.CustomerId, shipment.TrackingId);

        return CheckoutResult.Success(shipment.TrackingId);
    }
}
```

**Key characteristic:** Components do not talk to each other directly. The mediator knows all components and orchestrates their interactions. Adding a new component means modifying the mediator, not every other component.

**Production uses:** MediatR library (the name is literal), checkout orchestration, chat rooms, complex form validation, workflow engines.

## How They Relate

### Command + Chain of Responsibility

MediatR combines these: a Command (request) is sent through a pipeline of behaviors (CoR) before reaching its handler. Pipeline behaviors add logging, validation, and transaction management:

```csharp
// The request is a Command
public record CreateOrderCommand(string CustomerId, List<LineItem> Items) : IRequest<OrderResult>;

// Pipeline behaviors form a Chain of Responsibility
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Validate before passing to next handler in chain
        var failures = await _validators.ValidateAsync(request);
        if (failures.Any()) throw new ValidationException(failures);
        return await next(); // pass to next behavior or final handler
    }
}
```

### Command + Mediator

The Mediator dispatches commands to their handlers. You send a command to the mediator, and it routes it to the correct handler. This decouples the sender from the handler.

### Chain of Responsibility + Mediator

ASP.NET Core's middleware pipeline is CoR, and the endpoint routing acts as a Mediator (routing requests to the correct controller/handler based on the URL).

## Decision Framework

| Question | Answer | Pattern |
|----------|--------|---------|
| Do you need undo/redo or queuing? | Yes | Command |
| Is the request processed by a pipeline of handlers? | Yes | Chain of Responsibility |
| Are multiple components interacting in complex ways? | Yes | Mediator |
| Do you need all three? | Yes | MediatR (literally combines them) |

## Common Mistakes

1. **Command without undo.** If your "command" has no `Undo()` and is not queued/logged, it may just be a method call dressed up as a pattern.
2. **CoR where order does not matter.** If handlers are independent and order-insensitive, you may want Observer (publish/subscribe) instead of a chain.
3. **Mediator as a God class.** If the mediator grows to coordinate 20+ components with complex conditional logic, break it into smaller focused mediators or consider a workflow engine.
4. **Using MediatR for simple CRUD.** Sending a `GetUserByIdQuery` through MediatR when a direct service call would suffice adds ceremony without benefit.

## Interview-Worthy Insights

- ASP.NET Core middleware is **Chain of Responsibility** -- each middleware calls `next()` or short-circuits.
- MediatR is both a **Mediator** (routes requests to handlers) and a **Chain of Responsibility** (pipeline behaviors).
- The Saga pattern uses **Commands** as its steps -- each saga step is a command with an execute and compensate action.
- Command turns a verb (action) into a noun (object). This reification enables features impossible with method calls: serialization, queuing, undo, replay.
- Key differentiator in interviews: Command answers "WHAT should happen." CoR answers "WHO should handle it." Mediator answers "HOW should components coordinate."
