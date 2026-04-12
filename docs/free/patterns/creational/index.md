# Creational Patterns

Creational patterns deal with object creation mechanisms, providing flexible ways to create objects while hiding the creation logic from the consuming code. They help make a system independent of how its objects are created, composed, and represented.

---

## Factory Method

The Factory Method pattern defines an interface for creating an object but lets subclasses decide which class to instantiate. Instead of calling `new` directly, a factory method encapsulates the creation decision. This is ideal when the concrete type depends on runtime input or configuration -- for example, selecting a payment processor (Stripe, PayPal, Square) based on merchant settings. The pattern respects the Open/Closed Principle: adding a new product means adding a new creator subclass, not modifying existing code.

[View source and full documentation](../../../../src/DesignPatterns.Creational/FactoryMethod/README.md)

---

## Abstract Factory

The Abstract Factory pattern provides an interface for creating families of related objects without specifying their concrete classes. Where Factory Method creates one product, Abstract Factory creates an entire coordinated set -- for example, an AWS cloud infrastructure factory that produces matching S3 blob storage, SQS queue clients, and DynamoDB table clients, versus an Azure factory producing Blob Storage, Queue Storage, and Table Storage. Swapping the factory swaps the entire family, ensuring components are never mixed across providers.

[View source and full documentation](../../../../src/DesignPatterns.Creational/AbstractFactory/README.md)

---

## Builder

The Builder pattern separates the construction of a complex object from its representation, allowing the same construction process to create different representations. It is especially useful when an object has many optional parameters (the telescoping constructor problem). This repository demonstrates three variants: Classic Builder with a Director, Fluent Builder with method chaining (`.WithX().WithY().Build()`), and Step Builder with compile-time enforcement of mandatory build steps through interface chaining.

[View source and full documentation](../../../../src/DesignPatterns.Creational/Builder/README.md)

---

## Prototype

The Prototype pattern creates new objects by cloning an existing instance rather than constructing from scratch. When object creation involves expensive operations -- database lookups, network calls, or complex initialization -- cloning a pre-configured prototype is significantly cheaper. The pattern supports both shallow and deep copy semantics. In .NET, this maps to implementing `ICloneable` or providing a custom `Clone()` method, with careful attention to whether reference-type fields need deep copying.

[View source and full documentation](../../../../src/DesignPatterns.Creational/Prototype/README.md)

---

## Singleton

The Singleton pattern ensures a class has exactly one instance and provides a global access point to it. While the classic implementation uses a static `Instance` property with lazy initialization and thread-safety locks, modern .NET applications should prefer the dependency injection approach: register the service with `services.AddSingleton<T>()`. This provides the same single-instance guarantee with explicit dependency injection, full testability through mocking, and no hidden global state. The DI container manages the lifetime, thread safety, and disposal.

[View source and full documentation](../../../../src/DesignPatterns.Creational/Singleton/README.md)
