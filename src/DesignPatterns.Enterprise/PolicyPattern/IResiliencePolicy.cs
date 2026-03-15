namespace DesignPatterns.Enterprise.PolicyPattern;

/// <summary>
/// A resilience policy that wraps an operation and applies a strategy
/// (retry, circuit breaker, timeout, etc.).
/// </summary>
public interface IResiliencePolicy
{
    string Name { get; }
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default);
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
