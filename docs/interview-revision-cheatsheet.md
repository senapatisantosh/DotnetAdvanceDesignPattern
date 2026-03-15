# Interview Revision Cheat Sheet

A master quick-reference guide for design pattern revision. Use this before interviews, code reviews, or architecture discussions.

---

## One-Liner Per Pattern

### Creational Patterns

| Pattern          | One-Liner                                                              |
|------------------|------------------------------------------------------------------------|
| Factory Method   | Defer object creation to subclasses or factory methods                 |
| Abstract Factory | Create families of related objects without specifying concrete classes  |
| Builder          | Construct complex objects step by step with a fluent API               |
| Prototype        | Clone existing objects to avoid expensive re-creation                  |
| Singleton        | Ensure exactly one instance exists; prefer DI singleton lifetime       |

### Structural Patterns

| Pattern    | One-Liner                                                                |
|------------|--------------------------------------------------------------------------|
| Adapter    | Convert an incompatible interface into the one clients expect            |
| Bridge     | Separate abstraction from implementation so both vary independently      |
| Composite  | Treat individual and composite objects uniformly in tree structures       |
| Decorator  | Wrap an object to add behavior dynamically without subclassing           |
| Facade     | Provide a simple interface to a complex subsystem                        |
| Flyweight  | Share immutable state across many objects to save memory                  |
| Proxy      | Control access to an object (caching, auth, lazy loading)                |

### Behavioral Patterns

| Pattern                  | One-Liner                                                          |
|--------------------------|--------------------------------------------------------------------|
| Chain of Responsibility  | Pass a request along a chain until a handler processes it          |
| Command                  | Encapsulate an action as an object for undo, queue, and logging    |
| Interpreter              | Parse and evaluate expressions in a domain-specific language       |
| Iterator                 | Traverse a collection without exposing its internal structure       |
| Mediator                 | Centralize complex communications between multiple objects         |
| Memento                  | Capture and restore an object's state without breaking encapsulation|
| Observer                 | Notify multiple dependents when a subject's state changes          |
| State                    | Change an object's behavior when its internal state changes        |
| Strategy                 | Swap algorithms at runtime by injecting different implementations  |
| Template Method          | Define algorithm skeleton in base class; subclasses fill in steps  |
| Visitor                  | Add operations to a type hierarchy without modifying it            |

### Enterprise Patterns

| Pattern         | One-Liner                                                            |
|-----------------|----------------------------------------------------------------------|
| Repository      | Abstract data access behind a collection-like interface              |
| Unit of Work    | Group multiple changes into a single atomic transaction              |
| Specification   | Encapsulate query criteria as composable, reusable objects            |
| Result Pattern  | Represent success/failure as a return type instead of exceptions      |
| CQRS            | Separate the read model from the write model                         |
| Domain Events   | Raise events from aggregates to trigger decoupled side effects       |
| Outbox          | Store events transactionally alongside data for reliable delivery    |
| Null Object     | Provide a do-nothing implementation instead of returning null        |
| Value Object    | Model immutable domain concepts compared by value, not identity      |
| Saga            | Coordinate distributed transactions with compensating actions        |
| Options Pattern | Bind configuration to strongly-typed objects with validation         |
| Policy Pattern  | Compose business rules as first-class, evaluable objects             |

---

## Pattern-to-Use-Case Quick Lookup

| Use Case                                     | Pattern(s)                          |
|----------------------------------------------|-------------------------------------|
| Create objects without knowing concrete type  | Factory Method, Abstract Factory    |
| Build complex objects with many options       | Builder                             |
| Clone expensive objects                       | Prototype                           |
| Ensure single instance                       | Singleton (DI lifetime)             |
| Integrate third-party APIs                    | Adapter                             |
| Two dimensions of variation                  | Bridge                              |
| Tree/hierarchy structures                    | Composite                           |
| Add logging, retry, caching to services      | Decorator                           |
| Simplify complex subsystem                   | Facade                              |
| Reduce memory for many similar objects       | Flyweight                           |
| Lazy loading, caching, access control        | Proxy                               |
| Approval pipelines, validation chains        | Chain of Responsibility             |
| Undo/redo, command queuing                   | Command + Memento                   |
| Parse domain-specific expressions            | Interpreter                         |
| Lazy/async collection traversal              | Iterator                            |
| Coordinate multiple services                 | Mediator                            |
| State-dependent behavior (order lifecycle)   | State                               |
| Runtime algorithm selection                  | Strategy                            |
| Fixed algorithm with variable steps          | Template Method                     |
| Add operations to stable type hierarchy      | Visitor                             |
| Abstract database access                     | Repository + Unit of Work           |
| Composable query filters                     | Specification                       |
| Error handling without exceptions            | Result Pattern                      |
| Separate read/write models                   | CQRS                                |
| Decouple side effects from domain logic      | Domain Events                       |
| Guaranteed event delivery                    | Outbox                              |
| Eliminate null checks                        | Null Object                         |
| Immutable domain primitives                  | Value Object                        |
| Multi-step distributed workflows             | Saga                                |
| Typed configuration with validation          | Options Pattern                     |
| Composable business rules                    | Policy Pattern                      |

