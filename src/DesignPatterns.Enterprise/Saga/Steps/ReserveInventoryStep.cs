namespace DesignPatterns.Enterprise.Saga.Steps;

/// <summary>
/// Step 2: Reserves inventory for the order items.
/// Compensation releases the reserved inventory.
/// </summary>
public sealed class ReserveInventoryStep : ISagaStep
{
    public string Name => "ReserveInventory";

    public bool WasExecuted { get; private set; }
    public bool WasCompensated { get; private set; }
    public bool ShouldFail { get; set; }

    public Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default)
    {
        if (ShouldFail)
            return Task.FromResult(SagaStepResult.Fail("Insufficient inventory."));

        WasExecuted = true;
        context.Set("InventoryReserved", true);
        context.Set("ReservationId", Guid.NewGuid().ToString());
        return Task.FromResult(SagaStepResult.Ok());
    }

    public Task CompensateAsync(SagaContext context, CancellationToken ct = default)
    {
        WasCompensated = true;
        context.Set("InventoryReserved", false);
        return Task.CompletedTask;
    }
}
