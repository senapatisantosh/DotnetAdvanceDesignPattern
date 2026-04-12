---
title: ".NET 10 Advanced Design Patterns"
contentKey: "readme"
section: "root"
accessLevel: "free"
contentType: "landing-page"
tags: ["dotnet", "csharp", "design-patterns", "software-architecture"]
sourceType: "same_repo"
sourcePath: "README.md"
routePath: "/project/dotnet-advanced-design-patterns"
isPublished: true
---

# .NET 10 Advanced Design Patterns

A production-grade collection of **35+ design patterns** implemented in **.NET 10 / C# 13**, organized for learning, interview preparation, and real-world architecture reference.

Every pattern uses a **realistic business scenario** — payment processing, cloud storage, healthcare IoT, order fulfillment, fraud detection, and more. No abstract `Shape` or `Animal` examples.

---

## Who This Repository Is For

- **Backend developers** who want to deeply understand design patterns through real .NET code
- **Interview candidates** preparing for system design and coding interviews
- **Architects** looking for a production-style reference of pattern implementations
- **Students** who want to go beyond textbook examples

---

## What Is Free

All source code, tests, and introductory documentation are freely available:

| Free Content | Description |
|---|---|
| **Source Code** | 236 C# files implementing 35+ patterns with realistic business scenarios |
| **Unit Tests** | 34 test files with xUnit + FluentAssertions |
| **Interactive Playground** | Console app to run any pattern demo interactively |
| **Getting Started Guides** | Architecture overview, pattern categories, pattern selection guide |
| **Pattern Overviews** | Summary docs for all four categories (Creational, Structural, Behavioral, Enterprise) |
| **Pattern Indexes** | Quick-reference listings with one-paragraph summaries per pattern |

## What Is Premium

Deep-dive content, interview prep, and architecture guidance are available through the learning platform:

| Premium Content | Description |
|---|---|
| **Deep Dives** | 10 in-depth pattern analysis articles (Factory Method, Builder, Decorator vs Proxy, Strategy vs State, CQRS, Saga, and more) |
| **Anti-Patterns Guide** | 12 anti-patterns with symptoms, code smells, and better alternatives |
| **Interview Cheat Sheet** | Master revision guide with one-liners, common questions, and tradeoff tables for all 35+ patterns |
| **Decision Matrix** | Problem-to-pattern mapping with Mermaid decision flowcharts |
| **Comparison Maps** | 10 side-by-side pattern comparisons with diagrams |
| **Premium Blog Posts** | Production-focused articles on building resilient APIs, implementing sagas, and more |

> Premium access is delivered through the main learning platform. This repository contains all code and free docs; premium content is marked accordingly.

---

## Learning Path

### Beginner Path

| Step | Pattern | Why Start Here |
|---|---|---|
| 1 | Strategy | Simplest behavioral pattern — just swap algorithms |
| 2 | Factory Method | Most common creational pattern in .NET |
| 3 | Observer | Foundation of event-driven programming |
| 4 | Decorator | Understand composition over inheritance |
| 5 | Builder | Essential for complex object construction |

Then explore: Adapter, Facade, State, Command, Template Method.

### Interview Prep Path

| Priority | Patterns | Interview Topic |
|---|---|---|
| High | Strategy, Factory Method, Observer | "Which pattern would you use for...?" |
| High | Singleton | Thread safety, DI alternative |
| High | CQRS, Repository, Unit of Work | Data access architecture |
| Medium | Decorator, Proxy, Adapter | Structural composition |
| Medium | State, Command, Chain of Responsibility | Behavioral workflows |

### Architect Path

1. **Domain Modeling**: Value Object, Specification, Domain Events, Result Pattern
2. **Data Pipeline**: Repository + Unit of Work + Specification
3. **Event-Driven**: Observer + Domain Events + Outbox + Saga
4. **Resilience**: Decorator (retry/circuit-breaker) + Proxy (caching) + Policy Pattern
5. **CQRS + Event Sourcing**: CQRS + Domain Events + Outbox

---

## Pattern Categories

### Creational Patterns — *How objects are created*

| Pattern | Business Scenario | Key Idea |
|---|---|---|
| Factory Method | Payment Gateways | Defer instantiation to subclasses |
| Abstract Factory | Cloud Storage | Create families of related objects |
| Builder | Invoice Generation | Construct complex objects step by step |
| Prototype | Feature Flags | Clone existing objects to avoid costly creation |
| Singleton | Telemetry Registry | Ensure one instance with global access |

### Structural Patterns — *How objects are composed*

| Pattern | Business Scenario | Key Idea |
|---|---|---|
| Adapter | Shipping Carriers | Make incompatible interfaces work together |
| Bridge | Notifications | Separate abstraction from implementation |
| Composite | Permission Hierarchy | Treat individual and composite objects uniformly |
| Decorator | API Client Resilience | Add behavior dynamically without subclassing |
| Facade | Report Generation | Provide a simplified interface to a subsystem |
| Flyweight | Tax Rates | Share common state to reduce memory usage |
| Proxy | Inventory Service | Control access to another object |

### Behavioral Patterns — *How objects communicate*

| Pattern | Business Scenario | Key Idea |
|---|---|---|
| Chain of Responsibility | Expense Approval | Pass request along a chain of handlers |
| Command | Order Fulfillment | Encapsulate a request as an object |
| Interpreter | Search Query DSL | Define a grammar and interpret sentences |
| Iterator | Paginated API | Access elements sequentially without exposure |
| Mediator | Checkout Workflow | Reduce coupling via a central coordinator |
| Memento | Document Editor | Capture and restore object state |
| Observer | Healthcare IoT | Notify dependents of state changes |
| State | Order Lifecycle | Alter behavior when internal state changes |
| Strategy | Pricing Engine | Swap algorithms at runtime |
| Template Method | Document Export | Define algorithm skeleton; let subclasses fill in |
| Visitor | Fraud Detection | Add operations without modifying element classes |

