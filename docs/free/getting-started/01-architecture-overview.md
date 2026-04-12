# Architecture Overview

This document describes the high-level architecture of the repository, explains how the projects relate to each other, and maps the connections between design patterns.

---

## Solution Architecture

The solution follows a layered project structure where each design pattern category lives in its own class library. A shared project provides common abstractions, and a playground console application ties everything together for interactive exploration.

```
DotnetAdvanceDesignPattern.sln
│
├── src/
│   ├── DesignPatterns.Shared          # Common interfaces, models, enums
│   ├── DesignPatterns.Creational      # 5 creational patterns
│   ├── DesignPatterns.Structural      # 7 structural patterns
│   ├── DesignPatterns.Behavioral      # 11 behavioral patterns
│   ├── DesignPatterns.Enterprise      # 12 enterprise patterns
│   └── DesignPatterns.Playground      # Console runner referencing all projects
│
└── tests/
    ├── DesignPatterns.Creational.Tests
    ├── DesignPatterns.Structural.Tests
    ├── DesignPatterns.Behavioral.Tests
    └── DesignPatterns.Enterprise.Tests
```

### Project Dependencies

```mermaid
graph TD
    Playground[DesignPatterns.Playground] --> Creational[DesignPatterns.Creational]
    Playground --> Structural[DesignPatterns.Structural]
    Playground --> Behavioral[DesignPatterns.Behavioral]
    Playground --> Enterprise[DesignPatterns.Enterprise]

    Creational --> Shared[DesignPatterns.Shared]
    Structural --> Shared
    Behavioral --> Shared
    Enterprise --> Shared

    CreationalTests[Creational.Tests] --> Creational
    StructuralTests[Structural.Tests] --> Structural
    BehavioralTests[Behavioral.Tests] --> Behavioral
    EnterpriseTests[Enterprise.Tests] --> Enterprise

    style Playground fill:#4CAF50,color:#fff
    style Shared fill:#2196F3,color:#fff
    style Creational fill:#FF9800,color:#fff
    style Structural fill:#FF9800,color:#fff
    style Behavioral fill:#FF9800,color:#fff
    style Enterprise fill:#FF9800,color:#fff
```

---

## Pattern Relationship Map

Design patterns do not exist in isolation. The following diagram shows how patterns in this repository relate to and build upon each other.

```mermaid
graph LR
    subgraph Creational
        FM[Factory Method]
        AF[Abstract Factory]
        BU[Builder]
        PR[Prototype]
        SI[Singleton]
    end

    subgraph Structural
        AD[Adapter]
        BR[Bridge]
        CO[Composite]
        DE[Decorator]
        FA[Facade]
        FW[Flyweight]
        PX[Proxy]
    end

    subgraph Behavioral
        CoR[Chain of Responsibility]
        CMD[Command]
        INT[Interpreter]
        IT[Iterator]
        MED[Mediator]
        MEM[Memento]
        OBS[Observer]
        ST[State]
        STR[Strategy]
        TM[Template Method]
        VIS[Visitor]
    end

    subgraph Enterprise
        REPO[Repository]
        UOW[Unit of Work]
        SPEC[Specification]
        RES[Result Pattern]
        CQRS[CQRS]
        DE_EV[Domain Events]
        OUT[Outbox]
        NULL[Null Object]
        VO[Value Object]
        SAGA[Saga]
        OPT[Options Pattern]
        POL[Policy Pattern]
    end

    %% Creational relationships
    FM -->|"evolves into"| AF
    AF -->|"may use"| PR
    BU -->|"alternative to"| AF

    %% Structural relationships
    DE -->|"similar structure"| PX
    AD -->|"related intent"| FA
    CO -->|"traversed by"| IT
    CO -->|"operated on by"| VIS

    %% Behavioral relationships
    STR -->|"behavior per state"| ST
    CMD -->|"undo via"| MEM
    CMD -->|"chained via"| CoR
    OBS -->|"centralized by"| MED
    STR -->|"fixed skeleton"| TM

    %% Enterprise compositions
    REPO -->|"transacted by"| UOW
    REPO -->|"filtered by"| SPEC
    CQRS -->|"dispatches"| DE_EV
    DE_EV -->|"persisted via"| OUT
    OUT -->|"orchestrated by"| SAGA
    OBS -->|"enterprise form"| DE_EV
    STR -->|"rule composition"| POL
    RES -->|"evaluated by"| SPEC
```

