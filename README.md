# .NET 10 Advanced Design Patterns — A Production-Style Learning Repository

A comprehensive, production-grade collection of **35+ design patterns** implemented in **.NET 10 / C# 13**, organized for learning, interview preparation, and real-world architecture reference.

Every pattern is demonstrated with a **realistic business scenario** — no abstract `Shape` or `Animal` examples. Each implementation includes unit tests, XML documentation, and notes on when (and when not) to use the pattern.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Repository Structure](#repository-structure)
- [Getting Started](#getting-started)
- [Study Guide](#study-guide)
- [Pattern Categories](#pattern-categories)
- [Quick Reference](#quick-reference)
- [GoF vs Enterprise Patterns](#gof-vs-enterprise-patterns)
- [Navigating Related Patterns](#navigating-related-patterns)
- [Documentation](#documentation)

---

## Project Overview

This repository teaches design patterns through **production-style examples** that mirror challenges you encounter in real .NET applications:

- **Payment processing**, **cloud storage**, **invoice generation**, **order fulfillment**, **healthcare IoT**, and more.
- Each pattern lives in its own folder with clearly separated abstractions, concrete implementations, and a playground demo.
- A shared project (`DesignPatterns.Shared`) provides common models and interfaces reused across patterns.
- A playground console app (`DesignPatterns.Playground`) lets you run every pattern interactively.
- Full test coverage in dedicated test projects per category.

### What You Will Learn

1. The **intent** behind each pattern — the problem it solves.
2. How to implement it idiomatically in modern C# (records, primary constructors, pattern matching, `IAsyncEnumerable`).
3. When to **use** it and, equally important, when to **avoid** it.
4. How patterns **compose** — e.g., Builder + Fluent Interface, Strategy + DI, CQRS + Domain Events + Outbox.

---

## Repository Structure

```
DotnetAdvanceDesignPattern/
├── DotnetAdvanceDesignPattern.sln
├── Directory.Build.props
├── README.md
├── docs/
│   ├── architecture-overview.md
│   ├── design-pattern-categories.md
│   ├── pattern-selection-guide.md
│   ├── creational-patterns-summary.md
│   ├── structural-patterns-summary.md
│   ├── behavioral-patterns-summary.md
│   ├── enterprise-patterns-summary.md
│   ├── anti-patterns.md
│   ├── interview-revision-cheatsheet.md
│   ├── decision-matrix.md
│   └── comparison-maps.md
├── src/
│   ├── DesignPatterns.Creational/
│   │   ├── FactoryMethod/        # Payment Gateways
│   │   ├── AbstractFactory/      # Cloud Storage (AWS / Azure)
│   │   ├── Builder/              # Invoice Generation (Step Builder)
│   │   ├── Prototype/            # Feature Flags
│   │   └── Singleton/            # Telemetry Registry (Thread-Safe, Lazy, DI)
│   ├── DesignPatterns.Structural/
│   │   ├── Adapter/              # Shipping Carriers (Class & Object Adapter)
│   │   ├── Bridge/               # Notifications (Channels x Message Types)
│   │   ├── Composite/            # Permission Hierarchy
│   │   ├── Decorator/            # API Client Resilience (Retry, Logging, Caching)
│   │   ├── Facade/               # Report Generation (Subsystems)
│   │   ├── Flyweight/            # Tax Rates
│   │   └── Proxy/                # Inventory Service (Caching, Virtual, Protection)
│   ├── DesignPatterns.Behavioral/
│   │   ├── ChainOfResponsibility/ # Expense Approval Pipeline
│   │   ├── Command/              # Order Fulfillment
│   │   ├── Interpreter/          # Search Query DSL
│   │   ├── Iterator/             # Paginated API
│   │   ├── Mediator/             # Checkout Workflow
│   │   ├── Memento/              # Document Editor (Undo/Redo)
│   │   ├── Observer/             # Healthcare IoT (Event-Based)
│   │   ├── State/                # Order Lifecycle
│   │   ├── Strategy/             # Pricing Engine
│   │   ├── TemplateMethod/       # Document Export
│   │   └── Visitor/              # Fraud Detection
│   ├── DesignPatterns.Enterprise/
│   │   ├── Repository/           # Generic Repository
│   │   ├── UnitOfWork/           # Transactional Consistency
│   │   ├── Specification/        # Composable Query Specs
│   │   ├── ResultPattern/        # Railway-Oriented Results
│   │   ├── Cqrs/                 # Command/Query Separation
│   │   ├── DomainEvents/         # Event Dispatch
│   │   ├── Outbox/               # Reliable Messaging
│   │   ├── NullObject/           # Safe Defaults
│   │   ├── ValueObject/          # Immutable Domain Primitives
│   │   ├── Saga/                 # Distributed Transactions
│   │   ├── OptionsPattern/       # Typed Configuration
│   │   └── PolicyPattern/        # Composable Business Rules
│   ├── DesignPatterns.Shared/    # Common models and interfaces
│   └── DesignPatterns.Playground/ # Interactive console runner
└── tests/
    ├── DesignPatterns.Creational.Tests/
    ├── DesignPatterns.Structural.Tests/
    ├── DesignPatterns.Behavioral.Tests/
    └── DesignPatterns.Enterprise.Tests/
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Any IDE: Visual Studio 2025, VS Code with C# Dev Kit, JetBrains Rider

### Run Examples

```bash
# Run the interactive playground
dotnet run --project src/DesignPatterns.Playground

# Run all tests
dotnet test

# Run tests for a specific category
dotnet test tests/DesignPatterns.Creational.Tests
dotnet test tests/DesignPatterns.Behavioral.Tests

# Build the entire solution
dotnet build
```

### Explore a Single Pattern

Each pattern folder is self-contained. Browse to any pattern directory and read the code top-down:

1. **Interface / Abstraction** — the contract the pattern defines.
2. **Concrete implementations** — the real-world variants.
3. **Tests** — executable specification of the pattern's behavior.
4. **Playground demo** — see it run end to end.

---

## Study Guide

### For Beginners — "I'm learning design patterns for the first time"

Start with these five patterns in order. They are the most frequently encountered and easiest to grasp:

| Order | Pattern          | Why Start Here                                      |
|-------|------------------|-----------------------------------------------------|
| 1     | Strategy         | Simplest behavioral pattern; just swap algorithms   |
| 2     | Factory Method   | Most common creational pattern in .NET              |
| 3     | Observer         | Foundation of event-driven programming              |
| 4     | Decorator        | Understand composition over inheritance             |
| 5     | Builder          | Essential for complex object construction           |

Then explore: Adapter, Facade, State, Command, Template Method.

### For Interview Prep — "I have a system design interview"

Focus on patterns interviewers ask about most:

| Priority | Pattern(s)                              | Interview Topic                           |
|----------|-----------------------------------------|-------------------------------------------|
| High     | Strategy, Factory Method, Observer      | "Which pattern would you use for...?"     |
| High     | Singleton                               | Thread safety, DI alternative             |
| High     | CQRS, Repository, Unit of Work         | Data access architecture                  |
| Medium   | Decorator, Proxy, Adapter              | Structural composition                    |
| Medium   | State, Command, Chain of Responsibility | Behavioral workflows                      |
| Medium   | Result Pattern, Specification           | Clean code / domain modeling              |
| Lower    | Interpreter, Flyweight, Visitor         | Niche but shows depth                     |

Read the [Interview Revision Cheat Sheet](docs/interview-revision-cheatsheet.md) for one-liners and common questions.

### For Architects — "I'm designing a real system"

Focus on composition and enterprise patterns:

1. **Domain Modeling**: Value Object, Specification, Domain Events, Result Pattern
2. **Data Pipeline**: Repository + Unit of Work + Specification
3. **Event-Driven**: Observer + Domain Events + Outbox + Saga
4. **Resilience**: Decorator (retry/circuit-breaker) + Proxy (caching) + Policy Pattern
5. **CQRS + Event Sourcing**: CQRS + Domain Events + Outbox

Read the [Architecture Overview](docs/architecture-overview.md) and [Pattern Selection Guide](docs/pattern-selection-guide.md).

---

## Pattern Categories

### Creational Patterns — *How objects are created*

| Pattern           | Business Scenario        | Key Idea                                       |
|-------------------|--------------------------|-------------------------------------------------|
| Factory Method    | Payment Gateways         | Defer instantiation to subclasses               |
| Abstract Factory  | Cloud Storage            | Create families of related objects              |
| Builder           | Invoice Generation       | Construct complex objects step by step          |
| Prototype         | Feature Flags            | Clone existing objects to avoid costly creation |
| Singleton         | Telemetry Registry       | Ensure one instance with global access          |

### Structural Patterns — *How objects are composed*

| Pattern    | Business Scenario        | Key Idea                                          |
|------------|--------------------------|---------------------------------------------------|
| Adapter    | Shipping Carriers        | Make incompatible interfaces work together        |
| Bridge     | Notifications            | Separate abstraction from implementation          |
| Composite  | Permission Hierarchy     | Treat individual and composite objects uniformly  |
| Decorator  | API Client Resilience    | Add behavior dynamically without subclassing      |
| Facade     | Report Generation        | Provide a simplified interface to a subsystem     |
| Flyweight  | Tax Rates                | Share common state to reduce memory usage         |
| Proxy      | Inventory Service        | Control access to another object                  |

### Behavioral Patterns — *How objects communicate*

| Pattern                  | Business Scenario    | Key Idea                                          |
|--------------------------|----------------------|---------------------------------------------------|
| Chain of Responsibility  | Expense Approval     | Pass request along a chain of handlers            |
| Command                  | Order Fulfillment    | Encapsulate a request as an object                |
| Interpreter              | Search Query DSL     | Define a grammar and interpret sentences          |
| Iterator                 | Paginated API        | Access elements sequentially without exposure     |
| Mediator                 | Checkout Workflow    | Reduce coupling via a central coordinator         |
| Memento                  | Document Editor      | Capture and restore object state                  |
| Observer                 | Healthcare IoT       | Notify dependents of state changes                |
| State                    | Order Lifecycle      | Alter behavior when internal state changes        |
| Strategy                 | Pricing Engine       | Swap algorithms at runtime                        |
| Template Method          | Document Export      | Define algorithm skeleton; let subclasses fill in |
| Visitor                  | Fraud Detection      | Add operations without modifying element classes  |

### Enterprise Patterns — *How production systems are structured*

| Pattern         | Key Idea                                                 |
|-----------------|----------------------------------------------------------|
| Repository      | Abstract data access behind a collection-like interface  |
| Unit of Work    | Track changes and commit as a single transaction         |
| Specification   | Encapsulate query logic in composable, reusable objects  |
| Result Pattern  | Represent success/failure without exceptions             |
| CQRS            | Separate read models from write models                   |
| Domain Events   | Decouple side effects from domain logic                  |
| Outbox          | Guarantee event delivery with transactional outbox       |
| Null Object     | Provide safe no-op implementations                       |
| Value Object    | Model immutable domain concepts with value equality      |
| Saga            | Coordinate distributed transactions with compensation   |
| Options Pattern | Bind and validate configuration in a typed manner        |
| Policy Pattern  | Compose business rules as first-class objects            |

---

## Quick Reference

### Creational Patterns

- **Factory Method** — Use when the exact type to create isn't known until runtime. *Example*: Selecting a payment gateway (Stripe, PayPal, Square) based on merchant configuration.
- **Abstract Factory** — Use when you need families of related objects. *Example*: Cloud storage abstraction where each provider (AWS, Azure) supplies blob, queue, and table clients.
- **Builder** — Use when constructing an object requires many optional parameters. *Example*: Building invoices with optional line items, discounts, tax, and notes.
- **Prototype** — Use when creating an object is expensive and a similar one already exists. *Example*: Cloning feature flag configurations for A/B testing variants.
- **Singleton** — Use when exactly one instance must coordinate actions. *Example*: A telemetry metric registry shared across the application. Prefer DI-based singleton registration.

### Structural Patterns

- **Adapter** — Wrap a third-party or legacy interface so it matches your expected contract. *Example*: Unifying FedEx, UPS, and DHL shipping APIs behind `IShippingCarrier`.
- **Bridge** — Decouple an abstraction from its implementation so both can vary. *Example*: Notification system where message types (Alert, Report) and channels (Email, SMS, Push) vary independently.
- **Composite** — Build tree structures where leaves and composites share the same interface. *Example*: Permission hierarchy where roles contain other roles and individual permissions.
- **Decorator** — Wrap an object to add behavior. *Example*: Adding retry, logging, and caching layers around an HTTP API client.
- **Facade** — Simplify a complex subsystem with a single entry point. *Example*: `ReportGenerationFacade` that orchestrates data fetching, formatting, and PDF rendering.
- **Flyweight** — Share immutable state across many objects. *Example*: Tax rate lookup where rates are shared across millions of transaction calculations.
- **Proxy** — Control access to an object. *Example*: Caching proxy for inventory lookups, protection proxy for authorization, virtual proxy for lazy loading.

### Behavioral Patterns

- **Chain of Responsibility** — Build a pipeline of handlers that each decide whether to process or pass along. *Example*: Expense approval where amount thresholds route to Team Lead, Manager, VP, or CFO.
- **Command** — Encapsulate an action as an object with execute/undo. *Example*: Order fulfillment commands (PickItems, PackOrder, ShipOrder) with undo support.
- **Interpreter** — Parse and evaluate domain-specific expressions. *Example*: Search query DSL like `category:electronics AND price:<500`.
- **Iterator** — Provide sequential access without exposing the underlying structure. *Example*: `IAsyncEnumerable` over paginated REST API responses.
- **Mediator** — Centralize complex communications between objects. *Example*: Checkout workflow coordinating inventory, payment, shipping, and notification services.
- **Memento** — Save and restore state without violating encapsulation. *Example*: Document editor with undo/redo history.
- **Observer** — Publish/subscribe to state changes. *Example*: Healthcare IoT where patient monitors notify alert systems, dashboards, and logging.
- **State** — Change behavior based on internal state transitions. *Example*: Order lifecycle (Draft, Submitted, Approved, Shipped, Delivered, Cancelled).
- **Strategy** — Swap algorithms at runtime. *Example*: Pricing engine that applies Regular, Premium, Seasonal, or Bulk discount strategies.
- **Template Method** — Define algorithm structure; let subclasses implement steps. *Example*: Document export where PDF, Excel, and CSV exporters share the same flow.
- **Visitor** — Add operations to object structures without modifying them. *Example*: Fraud detection rules that visit different transaction types.

### Enterprise Patterns

- **Repository** — Abstract persistence behind `IRepository<T>` with Add, Get, Update, Delete.
- **Unit of Work** — Group multiple repository operations into a single transaction.
- **Specification** — Compose reusable query predicates: `new ActiveCustomerSpec().And(new PremiumTierSpec())`.
- **Result Pattern** — Return `Result<T>` instead of throwing exceptions for expected failures.
- **CQRS** — Separate `ICommandHandler<TCommand>` from `IQueryHandler<TQuery, TResult>`.
- **Domain Events** — Raise events like `OrderPlacedEvent` from aggregates; handle side effects in handlers.
- **Outbox** — Store events in a transactional outbox table, then relay to the message broker.
- **Null Object** — Return a no-op implementation (e.g., `NullLogger`) instead of `null`.
- **Value Object** — Immutable objects compared by value (e.g., `Money`, `Address`, `EmailAddress`).
- **Saga** — Orchestrate multi-step distributed processes with compensating actions on failure.
- **Options Pattern** — Bind `appsettings.json` sections to strongly-typed `IOptions<T>` classes.
- **Policy Pattern** — Encapsulate business rules as composable policy objects (e.g., discount eligibility).

---

## GoF vs Enterprise Patterns

| Aspect            | GoF Patterns (Gang of Four)                  | Enterprise Patterns                              |
|-------------------|----------------------------------------------|--------------------------------------------------|
| **Origin**        | *Design Patterns* book (1994)                | Martin Fowler's *PoEAA*, DDD, microservices era  |
| **Scope**         | Class/object-level design                    | Application/system-level architecture            |
| **Focus**         | Flexibility, reuse, decoupling              | Consistency, scalability, data integrity         |
| **Examples**      | Factory, Strategy, Observer, Decorator       | Repository, CQRS, Saga, Outbox                   |
| **Granularity**   | Single class or small group of classes       | Entire layers or bounded contexts                |
| **Testing**       | Unit testing individual classes              | Integration testing across layers                |
| **When to learn** | First — build foundation                     | After — apply to real systems                    |

**Key insight**: GoF patterns are *building blocks*. Enterprise patterns are *architectural blueprints* that often compose multiple GoF patterns. For example, CQRS uses Command and Strategy internally; the Outbox pattern uses Observer and Repository.

---

## Navigating Related Patterns

Patterns rarely exist in isolation. Here is how they connect:

| Starting Pattern     | Related Patterns                                   | Relationship                                   |
|----------------------|----------------------------------------------------|-------------------------------------------------|
| Factory Method       | Abstract Factory, Prototype, Builder               | All creational; differ in scope and flexibility |
| Strategy             | State, Template Method, Command                    | All vary behavior; differ in *what* varies      |
| Observer             | Mediator, Domain Events, Outbox                    | All decouple publishers from subscribers        |
| Decorator            | Proxy, Adapter, Chain of Responsibility            | All wrap objects; differ in intent              |
| Repository           | Unit of Work, Specification                        | Compose into a full data access layer           |
| CQRS                 | Domain Events, Outbox, Saga                        | Compose into event-driven architecture          |
| Command              | Memento, Chain of Responsibility                   | Command stores; CoR routes; Memento undoes      |
| Composite            | Iterator, Visitor                                  | Iterate and operate on tree structures          |
| Builder              | Fluent Interface, Prototype                        | Builder builds; Prototype clones                |
| Result Pattern       | Specification, Policy Pattern                      | Express outcomes of rule evaluation             |

---

## Documentation

| Document                                                         | Description                                      |
|------------------------------------------------------------------|--------------------------------------------------|
| [Architecture Overview](docs/architecture-overview.md)           | System architecture and pattern relationships    |
| [Pattern Categories](docs/design-pattern-categories.md)          | GoF + Enterprise category explanations           |
| [Pattern Selection Guide](docs/pattern-selection-guide.md)       | Decision tree for choosing the right pattern     |
| [Creational Patterns](docs/creational-patterns-summary.md)       | Deep dive into creational patterns               |
| [Structural Patterns](docs/structural-patterns-summary.md)       | Deep dive into structural patterns               |
| [Behavioral Patterns](docs/behavioral-patterns-summary.md)       | Deep dive into behavioral patterns               |
| [Enterprise Patterns](docs/enterprise-patterns-summary.md)       | Deep dive into enterprise patterns               |
| [Anti-Patterns](docs/anti-patterns.md)                           | Common mistakes and how to avoid them            |
| [Interview Cheat Sheet](docs/interview-revision-cheatsheet.md)   | Quick-reference revision guide                   |
| [Decision Matrix](docs/decision-matrix.md)                       | Problem-to-pattern mapping matrix                |
| [Comparison Maps](docs/comparison-maps.md)                       | Side-by-side pattern comparisons with diagrams   |

---

## License

This project is for educational purposes. Feel free to use, modify, and share.

---

> **Tip**: Star this repo and revisit it before interviews. Each pattern is designed to be read in under 10 minutes and remembered through its real-world scenario.
