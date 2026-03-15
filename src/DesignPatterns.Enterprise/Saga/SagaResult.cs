namespace DesignPatterns.Enterprise.Saga;

/// <summary>
/// Overall result of a saga execution.
/// </summary>
public sealed record SagaResult
{
    public bool Success { get; init; }
    public string? FailedStep { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> CompletedSteps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CompensatedSteps { get; init; } = Array.Empty<string>();
    public SagaContext Context { get; init; } = new();

    public static SagaResult Succeeded(IReadOnlyList<string> completedSteps, SagaContext context) =>
        new()
        {
            Success = true,
            CompletedSteps = completedSteps,
            Context = context
        };

    public static SagaResult Failed(
        string failedStep,
        string error,
        IReadOnlyList<string> completedSteps,
        IReadOnlyList<string> compensatedSteps,
        SagaContext context) =>
        new()
        {
            Success = false,
            FailedStep = failedStep,
            ErrorMessage = error,
            CompletedSteps = completedSteps,
            CompensatedSteps = compensatedSteps,
            Context = context
        };
}
