// ──────────────────────────────────────────────────────────────────────────────
// Design Patterns Playground — Interactive Console Explorer
// ──────────────────────────────────────────────────────────────────────────────

using DesignPatterns.Shared;

// ── Creational ───────────────────────────────────────────────────────────────
using DesignPatterns.Creational.FactoryMethod;
using DesignPatterns.Creational.FactoryMethod.SimpleFactory;
using DesignPatterns.Creational.AbstractFactory;
using DesignPatterns.Creational.AbstractFactory.Azure;
using DesignPatterns.Creational.AbstractFactory.Aws;
using DesignPatterns.Creational.Builder;
using DesignPatterns.Creational.Prototype;
using DesignPatterns.Creational.Singleton;
using DesignPatterns.Creational.Singleton.DiPreferred;

// ── Structural ───────────────────────────────────────────────────────────────
using DesignPatterns.Structural.Adapter;
using DesignPatterns.Structural.Adapter.ThirdParty;
using DesignPatterns.Structural.Bridge;
using DesignPatterns.Structural.Bridge.Channels;
using DesignPatterns.Structural.Bridge.Notifications;
using DesignPatterns.Structural.Composite;
using DesignPatterns.Structural.Decorator;
using DesignPatterns.Structural.Facade;
using DesignPatterns.Structural.Facade.Models;
using DesignPatterns.Structural.Flyweight;
using DesignPatterns.Structural.Proxy;
using DesignPatterns.Structural.Proxy.ProtectionProxy;

// ── Behavioral ───────────────────────────────────────────────────────────────
using DesignPatterns.Behavioral.ChainOfResponsibility;
using DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;
using DesignPatterns.Behavioral.Command;
using DesignPatterns.Behavioral.Command.Commands;
using DesignPatterns.Behavioral.Interpreter;
using DesignPatterns.Behavioral.Interpreter.Expressions;
using DesignPatterns.Behavioral.Iterator;
using DesignPatterns.Behavioral.Mediator;
using DesignPatterns.Behavioral.Mediator.Colleagues;
using DesignPatterns.Behavioral.Memento;
using DesignPatterns.Behavioral.Observer;
using DesignPatterns.Behavioral.Observer.Observers;
using DesignPatterns.Behavioral.State;
using DesignPatterns.Behavioral.Strategy;
using DesignPatterns.Behavioral.Strategy.Strategies;
using DesignPatterns.Behavioral.TemplateMethod;
using DesignPatterns.Behavioral.TemplateMethod.Exporters;
using DesignPatterns.Behavioral.Visitor;
using DesignPatterns.Behavioral.Visitor.Transactions;
using DesignPatterns.Behavioral.Visitor.Visitors;

// ── Enterprise ───────────────────────────────────────────────────────────────
using DesignPatterns.Enterprise.Repository;
using DesignPatterns.Enterprise.Specification;
using DesignPatterns.Enterprise.Specification.ProductSpecifications;
using DesignPatterns.Enterprise.ResultPattern;
using DesignPatterns.Enterprise.ResultPattern.Examples;
using DesignPatterns.Enterprise.Cqrs;
using DesignPatterns.Enterprise.Cqrs.Commands;
using DesignPatterns.Enterprise.Cqrs.Queries;
using DesignPatterns.Enterprise.Cqrs.Models;
using DesignPatterns.Enterprise.DomainEvents;
using DesignPatterns.Enterprise.DomainEvents.Events;
using DesignPatterns.Enterprise.DomainEvents.Handlers;
using DesignPatterns.Enterprise.Outbox;
using DesignPatterns.Enterprise.UnitOfWork;
using DesignPatterns.Enterprise.NullObject;
using DesignPatterns.Enterprise.ValueObject;
using DesignPatterns.Enterprise.Saga;
using DesignPatterns.Enterprise.Saga.Steps;
using DesignPatterns.Enterprise.PolicyPattern;

// ── Namespace aliases to resolve collisions ──────────────────────────────────
using InterpreterProduct = DesignPatterns.Behavioral.Interpreter.Product;
using RepositoryProduct = DesignPatterns.Enterprise.Repository.Product;
using CqrsInventoryItem = DesignPatterns.Enterprise.Cqrs.Models.InventoryItem;
using ProxyInventoryItem = DesignPatterns.Structural.Proxy.InventoryItem;
using BehavioralCommand = DesignPatterns.Behavioral.Command.ICommand;
using CqrsCommand = DesignPatterns.Enterprise.Cqrs.ICommand;
using NullObjectOrderService = DesignPatterns.Enterprise.NullObject.OrderService;
using NullObjectOrder = DesignPatterns.Enterprise.NullObject.Order;
using NullObjectOrderStatus = DesignPatterns.Enterprise.NullObject.OrderStatus;

// ─────────────────────────────────────────────────────────────────────────────
// Main program loop
// ─────────────────────────────────────────────────────────────────────────────

Console.Clear();
PrintWelcomeBanner();

var running = true;
while (running)
{
    Console.WriteLine();
    ConsoleHelper.WriteHeader("Main Menu - Design Pattern Categories");
    Console.WriteLine("  1. Creational Patterns");
    Console.WriteLine("  2. Structural Patterns");
    Console.WriteLine("  3. Behavioral Patterns");
    Console.WriteLine("  4. Enterprise Patterns");
    Console.WriteLine("  0. Exit");
    Console.WriteLine();
    Console.Write("  Select a category: ");

    var categoryChoice = Console.ReadLine()?.Trim();
    switch (categoryChoice)
    {
        case "1": await ShowCreationalMenu(); break;
        case "2": await ShowStructuralMenu(); break;
        case "3": await ShowBehavioralMenu(); break;
        case "4": await ShowEnterpriseMenu(); break;
        case "0": running = false; break;
        default:
            ConsoleHelper.WriteWarning("Invalid selection. Please try again.");
            break;
    }
}

ConsoleHelper.WriteHeader("Goodbye! Happy coding!");

// ─────────────────────────────────────────────────────────────────────────────
// Welcome banner
// ─────────────────────────────────────────────────────────────────────────────

void PrintWelcomeBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"
  ============================================================
       _____            _                _____      _   _
      |  __ \          (_)              |  __ \    | | | |
      | |  | | ___  ___ _  __ _ _ __   | |__) |_ _| |_| |_ ___ _ __ _ __  ___
      | |  | |/ _ \/ __| |/ _` | '_ \  |  ___/ _` | __| __/ _ \ '__| '_ \/ __|
      | |__| |  __/\__ \ | (_| | | | | | |  | (_| | |_| ||  __/ |  | | | \__ \
      |_____/ \___||___/_|\__, |_| |_| |_|   \__,_|\__|\__\___|_|  |_| |_|___/
                           __/ |
                          |___/           P L A Y G R O U N D
  ============================================================");
    Console.ResetColor();
    Console.WriteLine();
    ConsoleHelper.WriteInfo("Explore and run demos of Gang-of-Four and Enterprise design patterns.");
    ConsoleHelper.WriteInfo("Each demo creates objects, exercises the pattern, and prints results.");
    Console.WriteLine();
}

// ─────────────────────────────────────────────────────────────────────────────
// Category Menus
// ─────────────────────────────────────────────────────────────────────────────

async Task ShowCreationalMenu()
{
    while (true)
    {
        Console.WriteLine();
        ConsoleHelper.WriteHeader("Creational Patterns");
        Console.WriteLine("  1. Factory Method     - Payment processor creation");
        Console.WriteLine("  2. Abstract Factory   - Cloud storage families");
        Console.WriteLine("  3. Builder            - Invoice construction");
        Console.WriteLine("  4. Prototype          - Feature flag cloning");
        Console.WriteLine("  5. Singleton          - Telemetry registry");
        Console.WriteLine("  0. Back to main menu");
        Console.WriteLine();
        Console.Write("  Select a pattern: ");

        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1": await RunDemo("Factory Method", DemoFactoryMethod); break;
            case "2": await RunDemo("Abstract Factory", DemoAbstractFactory); break;
            case "3": await RunDemo("Builder", DemoBuilder); break;
            case "4": await RunDemo("Prototype", DemoPrototype); break;
            case "5": await RunDemo("Singleton", DemoSingleton); break;
            case "0": return;
            default: ConsoleHelper.WriteWarning("Invalid selection."); break;
        }
    }
}

async Task ShowStructuralMenu()
{
    while (true)
    {
        Console.WriteLine();
        ConsoleHelper.WriteHeader("Structural Patterns");
        Console.WriteLine("  1. Adapter            - Shipping service integration");
        Console.WriteLine("  2. Bridge             - Notifications x Channels");
        Console.WriteLine("  3. Composite          - Permission hierarchies");
        Console.WriteLine("  4. Decorator          - API client middleware");
        Console.WriteLine("  5. Facade             - Report generation");
        Console.WriteLine("  6. Flyweight          - Tax rate caching");
        Console.WriteLine("  7. Proxy              - Inventory access control");
        Console.WriteLine("  0. Back to main menu");
        Console.WriteLine();
        Console.Write("  Select a pattern: ");

        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1": await RunDemo("Adapter", DemoAdapter); break;
            case "2": await RunDemo("Bridge", DemoBridge); break;
            case "3": await RunDemo("Composite", DemoComposite); break;
            case "4": await RunDemo("Decorator", DemoDecorator); break;
            case "5": await RunDemo("Facade", DemoFacade); break;
            case "6": await RunDemo("Flyweight", DemoFlyweight); break;
            case "7": await RunDemo("Proxy", DemoProxy); break;
            case "0": return;
            default: ConsoleHelper.WriteWarning("Invalid selection."); break;
        }
    }
}

