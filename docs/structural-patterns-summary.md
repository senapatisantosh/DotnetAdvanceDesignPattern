# Structural Patterns Summary

Structural patterns deal with object and class **composition** — how classes and objects are assembled into larger structures while remaining flexible and efficient. This repository demonstrates all seven GoF structural patterns with production-style .NET 10 examples.

---

## Summary Table

| Pattern    | Use Case (This Repo)    | Key Benefit                              | When to Use                                             | When to Avoid                                        |
|------------|-------------------------|------------------------------------------|---------------------------------------------------------|------------------------------------------------------|
| Adapter    | Shipping Carriers       | Makes incompatible interfaces work       | Integrating third-party or legacy APIs                  | You control both interfaces and can change them      |
| Bridge     | Notifications           | Varies abstraction and impl independently| Two orthogonal dimensions of variation                  | Only one dimension varies                            |
| Composite  | Permission Hierarchy    | Uniform tree traversal                   | Part-whole hierarchies (menus, orgs, permissions)       | Structure is flat, not hierarchical                  |
| Decorator  | API Client Resilience   | Adds behavior without subclassing        | Cross-cutting concerns (logging, retry, caching)        | Single responsibility, no layering needed            |
| Facade     | Report Generation       | Simplifies complex subsystems            | Subsystem has many classes; clients need a simple API   | Subsystem is already simple                          |
| Flyweight  | Tax Rates               | Reduces memory via shared state          | Large number of objects share significant common state   | Objects are few or state is not shareable             |
| Proxy      | Inventory Service       | Controls access transparently            | Lazy loading, caching, access control, remote calls     | Direct access is sufficient and performant           |

---

## Pattern Details

### Adapter — Shipping Carriers

**Intent**: Convert the interface of a class into another interface clients expect. Adapter lets classes work together that could not otherwise because of incompatible interfaces.

**This repo's scenario**: Unifying FedEx, UPS, and DHL shipping APIs — each with different method signatures, data formats, and authentication — behind a single `IShippingCarrier` interface.

**Variants implemented**:
- **Object Adapter** (composition) — Wraps the third-party class in an adapter that implements your interface. Preferred approach.
- **Class Adapter** (inheritance) — Inherits from the adaptee and implements your interface. Possible in C# but less flexible.

**Key participants**:
- `IShippingCarrier` — Target interface your code expects
- `FedExApi`, `UpsApi` — Adaptees (third-party classes with incompatible interfaces)
- `FedExAdapter`, `UpsAdapter` — Adapters that bridge the gap

**When to choose Adapter**:
- You cannot modify the third-party or legacy code.
- You want to isolate your codebase from external API changes.
- You need to support multiple vendors with different APIs behind one interface.

---

### Bridge — Notifications

**Intent**: Decouple an abstraction from its implementation so that the two can vary independently.

**This repo's scenario**: A notification system where message types (Alert, Report, Reminder) and delivery channels (Email, SMS, Push) vary independently. Without Bridge, you would need `AlertEmail`, `AlertSms`, `AlertPush`, `ReportEmail`, `ReportSms`, etc. — an explosion of classes.

**Key participants**:
- `Notification` — Abstraction (message type)
- `INotificationChannel` — Implementation interface (delivery channel)
- `AlertNotification`, `ReportNotification` — Refined abstractions
- `EmailChannel`, `SmsChannel`, `PushChannel` — Concrete implementations

**When to choose Bridge**:
- Two independent dimensions of variation would cause a class explosion.
- You want to switch implementations at runtime.
- Both the abstraction and implementation should be extensible independently.

---

### Composite — Permission Hierarchy

**Intent**: Compose objects into tree structures to represent part-whole hierarchies. Composite lets clients treat individual objects and compositions uniformly.

**This repo's scenario**: A permission system where roles can contain individual permissions and other roles, forming a tree. Checking `HasPermission()` traverses the tree recursively.