---

## "Choose This When..." Bullets

### Creational

- **Factory Method** — Choose when the concrete type depends on runtime input or configuration, and you want to add new types without modifying existing code.
- **Abstract Factory** — Choose when you need coordinated families of objects (e.g., AWS vs Azure) where mixing implementations would be a bug.
- **Builder** — Choose when constructors have 4+ parameters, optional parts, or need step-by-step validation.
- **Prototype** — Choose when cloning an existing configured object is cheaper than constructing from scratch.
- **Singleton** — Choose when exactly one instance must exist AND you cannot use DI lifetime management. Otherwise, use `AddSingleton`.

### Structural

- **Adapter** — Choose when integrating code you cannot modify with an interface you cannot change.
- **Bridge** — Choose when two independent dimensions of variation would cause a class explosion (M x N combinations).
- **Composite** — Choose when you have tree structures and want to treat leaves and branches identically.
- **Decorator** — Choose when you need to add/remove/compose behaviors at runtime without modifying existing classes.
- **Facade** — Choose when a subsystem is complex and most clients only need a simplified API.
- **Flyweight** — Choose when memory profiling shows thousands of objects sharing significant common state.
- **Proxy** — Choose when you need to intercept access to an object transparently (caching, auth, lazy init).

### Behavioral

- **Chain of Responsibility** — Choose when multiple handlers might process a request, and you want to add/reorder handlers without changing callers.
- **Command** — Choose when you need undo/redo, operation logging, queuing, or macro recording.
- **Interpreter** — Choose when you have a small, stable grammar that users express repeatedly.
- **Iterator** — Choose when you need lazy, on-demand traversal of paginated or streamed data.
- **Mediator** — Choose when many objects have complex inter-dependencies and direct coupling would be a maintenance nightmare.
- **Memento** — Choose when you need state snapshots for undo/redo and direct state access would break encapsulation.
- **Observer** — Choose when multiple objects must react to events from a single source.
- **State** — Choose when an object's behavior is entirely determined by its current state and state transitions are well-defined.
- **Strategy** — Choose when you have multiple algorithms for the same task and the right one is selected at runtime.
- **Template Method** — Choose when the algorithm structure is fixed but specific steps vary across implementations.
- **Visitor** — Choose when you need many operations over a stable type hierarchy without modifying those types.

### Enterprise

- **Repository** — Choose when you need swappable persistence, complex query encapsulation, or testable data access.
- **Unit of Work** — Choose when multiple repository operations must succeed or fail together atomically.
- **Specification** — Choose when query criteria are complex, reusable, and need dynamic composition.
- **Result Pattern** — Choose when failures are expected (validation, not-found) and should be explicit in return types.
- **CQRS** — Choose when read and write shapes, frequencies, or scaling requirements differ significantly.
- **Domain Events** — Choose when domain actions have side effects that should not pollute the aggregate.
- **Outbox** — Choose when event delivery must survive process or broker failures.
- **Null Object** — Choose when a safe, well-defined default behavior exists for the absence of an object.
- **Value Object** — Choose when a domain concept has no identity and should be immutable with value equality.
- **Saga** — Choose when a business process spans multiple services and needs compensating actions on failure.
- **Options Pattern** — Choose when configuration needs strong typing, validation, and possible hot-reload.
- **Policy Pattern** — Choose when business rules are combinatorial and need independent evaluation and composition.

---

## Common Interview Questions

### Creational Patterns

