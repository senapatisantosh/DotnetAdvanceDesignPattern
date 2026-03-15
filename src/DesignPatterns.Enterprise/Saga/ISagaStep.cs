namespace DesignPatterns.Enterprise.Saga;

/// <summary>
/// A single step in a saga — an operation that can be executed and compensated.
/// If a later step fails, all previously completed steps are compensated in reverse order.
/// </summary>
public interface ISagaStep
{
    string Name { get; }
    Task<SagaStepResult> ExecuteAsync(SagaContext context, CancellationToken ct = default);
    Task CompensateAsync(SagaContext context, CancellationToken ct = default);
}

/// <summary>
/// Shared mutable context that saga steps use to pass data between each other.
/// </summary>
public sealed class SagaContext
{
    private readonly Dictionary<string, object> _data = new();

    public void Set<T>(string key, T value) where T : notnull => _data[key] = value;

    public T Get<T>(string key) =>
        _data.TryGetValue(key, out var value) ? (T)value
        : throw new KeyNotFoundException($"Saga context key '{key}' not found.");

    public bool TryGet<T>(string key, out T? value)
    {
        if (_data.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }
}

public sealed record SagaStepResult(bool Success, string? ErrorMessage = null)
{
    public static SagaStepResult Ok() => new(true);
    public static SagaStepResult Fail(string error) => new(false, error);
}
