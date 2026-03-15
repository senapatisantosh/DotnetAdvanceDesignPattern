namespace DesignPatterns.Enterprise.PolicyPattern;

/// <summary>
/// Circuit breaker that opens after <paramref name="failureThreshold"/> consecutive failures,
/// preventing calls for <paramref name="breakDuration"/>, then allows a single probe call.
///
/// States: Closed (normal) -> Open (blocking) -> HalfOpen (probe) -> Closed or Open.
/// </summary>
public sealed class CircuitBreakerPolicy(
    int failureThreshold = 3,
    TimeSpan? breakDuration = null) : IResiliencePolicy
{
    private readonly TimeSpan _breakDuration = breakDuration ?? TimeSpan.FromSeconds(30);
    private readonly object _lock = new();

    private int _consecutiveFailures;
    private DateTime _openedAt;

    public string Name => $"CircuitBreaker(threshold={failureThreshold})";
    public CircuitState State { get; private set; } = CircuitState.Closed;

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        EnsureNotOpen();

        try
        {
            var result = await operation(ct);
            OnSuccess();
            return result;
        }
        catch (Exception)
        {
            OnFailure();
            throw;
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

    /// <summary>Manually resets the circuit to Closed.</summary>
    public void Reset()
    {
        lock (_lock)
        {
            State = CircuitState.Closed;
            _consecutiveFailures = 0;
        }
    }

    private void EnsureNotOpen()
    {
        lock (_lock)
        {
            if (State == CircuitState.Open)
            {
                if (DateTime.UtcNow - _openedAt >= _breakDuration)
                {
                    State = CircuitState.HalfOpen;
                }
                else
                {
                    throw new CircuitBrokenException(
                        $"Circuit is open. Retry after {_breakDuration - (DateTime.UtcNow - _openedAt)}.");
                }
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;
            State = CircuitState.Closed;
        }
    }

    private void OnFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= failureThreshold)
            {
                State = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
            }
        }
    }
}

public enum CircuitState { Closed, Open, HalfOpen }

public sealed class CircuitBrokenException(string message) : Exception(message);
