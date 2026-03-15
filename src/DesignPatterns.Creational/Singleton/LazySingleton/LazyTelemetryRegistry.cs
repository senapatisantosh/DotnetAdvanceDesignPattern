using System.Collections.Concurrent;

namespace DesignPatterns.Creational.Singleton.LazySingleton;

/// <summary>
/// Lazy Singleton — uses Lazy&lt;T&gt; for thread-safe lazy initialization.
///
/// The instance is NOT created until the first time Instance is accessed.
/// Lazy&lt;T&gt; handles all the thread-safety internally using LazyThreadSafetyMode.
///
/// This is the recommended singleton approach in modern C# when you need
/// a singleton (but first consider whether DI singleton scope would be better).
/// </summary>
public sealed class LazyTelemetryRegistry
{
    private static readonly Lazy<LazyTelemetryRegistry> _lazy =
        new(() => new LazyTelemetryRegistry(), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, double> _gauges = new();
    private readonly ConcurrentBag<TelemetryEvent> _events = [];

    private LazyTelemetryRegistry() { }

    /// <summary>
    /// The single instance, created lazily on first access.
    /// </summary>
    public static LazyTelemetryRegistry Instance => _lazy.Value;

    /// <summary>
    /// Whether the singleton has been created yet. Useful for diagnostics.
    /// </summary>
    public static bool IsCreated => _lazy.IsValueCreated;

    public void IncrementCounter(string metricName, long amount = 1)
    {
        _counters.AddOrUpdate(metricName, amount, (_, current) => current + amount);
    }

    public void SetGauge(string metricName, double value)
    {
        _gauges[metricName] = value;
    }

    public void RecordEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        _events.Add(new TelemetryEvent
        {
            EventName = eventName,
            Timestamp = DateTimeOffset.UtcNow,
            Properties = properties ?? new Dictionary<string, string>()
        });
    }

    public long GetCounter(string metricName) => _counters.GetValueOrDefault(metricName, 0);
    public double GetGauge(string metricName) => _gauges.GetValueOrDefault(metricName, 0.0);
    public IReadOnlyList<TelemetryEvent> GetEvents() => _events.ToList();

    public void Reset()
    {
        _counters.Clear();
        _gauges.Clear();
        while (_events.TryTake(out _)) { }
    }
}

/// <summary>
/// Represents a telemetry event with a timestamp and properties.
/// </summary>
public sealed record TelemetryEvent
{
    public required string EventName { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();
}
