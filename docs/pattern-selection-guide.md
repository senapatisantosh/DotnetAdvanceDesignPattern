# Pattern Selection Guide

This guide helps you choose the right design pattern based on the problem you are facing. Use the decision trees, flowcharts, and tables below to navigate from **problem** to **pattern**.

---

## Master Decision Flowchart

```mermaid
flowchart TD
    Start([What problem are you solving?]) --> Q1{Is it about<br/>creating objects?}
    Q1 -->|Yes| Creational
    Q1 -->|No| Q2{Is it about<br/>composing structures?}
    Q2 -->|Yes| Structural
    Q2 -->|No| Q3{Is it about<br/>object interaction<br/>or algorithms?}
    Q3 -->|Yes| Behavioral
    Q3 -->|No| Q4{Is it about<br/>persistence, events,<br/>or system architecture?}
    Q4 -->|Yes| Enterprise
    Q4 -->|No| YAGNI[You might not<br/>need a pattern]

    subgraph Creational
        C1{Do you need<br/>families of<br/>related objects?}
        C1 -->|Yes| AF[Abstract Factory]
        C1 -->|No| C2{Is construction<br/>complex with many<br/>optional parts?}
        C2 -->|Yes| BU[Builder]
        C2 -->|No| C3{Should creation<br/>be deferred to<br/>subclasses?}
        C3 -->|Yes| FM[Factory Method]
        C3 -->|No| C4{Is creating the<br/>object expensive?}
        C4 -->|Yes| PR[Prototype]
        C4 -->|No| C5{Must there be<br/>exactly one instance?}
        C5 -->|Yes| SI[Singleton]
        C5 -->|No| FM
    end

    subgraph Structural
        S1{Need to make<br/>incompatible interfaces<br/>work together?}
        S1 -->|Yes| AD[Adapter]
        S1 -->|No| S2{Need to add<br/>behavior without<br/>subclassing?}
        S2 -->|Yes| DE[Decorator]
        S2 -->|No| S3{Need to simplify<br/>a complex<br/>subsystem?}
        S3 -->|Yes| FA[Facade]
        S3 -->|No| S4{Need to control<br/>access to<br/>an object?}
        S4 -->|Yes| PX[Proxy]
        S4 -->|No| S5{Need to vary<br/>abstraction and<br/>implementation?}
        S5 -->|Yes| BR[Bridge]
        S5 -->|No| S6{Need tree<br/>structures with<br/>uniform interface?}
        S6 -->|Yes| CO[Composite]
        S6 -->|No| FW[Flyweight]
    end

    subgraph Behavioral
        B1{Need to swap<br/>algorithms at<br/>runtime?}
        B1 -->|Yes| STR[Strategy]
        B1 -->|No| B2{Does behavior<br/>change with<br/>internal state?}
        B2 -->|Yes| ST[State]
        B2 -->|No| B3{Need to<br/>undo/redo?}
        B3 -->|Yes| B3a{Encapsulate<br/>actions as objects?}
        B3a -->|Yes| CMD[Command + Memento]
        B3 -->|No| B4{Need to notify<br/>multiple objects<br/>of changes?}
        B4 -->|Yes| OBS[Observer]
        B4 -->|No| B5{Need to pass<br/>requests through<br/>a chain?}
        B5 -->|Yes| CoR[Chain of Responsibility]
        B5 -->|No| B6{Need to reduce<br/>coupling between<br/>many objects?}
        B6 -->|Yes| MED[Mediator]
        B6 -->|No| B7{Need to define<br/>algorithm skeleton<br/>with variable steps?}
        B7 -->|Yes| TM[Template Method]
        B7 -->|No| B8{Need to add<br/>operations to<br/>object structures?}
        B8 -->|Yes| VIS[Visitor]
        B8 -->|No| B9{Need to parse<br/>a domain-specific<br/>language?}
        B9 -->|Yes| INT[Interpreter]
        B9 -->|No| IT[Iterator]
    end

    subgraph Enterprise
        E1{Need to abstract<br/>data access?}
        E1 -->|Yes| REPO[Repository + UoW]
        E1 -->|No| E2{Need to separate<br/>reads from writes?}
        E2 -->|Yes| CQRS[CQRS]
        E2 -->|No| E3{Need reliable<br/>event delivery?}
        E3 -->|Yes| OUTBOX[Outbox + Domain Events]
        E3 -->|No| E4{Need distributed<br/>transaction?}
        E4 -->|Yes| SAGA[Saga]
        E4 -->|No| E5{Need composable<br/>query criteria?}
        E5 -->|Yes| SPEC[Specification]
        E5 -->|No| E6{Need error handling<br/>without exceptions?}
        E6 -->|Yes| RES[Result Pattern]
        E6 -->|No| E7{Need typed<br/>configuration?}
        E7 -->|Yes| OPT[Options Pattern]
        E7 -->|No| POL[Policy Pattern]
    end
```

---

## Problem-to-Pattern Quick Lookup

### "I need to..."