async Task ShowBehavioralMenu()
{
    while (true)
    {
        Console.WriteLine();
        ConsoleHelper.WriteHeader("Behavioral Patterns");
        Console.WriteLine("  1.  Chain of Responsibility - Expense approval");
        Console.WriteLine("  2.  Command                 - Order operations with undo");
        Console.WriteLine("  3.  Interpreter             - Search query DSL");
        Console.WriteLine("  4.  Iterator                - Paged data enumeration");
        Console.WriteLine("  5.  Mediator                - Checkout orchestration");
        Console.WriteLine("  6.  Memento                 - Document undo/redo");
        Console.WriteLine("  7.  Observer                - Patient vital monitoring");
        Console.WriteLine("  8.  State                   - Order lifecycle");
        Console.WriteLine("  9.  Strategy                - Dynamic pricing");
        Console.WriteLine("  10. Template Method         - Document export");
        Console.WriteLine("  11. Visitor                 - Fraud detection");
        Console.WriteLine("  0.  Back to main menu");
        Console.WriteLine();
        Console.Write("  Select a pattern: ");

        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1": await RunDemo("Chain of Responsibility", DemoChainOfResponsibility); break;
            case "2": await RunDemo("Command", DemoCommand); break;
            case "3": await RunDemo("Interpreter", DemoInterpreter); break;
            case "4": await RunDemo("Iterator", DemoIterator); break;
            case "5": await RunDemo("Mediator", DemoMediator); break;
            case "6": await RunDemo("Memento", DemoMemento); break;
            case "7": await RunDemo("Observer", DemoObserver); break;
            case "8": await RunDemo("State", DemoState); break;
            case "9": await RunDemo("Strategy", DemoStrategy); break;
            case "10": await RunDemo("Template Method", DemoTemplateMethod); break;
            case "11": await RunDemo("Visitor", DemoVisitor); break;
            case "0": return;
            default: ConsoleHelper.WriteWarning("Invalid selection."); break;
        }
    }
}

