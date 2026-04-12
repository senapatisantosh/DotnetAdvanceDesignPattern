# Creational Patterns Summary

Creational patterns abstract the instantiation process, making the system independent of how objects are created. This repository demonstrates all five GoF creational patterns with production-style .NET 10 examples.

---

## Summary Table

| Pattern          | Use Case (This Repo)  | Key Benefit                          | When to Use                                          | When to Avoid                                       |
|------------------|-----------------------|--------------------------------------|------------------------------------------------------|------------------------------------------------------|
| Factory Method   | Payment Gateways      | Decouples client from concrete types | Type is determined at runtime by config or input      | Only 1-2 types and no future extensibility expected  |
| Abstract Factory | Cloud Storage         | Ensures compatible object families   | Multiple related objects must be created together     | Only one product type exists                         |
| Builder          | Invoice Generation    | Step-by-step construction            | Object has 4+ params, optional parts, or invariants  | Object is simple with few required fields            |
| Prototype        | Feature Flags         | Avoids expensive re-creation         | Object setup is costly; variants differ slightly     | Objects are cheap to create or have no shared state  |
| Singleton         | Telemetry Registry    | Guarantees single instance           | Exactly one coordinator must exist (registry, pool)  | DI container can manage lifetime instead             |

---

## Pattern Details

### Factory Method — Payment Gateways

**Intent**: Define an interface for creating an object, but let subclasses decide which class to instantiate.

**This repo's scenario**: A payment processing system where the gateway (Stripe, PayPal, Square) is selected based on merchant configuration. The factory method returns an `IPaymentProcessor` without the client knowing the concrete type.

**Variants implemented**:
- **Naive Factory** — `if/else` or `switch` inside a single method (shown as the "before" anti-pattern).
- **Simple Factory** — A dedicated factory class with a creation method.
- **Static Factory** — A static method on the product type itself.
- **Classic Factory Method** — Abstract creator class with overriding subclasses.

**Key participants**:
- `IPaymentProcessor` — Product interface
- `StripeProcessor`, `PayPalProcessor` — Concrete products
- `PaymentProcessorFactory` — Creator

**When to choose Factory Method over other creational patterns**:
- You need **one** product (not a family) created polymorphically.
- The creation logic should be extensible by adding new subclasses.
- You want to follow the Open/Closed Principle for object creation.

---

### Abstract Factory — Cloud Storage

**Intent**: Provide an interface for creating families of related objects without specifying their concrete classes.

**This repo's scenario**: Cloud storage abstraction where each provider (AWS, Azure) supplies a set of related services — blob storage, queue client, and table client. Switching providers means switching the factory, not individual services.

**Key participants**:
- `ICloudStorageFactory` — Abstract factory
- `AwsCloudStorageFactory`, `AzureCloudStorageFactory` — Concrete factories
- `IBlobStorage`, `IQueueClient`, `ITableClient` — Abstract products
- `S3BlobStorage`, `AzureBlobStorage`, etc. — Concrete products

**When to choose Abstract Factory over Factory Method**:
- You need **families** of related objects, not just one.
- Objects in a family must be used together (e.g., AWS S3 + SQS + DynamoDB).
- Mixing incompatible implementations would be a bug.

---

### Builder — Invoice Generation

**Intent**: Separate the construction of a complex object from its representation so that the same construction process can create different representations.

**This repo's scenario**: Building invoices with header info, line items, discounts, tax calculations, payment terms, and notes. A step builder ensures required fields are set before optional ones.

**Variants implemented**:
- **Step Builder** — Fluent interface that enforces a required sequence of calls via return types.

**Key participants**:
- `InvoiceBuilder` — Builder with fluent API
- `Invoice` — Complex product being constructed
- Step interfaces — `ISetCustomer`, `ISetLineItems`, `ISetOptionalDetails`, `IBuild`

**When to choose Builder over constructor/Abstract Factory**:
- The object has many parameters (constructor telescoping problem).
- Some parameters are optional with sensible defaults.
- You need validation during construction, not after.
- You want a readable, fluent construction API.

---

### Prototype — Feature Flags

**Intent**: Specify the kinds of objects to create using a prototypical instance, and create new objects by copying this prototype.

