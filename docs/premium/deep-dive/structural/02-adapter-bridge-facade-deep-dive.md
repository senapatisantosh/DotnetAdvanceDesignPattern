# Deep Dive: Adapter vs Bridge vs Facade -- The Structural Pattern Confusion Resolver

Adapter, Bridge, and Facade are the three structural patterns most frequently confused in interviews and code reviews. All three deal with interfaces and indirection, but they solve fundamentally different problems at different points in the design lifecycle.

## The Core Distinction

| Aspect | Adapter | Bridge | Facade |
|--------|---------|--------|--------|
| **When designed** | After the fact (fix incompatibility) | Before the fact (plan for variation) | After the fact (simplify complexity) |
| **Direction** | Outward (integrate external code) | Inward (structure your own code) | Inward (simplify your own subsystem) |
| **Scope** | One interface to one interface (1:1) | Two hierarchies varying independently (M+N) | Many interfaces to one interface (N:1) |
| **Code ownership** | You do NOT own the adaptee | You own both sides | You own the subsystem |

## Adapter: Translating the Foreign

Adapter wraps an incompatible interface to make it conform to what your code expects. You typically cannot modify the adaptee (third-party SDK, legacy system).

```csharp
// Your system expects this
public interface IShippingCarrier
{
    Task<ShipmentResult> Ship(Shipment shipment);
    Task<TrackingInfo> Track(string trackingId);
}

// FedEx SDK has a completely different API
public class FedExAdapter : IShippingCarrier
{
    private readonly FedExClient _fedEx;

    public async Task<ShipmentResult> Ship(Shipment shipment)
    {
        var request = new FedExCreateShipmentRequest
        {
            SenderAddress = MapAddress(shipment.From),
            RecipientAddress = MapAddress(shipment.To),
            PackageWeight = new FedExWeight(shipment.WeightKg, "KG")
        };
        var response = await _fedEx.PostShipmentAsync(request);
        return new ShipmentResult(response.TrackingNumber, response.EstimatedDelivery);
    }
}
```

**Key signals you need Adapter:**
- You are integrating a third-party library
- The external API does not match your interface
- You want to isolate your code from vendor-specific changes
- You might swap vendors later (FedEx to UPS)

## Bridge: Preventing Class Explosion

Bridge separates two independent dimensions of variation so they can evolve independently. You design this BEFORE the class explosion happens.

```csharp
// Dimension 1: What kind of notification
public abstract class Notification
{
    protected readonly IDeliveryChannel _channel;
    protected Notification(IDeliveryChannel channel) => _channel = channel;
    public abstract Task Send(string recipient, string content);
}

public class UrgentNotification : Notification { /* adds priority, retry logic */ }
public class MarketingNotification : Notification { /* adds unsubscribe link, tracking */ }

// Dimension 2: How it is delivered
public interface IDeliveryChannel
{
    Task Deliver(Message message);
}

public class SmtpChannel : IDeliveryChannel { /* SMTP delivery */ }
public class TwilioChannel : IDeliveryChannel { /* SMS delivery */ }
public class FirebaseChannel : IDeliveryChannel { /* push notification */ }
```

Without Bridge: `UrgentEmailNotification`, `UrgentSmsNotification`, `UrgentPushNotification`, `MarketingEmailNotification`, `MarketingSmsNotification`, `MarketingPushNotification` -- 6 classes for 2x3. With Bridge: 2 notification types + 3 channels = 5 classes. At 5x5, it would be 25 vs 10.

**Key signals you need Bridge:**
- You see two independent dimensions of variation
- Inheritance is creating M x N classes
- You want to extend either dimension without affecting the other
- The combination of dimensions is chosen at runtime or configuration

## Facade: Simplifying the Complex

Facade provides a single entry point to a complex subsystem. You own all the code; you are just making it easier to use.