async Task ShowEnterpriseMenu()
{
    while (true)
    {
        Console.WriteLine();
        ConsoleHelper.WriteHeader("Enterprise Patterns");
        Console.WriteLine("  1.  Repository          - Product CRUD operations");
        Console.WriteLine("  2.  Unit of Work        - Atomic change tracking");
        Console.WriteLine("  3.  Specification       - Composable business rules");
        Console.WriteLine("  4.  Result Pattern      - Error handling without exceptions");
        Console.WriteLine("  5.  CQRS                - Command/query separation");
        Console.WriteLine("  6.  Domain Events       - Event-driven decoupling");
        Console.WriteLine("  7.  Outbox              - Reliable message publishing");
        Console.WriteLine("  8.  Null Object         - Eliminating null checks");
        Console.WriteLine("  9.  Value Object        - Money, Address, DateRange");
        Console.WriteLine("  10. Saga                - Distributed transaction orchestration");
        Console.WriteLine("  11. Policy Pattern      - Resilience policies");
        Console.WriteLine("  0.  Back to main menu");
        Console.WriteLine();
        Console.Write("  Select a pattern: ");

        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1": await RunDemo("Repository", DemoRepository); break;
            case "2": await RunDemo("Unit of Work", DemoUnitOfWork); break;
            case "3": await RunDemo("Specification", DemoSpecification); break;
            case "4": await RunDemo("Result Pattern", DemoResultPattern); break;
            case "5": await RunDemo("CQRS", DemoCqrs); break;
            case "6": await RunDemo("Domain Events", DemoDomainEvents); break;
            case "7": await RunDemo("Outbox", DemoOutbox); break;
            case "8": await RunDemo("Null Object", DemoNullObject); break;
            case "9": await RunDemo("Value Object", DemoValueObject); break;
            case "10": await RunDemo("Saga", DemoSaga); break;
            case "11": await RunDemo("Policy Pattern", DemoPolicy); break;
            case "0": return;
            default: ConsoleHelper.WriteWarning("Invalid selection."); break;
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Demo runner with error handling
// ─────────────────────────────────────────────────────────────────────────────

async Task RunDemo(string patternName, Func<Task> demo)
{
    Console.WriteLine();
    ConsoleHelper.WriteHeader($"Demo: {patternName}");
    try
    {
        await demo();
        Console.WriteLine();
        ConsoleHelper.WriteSuccess($"{patternName} demo completed successfully.");
    }
    catch (Exception ex)
    {
        ConsoleHelper.WriteError($"{patternName} demo failed: {ex.Message}");
    }

    Console.WriteLine();
    ConsoleHelper.WriteInfo("Press any key to continue...");
    Console.ReadKey(true);
}

// =============================================================================
// CREATIONAL PATTERN DEMOS
// =============================================================================

Task DemoFactoryMethod()
{
    ConsoleHelper.WriteSubHeader("Simple Factory - Create processors by name");
    var simpleFactory = new PaymentProcessorSimpleFactory();
    foreach (var gateway in simpleFactory.SupportedGateways)
    {
        var processor = simpleFactory.Create(gateway);
        ConsoleHelper.WriteInfo($"Gateway: {processor.GatewayName}, Supports Recurring: {processor.SupportsRecurring}");
        var result = processor.ProcessPayment(99.99m, "USD", "cust_token_123");
        ConsoleHelper.WriteSuccess($"  Transaction: {result.TransactionId}, Amount: {result.AmountCharged:C}, Gateway: {result.Gateway}");
    }

    ConsoleHelper.WriteSubHeader("Factory Method - Subclass-based creation");
    PaymentProcessorFactory[] factories =
    [
        new StripeProcessorFactory(),
        new PayPalProcessorFactory(),
        new SquareProcessorFactory()
    ];
    foreach (var factory in factories)
    {
        var result = factory.ProcessMerchantPayment(250.00m, "USD", "token_abc");
        ConsoleHelper.WriteInfo($"  {result.Gateway}: Success={result.IsSuccess}, TxnId={result.TransactionId}");
    }

    return Task.CompletedTask;
}

Task DemoAbstractFactory()
{
    ICloudStorageFactory[] factories = [new AzureStorageFactory(), new AwsStorageFactory()];

    foreach (var factory in factories)
    {
        ConsoleHelper.WriteSubHeader($"Provider: {factory.ProviderName}");
        var service = new CloudStorageService(factory);

        var blobUrl = service.StoreDocumentAndNotify(
            "reports", "quarterly-report.pdf",
            System.Text.Encoding.UTF8.GetBytes("PDF content here"),
            "upload-notifications");

        ConsoleHelper.WriteSuccess($"  Document stored at: {blobUrl}");
        ConsoleHelper.WriteInfo($"  Provider: {service.ProviderName}");

        var metadata = new Dictionary<string, object>
        {
            ["Author"] = "Finance Team",
            ["Department"] = "Accounting"
        };
        service.StoreDocumentWithMetadata("reports", "budget.xlsx",
            System.Text.Encoding.UTF8.GetBytes("Excel data"), "metadata-table", metadata);
        ConsoleHelper.WriteSuccess("  Document with metadata stored.");
    }

    return Task.CompletedTask;
}

Task DemoBuilder()
{
    ConsoleHelper.WriteSubHeader("Fluent Builder - Direct usage");
    var invoice = new FluentInvoiceBuilder()
        .WithInvoiceNumber("INV-2024-001")
        .WithSeller("TechCorp Ltd", "100 Innovation Dr, Austin, TX")
        .WithBuyer("Acme Industries", "500 Commerce St, Dallas, TX")
        .AddLineItem("Cloud Hosting (Annual)", 1, 12000m)
        .AddLineItem("Premium Support", 12, 500m)
        .AddLineItem("Data Transfer (TB)", 50, 10m)
        .WithTaxRate(8.25m)
        .WithPaymentTerms("Net 30")
        .WithHeaderNote("Thank you for your continued partnership.")
        .Build();

    ConsoleHelper.WriteInfo($"  {invoice}");
    ConsoleHelper.WriteInfo($"  Subtotal: {invoice.Currency} {invoice.Subtotal:N2}");
    ConsoleHelper.WriteInfo($"  Tax ({invoice.TaxRate}%): {invoice.Currency} {invoice.TaxAmount:N2}");
    ConsoleHelper.WriteInfo($"  Total: {invoice.FormattedTotal}");
    ConsoleHelper.WriteInfo($"  Line items: {invoice.LineItems.Count}");

    ConsoleHelper.WriteSubHeader("Director + Standard Builder - Predefined recipes");
    var builder = new StandardInvoiceBuilder();
    var director = new InvoiceDirector(builder);

    var consultingInvoice = director.BuildConsultingInvoice(
        "INV-2024-002", "Globex Corp", "742 Evergreen Terrace, Springfield", 250m, 40);
    ConsoleHelper.WriteSuccess($"  Consulting: {consultingInvoice}");

    var subscriptionInvoice = director.BuildSubscriptionInvoice(
        "INV-2024-003", "Initech", "4120 Office Park", "Enterprise", 999m, 25, 15m);
    ConsoleHelper.WriteSuccess($"  Subscription: {subscriptionInvoice}");

    var quickQuote = director.BuildQuickQuote(
        "QUO-2024-001", "Vandelay Industries", "Architecture Consulting", 5000m);
    ConsoleHelper.WriteSuccess($"  Quick Quote: {quickQuote}");

    return Task.CompletedTask;
}

Task DemoPrototype()
{
    ConsoleHelper.WriteSubHeader("Creating a feature flag prototype");
    var original = new FeatureFlagConfig
    {
        FlagKey = "new-checkout-flow",
        Name = "New Checkout Flow",
        Description = "Redesigned checkout experience with fewer steps",
        IsEnabled = true,
        DefaultRolloutPercentage = 10.0,
        Variants = new Dictionary<string, string>
        {
            ["control"] = "Legacy checkout",
            ["variant-a"] = "New streamlined checkout"
        },
        Tags = new Dictionary<string, string>
        {
            ["team"] = "growth",
            ["sprint"] = "2024-Q1"
        },
        TargetingRules =
        [
            new TargetingRule
            {
                RuleId = "rule-1",
                Description = "Beta testers",
                Priority = 1,
                RolloutPercentage = 100,
                TargetSegments =
                [
                    new UserSegment
                    {
                        SegmentId = "seg-beta",
                        Name = "Beta Users",
                        IncludedUserIds = ["user-1", "user-2", "user-3"]
                    }
                ]
            }
        ]
    };

    ConsoleHelper.WriteInfo($"  Original: {original.FlagKey} - Rollout: {original.DefaultRolloutPercentage}%");

    ConsoleHelper.WriteSubHeader("Deep cloning for A/B test variant");
    var variantB = original.CloneAsAbTestVariant("variant-b", 50.0);
    ConsoleHelper.WriteInfo($"  Clone: {variantB.FlagKey} - Rollout: {variantB.DefaultRolloutPercentage}%");
    ConsoleHelper.WriteInfo($"  Clone tags: {string.Join(", ", variantB.Tags.Select(t => $"{t.Key}={t.Value}"))}");

    // Modify clone and verify original is unaffected
    variantB.TargetingRules[0].Description = "Modified in clone";
    ConsoleHelper.WriteSuccess($"  Original rule unchanged: '{original.TargetingRules[0].Description}'");
    ConsoleHelper.WriteInfo($"  Clone rule modified: '{variantB.TargetingRules[0].Description}'");

    ConsoleHelper.WriteSubHeader("Prototype Registry");
    var registry = new FeatureFlagRegistry();
    registry.Register("standard-feature", original);

    var fromTemplate = registry.CreateFromTemplate("standard-feature", config =>
    {
        config.FlagKey = "dark-mode";
        config.Name = "Dark Mode";
        config.DefaultRolloutPercentage = 25;
    });
    ConsoleHelper.WriteSuccess($"  From template: {fromTemplate.FlagKey} ({fromTemplate.DefaultRolloutPercentage}% rollout)");
    ConsoleHelper.WriteInfo($"  Registry templates: {string.Join(", ", registry.TemplateKeys)}");

    return Task.CompletedTask;
}

Task DemoSingleton()
{
    ConsoleHelper.WriteSubHeader("Classic Singleton - TelemetryRegistry");
    var registry1 = TelemetryRegistry.Instance;
    var registry2 = TelemetryRegistry.Instance;

    ConsoleHelper.WriteInfo($"  Same instance? {ReferenceEquals(registry1, registry2)}");

    registry1.IncrementCounter("http.requests", 100);
    registry1.IncrementCounter("http.errors", 5);
    registry1.SetGauge("cpu.usage", 42.5);
    registry1.SetGauge("memory.mb", 1024.0);

    ConsoleHelper.WriteInfo($"  http.requests = {registry2.GetCounter("http.requests")}");
    ConsoleHelper.WriteInfo($"  http.errors   = {registry2.GetCounter("http.errors")}");
    ConsoleHelper.WriteInfo($"  cpu.usage     = {registry2.GetGauge("cpu.usage")}%");
    ConsoleHelper.WriteInfo($"  memory.mb     = {registry2.GetGauge("memory.mb")} MB");

    ConsoleHelper.WriteSubHeader("DI-Preferred Approach - TelemetryService");
    var service = new TelemetryService();
    service.IncrementCounter("api.calls", 42);
    service.RecordEvent("UserLogin", new Dictionary<string, string> { ["user"] = "alice" });
    ConsoleHelper.WriteSuccess($"  api.calls = {service.GetCounter("api.calls")}");
    ConsoleHelper.WriteInfo("  TelemetryService is a plain class - singleton via DI container, not static.");

    return Task.CompletedTask;
}

// =============================================================================
// STRUCTURAL PATTERN DEMOS
// =============================================================================

async Task DemoAdapter()
{
    var shippingRequest = new ShippingRequest
    {
        OriginPostalCode = "98101",
        DestinationPostalCode = "10001",
        WeightInPounds = 5.0,
        LengthInInches = 12,
        WidthInInches = 8,
        HeightInInches = 6,
        ShipDate = DateTime.Now.AddDays(1),
        RequiresSignature = true,
        IsResidential = false
    };

    IShippingService[] carriers =
    [
        new FedExAdapter(new FedExApi()),
        new UpsAdapter(new UpsApi())
    ];

    foreach (var carrier in carriers)
    {
        ConsoleHelper.WriteSubHeader($"Carrier: {carrier.CarrierName}");
        var quote = await carrier.GetQuoteAsync(shippingRequest);
        ConsoleHelper.WriteInfo($"  Service: {quote.ServiceLevel}");
        ConsoleHelper.WriteInfo($"  Price: {quote.Price:C}");
        ConsoleHelper.WriteInfo($"  Transit days: {quote.EstimatedDaysInTransit}");
        ConsoleHelper.WriteInfo($"  Guaranteed: {quote.GuaranteedDelivery}");

        var trackingNumber = await carrier.CreateShipmentAsync(shippingRequest);
        ConsoleHelper.WriteSuccess($"  Shipment created: {trackingNumber}");

        var status = await carrier.GetTrackingStatusAsync(trackingNumber);
        ConsoleHelper.WriteInfo($"  Tracking status: {status}");
    }
}

async Task DemoBridge()
{
    ConsoleHelper.WriteSubHeader("Notification types x Delivery channels");
    ConsoleHelper.WriteInfo("  Each notification can be sent via any channel independently.");

    INotificationChannel[] channels = [new EmailChannel(), new SmsChannel()];
    var orderId = "ORD-2024-1234";

    foreach (var channel in channels)
    {
        var notification = new OrderConfirmationNotification(
            channel, orderId, 299.99m, "Alice Johnson", DateTime.UtcNow.AddDays(5));

        var receipt = await notification.SendAsync("alice@example.com");
        ConsoleHelper.WriteSuccess($"  [{channel.ChannelName}] {notification.NotificationType}: " +
                                   $"MessageId={receipt.MessageId[..20]}..., Success={receipt.Succeeded}");
    }

    ConsoleHelper.WriteSubHeader("Fraud Alert via different channels");
    foreach (var channel in channels)
    {
        var fraudAlert = new FraudAlertNotification(
            channel, "TXN-9999", 15000m, "Suspicious overseas transfer", "192.168.1.100", DateTime.UtcNow);

        var receipt = await fraudAlert.SendAsync("security-team@example.com");
        ConsoleHelper.WriteInfo($"  [{channel.ChannelName}] {fraudAlert.NotificationType}: Priority={fraudAlert.Priority}");
    }
}

Task DemoComposite()
{
    ConsoleHelper.WriteSubHeader("Building a permission hierarchy");
    var hierarchy = PermissionEvaluator.BuildSampleHierarchy();

    ConsoleHelper.WriteInfo("  Permission tree:");
    hierarchy.Display(2);

    ConsoleHelper.WriteSubHeader("Evaluating permissions");
    var evaluator = new PermissionEvaluator(hierarchy);

    string[] permissionsToCheck =
        ["orders:read", "orders:write", "refunds:approve", "settings:manage", "billing:view"];
    foreach (var perm in permissionsToCheck)
    {
        var authorized = evaluator.IsAuthorized(perm);
        if (authorized)
            ConsoleHelper.WriteSuccess($"  {perm}: GRANTED");
        else
            ConsoleHelper.WriteWarning($"  {perm}: DENIED");
    }

    ConsoleHelper.WriteSubHeader("All granted permissions");
    var allPerms = evaluator.GetAllGrantedPermissions();
    ConsoleHelper.WriteInfo($"  Total: {allPerms.Count} permissions");
    ConsoleHelper.WriteInfo($"  {string.Join(", ", allPerms)}");

    var missing = evaluator.GetMissingPermissions("orders:read", "billing:view", "settings:manage");
    if (missing.Count > 0)
        ConsoleHelper.WriteWarning($"  Missing: {string.Join(", ", missing)}");

    return Task.CompletedTask;
}

async Task DemoDecorator()
{
    ConsoleHelper.WriteSubHeader("Building a decorated API client stack");
    ConsoleHelper.WriteInfo("  Logging -> Caching -> Retry -> BaseClient");

    var client = ApiClientFactory.CreateResilientClient(failureRate: 0.0, maxRetries: 2);

    ConsoleHelper.WriteSubHeader("Making API calls");
    var getResponse = await client.GetAsync("https://api.example.com/users/42");
    ConsoleHelper.WriteSuccess($"  GET: Status={getResponse.StatusCode}, Success={getResponse.IsSuccess}");

    var postResponse = await client.PostAsync("https://api.example.com/orders", """{"item": "widget", "qty": 5}""");
    ConsoleHelper.WriteSuccess($"  POST: Status={postResponse.StatusCode}, Success={postResponse.IsSuccess}");

    // Second GET should come from cache
    var cachedResponse = await client.GetAsync("https://api.example.com/users/42");
    ConsoleHelper.WriteInfo($"  Cached GET: FromCache={cachedResponse.FromCache}");

    ConsoleHelper.WriteSubHeader("Simple logging-only client");
    var loggingClient = ApiClientFactory.CreateLoggingClient();
    await loggingClient.GetAsync("https://api.example.com/health");
    ConsoleHelper.WriteSuccess("  Logging client call completed.");
}

Task DemoFacade()
{
    ConsoleHelper.WriteSubHeader("Generating a report through the facade");
    ConsoleHelper.WriteInfo("  The facade coordinates: DataFetcher -> Aggregator -> Formatter -> Exporter");

    var facade = new ReportFacade();
    var request = new ReportRequest
    {
        ReportName = "Q4 Sales Summary",
        Type = ReportType.Sales,
        StartDate = new DateTime(2024, 10, 1),
        EndDate = new DateTime(2024, 12, 31),
        Format = ExportFormat.Pdf,
        Department = "North America",
        IncludeSummary = true
    };

    var result = facade.GenerateReport(request);
    ConsoleHelper.WriteInfo($"  Report: {result.ReportName}");
    ConsoleHelper.WriteInfo($"  File: {result.FileName}");
    ConsoleHelper.WriteInfo($"  Records: {result.TotalRecords}");
    ConsoleHelper.WriteInfo($"  Pages: {result.PageCount}");
    ConsoleHelper.WriteInfo($"  Generated in: {result.GenerationDuration.TotalMilliseconds:F1}ms");
    if (result.IsSuccess)
        ConsoleHelper.WriteSuccess($"  Content size: {result.Content.Length} bytes");
    else
        ConsoleHelper.WriteError($"  Error: {result.ErrorMessage}");

    if (result.SummaryMetrics.Count > 0)
    {
        ConsoleHelper.WriteSubHeader("Summary Metrics");
        foreach (var metric in result.SummaryMetrics)
            ConsoleHelper.WriteInfo($"  {metric.Key}: {metric.Value:N2}");
    }

    return Task.CompletedTask;
}

Task DemoFlyweight()
{
    ConsoleHelper.WriteSubHeader("Processing tax calculations with shared TaxRate flyweights");

    var factory = new TaxRateFactory();
    factory.PreloadCommonRates();
    var engine = new TaxCalculationEngine(factory);

    var transactions = new[]
    {
        ("TXN-001", 1500.00m, "US-CA", DateTime.UtcNow),
        ("TXN-002", 250.00m, "US-TX", DateTime.UtcNow),
        ("TXN-003", 3200.00m, "US-CA", DateTime.UtcNow),
        ("TXN-004", 899.99m, "US-NY", DateTime.UtcNow),
        ("TXN-005", 50.00m, "US-OR", DateTime.UtcNow),
        ("TXN-006", 1200.00m, "GB", DateTime.UtcNow),
        ("TXN-007", 750.00m, "US-TX", DateTime.UtcNow),
        ("TXN-008", 4500.00m, "DE", DateTime.UtcNow),
    };

    var calculations = engine.ProcessBatch(transactions);
    foreach (var calc in calculations)
        ConsoleHelper.WriteInfo($"  {calc}");

    ConsoleHelper.WriteSubHeader("Flyweight pool statistics");
    ConsoleHelper.WriteInfo($"  Transactions processed: {calculations.Count}");
    ConsoleHelper.WriteInfo($"  Unique TaxRate objects in pool: {engine.GetFlyweightPoolSize()}");

    var summary = engine.SummarizeByJurisdiction(calculations);
    ConsoleHelper.WriteSubHeader("Tax summary by jurisdiction");
    foreach (var (jurisdiction, totalTax) in summary)
        ConsoleHelper.WriteInfo($"  {jurisdiction}: {totalTax:C}");

    return Task.CompletedTask;
}

async Task DemoProxy()
{
    ConsoleHelper.WriteSubHeader("Real Inventory Service - Direct access");
    var realService = new RealInventoryService();
    var items = await realService.GetAllItemsAsync();
    ConsoleHelper.WriteInfo($"  Total items: {items.Count}");
    foreach (var item in items.Take(3))
        ConsoleHelper.WriteInfo($"    {item.Sku}: {item.ProductName} - Available: {item.QuantityAvailable}");

    ConsoleHelper.WriteSubHeader("Protection Proxy - Access control");
    var readOnlyUser = UserContext.ReadOnlyUser("viewer@company.com");
    var proxy = new AuthenticatedInventoryProxy(realService, readOnlyUser);

    var laptop = await proxy.GetItemAsync("LAPTOP-001");
    ConsoleHelper.WriteSuccess($"  Read OK: {laptop?.ProductName} ({laptop?.QuantityAvailable} available)");

    try
    {
        await proxy.ReserveAsync("LAPTOP-001", 1);
    }
    catch (UnauthorizedAccessException ex)
    {
        ConsoleHelper.WriteWarning($"  Write blocked: {ex.Message}");
    }

    ConsoleHelper.WriteSubHeader("Full-access user - Reserve inventory");
    var adminUser = UserContext.FullAccessUser("admin@company.com");
    var adminProxy = new AuthenticatedInventoryProxy(realService, adminUser);
    var reservation = await adminProxy.ReserveAsync("KEYBOARD-001", 5);
    ConsoleHelper.WriteSuccess($"  Reserved: {reservation.QuantityReserved} units, ID: {reservation.ReservationId}");
}

// =============================================================================
// BEHAVIORAL PATTERN DEMOS
// =============================================================================

Task DemoChainOfResponsibility()
{
    ConsoleHelper.WriteSubHeader("Building the approval chain");
    ConsoleHelper.WriteInfo("  Auto(<$100) -> Manager(<$1K) -> Director(<$10K) -> VP(<$100K)");

    var auto = new AutoApprovalHandler();
    var manager = new ManagerApprovalHandler();
    var director = new DirectorApprovalHandler();
    var vp = new VpApprovalHandler();

    auto.SetNext(manager).SetNext(director).SetNext(vp);

    var requests = new[]
    {
        new ExpenseRequest { Id = Guid.NewGuid(), EmployeeName = "Alice", Amount = 50m, Description = "Office supplies", Department = "Engineering" },
        new ExpenseRequest { Id = Guid.NewGuid(), EmployeeName = "Bob", Amount = 800m, Description = "Conference tickets", Department = "Marketing" },
        new ExpenseRequest { Id = Guid.NewGuid(), EmployeeName = "Charlie", Amount = 5000m, Description = "Team offsite", Department = "Product" },
        new ExpenseRequest { Id = Guid.NewGuid(), EmployeeName = "Diana", Amount = 50000m, Description = "Annual software licenses", Department = "IT" },
        new ExpenseRequest { Id = Guid.NewGuid(), EmployeeName = "Eve", Amount = 150000m, Description = "Office renovation", Department = "Facilities" },
    };

    ConsoleHelper.WriteSubHeader("Processing expense requests");
    foreach (var request in requests)
    {
        var result = auto.Handle(request);
        if (result is not null)
        {
            if (result.IsApproved)
                ConsoleHelper.WriteSuccess($"  ${request.Amount:N0} by {request.EmployeeName}: APPROVED by {result.ApprovedBy}");
            else
                ConsoleHelper.WriteError($"  ${request.Amount:N0} by {request.EmployeeName}: REJECTED - {result.Reason}");
        }
        else
        {
            ConsoleHelper.WriteWarning($"  ${request.Amount:N0} by {request.EmployeeName}: No handler could process");
        }
    }

    return Task.CompletedTask;
}

Task DemoCommand()
{
    ConsoleHelper.WriteSubHeader("Creating an order and executing commands");

    var order = new DesignPatterns.Behavioral.Command.Order
    {
        CustomerName = "Alice Johnson",
        Items = new List<string> { "Laptop", "Mouse", "Keyboard" },
        TotalAmount = 1599.97m
    };

    var invoker = new OrderCommandInvoker();

    var placeCmd = new PlaceOrderCommand(order);
    invoker.ExecuteCommand(placeCmd);
    ConsoleHelper.WriteSuccess($"  Placed: Status={order.Status}");

    var shippingCmd = new UpdateShippingCommand(order, "456 Oak Ave, Portland, OR 97201");
    invoker.ExecuteCommand(shippingCmd);
    ConsoleHelper.WriteSuccess($"  Shipping updated: {order.ShippingAddress}");

    ConsoleHelper.WriteSubHeader("Undo operations");
    invoker.Undo();
    ConsoleHelper.WriteInfo($"  After undo shipping: Address='{order.ShippingAddress}'");

    invoker.Undo();
    ConsoleHelper.WriteInfo($"  After undo place: Status={order.Status}");

    ConsoleHelper.WriteSubHeader("Redo operations");
    invoker.Redo();
    ConsoleHelper.WriteInfo($"  After redo place: Status={order.Status}");

    ConsoleHelper.WriteSubHeader("Audit log");
    foreach (var entry in order.AuditLog)
        ConsoleHelper.WriteInfo($"  - {entry}");

    ConsoleHelper.WriteInfo($"  Undo stack: {invoker.UndoCount}, Redo stack: {invoker.RedoCount}");

    return Task.CompletedTask;
}

Task DemoInterpreter()
{
    ConsoleHelper.WriteSubHeader("Product catalog for search");
    var products = new List<InterpreterProduct>
    {
        new() { Name = "MacBook Pro", Category = "Electronics", Brand = "Apple", Price = 2499m, Color = "Silver" },
        new() { Name = "iPhone 15", Category = "Electronics", Brand = "Apple", Price = 999m, Color = "Blue" },
        new() { Name = "Galaxy S24", Category = "Electronics", Brand = "Samsung", Price = 899m, Color = "Black" },
        new() { Name = "Air Max 90", Category = "Shoes", Brand = "Nike", Price = 130m, Color = "White" },
        new() { Name = "Ultraboost", Category = "Shoes", Brand = "Adidas", Price = 180m, Color = "Black" },
        new() { Name = "Classic T-Shirt", Category = "Clothing", Brand = "Generic", Price = 25m, Color = "Red" },
    };

    foreach (var p in products)
        ConsoleHelper.WriteInfo($"  {p.Name} | {p.Category} | {p.Brand} | {p.Price:C} | {p.Color}");

    var parser = new SearchQueryParser();
    string[] queries =
    [
        "category:Electronics AND brand:Apple",
        "category:Shoes OR category:Clothing",
        "NOT color:Black AND category:Electronics",
        "brand:Nike OR brand:Adidas"
    ];

    ConsoleHelper.WriteSubHeader("Search query evaluation");
    foreach (var query in queries)
    {
        var expression = parser.Parse(query);
        var matches = products.Where(p => expression.Interpret(p)).ToList();
        ConsoleHelper.WriteInfo($"  Query: \"{query}\"");
        ConsoleHelper.WriteSuccess($"    Matches ({matches.Count}): {string.Join(", ", matches.Select(m => m.Name))}");
    }

    return Task.CompletedTask;
}

async Task DemoIterator()
{
    ConsoleHelper.WriteSubHeader("Paged data source with lazy fetching");

    var allItems = Enumerable.Range(1, 47).Select(i => $"Item-{i:D3}").ToList();
    var dataSource = new InMemoryPagedDataSource<string>(allItems);
    var paged = new PagedEnumerable<string>(dataSource, pageSize: 10);

    ConsoleHelper.WriteInfo($"  Total items: {allItems.Count}");
    ConsoleHelper.WriteInfo("  Iterating page by page (page size = 10):");

    await foreach (var page in paged.GetPagesAsync())
    {
        ConsoleHelper.WriteInfo($"  Page {page.PageNumber}/{page.TotalPages}: " +
                                $"[{page.Items[0]}..{page.Items[^1]}] ({page.Items.Count} items)");
    }

    ConsoleHelper.WriteInfo($"  Pages fetched from data source: {dataSource.FetchCount}");

    ConsoleHelper.WriteSubHeader("Transparent item iteration");
    var dataSource2 = new InMemoryPagedDataSource<string>(allItems);
    var paged2 = new PagedEnumerable<string>(dataSource2, pageSize: 15);
    var count = 0;
    await foreach (var item in paged2.GetAllItemsAsync())
    {
        count++;
    }
    ConsoleHelper.WriteSuccess($"  Iterated {count} items transparently ({dataSource2.FetchCount} page fetches)");
}

async Task DemoMediator()
{
    ConsoleHelper.WriteSubHeader("Checkout orchestration via mediator");

    var inventory = new InventoryColleague(new Dictionary<string, int>
    {
        ["PROD-001"] = 10,
        ["PROD-002"] = 5
    });
    var payment = new PaymentColleague();
    var shipping = new ShippingColleague();
    var notification = new NotificationColleague();

    var mediator = new CheckoutMediator(inventory, payment, shipping, notification);

    var request = new CheckoutRequest
    {
        OrderId = Guid.NewGuid(),
        CustomerId = "CUST-42",
        Items =
        [
            new CheckoutItem { ProductId = "PROD-001", ProductName = "Widget", Quantity = 2, UnitPrice = 25m },
            new CheckoutItem { ProductId = "PROD-002", ProductName = "Gadget", Quantity = 1, UnitPrice = 75m }
        ],
        ShippingAddress = "123 Main St, Springfield, IL",
        PaymentMethod = "credit_card",
        TotalAmount = 125m
    };

    var result = await mediator.ProcessCheckoutAsync(request);

    if (result.Success)
    {
        ConsoleHelper.WriteSuccess($"  Checkout succeeded! Order: {result.OrderId}");
        ConsoleHelper.WriteInfo($"  Tracking: {result.TrackingNumber}");
        ConsoleHelper.WriteInfo($"  Payment: {result.PaymentTransactionId}");
    }
    else
    {
        ConsoleHelper.WriteError($"  Checkout failed: {result.FailureReason}");
    }

    ConsoleHelper.WriteSubHeader("Orchestration steps");
    foreach (var step in result.Steps)
        ConsoleHelper.WriteInfo($"  {step}");
}

Task DemoMemento()
{
    ConsoleHelper.WriteSubHeader("Document editing with undo/redo");

    var doc = new Document { Title = "Draft", Content = "Hello, world!", FontFamily = "Arial", FontSize = 12 };
    var history = new DocumentHistory(doc);
    ConsoleHelper.WriteInfo($"  Initial: {doc}");

    // Edit 1
    history.SaveState("Before title change");
    doc.Title = "Design Patterns Guide";
    doc.Content = "Chapter 1: Introduction to Design Patterns";
    ConsoleHelper.WriteInfo($"  Edit 1: {doc}");

    // Edit 2
    history.SaveState("Before font change");
    doc.FontFamily = "Consolas";
    doc.FontSize = 14;
    ConsoleHelper.WriteInfo($"  Edit 2: {doc}");

    // Edit 3
    history.SaveState("Before content update");
    doc.Content = "Chapter 1: Introduction to Design Patterns\nChapter 2: Creational Patterns";
    ConsoleHelper.WriteInfo($"  Edit 3: {doc}");

    ConsoleHelper.WriteSubHeader("Undo operations");
    history.Undo();
    ConsoleHelper.WriteInfo($"  After undo 1: {doc}");

    history.Undo();
    ConsoleHelper.WriteInfo($"  After undo 2: {doc}");

    ConsoleHelper.WriteSubHeader("Redo operation");
    history.Redo();
    ConsoleHelper.WriteInfo($"  After redo: {doc}");

    ConsoleHelper.WriteInfo($"  Undo count: {history.UndoCount}, Redo count: {history.RedoCount}");

    ConsoleHelper.WriteSubHeader("Undo history labels");
    foreach (var label in history.GetUndoHistory())
        ConsoleHelper.WriteInfo($"  - {label}");

    return Task.CompletedTask;
}

Task DemoObserver()
{
    ConsoleHelper.WriteSubHeader("Patient monitoring with multiple observers");

    var monitor = new PatientMonitor("P-1001", "John Doe");

    var alertService = new AlertService();
    var dashboard = new DashboardUpdater();
    var auditLogger = new Behavioral.Observer.Observers.AuditLogger();

    monitor.Subscribe(alertService);
    monitor.Subscribe(dashboard);
    monitor.Subscribe(auditLogger);

    ConsoleHelper.WriteInfo($"  Patient: {monitor.PatientName} ({monitor.PatientId})");
    ConsoleHelper.WriteInfo($"  Observers subscribed: {monitor.ObserverCount}");

    VitalSignReading[] readings =
    [
        new() { PatientId = "P-1001", PatientName = "John Doe", Type = VitalSignType.HeartRate, Value = 72, Unit = "bpm", DeviceId = "ECG-01" },
        new() { PatientId = "P-1001", PatientName = "John Doe", Type = VitalSignType.BloodOxygen, Value = 98, Unit = "%", DeviceId = "OX-01" },
        new() { PatientId = "P-1001", PatientName = "John Doe", Type = VitalSignType.HeartRate, Value = 160, Unit = "bpm", DeviceId = "ECG-01" },
        new() { PatientId = "P-1001", PatientName = "John Doe", Type = VitalSignType.BloodOxygen, Value = 88, Unit = "%", DeviceId = "OX-01" },
        new() { PatientId = "P-1001", PatientName = "John Doe", Type = VitalSignType.Temperature, Value = 36.8, Unit = "C", DeviceId = "TEMP-01" },
    ];

    foreach (var reading in readings)
    {
        monitor.RecordReading(reading);
        var marker = reading.IsCritical ? "** CRITICAL **" : "normal";
        ConsoleHelper.WriteInfo($"  {reading.Type}: {reading.Value}{reading.Unit} [{marker}]");
    }

    ConsoleHelper.WriteSubHeader("Alert Service results");
    foreach (var alert in alertService.Alerts)
        ConsoleHelper.WriteError($"  {alert.Message}");

    ConsoleHelper.WriteSubHeader("Dashboard latest readings");
    var dashboardData = dashboard.GetPatientDashboard("P-1001");
    foreach (var (type, reading) in dashboardData)
        ConsoleHelper.WriteInfo($"  {type}: {reading.Value}{reading.Unit}");

    ConsoleHelper.WriteInfo($"  Audit log entries: {auditLogger.Log.Count}");

    return Task.CompletedTask;
}

Task DemoState()
{
    ConsoleHelper.WriteSubHeader("Order lifecycle state machine");

    var order = new OrderContext(Guid.NewGuid(), "Alice");
    ConsoleHelper.WriteInfo($"  Initial state: {order.CurrentStateName}");

    order.Submit();
    ConsoleHelper.WriteInfo($"  After Submit: {order.CurrentStateName}");

    order.Approve();
    ConsoleHelper.WriteInfo($"  After Approve: {order.CurrentStateName}");

    order.Ship();
    ConsoleHelper.WriteInfo($"  After Ship: {order.CurrentStateName}");

    order.Deliver();
    ConsoleHelper.WriteInfo($"  After Deliver: {order.CurrentStateName}");

    ConsoleHelper.WriteSubHeader("Transition log");
    foreach (var entry in order.TransitionLog)
        ConsoleHelper.WriteInfo($"  {entry}");

    ConsoleHelper.WriteSubHeader("Invalid transition handling");
    try
    {
        order.Cancel();
    }
    catch (InvalidOperationException ex)
    {
        ConsoleHelper.WriteWarning($"  Cannot cancel delivered order: {ex.Message}");
    }

    ConsoleHelper.WriteSubHeader("Order with cancellation");
    var order2 = new OrderContext(Guid.NewGuid(), "Bob");
    order2.Submit();
    order2.Cancel();
    ConsoleHelper.WriteInfo($"  Bob's order state: {order2.CurrentStateName}");

    return Task.CompletedTask;
}

Task DemoStrategy()
{
    ConsoleHelper.WriteSubHeader("Different pricing strategies for the same order");

    var order = new OrderDetails
    {
        BasePrice = 100m,
        Quantity = 15,
        CustomerTier = "Premium",
        LoyaltyPoints = 750,
        IsPromotionActive = true,
        PromotionDiscountPercent = 20m
    };

    ConsoleHelper.WriteInfo($"  Order: {order.Quantity} x {order.BasePrice:C} = {order.Subtotal:C} (subtotal)");
    ConsoleHelper.WriteInfo($"  Customer: {order.CustomerTier}, Loyalty: {order.LoyaltyPoints}pts, Promo: {order.IsPromotionActive}");

    IPricingStrategy[] strategies =
    [
        new StandardPricingStrategy(),
        new PremiumPricingStrategy(),
        new VolumeDiscountStrategy(),
        new LoyaltyPricingStrategy(),
        new PromotionalPricingStrategy()
    ];

    foreach (var strategy in strategies)
    {
        var price = strategy.CalculatePrice(order);
        ConsoleHelper.WriteInfo($"  {strategy.Name,-25} => {price:C}");
    }

    ConsoleHelper.WriteSubHeader("Factory-selected strategy");
    var factory = new PricingStrategyFactory();
    var autoSelected = factory.GetStrategy(order);
    ConsoleHelper.WriteSuccess($"  Auto-selected: {autoSelected.Name} => {autoSelected.CalculatePrice(order):C}");

    var (bestPrice, bestName) = factory.GetBestPrice(order);
    ConsoleHelper.WriteSuccess($"  Best price: {bestName} => {bestPrice:C}");

    ConsoleHelper.WriteSubHeader("Runtime strategy swap via PricingContext");
    var context = new PricingContext(new StandardPricingStrategy());
    ConsoleHelper.WriteInfo($"  {context.CurrentStrategyName}: {context.CalculatePrice(order):C}");
    context.SetStrategy(new VolumeDiscountStrategy());
    ConsoleHelper.WriteInfo($"  {context.CurrentStrategyName}: {context.CalculatePrice(order):C}");

    return Task.CompletedTask;
}

Task DemoTemplateMethod()
{
    ConsoleHelper.WriteSubHeader("Exporting data using different format exporters");

    var data = new List<Dictionary<string, string>>
    {
        new() { ["Name"] = "Widget A", ["Category"] = "Hardware", ["Price"] = "29.99", ["Stock"] = "150" },
        new() { ["Name"] = "Widget B", ["Category"] = "Software", ["Price"] = "49.99", ["Stock"] = "300" },
        new() { ["Name"] = "Widget C", ["Category"] = "Hardware", ["Price"] = "19.99", ["Stock"] = "75" },
        new() { ["Name"] = "Service X", ["Category"] = "Services", ["Price"] = "99.99", ["Stock"] = "N/A" },
    };

    DocumentExporter[] exporters =
    [
        new PdfExporter("Product Catalog"),
        new CsvExporter(),
        new ExcelExporter()
    ];

    foreach (var exporter in exporters)
    {
        var result = exporter.Export(data);
        ConsoleHelper.WriteSubHeader($"Format: {result.Format}");
        ConsoleHelper.WriteInfo($"  Success: {result.Success}");
        ConsoleHelper.WriteInfo($"  Records: {result.RecordCount}");
        ConsoleHelper.WriteInfo($"  Steps: {string.Join(" -> ", result.Steps)}");
        ConsoleHelper.WriteInfo($"  Duration: {result.Duration.TotalMilliseconds:F2}ms");
        if (result.Success)
        {
            var preview = result.Content.Length > 120
                ? result.Content[..120] + "..."
                : result.Content;
            ConsoleHelper.WriteInfo($"  Preview: {preview}");
        }
    }

    return Task.CompletedTask;
}

Task DemoVisitor()
{
    ConsoleHelper.WriteSubHeader("Transactions to analyze for fraud");

    ITransaction[] transactions =
    [
        new CardTransaction
        {
            TransactionId = "CARD-001", Amount = 250m, OriginCountry = "US",
            CardLastFour = "4242", MerchantCategory = "Electronics", IsOnline = true
        },
        new WireTransfer
        {
            TransactionId = "WIRE-001", Amount = 50000m, OriginCountry = "US",
            DestinationCountry = "CH", SenderBankCode = "BOFA", ReceiverBankCode = "UBS"
        },
        new CryptoTransaction
        {
            TransactionId = "CRYPTO-001", Amount = 15000m, OriginCountry = "RU",
            WalletAddress = "0xABC123", CryptoCurrency = "BTC", ExchangeName = "Binance"
        },
    ];

    IFraudDetectionVisitor[] visitors =
    [
        new VelocityCheckVisitor(),
        new AmountAnomalyVisitor(),
        new GeoAnomalyVisitor()
    ];

    foreach (var transaction in transactions)
    {
        ConsoleHelper.WriteSubHeader($"Transaction: {transaction.TransactionId} ({transaction.GetType().Name})");
        ConsoleHelper.WriteInfo($"  Amount: {transaction.Amount:C}, Country: {transaction.OriginCountry}");

        foreach (var visitor in visitors)
        {
            var result = transaction.Accept(visitor);
            if (result.IsSuspicious)
                ConsoleHelper.WriteWarning($"  [{result.RuleName}] SUSPICIOUS (Risk: {result.Risk}) - {result.Reason}");
            else
                ConsoleHelper.WriteSuccess($"  [{result.RuleName}] Clear - {result.Reason}");
        }
    }

    return Task.CompletedTask;
}

// =============================================================================
// ENTERPRISE PATTERN DEMOS
// =============================================================================

async Task DemoRepository()
{
    ConsoleHelper.WriteSubHeader("In-memory product repository with tenant isolation");

    var repo = new InMemoryProductRepository();

    var products = new[]
    {
        new RepositoryProduct { TenantId = "store-a", Name = "Laptop Pro", Category = "Electronics", Price = 1299m, StockQuantity = 25 },
        new RepositoryProduct { TenantId = "store-a", Name = "Wireless Mouse", Category = "Electronics", Price = 29.99m, StockQuantity = 150 },
        new RepositoryProduct { TenantId = "store-a", Name = "Desk Lamp", Category = "Furniture", Price = 45m, StockQuantity = 0 },
        new RepositoryProduct { TenantId = "store-b", Name = "Running Shoes", Category = "Footwear", Price = 89.99m, StockQuantity = 60 },
    };

    foreach (var p in products)
        await repo.AddAsync(p);

    ConsoleHelper.WriteInfo($"  Total products: {await repo.CountAsync()}");

    var storeAProducts = await repo.GetByTenantAsync("store-a");
    ConsoleHelper.WriteInfo($"  Store A products: {storeAProducts.Count}");
    foreach (var p in storeAProducts)
        ConsoleHelper.WriteInfo($"    {p}");

    var electronics = await repo.GetByCategoryAsync("store-a", "Electronics");
    ConsoleHelper.WriteSuccess($"  Store A electronics: {electronics.Count}");

    var affordable = await repo.GetInPriceRangeAsync("store-a", 0, 50);
    ConsoleHelper.WriteInfo($"  Store A under $50: {string.Join(", ", affordable.Select(p => p.Name))}");

    var categories = await repo.GetCategoriesAsync("store-a");
    ConsoleHelper.WriteInfo($"  Store A categories: {string.Join(", ", categories)}");
}

Task DemoUnitOfWork()
{
    ConsoleHelper.WriteSubHeader("Tracking and committing changes atomically");

    using var uow = new InMemoryUnitOfWork();

    var entity1 = new SampleEntity { Name = "First Entity" };
    var entity2 = new SampleEntity { Name = "Second Entity" };

    uow.RegisterNew(entity1);
    uow.RegisterNew(entity2);
    ConsoleHelper.WriteInfo($"  Registered 2 new entities");

    var committed = uow.CommitAsync().Result;
    ConsoleHelper.WriteSuccess($"  Committed {committed} changes");

    entity1.Name = "Updated First Entity";
    uow.RegisterDirty(entity1);
    committed = uow.CommitAsync().Result;
    ConsoleHelper.WriteSuccess($"  Updated entity, committed {committed} change(s)");

    var changeLog = uow.GetChangeLog();
    ConsoleHelper.WriteSubHeader("Change log");
    foreach (var record in changeLog)
        ConsoleHelper.WriteInfo($"  {record.ChangeType}: {record.EntityType} ({record.EntityId})");

    var retrieved = uow.TryGet<SampleEntity>(entity1.Id);
    ConsoleHelper.WriteInfo($"  Retrieved: {retrieved?.Name}");

    return Task.CompletedTask;
}

Task DemoSpecification()
{
    ConsoleHelper.WriteSubHeader("Composable business rules for product filtering");

    var products = new List<RepositoryProduct>
    {
        new() { TenantId = "t1", Name = "Gaming Laptop", Category = "Electronics", Price = 1499m, StockQuantity = 10 },
        new() { TenantId = "t1", Name = "Budget Tablet", Category = "Electronics", Price = 199m, StockQuantity = 50 },
        new() { TenantId = "t1", Name = "USB Cable", Category = "Accessories", Price = 9.99m, StockQuantity = 500 },
        new() { TenantId = "t1", Name = "Monitor Stand", Category = "Accessories", Price = 79m, StockQuantity = 0 },
        new() { TenantId = "t1", Name = "Noise-Canceling Headphones", Category = "Electronics", Price = 349m, StockQuantity = 25 },
    };

    ConsoleHelper.WriteInfo("  Products:");
    foreach (var p in products)
        ConsoleHelper.WriteInfo($"    {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity}");

    var electronics = new CategorySpecification("Electronics");
    var affordable = new PriceRangeSpecification(0, 500);
    var inStock = new InStockSpecification();

    // Compose: Affordable electronics that are in stock
    var spec = electronics.And(affordable).And(inStock);
    var filtered = products.Where(p => spec.IsSatisfiedBy(p)).ToList();
    ConsoleHelper.WriteSubHeader($"Affordable in-stock electronics ({spec})");
    foreach (var p in filtered)
        ConsoleHelper.WriteSuccess($"    {p.Name} - {p.Price:C}");

    // Not electronics
    var notElectronics = electronics.Not();
    var nonElec = products.Where(p => notElectronics.IsSatisfiedBy(p)).ToList();
    ConsoleHelper.WriteSubHeader($"Non-electronics ({notElectronics})");
    foreach (var p in nonElec)
        ConsoleHelper.WriteInfo($"    {p.Name}");

    // Out of stock OR expensive
    var outOfStock = inStock.Not();
    var expensive = new PriceRangeSpecification(1000, decimal.MaxValue);
    var problemItems = outOfStock.Or(expensive);
    var problems = products.Where(p => problemItems.IsSatisfiedBy(p)).ToList();
    ConsoleHelper.WriteSubHeader($"Problem items - out of stock OR expensive ({problemItems})");
    foreach (var p in problems)
        ConsoleHelper.WriteWarning($"    {p.Name} - Stock: {p.StockQuantity}, Price: {p.Price:C}");

    return Task.CompletedTask;
}

Task DemoResultPattern()
{
    ConsoleHelper.WriteSubHeader("User registration with Result pattern");

    var service = new UserRegistrationService();

    var testCases = new[]
    {
        ("alice@example.com", "Passw0rd!", "Alice Johnson"),
        ("", "Passw0rd!", "Bad Email"),
        ("bob@example.com", "short", "Bob Smith"),
        ("charlie@example.com", "nDigitHere", "Charlie Brown"),
        ("alice@example.com", "Passw0rd!", "Alice Duplicate"),
    };

    foreach (var (email, password, name) in testCases)
    {
        var result = service.Register(email, password, name);

        if (result.IsSuccess)
        {
            ConsoleHelper.WriteSuccess($"  Registered: {result.Value.Email} (ID: {result.Value.Id})");
        }
        else
        {
            ConsoleHelper.WriteWarning($"  Failed for '{name}': [{result.Error.Code}] {result.Error.Message} (Type: {result.Error.Type})");
        }
    }

    ConsoleHelper.WriteSubHeader("Result functional extensions");
    var chainResult = service.Register("dave@example.com", "Str0ngPass", "Dave Wilson");
    var greeting = chainResult.Match(
        onSuccess: user => $"Welcome, {user.FullName}! Account created at {user.CreatedAt:HH:mm:ss}",
        onFailure: error => $"Registration error: {error.Message}");
    ConsoleHelper.WriteInfo($"  {greeting}");

    return Task.CompletedTask;
}

async Task DemoCqrs()
{
    ConsoleHelper.WriteSubHeader("CQRS - Separate command and query handlers");

    var store = new InMemoryInventoryStore();
    store.Seed(
        new CqrsInventoryItem { Sku = "WIDGET-A", ProductName = "Widget Alpha", AvailableQuantity = 100, ReservedQuantity = 0, ReorderThreshold = 20 },
        new CqrsInventoryItem { Sku = "WIDGET-B", ProductName = "Widget Beta", AvailableQuantity = 15, ReservedQuantity = 5, ReorderThreshold = 20 },
        new CqrsInventoryItem { Sku = "GADGET-X", ProductName = "Gadget X", AvailableQuantity = 50, ReservedQuantity = 10, ReorderThreshold = 15 }
    );

    var reserveHandler = new ReserveInventoryHandler(store);
    var queryHandler = new GetInventoryLevelHandler(store);

    ConsoleHelper.WriteInfo("  Initial inventory:");
    foreach (var item in store.GetAll())
        ConsoleHelper.WriteInfo($"    {item}");

    ConsoleHelper.WriteSubHeader("Command: Reserve inventory");
    var reserveCmd = new ReserveInventoryCommand("WIDGET-A", 25, "ORD-001");
    await reserveHandler.HandleAsync(reserveCmd);
    ConsoleHelper.WriteSuccess("  Reserved 25 units of WIDGET-A for order ORD-001");

    ConsoleHelper.WriteSubHeader("Query: Check inventory level");
    var level = await queryHandler.HandleAsync(new GetInventoryLevelQuery("WIDGET-A"));
    ConsoleHelper.WriteInfo($"  WIDGET-A: {level}");
    ConsoleHelper.WriteInfo($"  Low stock? {level?.IsLowStock}");

    ConsoleHelper.WriteSubHeader("Query: Low stock items");
    var lowStockHandler = new GetLowStockItemsHandler(store);
    var lowStockItems = await lowStockHandler.HandleAsync(new GetLowStockItemsQuery());
    ConsoleHelper.WriteInfo($"  Low stock items: {lowStockItems.Count}");
    foreach (var item in lowStockItems)
        ConsoleHelper.WriteWarning($"    {item}");
}

async Task DemoDomainEvents()
{
    ConsoleHelper.WriteSubHeader("Domain event dispatch with multiple handlers");

    var dispatcher = new DomainEventDispatcher();

    var emailHandler = new OrderPlacedEmailHandler();
    var inventoryHandler = new OrderPlacedInventoryHandler();
    var auditHandler = new PaymentReceivedAuditHandler();

    dispatcher.Register(emailHandler);
    dispatcher.Register(inventoryHandler);
    dispatcher.Register(auditHandler);

    ConsoleHelper.WriteSubHeader("Publishing OrderPlacedEvent");
    var orderPlaced = new OrderPlacedEvent(
        Guid.NewGuid(),
        "CUST-100",
        new List<OrderLineItem>
        {
            new("SKU-001", "Wireless Keyboard", 2, 49.99m),
            new("SKU-002", "USB-C Hub", 1, 79.99m)
        },
        179.97m);

    var handleCount = await dispatcher.DispatchAsync(orderPlaced);
    ConsoleHelper.WriteSuccess($"  OrderPlaced dispatched to {handleCount} handler(s)");

    ConsoleHelper.WriteInfo($"  Emails sent: {emailHandler.SentEmails.Count}");
    foreach (var email in emailHandler.SentEmails)
        ConsoleHelper.WriteInfo($"    {email}");

    ConsoleHelper.WriteInfo($"  Inventory reservations: {inventoryHandler.Reservations.Count}");
    foreach (var (sku, qty) in inventoryHandler.Reservations)
        ConsoleHelper.WriteInfo($"    {sku} x {qty}");

    ConsoleHelper.WriteSubHeader("Publishing PaymentReceivedEvent");
    var paymentReceived = new PaymentReceivedEvent(
        orderPlaced.OrderId, 179.97m, "USD", "CreditCard", "TXN-ABC123");
    handleCount = await dispatcher.DispatchAsync(paymentReceived);
    ConsoleHelper.WriteSuccess($"  PaymentReceived dispatched to {handleCount} handler(s)");

    foreach (var entry in auditHandler.AuditLog)
        ConsoleHelper.WriteInfo($"    Audit: {entry.Description}");
}

async Task DemoOutbox()
{
    ConsoleHelper.WriteSubHeader("Transactional outbox for reliable messaging");

    var outboxStore = new InMemoryOutboxStore();
    var outboxPublisher = new OutboxPublisher(outboxStore);

    // Simulate writing events to the outbox (same DB transaction as business logic)
    await outboxPublisher.PublishAsync(new { EventType = "OrderCreated", OrderId = Guid.NewGuid(), Amount = 99.99 });
    await outboxPublisher.PublishAsync(new { EventType = "PaymentProcessed", TransactionId = "TXN-123" });
    await outboxPublisher.PublishAsync(new { EventType = "ShipmentScheduled", TrackingNumber = "TRACK-456" });

    var pendingCount = await outboxStore.GetPendingCountAsync();
    ConsoleHelper.WriteInfo($"  Pending messages in outbox: {pendingCount}");

    ConsoleHelper.WriteSubHeader("Processing outbox with message publisher");
    var publisher = new InMemoryMessagePublisher();
    var processor = new OutboxProcessor(outboxStore, publisher);

    var processed = await processor.ProcessBatchAsync(10);
    ConsoleHelper.WriteSuccess($"  Processed {processed} message(s)");

    var remaining = await outboxStore.GetPendingCountAsync();
    ConsoleHelper.WriteInfo($"  Remaining pending: {remaining}");

    ConsoleHelper.WriteInfo($"  Published messages:");
    foreach (var msg in publisher.Published)
        ConsoleHelper.WriteInfo($"    [{msg.EventType}] {msg.Payload[..Math.Min(60, msg.Payload.Length)]}...");
}

async Task DemoNullObject()
{
    ConsoleHelper.WriteSubHeader("Real audit logger (database-backed)");
    var realLogger = new DatabaseAuditLogger();
    var serviceWithAudit = new NullObjectOrderService(realLogger);

    var orderId = await serviceWithAudit.PlaceOrderAsync("customer-42", 199.99m);
    ConsoleHelper.WriteSuccess($"  Order placed: {orderId}");

    var logs = await realLogger.GetLogsAsync();
    ConsoleHelper.WriteInfo($"  Audit log entries: {logs.Count}");
    foreach (var log in logs)
        ConsoleHelper.WriteInfo($"    [{log.Action}] {log.EntityType}/{log.EntityId} - {log.Details}");

    ConsoleHelper.WriteSubHeader("Null Object logger (auditing disabled)");
    var nullLogger = NullAuditLogger.Instance;
    var serviceNoAudit = new NullObjectOrderService(nullLogger);

    var orderId2 = await serviceNoAudit.PlaceOrderAsync("customer-99", 49.99m);
    ConsoleHelper.WriteSuccess($"  Order placed: {orderId2}");
    ConsoleHelper.WriteInfo($"  Logger enabled? {nullLogger.IsEnabled}");
    ConsoleHelper.WriteInfo("  No null checks needed - NullAuditLogger silently absorbs all calls.");
}

Task DemoValueObject()
{
    ConsoleHelper.WriteSubHeader("Money - Currency-safe arithmetic");
    var price = Money.USD(29.99m);
    var shipping = Money.USD(5.99m);
    var total = price + shipping;
    var discounted = total * 0.9m;

    ConsoleHelper.WriteInfo($"  Price: {price}");
    ConsoleHelper.WriteInfo($"  Shipping: {shipping}");
    ConsoleHelper.WriteInfo($"  Total: {total}");
    ConsoleHelper.WriteInfo($"  10% off: {discounted}");
    ConsoleHelper.WriteInfo($"  price == Money.USD(29.99)? {price == Money.USD(29.99m)}");

    try
    {
        var euros = Money.EUR(10m);
        var invalid = price + euros;
    }
    catch (InvalidOperationException ex)
    {
        ConsoleHelper.WriteWarning($"  Currency mismatch: {ex.Message}");
    }

    ConsoleHelper.WriteSubHeader("Address - Immutable value object");
    var addr1 = new Address("123 Main St", "Seattle", "WA", "98101", "US");
    var addr2 = new Address("123 Main St", "Seattle", "WA", "98101", "US");
    var addr3 = addr1.WithCity("Portland");

    ConsoleHelper.WriteInfo($"  Address 1: {addr1}");
    ConsoleHelper.WriteInfo($"  Address 2: {addr2}");
    ConsoleHelper.WriteInfo($"  addr1 == addr2? {addr1 == addr2}");
    ConsoleHelper.WriteInfo($"  Modified (new city): {addr3}");
    ConsoleHelper.WriteInfo($"  addr1 == addr3? {addr1 == addr3}");

    ConsoleHelper.WriteSubHeader("DateRange - Overlap and intersection");
    var q1 = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31));
    var feb = new DateRange(new DateTime(2024, 2, 1), new DateTime(2024, 2, 29));
    var q2 = new DateRange(new DateTime(2024, 4, 1), new DateTime(2024, 6, 30));

    ConsoleHelper.WriteInfo($"  Q1: {q1}");
    ConsoleHelper.WriteInfo($"  Feb: {feb}");
    ConsoleHelper.WriteInfo($"  Q2: {q2}");
    ConsoleHelper.WriteInfo($"  Q1 overlaps Feb? {q1.Overlaps(feb)}");
    ConsoleHelper.WriteInfo($"  Q1 overlaps Q2? {q1.Overlaps(q2)}");
    var intersection = q1.Intersect(feb);
    ConsoleHelper.WriteInfo($"  Q1 intersect Feb: {intersection}");
    ConsoleHelper.WriteInfo($"  Feb contains 2024-02-15? {feb.Contains(new DateTime(2024, 2, 15))}");

    return Task.CompletedTask;
}