| Problem Statement                                                          | Recommended Pattern(s)                |
|----------------------------------------------------------------------------|---------------------------------------|
| Create objects without specifying the exact class                          | Factory Method, Abstract Factory      |
| Build an object with many optional parts                                   | Builder                               |
| Copy an object that is expensive to create                                 | Prototype                             |
| Ensure only one instance exists globally                                   | Singleton (prefer DI registration)    |
| Integrate a third-party library with an incompatible API                  | Adapter                               |
| Vary an abstraction and its implementation independently                  | Bridge                                |
| Represent a tree structure (files, menus, permissions)                    | Composite                             |
| Add logging, caching, retry, or metrics to an existing service            | Decorator                             |
| Simplify a complex subsystem into one entry point                         | Facade                                |
| Reduce memory for thousands of similar objects                            | Flyweight                             |
| Add access control, lazy loading, or caching transparently                | Proxy                                 |
| Route a request through multiple handlers                                 | Chain of Responsibility               |
| Queue, log, or undo operations                                           | Command                               |
| Parse and evaluate a domain-specific language                             | Interpreter                           |
| Iterate over a collection without exposing its structure                  | Iterator                              |
| Coordinate multiple services without direct coupling                      | Mediator                              |
| Save and restore an object's state (undo/redo)                           | Memento                               |
| Notify multiple components when something changes                        | Observer                              |
| Change an object's behavior based on its state                           | State                                 |
| Choose an algorithm at runtime                                            | Strategy                              |
| Define a fixed algorithm structure with customizable steps                | Template Method                       |
| Add operations to a class hierarchy without modifying it                  | Visitor                               |
| Abstract data access and make it testable                                 | Repository + Unit of Work             |
| Compose reusable query/filter criteria                                    | Specification                         |
| Handle errors without throwing exceptions                                 | Result Pattern                        |
| Separate read and write concerns for different scaling                    | CQRS                                  |
| Raise events from domain logic for side effects                          | Domain Events                         |
| Guarantee event delivery alongside database transactions                 | Outbox                                |
| Avoid null checks everywhere                                             | Null Object                           |
| Model domain concepts like Money, Address, Email                         | Value Object                          |
| Manage a multi-step distributed process with rollback                    | Saga                                  |
| Bind configuration files to strongly-typed objects                       | Options Pattern                       |
| Compose business rules that can be evaluated independently               | Policy Pattern                        |

---

## Decision by Concern

### Object Creation Decisions

| Situation                                        | Pattern          | Why                                          |
|--------------------------------------------------|------------------|----------------------------------------------|
| Type is determined by runtime configuration      | Factory Method   | Defers instantiation to factory              |
| Multiple related types must be created together  | Abstract Factory | Guarantees compatible families               |
| Object has 4+ constructor parameters             | Builder          | Step-by-step construction with validation    |
| Object creation involves deep copy               | Prototype        | Clone instead of rebuild                     |
| Global state coordination is needed              | Singleton        | One instance, but prefer DI singleton scope  |

### Wrapping / Composition Decisions

| Situation                                        | Pattern    | Why                                           |
|--------------------------------------------------|------------|-----------------------------------------------|
| Third-party interface does not match yours       | Adapter    | Translates interface                          |
| Need to add behavior without changing the class  | Decorator  | Wraps and enhances                            |
| Need to control access (auth, cache, lazy)       | Proxy      | Wraps and controls                            |
| Need a simple API for a complex subsystem        | Facade     | Hides complexity                              |

### Algorithm / Behavior Decisions

| Situation                                        | Pattern          | Why                                          |
|--------------------------------------------------|------------------|----------------------------------------------|
| Algorithm varies by type/configuration           | Strategy         | Runtime algorithm swap                       |
| Behavior depends on object's lifecycle state     | State            | State-driven behavior                        |
| Steps are fixed but details vary                 | Template Method  | Inheritance-based customization              |
| Need undo/redo for operations                    | Command + Memento| Command records; Memento snapshots           |
| Multiple objects must react to changes           | Observer         | Pub/sub notification                         |

### Architecture Decisions

| Situation                                        | Pattern(s)              | Why                                         |
|--------------------------------------------------|-------------------------|---------------------------------------------|
| Read-heavy system with different read/write needs| CQRS                    | Optimize each side independently            |
| Side effects (email, audit) from domain actions  | Domain Events           | Decouple side effects from core logic       |
| Need at-least-once event delivery guarantee      | Domain Events + Outbox  | Transactional outbox for reliability        |
| Multi-service workflow with failure compensation | Saga                    | Coordinated rollback                        |
| Complex query filters combined dynamically       | Specification           | Composable predicates                       |
| Configuration with validation and hot reload     | Options Pattern         | `IOptions<T>` / `IOptionsMonitor<T>`        |

---

## When NOT to Use a Pattern

| Temptation                                  | Better Alternative                                    |
|---------------------------------------------|-------------------------------------------------------|
| Factory for 1-2 simple types               | Direct instantiation or a simple `switch`             |
| Singleton for everything                    | DI container with appropriate lifetime                |
| Repository wrapping EF Core with no benefit | Use `DbContext` directly if no abstraction needed     |
| Decorator for one cross-cutting concern     | Middleware or a simple wrapper method                 |
| Mediator to call one service               | Direct dependency injection                           |
| CQRS for a simple CRUD app                 | Standard service + repository                        |
| Specification for trivial `Where` clauses  | LINQ directly                                        |
| Strategy when there is only one algorithm  | Just write the code inline                            |

> **Rule of thumb**: If introducing a pattern adds more code than the problem it solves, you do not need it yet. Patterns earn their keep when complexity grows.

---

## Next Steps

- [Decision Matrix](decision-matrix.md) — Detailed problem-to-pattern mapping table.
- [Comparison Maps](comparison-maps.md) — Side-by-side comparisons of similar patterns.
- [Anti-Patterns](anti-patterns.md) — Learn what happens when patterns are misapplied.
