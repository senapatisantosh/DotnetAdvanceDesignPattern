using DesignPatterns.Behavioral.Mediator;
using DesignPatterns.Behavioral.Mediator.Colleagues;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class MediatorTests
{
    private static CheckoutRequest CreateCheckoutRequest() => new()
    {
        OrderId = Guid.NewGuid(),
        CustomerId = "CUST-001",
        Items =
        [
            new CheckoutItem { ProductId = "PROD-1", ProductName = "Widget", Quantity = 2, UnitPrice = 25.00m },
            new CheckoutItem { ProductId = "PROD-2", ProductName = "Gadget", Quantity = 1, UnitPrice = 50.00m }
        ],
        ShippingAddress = "123 Main St, Anytown, USA",
        PaymentMethod = "CreditCard",
        TotalAmount = 100.00m
    };

    private static Dictionary<string, int> CreateStock() => new()
    {
        ["PROD-1"] = 10,
        ["PROD-2"] = 5
    };

    [Fact]
    public async Task SuccessfulCheckout_CompletesAllSteps()
    {
        var inventory = new InventoryColleague(CreateStock());
        var payment = new PaymentColleague();
        var shipping = new ShippingColleague();
        var notification = new NotificationColleague();
        var mediator = new CheckoutMediator(inventory, payment, shipping, notification);

        var result = await mediator.ProcessCheckoutAsync(CreateCheckoutRequest());

        result.Success.Should().BeTrue();
        result.TrackingNumber.Should().NotBeNullOrEmpty();
        result.PaymentTransactionId.Should().NotBeNullOrEmpty();
        result.Steps.Should().HaveCountGreaterThan(4);
        payment.ProcessedPayments.Should().HaveCount(1);
        shipping.Shipments.Should().HaveCount(1);
        notification.SentNotifications.Should().ContainSingle(n => n.Type == "OrderConfirmation");
    }

    [Fact]
    public async Task InsufficientInventory_FailsAndNotifiesCustomer()
    {
        var inventory = new InventoryColleague(new Dictionary<string, int> { ["PROD-1"] = 1, ["PROD-2"] = 0 });
        var payment = new PaymentColleague();
        var shipping = new ShippingColleague();
        var notification = new NotificationColleague();
        var mediator = new CheckoutMediator(inventory, payment, shipping, notification);

        var result = await mediator.ProcessCheckoutAsync(CreateCheckoutRequest());

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Insufficient stock");
        payment.ProcessedPayments.Should().BeEmpty();
        shipping.Shipments.Should().BeEmpty();
        notification.SentNotifications.Should().ContainSingle(n => n.Type == "InventoryFailure");
    }

    [Fact]
    public async Task PaymentFailure_ReleasesInventoryAndNotifies()
    {
        var inventory = new InventoryColleague(CreateStock());
        var payment = new PaymentColleague(shouldFail: true);
        var shipping = new ShippingColleague();
        var notification = new NotificationColleague();
        var mediator = new CheckoutMediator(inventory, payment, shipping, notification);

        var result = await mediator.ProcessCheckoutAsync(CreateCheckoutRequest());

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("declined");
        shipping.Shipments.Should().BeEmpty();
        notification.SentNotifications.Should().ContainSingle(n => n.Type == "PaymentFailure");
    }

    [Fact]
    public async Task SuccessfulCheckout_RecordsAllStepsInOrder()
    {
        var inventory = new InventoryColleague(CreateStock());
        var payment = new PaymentColleague();
        var shipping = new ShippingColleague();
        var notification = new NotificationColleague();
        var mediator = new CheckoutMediator(inventory, payment, shipping, notification);

        var result = await mediator.ProcessCheckoutAsync(CreateCheckoutRequest());

        result.Steps.Should().ContainInConsecutiveOrder(
            result.Steps.First(s => s.Contains("[Inventory]")),
            result.Steps.First(s => s.Contains("[Payment]")),
            result.Steps.First(s => s.Contains("[Shipping]")),
            result.Steps.First(s => s.Contains("[Notification]"))
        );
    }
}
