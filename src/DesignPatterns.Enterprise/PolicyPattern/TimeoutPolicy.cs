namespace DesignPatterns.Enterprise.PolicyPattern;

/// <summary>
/// Cancels an operation if it exceeds the specified timeout.
/// </summary>
public sealed class TimeoutPolicy(TimeSpan timeout) : IResiliencePolicy
{
    public string Name => $"Timeout({timeout.TotalSeconds}s)";

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            return await operation(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds}s.");
        }
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
