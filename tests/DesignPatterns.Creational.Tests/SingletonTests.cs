using DesignPatterns.Creational.Singleton;
using DesignPatterns.Creational.Singleton.DiPreferred;
using DesignPatterns.Creational.Singleton.LazySingleton;
using DesignPatterns.Creational.Singleton.ThreadSafe;
using FluentAssertions;

namespace DesignPatterns.Creational.Tests;

public class SingletonTests
{
    // --- Classic Singleton Tests ---

    [Fact]
    public void ClassicSingleton_ReturnsSameInstance()
    {
        var instance1 = TelemetryRegistry.Instance;
        var instance2 = TelemetryRegistry.Instance;

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void ClassicSingleton_SharedState_AcrossReferences()
    {
        var registry = TelemetryRegistry.Instance;
        registry.Reset();

        registry.IncrementCounter("test-requests", 5);
        registry.SetGauge("test-cpu", 42.5);

        // Same instance — state is shared.
        var sameRegistry = TelemetryRegistry.Instance;
        sameRegistry.GetCounter("test-requests").Should().Be(5);
        sameRegistry.GetGauge("test-cpu").Should().Be(42.5);
    }

    [Fact]
    public void ClassicSingleton_CounterIncrement_Accumulates()
    {
        var registry = TelemetryRegistry.Instance;
        registry.Reset();

        registry.IncrementCounter("api-calls");
        registry.IncrementCounter("api-calls");
        registry.IncrementCounter("api-calls", 3);

        registry.GetCounter("api-calls").Should().Be(5);
    }

    [Fact]
    public void ClassicSingleton_GetCounterSnapshot_ReturnsIndependentCopy()
    {
        var registry = TelemetryRegistry.Instance;
        registry.Reset();
        registry.IncrementCounter("snapshot-test", 10);

        var snapshot = registry.GetCounterSnapshot();
        registry.IncrementCounter("snapshot-test", 5);

        snapshot["snapshot-test"].Should().Be(10, "snapshot is independent of live data");
        registry.GetCounter("snapshot-test").Should().Be(15);
    }

    // --- Lazy Singleton Tests ---

    [Fact]
    public void LazySingleton_ReturnsSameInstance()
    {
        var instance1 = LazyTelemetryRegistry.Instance;
        var instance2 = LazyTelemetryRegistry.Instance;

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void LazySingleton_IsCreated_ReturnsTrueAfterAccess()
    {
        _ = LazyTelemetryRegistry.Instance;

        LazyTelemetryRegistry.IsCreated.Should().BeTrue();
    }

    [Fact]
    public void LazySingleton_RecordsEvents()
    {
        var registry = LazyTelemetryRegistry.Instance;
        registry.Reset();

        registry.RecordEvent("user-login", new Dictionary<string, string> { ["userId"] = "u-123" });
        registry.RecordEvent("page-view");

        var events = registry.GetEvents();
        events.Should().HaveCount(2);
        events[0].EventName.Should().Be("user-login");
        events[0].Properties.Should().ContainKey("userId");
    }

    // --- Thread-Safe Singleton Tests ---

    [Fact]
    public void ThreadSafeSingleton_ReturnsSameInstance()
    {
        ThreadSafeTelemetryRegistry.ResetInstanceForTesting();

        var instance1 = ThreadSafeTelemetryRegistry.Instance;
        var instance2 = ThreadSafeTelemetryRegistry.Instance;

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void ThreadSafeSingleton_CreatedOnlyOnce_UnderConcurrentAccess()
    {
        ThreadSafeTelemetryRegistry.ResetInstanceForTesting();

        // Hammer the Instance property from many threads simultaneously.
        var instances = new ThreadSafeTelemetryRegistry[100];
        Parallel.For(0, 100, i =>
        {
            instances[i] = ThreadSafeTelemetryRegistry.Instance;
        });

        // All references should point to the same instance.
        instances.Distinct().Should().HaveCount(1);
        ThreadSafeTelemetryRegistry.CreationCount.Should().Be(1);
    }

    [Fact]
    public void ThreadSafeSingleton_ConcurrentIncrements_AreThreadSafe()
    {
        ThreadSafeTelemetryRegistry.ResetInstanceForTesting();
        var registry = ThreadSafeTelemetryRegistry.Instance;
        registry.Reset();

        // Increment from many threads.
        Parallel.For(0, 1000, _ =>
        {
            registry.IncrementCounter("concurrent-test");
        });

        registry.GetCounter("concurrent-test").Should().Be(1000);
    }

    // --- DI-Preferred Approach Tests ---

    [Fact]
    public void DiTelemetryService_IsRegularClass_CanBeInstantiated()
    {
        // No static Instance — just new it up (or let DI do it).
        var service1 = new TelemetryService();
        var service2 = new TelemetryService();

        // These are DIFFERENT instances (DI container would make them singletons).
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void DiTelemetryService_ImplementsInterface()
    {
        ITelemetryService service = new TelemetryService();

        service.IncrementCounter("di-test", 42);
        service.GetCounter("di-test").Should().Be(42);
    }

    [Fact]
    public void DiTelemetryService_CanBeMocked_ViaInterface()
    {
        // This demonstrates the testability advantage of the DI approach.
        // In a real test, you'd use Moq/NSubstitute to mock ITelemetryService.
        ITelemetryService service = new TelemetryService();

        service.IncrementCounter("orders-processed");
        service.IncrementCounter("orders-processed");
        service.SetGauge("queue-depth", 15.0);

        service.GetCounter("orders-processed").Should().Be(2);
        service.GetGauge("queue-depth").Should().Be(15.0);
    }

    [Fact]
    public void DiTelemetryService_CounterSnapshot_IsIndependent()
    {
        var service = new TelemetryService();
        service.IncrementCounter("snap", 100);

        var snapshot = service.GetCounterSnapshot();
        service.IncrementCounter("snap", 50);

        snapshot["snap"].Should().Be(100);
        service.GetCounter("snap").Should().Be(150);
    }

    // --- Classic Singleton concurrent counter test ---

    [Fact]
    public void ClassicSingleton_ConcurrentAccess_IsThreadSafe()
    {
        var registry = TelemetryRegistry.Instance;
        registry.Reset();

        Parallel.For(0, 500, _ =>
        {
            registry.IncrementCounter("parallel-classic");
            registry.SetGauge("parallel-gauge", Thread.CurrentThread.ManagedThreadId);
        });

        registry.GetCounter("parallel-classic").Should().Be(500);
    }
}
