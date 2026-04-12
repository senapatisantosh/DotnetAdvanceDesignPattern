# Behavioral Patterns Summary

Behavioral patterns focus on **algorithms**, **responsibility assignment**, and **communication** between objects. They describe how objects interact and distribute work. This repository demonstrates all eleven GoF behavioral patterns with production-style .NET 10 examples.

---

## Summary Table

| Pattern                  | Use Case (This Repo) | Key Benefit                             | When to Use                                            | When to Avoid                                     |
|--------------------------|----------------------|-----------------------------------------|--------------------------------------------------------|---------------------------------------------------|
| Chain of Responsibility  | Expense Approval     | Decouples sender from receivers         | Request can be handled by one of many handlers         | Only one handler will ever exist                  |
| Command                  | Order Fulfillment    | Encapsulates actions as objects         | Need undo/redo, queuing, logging of operations         | Simple direct method call is sufficient           |
| Interpreter              | Search Query DSL     | Evaluates domain-specific languages     | Recurring, parseable domain expressions                | Grammar is complex (use a parser generator)       |
| Iterator                 | Paginated API        | Sequential access without exposure      | Need to traverse collections lazily or asynchronously  | Simple `foreach` over a list is enough            |
| Mediator                 | Checkout Workflow    | Reduces inter-object coupling           | Many objects communicate in complex ways               | Only two objects communicate                      |
| Memento                  | Document Editor      | Captures state without violating encap. | Need undo/redo or state snapshots                      | State is trivial or too large to snapshot          |
| Observer                 | Healthcare IoT       | Reactive notification                   | Multiple dependents must react to state changes        | Only one consumer; direct call is simpler         |
| State                    | Order Lifecycle      | State-specific behavior without ifs     | Object behavior changes significantly with state       | Only 2-3 states with minimal behavior differences |
| Strategy                 | Pricing Engine       | Runtime algorithm swapping              | Multiple algorithms for the same task                  | Only one algorithm exists or will exist           |
| Template Method          | Document Export      | Reuse algorithm skeleton                | Algorithm steps are fixed; only details vary           | No common algorithm structure across variants     |
| Visitor                  | Fraud Detection      | Add operations without modifying types  | Need many operations over a stable type hierarchy      | Type hierarchy changes frequently                 |

---

## Pattern Details

### Chain of Responsibility — Expense Approval

**Intent**: Avoid coupling the sender of a request to its receiver by giving more than one object a chance to handle the request. Chain the receiving objects and pass the request along the chain.

**This repo's scenario**: Expense approval pipeline where amounts route to different approval levels: Team Lead (up to $1,000), Manager (up to $5,000), VP (up to $25,000), CFO (unlimited).

**Key participants**:
- `IApprovalHandler` — Handler interface with `SetNext()` and `Handle()`
- `TeamLeadHandler`, `ManagerHandler`, `VpHandler`, `CfoHandler` — Concrete handlers
- `ApprovalPipeline` — Builds and executes the chain

**When to choose**:
- Multiple objects might handle a request, determined at runtime.
- You want to add or reorder handlers without changing client code.
- Requests should propagate until handled.

---

### Command — Order Fulfillment

**Intent**: Encapsulate a request as an object, thereby letting you parameterize clients with different requests, queue or log requests, and support undoable operations.

**This repo's scenario**: Order fulfillment commands — `PickItemsCommand`, `PackOrderCommand`, `ShipOrderCommand` — each encapsulating an operation that can be executed, undone, queued, and logged.

**Key participants**:
- `ICommand` — Command interface with `Execute()` and `Undo()`
- `PickItemsCommand`, `PackOrderCommand`, `ShipOrderCommand` — Concrete commands
- `FulfillmentInvoker` — Stores and executes commands

**When to choose**:
- You need undo/redo functionality.
- You want to queue, schedule, or log operations.
- You need to parameterize objects with operations.
- You want transactional behavior (execute all or rollback all).

---

### Interpreter — Search Query DSL

**Intent**: Given a language, define a representation for its grammar along with an interpreter that uses the representation to interpret sentences in the language.

**This repo's scenario**: A search query DSL that parses expressions like `category:electronics AND price:<500 OR brand:Sony` into an expression tree and evaluates it against product data.

**Key participants**:
- `IExpression` — Abstract expression with `Interpret(context)`
- `TerminalExpression` — Leaf expressions (field comparisons)
- `AndExpression`, `OrExpression`, `NotExpression` — Composite expressions

**When to choose**:
- You have a simple, recurring language or notation.
- The grammar is small and not performance-critical.
- Expressions can be represented as a tree.

**When to avoid**: Complex grammars — use ANTLR, Sprache, or Pidgin parser libraries instead.

---

### Iterator — Paginated API

**Intent**: Provide a way to access the elements of an aggregate object sequentially without exposing its underlying representation.

**This repo's scenario**: An `IAsyncEnumerable<T>` iterator over a paginated REST API that transparently fetches the next page when the current page is exhausted. Consumers use `await foreach` without knowing about pagination.

**Key participants**:
- `IAsyncEnumerable<T>` — .NET's built-in async iterator interface
- `PaginatedApiIterator<T>` — Custom async enumerator that handles page fetching

