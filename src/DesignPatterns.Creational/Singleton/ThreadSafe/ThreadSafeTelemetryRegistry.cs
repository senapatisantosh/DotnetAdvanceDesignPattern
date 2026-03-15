using System.Collections.Concurrent;

namespace DesignPatterns.Creational.Singleton.ThreadSafe;

/// <summary>
/// Double-check locking Singleton — the classic thread-safe pattern.
///
/// This is the traditional approach before Lazy&lt;T&gt; existed in .NET.
/// It is included here for educational purposes and interview preparation.
///
/// The double-check pattern:
/// 1. First check without lock (fast path — instance already exists).
/// 2. If null, acquire lock.
/// 3. Second check inside lock (another thread may have created it while we waited).
/// 4. Create instance.
///
/// In modern C#, prefer Lazy&lt;T&gt; or the eager static field approach.
/// </summary>
public sealed class ThreadSafeTelemetryRegistry
{
    private static volatile ThreadSafeTelemetryRegistry? _instance;
    private static readonly object _lock = new();

    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, double> _gauges = new();

    // Track creation for testing thread safety.
    private static int _creationCount;

    private ThreadSafeTelemetryRegistry()
    {
        Interlocked.Increment(ref _creationCount);
    }

    /// <summary>
    /// Gets the single instance using double-check locking.
    /// </summary>
    public static ThreadSafeTelemetryRegistry Instance
    {
        get
        {
            // First check — no lock needed. Fast path for the common case.
            if (_instance is not null)
                return _instance;

            lock (_lock)
            {
                // Second check — inside lock. Prevents double creation.
                _instance ??= new ThreadSafeTelemetryRegistry();
            }

            return _instance;
        }
    }

    /// <summary>
    /// How many times the constructor was called. Should always be 0 or 1.
    /// Useful for verifying thread-safety in tests.
    /// </summary>
    public static int CreationCount => _creationCount;

    public void IncrementCounter(string metricName, long amount = 1)
    {
        _counters.AddOrUpdate(metricName, amount, (_, current) => current + amount);
    }

    public void SetGauge(string metricName, double value)
    {
        _gauges[metricName] = value;
    }

    public long GetCounter(string metricName) => _counters.GetValueOrDefault(metricName, 0);
    public double GetGauge(string metricName) => _gauges.GetValueOrDefault(metricName, 0.0);

    public IReadOnlyDictionary<string, long> GetCounterSnapshot() => new Dictionary<string, long>(_counters);

    public void Reset()
    {
        _counters.Clear();
        _gauges.Clear();
    }

    /// <summary>
    /// Resets the singleton instance. FOR TESTING ONLY.
    /// In production, a singleton should never be reset.
    /// </summary>
    internal static void ResetInstanceForTesting()
    {
        lock (_lock)
        {
            _instance = null;
            _creationCount = 0;
        }
    }
}
