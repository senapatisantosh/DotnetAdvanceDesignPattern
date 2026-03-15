using System.Collections.Concurrent;

namespace DesignPatterns.Creational.Singleton;

/// <summary>
/// Classic Singleton — eagerly initialized via a static readonly field.
///
/// The CLR guarantees that static field initializers run exactly once,
/// in a thread-safe manner, before the field is first accessed.
///
/// This is the simplest correct singleton in C#, but it has drawbacks:
/// - Hard to test (global state, can't substitute)
/// - Violates Dependency Inversion (consumers depend on concrete class)
/// - Instance is created even if never used
/// </summary>
public sealed class TelemetryRegistry
{
    private static readonly TelemetryRegistry _instance = new();

    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, double> _gauges = new();

    // Private constructor prevents external instantiation.
    private TelemetryRegistry() { }

    /// <summary>
    /// The single global instance.
    /// </summary>
    public static TelemetryRegistry Instance => _instance;

    /// <summary>
    /// Increments a named counter by the specified amount.
    /// </summary>
    public void IncrementCounter(string metricName, long amount = 1)
    {
        _counters.AddOrUpdate(metricName, amount, (_, current) => current + amount);
    }

    /// <summary>
    /// Sets a gauge to a specific value (e.g., current CPU usage, queue depth).
    /// </summary>
    public void SetGauge(string metricName, double value)
    {
        _gauges[metricName] = value;
    }

    /// <summary>
    /// Gets the current value of a counter.
    /// </summary>
    public long GetCounter(string metricName)
    {
        return _counters.GetValueOrDefault(metricName, 0);
    }

    /// <summary>
    /// Gets the current value of a gauge.
    /// </summary>
    public double GetGauge(string metricName)
    {
        return _gauges.GetValueOrDefault(metricName, 0.0);
    }

    /// <summary>
    /// Returns all registered metric names.
    /// </summary>
    public IReadOnlyCollection<string> CounterNames => _counters.Keys;
    public IReadOnlyCollection<string> GaugeNames => _gauges.Keys;

    /// <summary>
    /// Returns a snapshot of all counters.
    /// </summary>
    public IReadOnlyDictionary<string, long> GetCounterSnapshot()
    {
        return new Dictionary<string, long>(_counters);
    }

    /// <summary>
    /// Resets all metrics. Useful for testing.
    /// </summary>
    public void Reset()
    {
        _counters.Clear();
        _gauges.Clear();
    }
}
