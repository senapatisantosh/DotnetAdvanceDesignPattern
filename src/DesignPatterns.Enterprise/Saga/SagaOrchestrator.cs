namespace DesignPatterns.Enterprise.Saga;

/// <summary>
/// Orchestrator that executes saga steps in order and compensates on failure.
/// Steps run sequentially; if step N fails, steps N-1 through 0 are compensated in reverse.
/// </summary>
public sealed class SagaOrchestrator
{
    private readonly List<ISagaStep> _steps = new();

    public SagaOrchestrator AddStep(ISagaStep step)
    {
        _steps.Add(step);
        return this;
    }

    public async Task<SagaResult> ExecuteAsync(SagaContext? context = null, CancellationToken ct = default)
    {
        context ??= new SagaContext();
        var completedSteps = new List<string>();

        for (var i = 0; i < _steps.Count; i++)
        {
            var step = _steps[i];
            SagaStepResult result;

            try
            {
                result = await step.ExecuteAsync(context, ct);
            }
            catch (Exception ex)
            {
                result = SagaStepResult.Fail(ex.Message);
            }

            if (!result.Success)
            {
                // Compensate all previously completed steps in reverse order
                var compensated = await CompensateAsync(completedSteps, context, ct);
                return SagaResult.Failed(step.Name, result.ErrorMessage ?? "Unknown error",
                    completedSteps.AsReadOnly(), compensated, context);
            }

            completedSteps.Add(step.Name);
        }

        return SagaResult.Succeeded(completedSteps.AsReadOnly(), context);
    }

    private async Task<IReadOnlyList<string>> CompensateAsync(
        List<string> completedStepNames, SagaContext context, CancellationToken ct)
    {
        var compensated = new List<string>();

        // Walk completed steps in reverse
        for (var i = completedStepNames.Count - 1; i >= 0; i--)
        {
            var stepName = completedStepNames[i];
            var step = _steps.First(s => s.Name == stepName);

            try
            {
                await step.CompensateAsync(context, ct);
                compensated.Add(stepName);
            }
            catch
            {
                // Log but continue compensating remaining steps
                compensated.Add($"{stepName} (compensation failed)");
            }
        }

        return compensated.AsReadOnly();
    }
}