async Task DemoSaga()
{
    ConsoleHelper.WriteSubHeader("Saga: Successful order processing");

    var validateStep = new ValidateOrderStep();
    var reserveStep = new ReserveInventoryStep();
    var paymentStep = new ProcessPaymentStep();
    var shippingStep = new ArrangeShippingStep();

    var saga = new SagaOrchestrator()
        .AddStep(validateStep)
        .AddStep(reserveStep)
        .AddStep(paymentStep)
        .AddStep(shippingStep);

    var context = new SagaContext();
    context.Set("OrderId", "ORD-2024-001");
    context.Set("TotalAmount", 299.99m);
    context.Set("ItemCount", 3);

    var result = await saga.ExecuteAsync(context);
    ConsoleHelper.WriteInfo($"  Success: {result.Success}");
    ConsoleHelper.WriteInfo($"  Completed steps: {string.Join(" -> ", result.CompletedSteps)}");
    if (result.Success)
    {
        ConsoleHelper.WriteSuccess($"  Tracking: {result.Context.Get<string>("TrackingNumber")}");
        ConsoleHelper.WriteSuccess($"  Payment: {result.Context.Get<string>("PaymentTransactionId")}");
    }

    ConsoleHelper.WriteSubHeader("Saga: Payment failure with compensation");

    var validateStep2 = new ValidateOrderStep();
    var reserveStep2 = new ReserveInventoryStep();
    var paymentStep2 = new ProcessPaymentStep { ShouldFail = true };
    var shippingStep2 = new ArrangeShippingStep();

    var saga2 = new SagaOrchestrator()
        .AddStep(validateStep2)
        .AddStep(reserveStep2)
        .AddStep(paymentStep2)
        .AddStep(shippingStep2);

    var context2 = new SagaContext();
    context2.Set("OrderId", "ORD-2024-002");
    context2.Set("TotalAmount", 599.99m);
    context2.Set("ItemCount", 2);

    var result2 = await saga2.ExecuteAsync(context2);
    ConsoleHelper.WriteError($"  Failed at: {result2.FailedStep}");
    ConsoleHelper.WriteError($"  Error: {result2.ErrorMessage}");
    ConsoleHelper.WriteInfo($"  Completed steps: {string.Join(", ", result2.CompletedSteps)}");
    ConsoleHelper.WriteInfo($"  Compensated steps: {string.Join(", ", result2.CompensatedSteps)}");
    ConsoleHelper.WriteInfo($"  Inventory reserved? {reserveStep2.WasCompensated}");
}