```csharp
public class ReportFacade
{
    private readonly IDataFetcher _fetcher;
    private readonly IFormatter _formatter;
    private readonly IPdfRenderer _renderer;
    private readonly IEmailSender _sender;

    public async Task GenerateAndSendReport(ReportRequest request)
    {
        var data = await _fetcher.FetchAsync(request.DataSource, request.DateRange);
        var formatted = _formatter.Format(data, request.ReportType);
        var pdf = await _renderer.RenderToPdfAsync(formatted);
        await _sender.SendAsync(request.Recipients, pdf, $"Report: {request.Title}");
    }
}
```

**Key signals you need Facade:**
- A subsystem has many interdependent classes
- Most clients need only a simplified, high-level API
- You want to reduce coupling between layers
- New developers should use the subsystem without understanding its internals

## Decision Flowchart

```
Is the code you are wrapping yours?
├── NO (third-party, legacy) ─── Is it one interface? 
│                                 ├── YES → ADAPTER
│                                 └── NO  → Multiple ADAPTERS (one per interface)
└── YES (your own code)
    ├── Are you simplifying a complex subsystem? → FACADE
    └── Are you preventing M x N class explosion? → BRIDGE
```

## How They Differ from Each Other

### Adapter vs Facade

- Adapter translates **one** external interface (1:1 mapping). Facade simplifies **many** internal interfaces (N:1 mapping).
- Adapter exists because the external code cannot be changed. Facade exists because the internal code is too complex to use directly.
- Adapter preserves the adaptee's full capability. Facade intentionally hides subsystem detail.

### Adapter vs Bridge

- Adapter is **reactive** -- you discover incompatibility and fix it. Bridge is **proactive** -- you anticipate two dimensions of variation and plan for them.
- Adapter wraps an existing incompatible interface. Bridge separates abstraction from implementation before building.
- Adapter has one wrapper. Bridge has two hierarchies connected by composition.

### Bridge vs Facade

- Bridge manages **two varying hierarchies** that compose together. Facade manages **one complex subsystem** behind a simple API.
- Bridge is about flexibility (extend either dimension). Facade is about simplicity (hide complexity).
- Bridge adds structure. Facade removes structure (from the caller's perspective).

## Production Considerations in .NET

**Adapter with DI:** Register adapters in the DI container so they are swappable:
```csharp
services.AddScoped<IShippingCarrier, FedExAdapter>();  // swap to UpsAdapter later
```

**Bridge with DI:** Inject the implementation dimension:
```csharp
services.AddScoped<IDeliveryChannel, SmtpChannel>();
services.AddScoped<Notification, UrgentNotification>();
```

**Facade with DI:** The facade itself is registered and its dependencies are injected:
```csharp
services.AddScoped<ReportFacade>();  // all dependencies auto-resolved
```

## Common Mistakes

1. **Using Adapter when you own both sides.** If you control both interfaces, just make them compatible directly. Adapter is for when you cannot modify the adaptee.
2. **Facade becoming a God class.** If the facade grows to 20+ methods, it has absorbed too much of the subsystem's surface. Split into multiple focused facades.
3. **Skipping Bridge until the explosion happens.** By the time you have 12 classes that are clearly M x N combinations, refactoring to Bridge is painful. Identify the two dimensions early.
4. **Confusing Bridge with Strategy.** Strategy swaps one algorithm dimension. Bridge separates two class hierarchies. If you only have one varying dimension, use Strategy.

## Interview-Worthy Insights

- Adapter and Facade are **about existing code** (fix or simplify). Bridge is **about new design** (plan for variation).
- In .NET, `DbDataAdapter` is literally named after the Adapter pattern -- it translates between `DataSet` and database-specific APIs.
- ASP.NET Core's `IHostEnvironment` is a Facade over multiple environment concerns (content root, environment name, application name).
- The `HttpClientFactory` in .NET acts as a Bridge between `HttpClient` (abstraction) and `HttpMessageHandler` (implementation), allowing handler pipelines to vary independently from client usage.