**This repo's scenario**: Feature flag configurations where a base configuration is cloned and tweaked for A/B testing variants. Cloning avoids re-reading config files or re-querying databases.

**Key participants**:
- `IPrototype<T>` — Declares `Clone()` method
- `FeatureFlagConfig` — Concrete prototype with deep copy

**When to choose Prototype**:
- Creating a new instance is expensive (I/O, computation).
- Objects differ only slightly from an existing instance.
- You want to avoid subclass proliferation just for configuration differences.

---

### Singleton — Telemetry Registry

**Intent**: Ensure a class has only one instance and provide a global point of access.

**This repo's scenario**: A telemetry metric registry that collects counters and histograms across the application. Multiple registries would fragment metrics.

**Variants implemented**:
- **Thread-Safe Singleton** — Using `lock` statement.
- **Lazy Singleton** — Using `Lazy<T>` for thread-safe lazy initialization.
- **DI-Preferred** — Singleton lifetime via `services.AddSingleton<T>()` (recommended approach).

**Key participants**:
- `TelemetryRegistry` — Singleton class
- DI registration — `services.AddSingleton<ITelemetryRegistry, TelemetryRegistry>()`

**When to choose Singleton**:
- Exactly one instance must coordinate access to a shared resource.
- The instance must be available across the entire application.

**Important caveat**: In modern .NET, prefer registering a class with singleton lifetime in the DI container rather than implementing the GoF singleton pattern. This is testable, injectable, and does not require static access.

---

## Comparison Notes

### Factory Method vs Abstract Factory

| Aspect              | Factory Method                          | Abstract Factory                           |
|---------------------|------------------------------------------|--------------------------------------------|
| **Products**        | One product type                        | Family of related products                 |
| **Extension**       | Add new creator subclass                | Add new factory implementation             |
| **Complexity**      | Lower                                   | Higher (more interfaces)                   |
| **Use when**        | Single object varies by config          | Multiple objects must be compatible        |
| **Example**         | `CreatePaymentProcessor()`              | `CreateBlobStorage()` + `CreateQueue()`    |

**Rule of thumb**: If you need one product, use Factory Method. If you need a coordinated set, use Abstract Factory.

### Builder vs Abstract Factory

| Aspect              | Builder                                  | Abstract Factory                           |
|---------------------|------------------------------------------|--------------------------------------------|
| **Focus**           | Step-by-step construction               | Family of products created at once         |
| **Product**         | One complex product                     | Multiple related products                  |
| **Flexibility**     | Optional parts, validation at each step | All-or-nothing creation                    |
| **API style**       | Fluent method chaining                  | Factory method calls                       |
| **Use when**        | Object construction is complex          | Object families must be compatible         |

**Rule of thumb**: Builder constructs ONE complex thing step by step. Abstract Factory creates MANY related things at once.

### Factory Method vs Prototype

| Aspect              | Factory Method                          | Prototype                                  |
|---------------------|------------------------------------------|--------------------------------------------|
| **Mechanism**       | Subclass overrides creation method      | Clone an existing instance                 |
| **Inheritance**     | Requires class hierarchy                | No hierarchy needed                        |
| **Performance**     | Normal construction cost                | Avoids expensive initialization            |
| **Use when**        | Type varies by subclass                 | Initialization is expensive                |

---

## .NET-Specific Idioms

| Pattern          | .NET Idiom                                                          |
|------------------|---------------------------------------------------------------------|
| Factory Method   | `Func<T>` delegates, `IServiceProvider.GetRequiredService<T>()`     |
| Abstract Factory | Keyed DI services (`[FromKeyedServices]` in .NET 8+)               |
| Builder          | Fluent APIs like `WebApplication.CreateBuilder()`, `IHostBuilder`   |
| Prototype        | `ICloneable`, `record` with `with` expressions for shallow copy    |
| Singleton        | `services.AddSingleton<T>()`, `Lazy<T>`                            |

---

## Next Steps

- [Structural Patterns Summary](structural-patterns-summary.md)
- [Comparison Maps](comparison-maps.md) — Factory Method vs Abstract Factory deep dive
- [Anti-Patterns](anti-patterns.md) — Singleton abuse and God Factory
