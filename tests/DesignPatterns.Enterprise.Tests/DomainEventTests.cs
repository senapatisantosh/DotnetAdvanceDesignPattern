using DesignPatterns.Enterprise.DomainEvents;
using DesignPatterns.Enterprise.DomainEvents.Events;
using DesignPatterns.Enterprise.DomainEvents.Handlers;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class DomainEventTests
{
    [Fact]
    public async Task Dispatcher_invokes_registered_handler()
    {
        var dispatcher = new DomainEventDispatcher();
        var emailHandler = new OrderPlacedEmailHandler();
        dispatcher.Register(emailHandler);

        var evt = new OrderPlacedEvent(
            Guid.NewGuid(), "CUST-1",
            new[] { new OrderLineItem("SKU-1", "Widget", 2, 10m) },
            20m);

        var count = await dispatcher.DispatchAsync(evt);

        count.Should().Be(1);
        emailHandler.SentEmails.Should().HaveCount(1);
    }

    [Fact]
    public async Task Dispatcher_invokes_multiple_handlers_for_same_event()
    {
        var dispatcher = new DomainEventDispatcher();
        var emailHandler = new OrderPlacedEmailHandler();
        var inventoryHandler = new OrderPlacedInventoryHandler();
        dispatcher.Register(emailHandler);
        dispatcher.Register(inventoryHandler);

        var evt = new OrderPlacedEvent(
            Guid.NewGuid(), "CUST-1",
            new[]
            {
                new OrderLineItem("SKU-1", "Widget", 2, 10m),
                new OrderLineItem("SKU-2", "Gadget", 1, 30m)
            },
            50m);

        var count = await dispatcher.DispatchAsync(evt);

        count.Should().Be(2);
        emailHandler.SentEmails.Should().HaveCount(1);
        inventoryHandler.Reservations.Should().HaveCount(2);
    }

    [Fact]
    public async Task Dispatcher_returns_zero_for_unregistered_event()
    {
        var dispatcher = new DomainEventDispatcher();

        var evt = new OrderShippedEvent(Guid.NewGuid(), "TRACK-123", "FedEx", DateTime.UtcNow.AddDays(3));

        var count = await dispatcher.DispatchAsync(evt);

        count.Should().Be(0);
    }

    [Fact]
    public async Task Payment_received_creates_audit_entry()
    {
        var dispatcher = new DomainEventDispatcher();
        var auditHandler = new PaymentReceivedAuditHandler();
        dispatcher.Register(auditHandler);

        var evt = new PaymentReceivedEvent(
            Guid.NewGuid(), 99.99m, "USD", "CreditCard", "TXN-12345");

        await dispatcher.DispatchAsync(evt);

        auditHandler.AuditLog.Should().HaveCount(1);
        auditHandler.AuditLog[0].Description.Should().Contain("99.99");
        auditHandler.AuditLog[0].Description.Should().Contain("TXN-12345");
    }

    [Fact]
    public async Task DispatchAll_processes_multiple_events_in_order()
    {
        var dispatcher = new DomainEventDispatcher();
        var emailHandler = new OrderPlacedEmailHandler();
        var auditHandler = new PaymentReceivedAuditHandler();
        dispatcher.Register(emailHandler);
        dispatcher.Register(auditHandler);

        var events = new IDomainEvent[]
        {
            new OrderPlacedEvent(Guid.NewGuid(), "C1",
                new[] { new OrderLineItem("S1", "W", 1, 10m) }, 10m),
            new PaymentReceivedEvent(Guid.NewGuid(), 10m, "USD", "Card", "TX1"),
            new OrderPlacedEvent(Guid.NewGuid(), "C2",
                new[] { new OrderLineItem("S2", "G", 1, 20m) }, 20m)
        };

        var total = await dispatcher.DispatchAllAsync(events);

        total.Should().Be(3); // 2 order placed + 1 payment
        emailHandler.SentEmails.Should().HaveCount(2);
        auditHandler.AuditLog.Should().HaveCount(1);
    }

    [Fact]
    public void Events_have_unique_ids_and_timestamps()
    {
        var e1 = new OrderPlacedEvent(Guid.NewGuid(), "C1", Array.Empty<OrderLineItem>(), 0m);
        var e2 = new OrderPlacedEvent(Guid.NewGuid(), "C1", Array.Empty<OrderLineItem>(), 0m);

        e1.EventId.Should().NotBe(e2.EventId);
        e1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
