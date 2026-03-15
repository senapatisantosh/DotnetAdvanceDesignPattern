using DesignPatterns.Enterprise.Saga;
using DesignPatterns.Enterprise.Saga.Steps;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class SagaTests
{
    private static SagaContext CreateOrderContext(string orderId = "ORD-1", decimal amount = 99.99m, int items = 2)
    {
        var ctx = new SagaContext();
        ctx.Set("OrderId", orderId);
        ctx.Set("TotalAmount", amount);
        ctx.Set("ItemCount", items);
        return ctx;
    }

    [Fact]
    public async Task Saga_succeeds_when_all_steps_pass()
    {
        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(new ReserveInventoryStep())
            .AddStep(new ProcessPaymentStep())
            .AddStep(new ArrangeShippingStep());

        var result = await saga.ExecuteAsync(CreateOrderContext());

        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().HaveCount(4);
        result.Context.Get<bool>("ShippingArranged").Should().BeTrue();
    }

    [Fact]
    public async Task Saga_fails_and_compensates_when_payment_fails()
    {
        var inventoryStep = new ReserveInventoryStep();
        var paymentStep = new ProcessPaymentStep { ShouldFail = true };

        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(inventoryStep)
            .AddStep(paymentStep)
            .AddStep(new ArrangeShippingStep());

        var result = await saga.ExecuteAsync(CreateOrderContext());

        result.Success.Should().BeFalse();
        result.FailedStep.Should().Be("ProcessPayment");
        result.ErrorMessage.Should().Be("Payment declined.");
        result.CompletedSteps.Should().Contain("ValidateOrder");
        result.CompletedSteps.Should().Contain("ReserveInventory");
        result.CompensatedSteps.Should().Contain("ReserveInventory");
        inventoryStep.WasCompensated.Should().BeTrue();
    }

    [Fact]
    public async Task Saga_fails_at_first_step()
    {
        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(new ReserveInventoryStep());

        var ctx = new SagaContext();
        ctx.Set("OrderId", "");
        ctx.Set("TotalAmount", 0m);
        ctx.Set("ItemCount", 0);

        var result = await saga.ExecuteAsync(ctx);

        result.Success.Should().BeFalse();
        result.FailedStep.Should().Be("ValidateOrder");
        result.CompensatedSteps.Should().BeEmpty(); // nothing to compensate
    }

    [Fact]
    public async Task Saga_compensates_in_reverse_order()
    {
        var inventory = new ReserveInventoryStep();
        var payment = new ProcessPaymentStep();
        var shipping = new ArrangeShippingStep { ShouldFail = true };

        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(inventory)
            .AddStep(payment)
            .AddStep(shipping);

        var result = await saga.ExecuteAsync(CreateOrderContext());

        result.Success.Should().BeFalse();
        result.CompensatedSteps.Should().HaveCount(3);
        // Compensation order: ProcessPayment, ReserveInventory, ValidateOrder
        result.CompensatedSteps[0].Should().Be("ProcessPayment");
        result.CompensatedSteps[1].Should().Be("ReserveInventory");
        payment.WasCompensated.Should().BeTrue();
        inventory.WasCompensated.Should().BeTrue();
    }

    [Fact]
    public async Task Saga_sets_context_data_across_steps()
    {
        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(new ReserveInventoryStep())
            .AddStep(new ProcessPaymentStep())
            .AddStep(new ArrangeShippingStep());

        var result = await saga.ExecuteAsync(CreateOrderContext());

        result.Context.Get<bool>("OrderValidated").Should().BeTrue();
        result.Context.Get<bool>("InventoryReserved").Should().BeTrue();
        result.Context.Get<string>("PaymentTransactionId").Should().NotBeNullOrEmpty();
        result.Context.Get<string>("TrackingNumber").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Saga_compensates_inventory_when_shipping_fails()
    {
        var inventory = new ReserveInventoryStep();
        var shipping = new ArrangeShippingStep { ShouldFail = true };

        var saga = new SagaOrchestrator()
            .AddStep(new ValidateOrderStep())
            .AddStep(inventory)
            .AddStep(new ProcessPaymentStep())
            .AddStep(shipping);

        var result = await saga.ExecuteAsync(CreateOrderContext());

        result.Success.Should().BeFalse();
        result.Context.Get<bool>("InventoryReserved").Should().BeFalse(); // compensated
        result.Context.Get<bool>("PaymentRefunded").Should().BeTrue(); // compensated
    }

    [Fact]
    public void SagaContext_get_missing_key_throws()
    {
        var ctx = new SagaContext();

        var act = () => ctx.Get<string>("missing");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void SagaContext_try_get_returns_false_for_missing()
    {
        var ctx = new SagaContext();

        ctx.TryGet<string>("missing", out var value).Should().BeFalse();
        value.Should().BeNull();
    }
}
