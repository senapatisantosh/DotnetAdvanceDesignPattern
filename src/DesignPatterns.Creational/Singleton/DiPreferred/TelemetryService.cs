using System.Collections.Concurrent;

namespace DesignPatterns.Creational.Singleton.DiPreferred;

/// <summary>
/// THE PREFERRED APPROACH — a regular class registered as a DI singleton.
///
/// Instead of enforcing singleton-ness in the class itself, let the DI container
/// manage the lifetime:
///
///   services.AddSingleton&lt;ITelemetryService, TelemetryService&gt;();
///
/// Benefits over classic Singleton:
/// 1. Testable — inject a mock ITelemetryService in tests.
/// 2. Follows Dependency Inversion — consumers depend on abstraction, not concretion.
/// 3. Configurable — different environments can use different implementations.
/// 4. No global state — instance is scoped to the DI container, not the AppDomain.
/// 5. Works with all DI features — decorators, interception, scoped lifetimes.
///
/// The class itself is a plain POCO — no static Instance, no private constructor tricks.
/// The "singleton" behavior is a DEPLOYMENT DECISION, not a CLASS DESIGN DECISION.
/// </summary>
public interface ITelemetryService
{
    void IncrementCounter(string metricName, long amount = 1);
    void SetGauge(string metricName, double value);
    void RecordEvent(string eventName, Dictionary<string, string>? properties = null);
    long GetCounter(string metricName);
    double GetGauge(string metricName);
    IReadOnlyDictionary<string, long> GetCounterSnapshot();
}

/// <summary>
/// Concrete telemetry service — a regular class with no singleton infrastructure.
/// Registered as singleton via DI: services.AddSingleton&lt;ITelemetryService, TelemetryService&gt;();
/// </summary>
public sealed class TelemetryService : ITelemetryService
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, double> _gauges = new();
    private readonly ConcurrentBag<(string Event, DateTimeOffset Timestamp, Dictionary<string, string> Props)> _events = [];

    // Public constructor — DI container calls this. No tricks.
    public TelemetryService() { }

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
        _events.Add((eventName, DateTimeOffset.UtcNow, properties ?? new Dictionary<string, string>()));
    }

    public long GetCounter(string metricName) => _counters.GetValueOrDefault(metricName, 0);
    public double GetGauge(string metricName) => _gauges.GetValueOrDefault(metricName, 0.0);

    public IReadOnlyDictionary<string, long> GetCounterSnapshot() => new Dictionary<string, long>(_counters);
}
