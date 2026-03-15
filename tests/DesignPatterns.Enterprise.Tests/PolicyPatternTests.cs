using DesignPatterns.Enterprise.PolicyPattern;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class PolicyPatternTests
{
    // ── Retry ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RetryPolicy_succeeds_on_first_attempt()
    {
        var policy = new RetryPolicy(maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(1));

        var result = await policy.ExecuteAsync<int>(_ => Task.FromResult(42));

        result.Should().Be(42);
        policy.TotalAttempts.Should().Be(1);
        policy.TotalRetries.Should().Be(0);
    }

    [Fact]
    public async Task RetryPolicy_retries_on_transient_failure()
    {
        var attempts = 0;
        var policy = new RetryPolicy(maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(1));

        var result = await policy.ExecuteAsync<int>(_ =>
        {
            attempts++;
            if (attempts < 3) throw new IOException("transient");
            return Task.FromResult(42);
        });

        result.Should().Be(42);
        attempts.Should().Be(3);
        policy.TotalRetries.Should().Be(2);
    }

    [Fact]
    public async Task RetryPolicy_throws_after_max_retries_exhausted()
    {
        var policy = new RetryPolicy(maxRetries: 2, initialDelay: TimeSpan.FromMilliseconds(1));

        var act = () => policy.ExecuteAsync<int>(_ => throw new IOException("persistent"));

        await act.Should().ThrowAsync<IOException>();
        policy.TotalAttempts.Should().Be(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task RetryPolicy_filters_exceptions()
    {
        var policy = new RetryPolicy(
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(1),
            shouldRetry: ex => ex is IOException);

        // Non-retryable exception should not retry
        var act = () => policy.ExecuteAsync<int>(_ => throw new InvalidOperationException("fatal"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        policy.TotalRetries.Should().Be(0);
    }

    // ── Circuit Breaker ────────────────────────────────────────────────

    [Fact]
    public async Task CircuitBreaker_starts_closed()
    {
        var cb = new CircuitBreakerPolicy(failureThreshold: 3);

        cb.State.Should().Be(CircuitState.Closed);
        var result = await cb.ExecuteAsync<int>(_ => Task.FromResult(1));
        result.Should().Be(1);
    }

    [Fact]
    public async Task CircuitBreaker_opens_after_threshold_failures()
    {
        var cb = new CircuitBreakerPolicy(failureThreshold: 2, breakDuration: TimeSpan.FromMinutes(1));

        for (int i = 0; i < 2; i++)
        {
            try { await cb.ExecuteAsync<int>(_ => throw new Exception("fail")); }
            catch { }
        }

        cb.State.Should().Be(CircuitState.Open);

        var act = () => cb.ExecuteAsync<int>(_ => Task.FromResult(1));
        await act.Should().ThrowAsync<CircuitBrokenException>();
    }

    [Fact]
    public async Task CircuitBreaker_resets_on_success()
    {
        var cb = new CircuitBreakerPolicy(failureThreshold: 3);

        // One failure
        try { await cb.ExecuteAsync<int>(_ => throw new Exception("fail")); }
        catch { }

        // Success resets counter
        await cb.ExecuteAsync<int>(_ => Task.FromResult(1));
        cb.State.Should().Be(CircuitState.Closed);

        // Need full threshold again
        try { await cb.ExecuteAsync<int>(_ => throw new Exception("fail")); }
        catch { }
        cb.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void CircuitBreaker_manual_reset()
    {
        var cb = new CircuitBreakerPolicy(failureThreshold: 1, breakDuration: TimeSpan.FromMinutes(1));

        try { cb.ExecuteAsync<int>(_ => throw new Exception("fail")).GetAwaiter().GetResult(); }
        catch { }

        cb.State.Should().Be(CircuitState.Open);
        cb.Reset();
        cb.State.Should().Be(CircuitState.Closed);
    }

    // ── Timeout ────────────────────────────────────────────────────────

    [Fact]
    public async Task TimeoutPolicy_succeeds_within_limit()
    {
        var policy = new TimeoutPolicy(TimeSpan.FromSeconds(5));

        var result = await policy.ExecuteAsync<int>(_ => Task.FromResult(42));

        result.Should().Be(42);
    }

    [Fact]
    public async Task TimeoutPolicy_throws_when_exceeded()
    {
        var policy = new TimeoutPolicy(TimeSpan.FromMilliseconds(50));

        var act = () => policy.ExecuteAsync<int>(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return 42;
        });

        await act.Should().ThrowAsync<TimeoutException>();
    }

    // ── Pipeline ───────────────────────────────────────────────────────

    [Fact]
    public async Task Pipeline_composes_policies()
    {
        var retry = new RetryPolicy(maxRetries: 2, initialDelay: TimeSpan.FromMilliseconds(1));
        var timeout = new TimeoutPolicy(TimeSpan.FromSeconds(5));

        var pipeline = new PolicyPipeline()
            .Add(timeout)
            .Add(retry);

        var attempts = 0;
        var result = await pipeline.ExecuteAsync<int>(_ =>
        {
            attempts++;
            if (attempts < 2) throw new IOException("transient");
            return Task.FromResult(42);
        });

        result.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Pipeline_void_operations()
    {
        var retry = new RetryPolicy(maxRetries: 1, initialDelay: TimeSpan.FromMilliseconds(1));
        var pipeline = new PolicyPipeline().Add(retry);

        var executed = false;
        await pipeline.ExecuteAsync(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        executed.Should().BeTrue();
    }

    [Fact]
    public void Pipeline_name_shows_composition()
    {
        var pipeline = new PolicyPipeline()
            .Add(new TimeoutPolicy(TimeSpan.FromSeconds(5)))
            .Add(new RetryPolicy(maxRetries: 3));

        pipeline.Name.Should().Contain("Timeout");
        pipeline.Name.Should().Contain("Retry");
    }
}