### Enterprise Patterns — *How production systems are structured*

| Pattern | Key Idea |
|---|---|
| Repository | Abstract data access behind a collection-like interface |
| Unit of Work | Track changes and commit as a single transaction |
| Specification | Encapsulate query logic in composable, reusable objects |
| Result Pattern | Represent success/failure without exceptions |
| CQRS | Separate read models from write models |
| Domain Events | Decouple side effects from domain logic |
| Outbox | Guarantee event delivery with transactional outbox |
| Null Object | Provide safe no-op implementations |
| Value Object | Model immutable domain concepts with value equality |
| Saga | Coordinate distributed transactions with compensation |
| Options Pattern | Bind and validate configuration in a typed manner |
| Policy Pattern | Compose business rules as first-class objects |

---

## Repository Structure

```
DotnetAdvanceDesignPattern/
├── README.md                          # This file
├── meta.json                          # Machine-readable repository metadata
├── DotnetAdvanceDesignPattern.sln
├── Directory.Build.props
│
├── assets/                            # Images and previews
│   └── preview/
│
├── docs/
│   ├── free/                          # Publicly available documentation
│   │   ├── getting-started/           # Architecture overview, pattern categories
│   │   ├── overview/                  # Category summary docs
│   │   ├── guides/                    # Pattern selection guide
│   │   └── patterns/                  # Per-category pattern indexes
│   │       ├── creational/
│   │       ├── structural/
│   │       ├── behavioral/
│   │       └── enterprise/
│   ├── premium/                       # Premium content (platform access required)
│   │   ├── deep-dive/                 # In-depth pattern analysis
│   │   ├── architecture/              # Anti-patterns, architecture trade-offs
│   │   ├── interview/                 # Interview revision cheat sheet
│   │   └── cheatsheets/               # Decision matrix, comparison maps
│   └── shared/                        # Machine-readable metadata
│       ├── content-index.json
│       ├── sidebar.json
│       └── toc.json
│
├── blog/
│   ├── public/                        # Free blog posts
│   └── premium/                       # Premium tutorials
│
├── src/                               # All source code (public)
│   ├── DesignPatterns.Creational/     # 5 creational patterns
│   ├── DesignPatterns.Structural/     # 7 structural patterns
│   ├── DesignPatterns.Behavioral/     # 11 behavioral patterns
│   ├── DesignPatterns.Enterprise/     # 12 enterprise patterns
│   ├── DesignPatterns.Shared/         # Common interfaces and helpers
│   └── DesignPatterns.Playground/     # Interactive console runner
│
├── tests/                             # All tests (public)
│   ├── DesignPatterns.Creational.Tests/
│   ├── DesignPatterns.Structural.Tests/
│   ├── DesignPatterns.Behavioral.Tests/
│   └── DesignPatterns.Enterprise.Tests/
│
└── data/                              # Machine-readable manifests
    ├── tags.json
    ├── preview-manifest.json
    └── premium-manifest.json
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

# Build the entire solution
dotnet build
```

---

## Preview Content Map

| Section | Free Content Available |
|---|---|
| Getting Started | Architecture Overview, Pattern Categories |
| Pattern Overviews | Creational, Structural, Behavioral, Enterprise summaries |
| Guides | Pattern Selection Guide with decision trees |
| Pattern Indexes | Quick-reference for all 35 patterns with summaries |
| Source Code | Full implementations of all 35+ patterns |
| Tests | Complete test suites for all patterns |
| Playground | Interactive console app to explore patterns |

---

## GoF vs Enterprise Patterns

| Aspect | GoF Patterns (Gang of Four) | Enterprise Patterns |
|---|---|---|
| **Origin** | *Design Patterns* book (1994) | Fowler's *PoEAA*, DDD, microservices era |
| **Scope** | Class/object-level design | Application/system-level architecture |
| **Focus** | Flexibility, reuse, decoupling | Consistency, scalability, data integrity |
| **Granularity** | Single class or small group | Entire layers or bounded contexts |
| **When to learn** | First — build foundation | After — apply to real systems |

---

## Documentation

### Free Documentation

| Document | Description |
|---|---|
| [Architecture Overview](docs/free/getting-started/01-architecture-overview.md) | System architecture and pattern relationships |
| [Pattern Categories](docs/free/getting-started/02-design-pattern-categories.md) | GoF + Enterprise category explanations |
| [Pattern Selection Guide](docs/free/guides/01-pattern-selection-guide.md) | Decision tree for choosing the right pattern |
| [Creational Patterns](docs/free/overview/01-creational-patterns-summary.md) | Overview of creational patterns |
| [Structural Patterns](docs/free/overview/02-structural-patterns-summary.md) | Overview of structural patterns |
| [Behavioral Patterns](docs/free/overview/03-behavioral-patterns-summary.md) | Overview of behavioral patterns |
| [Enterprise Patterns](docs/free/overview/04-enterprise-patterns-summary.md) | Overview of enterprise patterns |

### Premium Documentation (Platform Access)

| Document | Description |
|---|---|
| Anti-Patterns Guide | 12 anti-patterns with symptoms and alternatives |
| Interview Cheat Sheet | Master revision guide for all 35+ patterns |
| Decision Matrix | Problem-to-pattern mapping with flowcharts |
| Comparison Maps | 10 side-by-side pattern comparisons |
| Deep Dives | In-depth analysis of key pattern pairs and enterprise patterns |

---

> **Note**: Premium access is delivered through the main learning platform. Star this repo for updates as new patterns and deep dives are added.