| Pattern          | Common Questions                                                                    |
|------------------|-------------------------------------------------------------------------------------|
| Factory Method   | "How does Factory Method differ from Abstract Factory?"                             |
|                  | "When would you use a factory instead of direct instantiation?"                     |
|                  | "How does the factory pattern support the Open/Closed Principle?"                   |
| Abstract Factory | "Give a real-world example of Abstract Factory."                                    |
|                  | "What problem does Abstract Factory solve that Factory Method cannot?"              |
| Builder          | "How does Builder solve the telescoping constructor problem?"                       |
|                  | "What is a step builder and why is it useful?"                                      |
| Prototype        | "What is the difference between deep copy and shallow copy?"                        |
|                  | "When is Prototype better than Factory Method?"                                     |
| Singleton        | "How do you make Singleton thread-safe in C#?"                                      |
|                  | "What are the downsides of Singleton? How does DI address them?"                    |
|                  | "Can Singleton be broken by reflection or serialization?"                           |

### Structural Patterns

| Pattern    | Common Questions                                                                      |
|------------|---------------------------------------------------------------------------------------|
| Adapter    | "Object Adapter vs Class Adapter — when to use which?"                               |
|            | "How does Adapter differ from Facade?"                                                |
| Bridge     | "What is the difference between Bridge and Adapter?"                                  |
|            | "Give an example where Bridge prevents class explosion."                              |
| Composite  | "How would you implement a permission system using Composite?"                        |
| Decorator  | "How does Decorator differ from Proxy?"                                               |
|            | "How do you implement Decorator in .NET with DI?"                                     |
|            | "What is the relationship between Decorator and middleware?"                           |
| Facade     | "When does Facade become a God class?"                                                |
| Flyweight  | "What is intrinsic vs extrinsic state?"                                               |
| Proxy      | "Name three types of Proxy and give examples."                                        |
|            | "How does Proxy differ from Decorator?"                                               |

### Behavioral Patterns

| Pattern                  | Common Questions                                                         |
|--------------------------|--------------------------------------------------------------------------|
| Chain of Responsibility  | "How is ASP.NET middleware related to Chain of Responsibility?"          |
|                          | "What happens if no handler in the chain processes the request?"        |
| Command                  | "How does Command enable undo/redo?"                                    |
|                          | "What is the difference between Command and Strategy?"                  |
| Observer                 | "How does Observer differ from Mediator?"                               |
|                          | "How do .NET events relate to the Observer pattern?"                    |
| State                    | "How does State differ from Strategy?"                                  |
|                          | "How would you model an order lifecycle using State?"                   |
| Strategy                 | "How does Strategy support the Open/Closed Principle?"                  |
|                          | "How do you select a strategy at runtime in .NET?"                      |

### Enterprise Patterns

| Pattern         | Common Questions                                                              |
|-----------------|-------------------------------------------------------------------------------|
| Repository      | "Is Repository still useful with EF Core? When?"                             |
|                 | "How do Repository and Specification work together?"                          |
| CQRS            | "What are the benefits and drawbacks of CQRS?"                               |
|                 | "Does CQRS require Event Sourcing?"                                          |
|                 | "When is CQRS overkill?"                                                     |
| Domain Events   | "How do Domain Events differ from integration events?"                       |
|                 | "Should domain events be dispatched before or after SaveChanges?"            |
| Result Pattern  | "When should you use Result Pattern vs exceptions?"                          |
| Value Object    | "What makes a Value Object different from an Entity?"                        |
|                 | "How do you implement Value Object in C# with records?"                      |
| Saga            | "Orchestration vs Choreography — when to use which?"                         |

---

## Key Tradeoffs Per Pattern

| Pattern                  | Benefit                          | Cost                                   |
|--------------------------|----------------------------------|----------------------------------------|
| Factory Method           | Open/Closed for creation         | More classes per product type          |
| Abstract Factory         | Consistent families              | Complex interface hierarchy            |
| Builder                  | Readable construction            | More code for simple objects           |
| Prototype                | Avoids expensive creation        | Deep copy complexity                   |
| Singleton                | Global coordination              | Hidden coupling, test difficulty       |
| Adapter                  | Integration isolation            | Extra indirection layer                |
| Bridge                   | Prevents class explosion         | Increased abstraction complexity       |
| Composite                | Uniform tree operations          | Type safety for leaf-only operations   |
| Decorator                | Flexible behavior composition    | Many small wrapper classes             |
| Facade                   | Simplified API                   | Can become a God class                 |
| Flyweight                | Memory savings                   | Complexity of extrinsic state mgmt     |
| Proxy                    | Transparent access control       | Extra indirection                      |
| Chain of Responsibility  | Flexible handler pipeline        | No guarantee of handling               |
| Command                  | Undo/redo, queuing               | More classes per operation             |
| Interpreter              | Flexible DSL evaluation          | Slow for complex grammars              |
| Iterator                 | Lazy, decoupled traversal        | Implementation complexity for async    |
| Mediator                 | Reduced direct coupling          | Mediator can become God class          |
| Memento                  | State restoration                | Memory for storing snapshots           |
| Observer                 | Loose coupling, reactivity       | Memory leaks if not unsubscribed       |
| State                    | Clean state-dependent behavior   | More classes per state                 |
| Strategy                 | Runtime algorithm flexibility    | Client must know available strategies  |
| Template Method          | Code reuse via skeleton          | Inheritance-based (less flexible)      |
| Visitor                  | New operations without mod       | Difficult if element types change      |
| Repository               | Testable data access             | Can be a thin unnecessary wrapper      |
| CQRS                     | Optimized read/write models      | Eventual consistency, more code        |
| Domain Events            | Decoupled side effects           | Event ordering, debugging difficulty   |
| Outbox                   | Reliable event delivery          | Infrastructure complexity              |
| Saga                     | Distributed rollback             | Compensating action complexity         |
| Specification            | Composable queries               | Overkill for simple filters            |
| Result Pattern           | Explicit error handling          | Verbose for simple operations          |

