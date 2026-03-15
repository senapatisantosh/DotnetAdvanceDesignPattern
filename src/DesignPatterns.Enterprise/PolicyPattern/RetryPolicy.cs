namespace DesignPatterns.Enterprise.PolicyPattern;

/// <summary>
/// Retries a failed operation up to <paramref name="maxRetries"/> times with exponential backoff.
/// Optionally filters which exceptions trigger a retry.
/// </summary>
public sealed class RetryPolicy(
    int maxRetries = 3,
    TimeSpan? initialDelay = null,
    Func<Exception, bool>? shouldRetry = null) : IResiliencePolicy
{
    private readonly TimeSpan _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(200);

    public string Name => $"Retry(max={maxRetries})";

    public int TotalAttempts { get; private set; }
    public int TotalRetries { get; private set; }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                attempt++;
                TotalAttempts++;
                return await operation(ct);
            }
            catch (Exception ex) when (attempt <= maxRetries && (shouldRetry?.Invoke(ex) ?? true))
            {
                TotalRetries++;
                var delay = _initialDelay * Math.Pow(2, attempt - 1);
                await Task.Delay(delay, ct);
            }
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
