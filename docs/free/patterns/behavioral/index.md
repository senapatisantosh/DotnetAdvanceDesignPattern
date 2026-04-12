# Behavioral Patterns

Behavioral patterns are concerned with algorithms and the assignment of responsibilities between objects. They describe not just objects and classes, but the patterns of communication between them, shifting focus from structure to interaction.

---

## Chain of Responsibility

The Chain of Responsibility pattern passes a request along a chain of handlers, where each handler either processes the request or forwards it to the next handler in the chain. This creates a pluggable processing pipeline -- validation middleware, approval workflows, or ASP.NET Core middleware are all implementations of this pattern. Handlers can be added, removed, or reordered without changing the caller. The chain can also short-circuit, returning a response before reaching the end.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/ChainOfResponsibility/README.md)

---

## Command

The Command pattern encapsulates a request as an object, allowing you to parameterize clients with operations, queue or log requests, and support undoable operations. By turning a method call into an object with `Execute()` and `Undo()` methods, the pattern enables features like undo/redo history, command queuing, macro recording, and audit logging. Each command is a self-contained action with the state needed to execute and reverse itself.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Command/README.md)

---

## Interpreter

The Interpreter pattern defines a representation for a grammar and an interpreter that uses the representation to interpret sentences in the language. Each grammar rule becomes a class, and each sentence is parsed into an expression tree. This pattern is suitable for simple, stable domain-specific languages (DSLs) -- for example, evaluating mathematical expressions, boolean rules, or query filters. For complex grammars, a dedicated parser or compiler is preferred.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Interpreter/README.md)

---

## Iterator

The Iterator pattern provides a way to access elements of a collection sequentially without exposing its underlying representation. In C#, this is deeply embedded in the language through `IEnumerable<T>`, `IAsyncEnumerable<T>`, and the `foreach` keyword. The pattern enables lazy, on-demand evaluation of elements and allows different traversal strategies without modifying the collection itself.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Iterator/README.md)

---

## Mediator

The Mediator pattern defines an object that encapsulates how a set of objects interact, promoting loose coupling by preventing objects from referring to each other directly. Instead of N components with N*(N-1) direct connections, all communication goes through a central mediator -- like air traffic control coordinating planes. This is especially useful for complex UI forms, chat rooms, or orchestration scenarios where many components interact in interdependent ways.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Mediator/README.md)

---

## Memento

The Memento pattern captures and externalizes an object's internal state so it can be restored later, without violating encapsulation. The originator creates a memento (snapshot) of its current state, a caretaker stores the memento, and the originator can later restore from it. This is the foundation for undo/redo systems, checkpointing, game saves, and any scenario where you need rollback capability.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Memento/README.md)

---

## Observer

The Observer pattern defines a one-to-many dependency between objects so that when one object changes state, all its dependents are notified and updated automatically. This is the newspaper subscription model -- a publisher sends updates to all subscribers. In .NET, this maps to `event EventHandler<T>` for synchronous in-process notifications. The pattern provides loose coupling between the event source and consumers, and the subscriber list can change at runtime.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Observer/README.md)

---

## State

The State pattern allows an object to alter its behavior when its internal state changes, appearing to change its class. Instead of large if/else or switch blocks that check state, each state is a class implementing a common interface. The context delegates behavior to its current state object, and states can trigger transitions to other states. This is ideal for modeling lifecycles -- order processing (Draft, Submitted, Shipped, Delivered), document workflows, or game entities.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/State/README.md)

---

## Strategy

The Strategy pattern defines a family of algorithms, encapsulates each one, and makes them interchangeable. The client selects which algorithm to use at runtime. Unlike State (where the object changes its own behavior), Strategy puts the choice in the caller's hands. Common examples include pricing algorithms (Regular, Premium, Bulk), sorting strategies, payment methods, and compression algorithms. New strategies are added without modifying existing code.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Strategy/README.md)

---

## Template Method

The Template Method pattern defines the skeleton of an algorithm in a base class, deferring specific steps to subclasses. The base class controls the overall structure and the invariant parts, while subclasses fill in the variable steps. This enforces a consistent algorithm structure while allowing customization. For example, a data import pipeline might always follow the steps: open source, read records, transform, validate, save -- but each step's implementation varies by data source.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/TemplateMethod/README.md)

---

## Visitor

The Visitor pattern lets you add operations to a stable set of element types without modifying those types. Using double dispatch, a visitor object is passed to each element, and the element calls back the appropriate method on the visitor. This is powerful when you frequently add new operations (export, validate, render) but rarely add new element types. The trade-off is that adding a new element type requires updating every visitor.

[View source and full documentation](../../../../src/DesignPatterns.Behavioral/Visitor/README.md)