**When to choose**:
- You need lazy, on-demand traversal of a collection.
- The data source is paginated, streamed, or too large to load at once.
- You want to hide the traversal mechanism from consumers.

---

### Mediator — Checkout Workflow

**Intent**: Define an object that encapsulates how a set of objects interact. Mediator promotes loose coupling by keeping objects from referring to each other explicitly.

**This repo's scenario**: A checkout workflow where inventory, payment, shipping, and notification services communicate through a `CheckoutMediator` instead of calling each other directly.

**Key participants**:
- `ICheckoutMediator` — Mediator interface
- `CheckoutMediator` — Concrete mediator coordinating all services
- `InventoryColleague`, `PaymentColleague`, `ShippingColleague`, `NotificationColleague` — Colleagues

**When to choose**:
- Multiple objects have complex, intertwined communication.
- You want to centralize control logic.
- Adding new participants should not require changing existing ones.

---

### Memento — Document Editor

**Intent**: Without violating encapsulation, capture and externalize an object's internal state so that the object can be restored to this state later.

**This repo's scenario**: A document editor with undo/redo history. Each edit creates a memento snapshot. The caretaker (history manager) stores snapshots without knowing the document's internal structure.

**Key participants**:
- `Document` — Originator that creates and restores from mementos
- `DocumentMemento` — Snapshot of document state
- `DocumentHistory` — Caretaker that manages the memento stack

**When to choose**:
- You need undo/redo or state rollback.
- Direct access to the object's internal state would violate encapsulation.
- You want to keep snapshots separate from the object itself.

---

### Observer — Healthcare IoT

**Intent**: Define a one-to-many dependency between objects so that when one object changes state, all its dependents are notified and updated automatically.

**This repo's scenario**: Patient monitoring devices (heart rate, blood pressure, oxygen) publish readings. Alert systems, dashboards, and audit loggers subscribe to relevant monitors and react to threshold violations.

**Variants implemented**:
- **Classic Observer** — `IObserver<T>` and `IObservable<T>` interfaces.
- **Event-Based** — Using C# `event` and `EventHandler<T>` delegates.

**Key participants**:
- `IPatientMonitor` — Subject (observable)
- `AlertSystem`, `DashboardUpdater`, `AuditLogger` — Observers
- `PatientReading` — Event data

**When to choose**:
- Multiple objects must react to state changes in another object.
- The set of dependents is dynamic (can subscribe/unsubscribe at runtime).
- You want loose coupling between the publisher and subscribers.

---

### State — Order Lifecycle

**Intent**: Allow an object to alter its behavior when its internal state changes. The object will appear to change its class.

**This repo's scenario**: An order that transitions through states — Draft, Submitted, Approved, Shipped, Delivered, Cancelled — where each state determines what operations are valid and what happens when they are invoked.

**Key participants**:
- `IOrderState` — State interface with methods like `Submit()`, `Approve()`, `Ship()`
- `DraftState`, `SubmittedState`, `ApprovedState`, `ShippedState`, `DeliveredState`, `CancelledState` — Concrete states
- `Order` — Context that delegates behavior to its current state

**When to choose**:
- An object's behavior depends heavily on its state.
- State transitions follow specific rules.
- You have large `switch` or `if/else` blocks keyed on a status enum — this is the State pattern waiting to happen.

---

### Strategy — Pricing Engine

**Intent**: Define a family of algorithms, encapsulate each one, and make them interchangeable. Strategy lets the algorithm vary independently from clients that use it.

**This repo's scenario**: A pricing engine that applies different discount strategies — Regular, Premium, Seasonal, Bulk — based on customer type and order context.

**Key participants**:
- `IPricingStrategy` — Strategy interface with `CalculatePrice()`
- `RegularPricingStrategy`, `PremiumPricingStrategy`, `SeasonalPricingStrategy`, `BulkPricingStrategy` — Concrete strategies
- `PricingEngine` — Context that uses the current strategy

**When to choose**:
- You have multiple algorithms for the same task.
- The algorithm should be selectable at runtime.
- You want to eliminate conditional logic for algorithm selection.

---

### Template Method — Document Export

**Intent**: Define the skeleton of an algorithm in an operation, deferring some steps to subclasses. Template Method lets subclasses redefine certain steps without changing the algorithm's structure.

**This repo's scenario**: Document exporters (PDF, Excel, CSV) that share the same high-level flow — load data, validate, format, write output — but differ in the formatting and writing steps.

**Key participants**:
- `DocumentExporter` — Abstract class with `Export()` template method
- `PdfExporter`, `ExcelExporter`, `CsvExporter` — Concrete subclasses overriding specific steps

**When to choose**:
- Multiple classes share the same algorithm structure but differ in specific steps.
- You want to control the algorithm's skeleton while allowing customization.
- You want to avoid code duplication across similar algorithms.

---

### Visitor — Fraud Detection

**Intent**: Represent an operation to be performed on the elements of an object structure. Visitor lets you define a new operation without changing the classes of the elements on which it operates.

