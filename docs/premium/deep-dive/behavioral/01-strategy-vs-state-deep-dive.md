# Deep Dive: Strategy vs State -- The Most Commonly Confused Pair

Strategy and State are structurally nearly identical -- both use an interface, concrete implementations, and a context that delegates to the current implementation. The difference is entirely in **who decides** and **when it changes**. This guide resolves the confusion with production scenarios.

## The Fundamental Distinction

| Aspect | Strategy | State |
|--------|----------|-------|
| **Who decides** | The client (external) | The object itself (internal) |
| **When it changes** | Client explicitly sets a new strategy | Automatically on internal events/transitions |
| **Awareness** | Strategies are independent, unaware of each other | States know valid next states |
| **Transition concept** | No transitions -- just replacement | Explicit state machine with defined transitions |
| **Typical trigger** | Configuration, user choice, DI | Domain events, method calls, lifecycle progression |

**The one-sentence test:** If the CALLER picks the behavior, it is Strategy. If the OBJECT evolves its own behavior based on what happens to it, it is State.

## Strategy in Practice

### Pricing Engine Example

```csharp
public interface IPricingStrategy
{
    decimal CalculatePrice(Order order);
}

public class RegularPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.Items.Sum(i => i.Price * i.Quantity);
}

public class PremiumPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order)
        => order.Items.Sum(i => i.Price * i.Quantity) * 0.9m; // 10% discount
}

public class BulkPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order)
    {
        var total = order.Items.Sum(i => i.Price * i.Quantity);
        return order.Items.Sum(i => i.Quantity) > 100 ? total * 0.75m : total * 0.85m;
    }
}
```

The **caller** selects the strategy based on customer tier, configuration, or feature flags. The pricing engine does not care which strategy is active. Strategies never transition between each other.

### DI-Friendly Strategy in .NET

```csharp
// Register all strategies
services.AddKeyedScoped<IPricingStrategy, RegularPricing>("regular");
services.AddKeyedScoped<IPricingStrategy, PremiumPricing>("premium");
services.AddKeyedScoped<IPricingStrategy, BulkPricing>("bulk");

// Or inject all and select
services.AddScoped<IPricingStrategy, RegularPricing>();
services.AddScoped<IPricingStrategy, PremiumPricing>();
services.AddScoped<IPricingStrategy, BulkPricing>();

// In the service
public class OrderService
{
    private readonly IEnumerable<IPricingStrategy> _strategies;

    public decimal CalculatePrice(Order order, string tier)
    {
        var strategy = _strategies.First(s => s.GetType().Name.StartsWith(tier, StringComparison.OrdinalIgnoreCase));
        return strategy.CalculatePrice(order);
    }
}
```

## State in Practice

### Order Lifecycle Example

```csharp
public interface IOrderState
{
    IOrderState AddItem(Order order, LineItem item);
    IOrderState Submit(Order order);
    IOrderState Ship(Order order);
    IOrderState Deliver(Order order);
    IOrderState Cancel(Order order);
}

public class DraftState : IOrderState
{
    public IOrderState AddItem(Order order, LineItem item)
    {
        order.Items.Add(item);
        return this; // stay in Draft
    }

    public IOrderState Submit(Order order)
    {
        if (!order.Items.Any()) throw new InvalidOperationException("Cannot submit empty order");
        order.SubmittedAt = DateTime.UtcNow;
        return new SubmittedState(); // transition to Submitted
    }

    public IOrderState Ship(Order order) => throw new InvalidOperationException("Cannot ship a draft order");
    public IOrderState Deliver(Order order) => throw new InvalidOperationException("Cannot deliver a draft order");
    public IOrderState Cancel(Order order) => new CancelledState();
}

public class SubmittedState : IOrderState
{
    public IOrderState AddItem(Order order, LineItem item)
        => throw new InvalidOperationException("Cannot modify a submitted order");

    public IOrderState Submit(Order order)
        => throw new InvalidOperationException("Order already submitted");

    public IOrderState Ship(Order order)
    {
        order.ShippedAt = DateTime.UtcNow;
        return new ShippedState(); // transition to Shipped
    }

    public IOrderState Cancel(Order order)
    {
        // Submitted orders can be cancelled with a refund
        order.CancelledAt = DateTime.UtcNow;
        return new CancelledState();
    }

    public IOrderState Deliver(Order order) => throw new InvalidOperationException("Must ship before delivering");
}
```

Notice how **states know about other states** and trigger transitions. The `Order` does not decide what state to move to -- the current state returns the next state based on the operation.

## Decision Framework

Ask these questions:

1. **Does the object change its own behavior over time?** Yes -> State. No -> Strategy.
2. **Are there defined transitions between behaviors?** Yes -> State. No -> Strategy.
3. **Does the caller select the behavior?** Yes -> Strategy. No -> State.
4. **Do the implementations know about each other?** Yes -> State. No -> Strategy.
5. **Is there a lifecycle (Draft -> Submitted -> Shipped -> Delivered)?** State.
6. **Is there a selection (Regular pricing vs Premium pricing vs Bulk pricing)?** Strategy.

## The Hybrid Case

Sometimes you need both. A shopping cart might use Strategy for pricing (customer selects their coupon/tier) and State for the cart lifecycle (Active -> Checkout -> Completed -> Archived). These are separate concerns using separate patterns on the same domain object.

## Production Considerations

### State Persistence

State pattern requires special attention for persistence. Options:

1. **Enum + Factory:** Store the state as an enum in the database. Reconstruct the state object on load: `StateFactory.Create(order.StatusEnum)`.
2. **State column:** Store the state class name and use reflection or a dictionary to reconstruct.
3. **EF Core value converter:** Map state objects to/from database columns transparently.

### Strategy and Open/Closed

Adding a new strategy should not require modifying existing code. In .NET, this means:
- Register new strategies in the DI container
- Use `IEnumerable<IStrategy>` injection to auto-discover all implementations
- Select strategies by attribute, name convention, or a `CanHandle()` method

### State Machine Libraries

For complex state machines (20+ states, guard conditions, hierarchical states), consider a library like `Stateless` or `MassTransit.Automatonymous` rather than hand-coding the State pattern. The pattern is best for 3-8 states with clear transitions.

## Common Mistakes

1. **Using Strategy when the object should manage its own transitions.** If you find the caller constantly switching strategies based on the object's properties, you need State.
2. **Using State when the caller should choose.** If the "state" is really a configuration choice (sort ascending vs descending), it is Strategy.
3. **States with no transitions.** If your "states" never transition, they are strategies with a misleading name.
4. **Giant switch statements instead of State.** The State pattern exists to replace `switch(order.Status)` blocks scattered across the codebase.

## Interview-Worthy Insights

- Strategy and State are **structurally identical** (UML class diagram). The difference is **behavioral** -- who initiates the change.
- In .NET, `IComparer<T>` implementations are strategies. `HttpClient` handler chains are strategies (you pick the handler pipeline).
- State machines map naturally to **database workflows** -- each row has a status column that determines available operations.
- Template Method is Strategy's inheritance-based cousin. Strategy uses composition (swap the algorithm object); Template Method uses inheritance (override the step method).
- An advanced question: "Can you use both on the same object?" Yes -- a `PaymentGateway` might use State for transaction lifecycle (Pending -> Authorized -> Captured -> Settled) and Strategy for the payment method (credit card vs bank transfer vs wallet).
