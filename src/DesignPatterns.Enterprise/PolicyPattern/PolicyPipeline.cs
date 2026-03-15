namespace DesignPatterns.Enterprise.PolicyPattern;

/// <summary>
/// Composes multiple resilience policies into a pipeline.
/// Policies execute from outermost to innermost (first added = outermost).
/// Example: Timeout -> Retry -> CircuitBreaker wraps the operation.
/// </summary>
public sealed class PolicyPipeline : IResiliencePolicy
{
    private readonly List<IResiliencePolicy> _policies = new();

    public string Name => $"Pipeline({string.Join(" -> ", _policies.Select(p => p.Name))})";

    public PolicyPipeline Add(IResiliencePolicy policy)
    {
        _policies.Add(policy);
        return this;
    }

    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        // Build the chain from innermost to outermost
        var chain = operation;

        for (var i = _policies.Count - 1; i >= 0; i--)
        {
            var policy = _policies[i];
            var next = chain; // capture for closure
            chain = token => policy.ExecuteAsync(next, token);
        }

        return chain(ct);
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await ExecuteAsync<object?>(async token =>
        {
            await operation(token);
            return null;
        }, ct);
    }
}
