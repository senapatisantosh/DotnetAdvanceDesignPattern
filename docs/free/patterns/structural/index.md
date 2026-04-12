---
title: "Structural Patterns Index"
contentKey: "patterns-structural-index"
section: "patterns-index"
accessLevel: "free"
contentType: "index"
tags: ["dotnet", "design-patterns", "structural"]
order: 2
sourceType: "same_repo"
sourcePath: "docs/free/patterns/structural/index.md"
routePath: "/project/dotnet-advanced-design-patterns/preview/patterns-structural-index"
isPublished: true
---

# Structural Patterns

Structural patterns are concerned with how classes and objects are composed to form larger structures. They use inheritance and composition to create flexible, efficient structures that keep the system manageable as it grows.

---

## Adapter

The Adapter pattern converts the interface of a class into another interface that clients expect. It acts as a translator between incompatible interfaces -- making a square peg fit a round hole. This is essential when integrating third-party libraries or legacy systems whose APIs do not match your application's expected interface. For example, wrapping a FedEx shipping SDK behind your own `IShippingCarrier` interface so your code is insulated from vendor-specific API details.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Adapter/README.md)

---

## Bridge

The Bridge pattern decouples an abstraction from its implementation so that the two can vary independently. It prevents the M x N class explosion that occurs when two independent dimensions of variation are combined through inheritance. For example, notification types (Email, SMS, Push) crossed with delivery channels (SMTP, Twilio, Firebase) would require N*M classes without Bridge. With it, you have N notification abstractions plus M channel implementations, each extensible independently.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Bridge/README.md)

---

## Composite

The Composite pattern composes objects into tree structures to represent part-whole hierarchies. It lets clients treat individual objects and compositions of objects uniformly -- a folder and a file both respond to `GetSize()`. This is ideal for modeling recursive structures like file systems, organization charts, UI component trees, or menu hierarchies where operations should apply seamlessly to both leaves and branches.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Composite/README.md)

---

## Decorator

The Decorator pattern attaches additional responsibilities to an object dynamically by wrapping it in another object that implements the same interface. Decorators are stackable -- `logging(retry(caching(service)))` -- and each layer is independently addable or removable. This avoids the class explosion that would result from using inheritance to combine behaviors. Common uses in .NET include adding logging, caching, retry logic, and validation as composable middleware around services.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Decorator/README.md)

---

## Facade

The Facade pattern provides a simplified interface to a complex subsystem. It acts as a single entry point that orchestrates multiple subsystem classes behind one convenient method call. For example, a `ReportFacade` might coordinate DataFetcher, Formatter, PdfRenderer, and EmailSender behind a single `GenerateReport()` call. Facade reduces coupling between application layers and makes complex subsystems accessible to new developers without requiring deep understanding of the internals.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Facade/README.md)

---

## Flyweight

The Flyweight pattern reduces memory usage by sharing common (intrinsic) state across many objects rather than storing it in each instance. When memory profiling reveals thousands of similar objects -- trees in a forest, characters in a text editor, particles in a game -- Flyweight extracts the shared data into a single instance and has each object reference it. Extrinsic state (position, color, context) is passed in or computed externally rather than stored per object.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Flyweight/README.md)

---

## Proxy

The Proxy pattern provides a surrogate or placeholder for another object to control access to it. This repository demonstrates three variants: Virtual Proxy (lazy-loads an expensive resource until first access), Protection Proxy (checks authorization before allowing access), and Caching Proxy (stores and returns cached results to avoid redundant calls). The proxy implements the same interface as the real object, so clients interact with it transparently without knowing a proxy is involved.

[View source and full documentation](../../../../src/DesignPatterns.Structural/Proxy/README.md)