**Key participants**:
- `IPermissionComponent` — Component interface with `HasPermission()`
- `Permission` — Leaf (individual permission)
- `Role` — Composite (contains permissions and other roles)

**When to choose Composite**:
- You have a tree/hierarchy structure (org charts, file systems, menus, permissions).
- Clients should treat leaves and containers the same way.
- Operations must cascade through the tree.

---

### Decorator — API Client Resilience

**Intent**: Attach additional responsibilities to an object dynamically. Decorators provide a flexible alternative to subclassing for extending functionality.

**This repo's scenario**: An HTTP API client for an external service, where decorators add retry logic, logging, response caching, and circuit-breaker behavior — each as a separate, composable layer.

**Key participants**:
- `IApiClient` — Component interface
- `HttpApiClient` — Concrete component (real HTTP calls)
- `RetryDecorator` — Adds retry with exponential backoff
- `LoggingDecorator` — Logs requests and responses
- `CachingDecorator` — Caches GET responses

**Decorator stacking** (order matters):
```
Logging → Retry → Caching → HttpApiClient
```

**When to choose Decorator**:
- You need to add behavior to individual objects, not entire classes.
- Adding behavior via subclassing would create too many subclasses.
- You want to combine behaviors flexibly (logging + retry but not caching).
- Cross-cutting concerns need to be applied selectively.

---

### Facade — Report Generation

**Intent**: Provide a unified interface to a set of interfaces in a subsystem. Facade defines a higher-level interface that makes the subsystem easier to use.

**This repo's scenario**: A `ReportGenerationFacade` that orchestrates data fetching, aggregation, formatting, and PDF rendering subsystems. Callers just call `GenerateReport(reportType, dateRange)`.

**Key participants**:
- `ReportGenerationFacade` — Simplified entry point
- `DataFetcher`, `DataAggregator`, `ReportFormatter`, `PdfRenderer` — Subsystem classes

**When to choose Facade**:
- A subsystem has many classes and complex interactions.
- You want to provide a simple API for the common use cases.
- You want to layer your system and define entry points to each layer.
- You want to reduce coupling between clients and subsystem internals.

---

### Flyweight — Tax Rates

**Intent**: Use sharing to support large numbers of fine-grained objects efficiently.

**This repo's scenario**: Tax rate objects shared across millions of transaction calculations. Instead of creating a new `TaxRate` object per transaction, a flyweight factory returns shared, immutable instances keyed by jurisdiction.

**Key participants**:
- `TaxRate` — Flyweight (immutable, shared state: jurisdiction, rate percentage)
- `TaxRateFactory` — Manages the flyweight pool
- Transaction calculation — Extrinsic state (amount, date) passed in at usage time

**When to choose Flyweight**:
- You have a very large number of objects.
- Most object state can be made extrinsic (passed in from outside).
- The intrinsic state is shared and immutable.
- Memory consumption is a measurable concern.

---

### Proxy — Inventory Service

**Intent**: Provide a surrogate or placeholder for another object to control access to it.

**This repo's scenario**: An inventory service wrapped with multiple proxy types for different access control needs.

**Variants implemented**:
- **Caching Proxy** — Caches inventory lookups to avoid repeated database calls.
- **Virtual Proxy** — Lazily initializes the real inventory service on first use.
- **Protection Proxy** — Checks user authorization before allowing inventory modifications.

**Key participants**:
- `IInventoryService` — Subject interface
- `InventoryService` — Real subject
- `CachingInventoryProxy`, `VirtualInventoryProxy`, `ProtectionInventoryProxy` — Proxies

**When to choose Proxy**:
- You need transparent caching, lazy initialization, or access control.
- The client should not know or care that it is talking to a proxy.
- You want to add infrastructure concerns without modifying the real service.

---

## Comparison Notes

### Decorator vs Proxy

