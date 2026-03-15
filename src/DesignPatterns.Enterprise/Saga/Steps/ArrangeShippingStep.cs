namespace DesignPatterns.Enterprise.Saga.Steps;

/// <summary>
/// Step 4: Creates a shipping label and schedules pickup.
/// Compensation cancels the shipment.
/// </summary>
public sealed class ArrangeShippingStep : ISagaStep
{
    public string Name => "ArrangeShipping";

    public bool WasExecuted { get; private set; }
    public bool WasCompensated { get; private set; }
    public bool ShouldFail { get; set; }

    public Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default)
    {
        if (ShouldFail)
            return Task.FromResult(SagaStepResult.Fail("No shipping carriers available."));

        WasExecuted = true;
        context.Set("TrackingNumber", $"TRACK-{Guid.NewGuid():N}"[..18]);
        context.Set("ShippingArranged", true);
        return Task.FromResult(SagaStepResult.Ok());
    }

    public Task CompensateAsync(SagaContext context, CancellationToken ct = default)
    {
        WasCompensated = true;
        context.Set("ShippingArranged", false);
        return Task.CompletedTask;
    }
}