---

## How Patterns Compose in Real Systems

### E-Commerce Order Flow

This example shows how multiple patterns combine to handle a realistic order placement:

```mermaid
sequenceDiagram
    participant Client
    participant Mediator as Checkout Mediator
    participant Strategy as Pricing Strategy
    participant Command as Place Order Command
    participant State as Order State Machine
    participant Observer as Domain Event Publisher
    participant Outbox as Outbox Writer
    participant Saga as Fulfillment Saga

    Client->>Mediator: Submit checkout
    Mediator->>Strategy: Calculate price (Premium/Bulk/Seasonal)
    Strategy-->>Mediator: Final price
    Mediator->>Command: Execute PlaceOrderCommand
    Command->>State: Transition to Submitted
    State->>Observer: Raise OrderPlacedEvent
    Observer->>Outbox: Persist event to outbox table
    Outbox->>Saga: Relay to fulfillment saga
    Saga->>Saga: Reserve inventory, charge payment, schedule shipping
```

### Pattern Layers

| Layer               | Patterns Used                                   | Purpose                              |
|---------------------|-------------------------------------------------|--------------------------------------|
| **Presentation**    | Facade, Adapter                                 | Simplify external API surface        |
| **Application**     | Mediator, Command, CQRS                         | Orchestrate use cases                |
| **Domain**          | Strategy, State, Specification, Value Object     | Express business rules               |
| **Infrastructure**  | Repository, Unit of Work, Outbox, Proxy          | Persist and communicate reliably     |
| **Cross-cutting**   | Decorator, Observer, Singleton, Options Pattern  | Add behavior across all layers       |

---

## Folder Convention Per Pattern

Each pattern folder follows a consistent structure:

```
PatternName/
├── IPatternAbstraction.cs       # Core interface or abstract class
├── ConcreteImplementationA.cs   # First real-world implementation
├── ConcreteImplementationB.cs   # Second variant
├── SubFolder/                   # Grouped variants (e.g., Handlers/, States/)
│   ├── SpecificVariant.cs
│   └── AnotherVariant.cs
└── (Models or DTOs if needed)
```

**Naming conventions**:
- Interfaces start with `I` (e.g., `IShippingCarrier`, `IPricingStrategy`).
- Abstract base classes use the pattern name (e.g., `DocumentExporter`, `ApprovalHandler`).
- Concrete classes use descriptive business names (e.g., `FedExAdapter`, `PremiumPricingStrategy`).

---

## Design Principles Demonstrated

The patterns in this repository reinforce these core principles:

| Principle                        | Patterns That Demonstrate It                           |
|----------------------------------|--------------------------------------------------------|
| **Single Responsibility**        | Command, Strategy, Specification                       |
| **Open/Closed**                  | Decorator, Strategy, Template Method, Visitor          |
| **Liskov Substitution**          | Factory Method, Abstract Factory, Null Object          |
| **Interface Segregation**        | Adapter, Bridge, Repository                            |
| **Dependency Inversion**         | All patterns via constructor injection                 |
| **Composition over Inheritance** | Decorator, Strategy, Bridge, Composite                 |
| **Encapsulate What Varies**      | Strategy, State, Factory Method                        |
| **Program to an Interface**      | Every pattern in this repository                       |

---

## Next Steps

- [Pattern Categories](design-pattern-categories.md) — Understand the four categories.
- [Pattern Selection Guide](pattern-selection-guide.md) — Choose the right pattern for your problem.
- [Decision Matrix](decision-matrix.md) — Map problems to patterns.
