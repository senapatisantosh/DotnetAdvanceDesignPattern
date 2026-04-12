# Deep Dive: Factory Method vs Abstract Factory vs Simple Factory

Understanding the factory family of patterns is one of the most common interview topics and a frequent source of confusion in production codebases. This guide clarifies when each variant applies and the architectural trade-offs involved.

## The Three Factory Variants at a Glance

| Aspect | Simple Factory | Factory Method | Abstract Factory |
|--------|---------------|---------------|-----------------|
| **GoF pattern?** | No (idiom) | Yes | Yes |
| **Mechanism** | Switch/match in a single method | Inheritance -- subclass overrides creation method | Composition -- factory object with multiple creation methods |
| **Products** | One product, chosen by parameter | One product, chosen by subclass | Family of related products |
| **Extensibility** | Modify the switch (violates OCP) | Add a new creator subclass | Add a new factory implementation |
| **Complexity** | Lowest | Medium | Highest |

## Simple Factory: When It Is Enough

A Simple Factory is a single class with a creation method that uses a switch or dictionary to select the concrete type. It is not a GoF pattern -- it is a pragmatic idiom.

```csharp
public class PaymentProcessorSimpleFactory
{
    public IPaymentProcessor Create(string provider) => provider switch
    {
        "stripe" => new StripeProcessor(),
        "paypal" => new PayPalProcessor(),
        _ => throw new ArgumentException($"Unknown provider: {provider}")
    };
}
```

**Use when:** The set of types is small, stable, and unlikely to grow. The creation logic is trivial. You want centralized creation without the overhead of an inheritance hierarchy.

**Stop using when:** You find yourself modifying the switch statement frequently, or the creation logic differs significantly per product.

## Factory Method: Polymorphic Creation

Factory Method moves the creation decision to subclasses. The base class defines the algorithm template; subclasses plug in the specific product.

```csharp
public abstract class PaymentProcessorFactory
{
    public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
    {
        var processor = CreateProcessor();  // Factory Method
        await processor.Validate(request);
        return await processor.Process(request);
    }

    protected abstract IPaymentProcessor CreateProcessor();
}

public class StripeProcessorFactory : PaymentProcessorFactory
{
    protected override IPaymentProcessor CreateProcessor()
        => new StripeProcessor(apiKey: Environment.GetVariable("STRIPE_KEY"));
}
```

**Key insight:** Factory Method is about more than just creation. The base class defines the workflow (`ProcessPayment`), and the creation decision is one pluggable step within that workflow. If you only need creation (no surrounding algorithm), Simple Factory or DI registration is simpler.

## Abstract Factory: Coordinated Families

Abstract Factory creates entire families of objects that must be used together. Mixing products from different families would be a bug.

```csharp
public interface ICloudInfrastructureFactory
{
    IBlobStorage CreateBlobStorage();
    IQueueClient CreateQueueClient();
    ITableClient CreateTableClient();
}
```

An `AwsFactory` returns S3 + SQS + DynamoDB. An `AzureFactory` returns Blob Storage + Queue Storage + Table Storage. The factory is selected once at startup, and all components come from the same provider.

**Key insight:** If you only have one product type, you do not need Abstract Factory. It exists specifically for the scenario where multiple products must be coordinated.

## Decision Framework

1. **How many product types?** One -> Simple Factory or Factory Method. Multiple related -> Abstract Factory.
2. **Does creation logic vary by subclass?** No -> Simple Factory. Yes -> Factory Method.
3. **Must products be from the same family?** Yes -> Abstract Factory. No -> separate Factory Methods.
4. **Is the set of types stable?** Yes, small -> Simple Factory. Growing -> Factory Method.
5. **Using DI?** Consider whether `services.AddTransient<IPaymentProcessor, StripeProcessor>()` with keyed services eliminates the need for a manual factory entirely.

## Production Considerations

**DI integration.** In modern .NET, factories often live inside the DI container. Register a `Func<string, IPaymentProcessor>` or use keyed services (`.AddKeyedTransient<IPaymentProcessor, StripeProcessor>("stripe")`) rather than hand-rolling factory classes.

**Testing.** Factory Method is easier to test than Simple Factory because you can create a test-specific subclass. Abstract Factory is testable by providing a mock factory that returns stub products.

**Configuration-driven selection.** In production, the factory selection is typically driven by configuration (`appsettings.json`, environment variables, or feature flags), not by client code making runtime decisions.

## Common Mistakes

1. **Using Abstract Factory when you have one product.** This over-engineers the solution. Start with Simple Factory or Factory Method and promote to Abstract Factory only when you genuinely have multiple coordinated products.

2. **God Factory.** A single factory class that creates dozens of unrelated types. Each factory should have a focused responsibility aligned with one product family.

3. **Ignoring DI.** In .NET, the DI container is itself a factory. If your "factory" just maps a string to a type with no additional logic, keyed DI services are simpler.

4. **Factory returning concrete types.** The factory should always return an interface or abstract type. Returning a concrete type defeats the purpose of the pattern.

## Interview-Worthy Insights

- Factory Method uses **inheritance** (the creator IS-A base class). Abstract Factory uses **composition** (the client HAS-A factory).
- Factory Method answers "**which one?**" Abstract Factory answers "**which family?**"
- Simple Factory centralizes creation but violates OCP. Factory Method respects OCP through polymorphism.
- In .NET, `TimeSpan.FromMinutes()` and `Task.FromResult()` are Static Factory Methods -- not the same as Factory Method (GoF), which requires inheritance.
- The real test: if adding a new product type requires modifying existing code, you need Factory Method. If existing code is untouched, your current approach is fine.

## Related Patterns

- **Builder** constructs one complex object step-by-step; Factory creates objects in a single call.
- **Prototype** creates objects by cloning; Factory creates objects by instantiation.
- **Strategy** and Factory Method are structurally similar (both use polymorphism), but Strategy swaps algorithms while Factory Method swaps creation logic.