| Aspect              | Decorator                                  | Proxy                                       |
|---------------------|--------------------------------------------|--------------------------------------------|
| **Intent**          | Add behavior/responsibility                | Control access                             |
| **Client awareness**| Client explicitly composes decorators      | Client typically does not know about proxy |
| **Wrapping**        | Multiple decorators can stack              | Usually one proxy per subject              |
| **Lifecycle**       | Same lifecycle as component                | May manage the subject's lifecycle         |
| **Use when**        | Adding logging, retry, caching layers      | Lazy loading, auth checks, remote access   |
| **Key difference**  | Enhances what an object *does*             | Controls *whether* the object is accessed  |

**One sentence**: Decorator **adds** functionality; Proxy **controls access** to functionality.

### Facade vs Adapter

| Aspect              | Facade                                     | Adapter                                    |
|---------------------|--------------------------------------------|--------------------------------------------|
| **Intent**          | Simplify a complex subsystem               | Make incompatible interfaces compatible    |
| **Scope**           | Entire subsystem (many classes)            | Usually one class/interface                |
| **Interface**       | Defines a new, simpler interface           | Conforms to an existing target interface   |
| **Direction**       | Inward (hides subsystem complexity)        | Outward (integrates external code)         |
| **Use when**        | You own the subsystem, want simple API     | You cannot change the external interface   |

**One sentence**: Facade **simplifies** your own complex code; Adapter **translates** someone else's incompatible code.

### Decorator vs Adapter

| Aspect              | Decorator                                  | Adapter                                    |
|---------------------|--------------------------------------------|--------------------------------------------|
| **Interface change**| Same interface in and out                  | Different interface in and out             |
| **Purpose**         | Add behavior                               | Translate interface                        |
| **Composability**   | Stack multiple decorators                  | Usually one adapter per adaptee            |

### Proxy vs Adapter

| Aspect              | Proxy                                      | Adapter                                    |
|---------------------|--------------------------------------------|--------------------------------------------|
| **Interface**       | Same interface as the real subject         | Converts from one interface to another     |
| **Purpose**         | Control access to existing behavior        | Bridge incompatible interfaces             |
| **Subject**         | Already conforms to the interface          | Does not conform to the target interface   |

---

## Structural Patterns and SOLID Principles

| Pattern    | Primary SOLID Principle               | How                                                  |
|------------|---------------------------------------|------------------------------------------------------|
| Adapter    | Dependency Inversion                  | Client depends on abstraction, not third-party types |
| Bridge     | Single Responsibility + Open/Closed   | Separates two concerns; both extensible              |
| Composite  | Liskov Substitution                   | Leaves and composites are interchangeable            |
| Decorator  | Open/Closed                           | Extends behavior without modifying existing code     |
| Facade     | Interface Segregation                 | Exposes only what clients need                       |
| Flyweight  | Single Responsibility                 | Separates intrinsic (shared) from extrinsic state    |
| Proxy      | Single Responsibility                 | Separates access control from business logic         |

---

## .NET-Specific Idioms

| Pattern    | .NET Idiom                                                              |
|------------|-------------------------------------------------------------------------|
| Adapter    | Implement your interface, compose the third-party class via DI          |
| Bridge     | Use DI to inject the implementation dimension into the abstraction      |
| Composite  | `IEnumerable<T>` for children; LINQ for traversal                      |
| Decorator  | Scrutor library for auto-decoration; `services.Decorate<I, D>()`       |
| Facade     | Extension methods on `IServiceCollection` that register subsystems      |
| Flyweight  | `ConcurrentDictionary<TKey, TValue>` as the flyweight pool             |
| Proxy      | `DispatchProxy` for dynamic proxies; Castle DynamicProxy               |

---

## Next Steps

- [Behavioral Patterns Summary](behavioral-patterns-summary.md)
- [Comparison Maps](comparison-maps.md) — Decorator vs Proxy and Facade vs Adapter deep dives
- [Anti-Patterns](anti-patterns.md) — Leaky abstractions and over-wrapping