**This repo's scenario**: Fraud detection rules that visit different transaction types (wire transfer, card payment, ACH transfer). New rules (velocity check, amount threshold, geo anomaly) can be added without modifying transaction classes.

**Key participants**:
- `ITransactionVisitor` — Visitor interface with `Visit()` overloads
- `VelocityCheckVisitor`, `AmountThresholdVisitor`, `GeoAnomalyVisitor` — Concrete visitors
- `ITransaction` — Element interface with `Accept(visitor)`
- `WireTransfer`, `CardPayment`, `AchTransfer` — Concrete elements

**When to choose**:
- You need to add many unrelated operations to a stable type hierarchy.
- The element classes rarely change but operations change frequently.
- You want to keep related behavior together in one visitor class.

---

## Comparison Notes

### Strategy vs State

| Aspect              | Strategy                                    | State                                       |
|---------------------|---------------------------------------------|---------------------------------------------|
| **What varies**     | Algorithm/behavior chosen by the client     | Behavior driven by internal state           |
| **Who decides**     | Client selects the strategy                 | Object transitions itself                   |
| **Transition**      | No concept of transitions                   | States transition to other states           |
| **Awareness**       | Strategies are unaware of each other        | States often know about valid transitions   |
| **Use when**        | Algorithm varies by config or user choice   | Behavior depends on lifecycle/state machine |

**One sentence**: Strategy swaps algorithms **chosen from outside**; State changes behavior **driven from inside**.

### Command vs Chain of Responsibility

| Aspect              | Command                                     | Chain of Responsibility                     |
|---------------------|---------------------------------------------|---------------------------------------------|
| **Focus**           | Encapsulate an action as an object          | Route a request through handlers            |
| **Handling**        | Exactly one receiver executes               | Zero or more handlers may process           |
| **Undo**            | Built-in undo/redo support                  | Not typically undoable                      |
| **Coupling**        | Client knows the command; not the receiver  | Client only knows the first handler         |
| **Use when**        | Need undo, queueing, macro recording        | Request routing, validation pipelines       |

**One sentence**: Command **encapsulates actions** for execution control; CoR **routes requests** through a pipeline.

### Observer vs Mediator

| Aspect              | Observer                                    | Mediator                                    |
|---------------------|---------------------------------------------|---------------------------------------------|
| **Communication**   | One-to-many broadcast                       | Many-to-many through central hub            |
| **Coupling**        | Subject knows observer interface only       | Mediator knows all colleagues               |
| **Direction**       | Subject pushes to all observers             | Colleagues communicate via mediator         |
| **Use when**        | One source, many listeners                  | Many sources, many listeners, complex rules |

**One sentence**: Observer **broadcasts** from one to many; Mediator **coordinates** among many.

### Strategy vs Template Method

| Aspect              | Strategy                                    | Template Method                             |
|---------------------|---------------------------------------------|---------------------------------------------|
| **Mechanism**       | Composition (inject strategy object)        | Inheritance (override in subclass)          |
| **Flexibility**     | Swap at runtime                             | Fixed at compile time                       |
| **Granularity**     | Entire algorithm is replaceable             | Only specific steps are replaceable         |
| **Preference**      | Favored in modern .NET (DI-friendly)        | Useful when algorithm skeleton is stable    |

### Command vs Strategy

| Aspect              | Command                                     | Strategy                                    |
|---------------------|---------------------------------------------|---------------------------------------------|
| **Intent**          | Encapsulate an action for later execution   | Encapsulate an algorithm for selection      |
| **Lifecycle**       | Created, queued, executed, possibly undone   | Selected once, used during computation      |
| **State**           | Commands carry their own parameters         | Strategies are typically stateless          |
| **Use when**        | Actions need history, undo, scheduling      | Algorithms need runtime selection           |

---

## .NET-Specific Idioms

| Pattern                  | .NET Idiom                                                     |
|--------------------------|----------------------------------------------------------------|
| Chain of Responsibility  | ASP.NET Core middleware pipeline                               |
| Command                  | MediatR `IRequest<T>`, `IRequestHandler<T>`                   |
| Interpreter              | Expression trees (`System.Linq.Expressions`)                   |
| Iterator                 | `IEnumerable<T>`, `IAsyncEnumerable<T>`, `yield return`       |
| Mediator                 | MediatR library, `IMediator.Send()`, `IMediator.Publish()`    |
| Memento                  | `record` types for immutable snapshots                         |
| Observer                 | `event`, `EventHandler<T>`, `IObservable<T>`, Reactive Extensions |
| State                    | State machines with Stateless library or custom implementation |
| Strategy                 | DI-injected `IEnumerable<IStrategy>` with resolution logic    |
| Template Method          | Abstract base class with `virtual` / `abstract` methods       |
| Visitor                  | Pattern matching with `switch` expressions as an alternative   |

---

## Next Steps

- [Enterprise Patterns Summary](enterprise-patterns-summary.md)
- [Comparison Maps](comparison-maps.md) — Strategy vs State and Observer vs Mediator deep dives
- [Interview Cheat Sheet](interview-revision-cheatsheet.md) — Quick revision for all behavioral patterns
