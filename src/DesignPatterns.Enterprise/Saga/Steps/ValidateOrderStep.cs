namespace DesignPatterns.Enterprise.Saga.Steps;

/// <summary>
/// Step 1: Validates the order details (items exist, quantities are positive, etc.).
/// No compensation needed because validation is a read-only operation.
/// </summary>
public sealed class ValidateOrderStep : ISagaStep
{
    public string Name => "ValidateOrder";

    public Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default)
    {
        var orderId = context.Get<string>("OrderId");
        var amount = context.Get<decimal>("TotalAmount");
        var itemCount = context.Get<int>("ItemCount");

        if (string.IsNullOrWhiteSpace(orderId))
            return Task.FromResult(SagaStepResult.Fail("Order ID is required."));

        if (amount <= 0)
            return Task.FromResult(SagaStepResult.Fail("Order total must be positive."));

        if (itemCount <= 0)
            return Task.FromResult(SagaStepResult.Fail("Order must contain at least one item."));

        context.Set("OrderValidated", true);
        return Task.FromResult(SagaStepResult.Ok());
    }

    public Task CompensateAsync(SagaContext context, CancellationToken ct = default)
    {
        // Nothing to undo — validation is side-effect free
        return Task.CompletedTask;
    }
}