async Task DemoPolicy()
{
    ConsoleHelper.WriteSubHeader("Retry Policy");

    var attemptCount = 0;
    var retryPolicy = new RetryPolicy(maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(50));

    var retryResult = await retryPolicy.ExecuteAsync<string>(async ct =>
    {
        attemptCount++;
        if (attemptCount < 3)
            throw new HttpRequestException($"Simulated failure (attempt {attemptCount})");
        return "Success after retries!";
    });

    ConsoleHelper.WriteSuccess($"  Result: {retryResult}");
    ConsoleHelper.WriteInfo($"  Total attempts: {retryPolicy.TotalAttempts}, Retries: {retryPolicy.TotalRetries}");

    ConsoleHelper.WriteSubHeader("Circuit Breaker Policy");
    var circuitBreaker = new CircuitBreakerPolicy(failureThreshold: 2, breakDuration: TimeSpan.FromSeconds(1));
    ConsoleHelper.WriteInfo($"  Initial state: {circuitBreaker.State}");

    for (var i = 0; i < 3; i++)
    {
        try
        {
            await circuitBreaker.ExecuteAsync<string>(ct =>
                throw new InvalidOperationException("Service unavailable"));
        }
        catch (Exception ex)
        {
            var exType = ex is CircuitBrokenException ? "CIRCUIT OPEN" : "Service error";
            ConsoleHelper.WriteWarning($"  Attempt {i + 1}: {exType} - State: {circuitBreaker.State}");
        }
    }

    ConsoleHelper.WriteSubHeader("Policy Pipeline (Timeout + Retry)");
    var pipeline = new PolicyPipeline()
        .Add(new TimeoutPolicy(TimeSpan.FromSeconds(5)))
        .Add(new RetryPolicy(maxRetries: 2, initialDelay: TimeSpan.FromMilliseconds(10)));

    ConsoleHelper.WriteInfo($"  Pipeline: {pipeline.Name}");

    var pipelineResult = await pipeline.ExecuteAsync<string>(async ct =>
    {
        await Task.Delay(10, ct);
        return "Pipeline executed successfully!";
    });
    ConsoleHelper.WriteSuccess($"  Result: {pipelineResult}");
}

// ─────────────────────────────────────────────────────────────────────────────
// Helper types
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Simple entity for Unit of Work demos.</summary>
class SampleEntity : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

/// <summary>In-memory message publisher for Outbox demo.</summary>
class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly List<(string EventType, string Payload)> _published = [];
    public IReadOnlyList<(string EventType, string Payload)> Published => _published.AsReadOnly();

    public Task PublishAsync(string eventType, string payload, CancellationToken ct = default)
    {
        _published.Add((eventType, payload));
        return Task.CompletedTask;
    }
}
