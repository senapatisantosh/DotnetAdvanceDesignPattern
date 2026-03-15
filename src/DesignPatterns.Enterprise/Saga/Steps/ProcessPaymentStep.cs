namespace DesignPatterns.Enterprise.Saga.Steps;

/// <summary>
/// Step 3: Charges the customer's payment method.
/// Compensation issues a refund.
/// </summary>
public sealed class ProcessPaymentStep : ISagaStep
{
    public string Name => "ProcessPayment";

    public bool WasExecuted { get; private set; }
    public bool WasCompensated { get; private set; }
    public bool ShouldFail { get; set; }

    public Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default)
    {
        if (ShouldFail)
            return Task.FromResult(SagaStepResult.Fail("Payment declined."));

        var amount = context.Get<decimal>("TotalAmount");
        WasExecuted = true;
        context.Set("PaymentTransactionId", $"PAY-{Guid.NewGuid():N}"[..16]);
        context.Set("AmountCharged", amount);
        return Task.FromResult(SagaStepResult.Ok());
    }

    public Task CompensateAsync(SagaContext context, CancellationToken ct = default)
    {
        WasCompensated = true;
        context.Set("PaymentRefunded", true);
        return Task.CompletedTask;
    }
}