---

## "Compare With..." Quick Reference

| Pattern A               | Compare With              | Key Difference                                            |
|-------------------------|---------------------------|-----------------------------------------------------------|
| Factory Method          | Abstract Factory          | One product vs family of products                         |
| Builder                 | Abstract Factory          | Step-by-step one product vs all-at-once family            |
| Strategy                | State                     | Client chooses algorithm vs object transitions internally |
| Strategy                | Template Method           | Composition (runtime) vs inheritance (compile-time)       |
| Decorator               | Proxy                     | Adds behavior vs controls access                          |
| Decorator               | Chain of Responsibility   | All decorators execute vs one handler processes            |
| Facade                  | Adapter                   | Simplifies own code vs translates external code           |
| Observer                | Mediator                  | One-to-many broadcast vs many-to-many coordination        |
| Observer                | Domain Events             | In-process notification vs architectural event system     |
| Command                 | Strategy                  | Encapsulates action for later vs selects algorithm now    |
| Command                 | Chain of Responsibility   | Execute one action vs route through pipeline              |
| Composite               | Decorator                 | Tree structure vs single wrapping                         |
| Repository              | Specification             | Data access vs query criteria                             |
| CQRS                    | CRUD                      | Separate read/write vs unified model                      |
| Domain Events           | Observer                  | System-level events vs object-level notifications         |
| Outbox                  | Direct Publish            | Transactional guarantee vs best-effort                    |
| Saga                    | Two-Phase Commit          | Compensating actions vs locking (distributed)             |

---

## Pattern Categories at a Glance

```
CREATIONAL        STRUCTURAL        BEHAVIORAL          ENTERPRISE
-----------       ----------        ----------          ----------
Factory Method    Adapter           Chain of Resp.      Repository
Abstract Factory  Bridge            Command             Unit of Work
Builder           Composite         Interpreter         Specification
Prototype         Decorator         Iterator            Result Pattern
Singleton         Facade            Mediator            CQRS
                  Flyweight         Memento             Domain Events
                  Proxy             Observer            Outbox
                                    State               Null Object
                                    Strategy            Value Object
                                    Template Method     Saga
                                    Visitor             Options Pattern
                                                        Policy Pattern
-----------       ----------        ----------          ----------
5 patterns        7 patterns        11 patterns         12 patterns
```

---

## Interview Tips

1. **Always start with the problem, not the pattern.** "I would use Strategy here because we need to swap pricing algorithms at runtime" is better than "I would use Strategy because it is a good pattern."

2. **Know when to argue against a pattern.** Saying "We do not need CQRS here because reads and writes share the same shape" shows deeper understanding than always advocating for patterns.

3. **Mention .NET-specific idioms.** "In .NET, I would register the strategies in DI and inject `IEnumerable<IPricingStrategy>`" shows practical knowledge.

4. **Connect patterns to SOLID.** "Decorator supports the Open/Closed Principle because we add behavior without modifying the existing class."

5. **Discuss tradeoffs.** Every pattern has a cost. Mentioning the downside alongside the benefit shows maturity.

6. **Use the business scenario.** "In our payment system, Factory Method lets us add Stripe or PayPal without changing the checkout flow" is more convincing than abstract descriptions.

---

## Next Steps

- [Comparison Maps](comparison-maps.md) — Deep-dive side-by-side comparisons
- [Decision Matrix](decision-matrix.md) — Map problems to patterns systematically
- [Anti-Patterns](anti-patterns.md) — Know what to avoid
