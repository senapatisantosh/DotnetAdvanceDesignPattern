using System.Diagnostics;

namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Concrete Component — the real API client that makes HTTP-like calls.
/// In production this would wrap HttpClient; here we simulate responses.
/// </summary>
public class BaseApiClient : IApiClient
{
    private int _callCount;
    private readonly double _failureRate;

    /// <summary>
    /// Creates a base API client.
    /// </summary>
    /// <param name="failureRate">Probability of simulated failure (0.0 to 1.0) for testing retry behavior.</param>
    public BaseApiClient(double failureRate = 0.0)
    {
        _failureRate = Math.Clamp(failureRate, 0.0, 1.0);
    }

    /// <summary>Number of actual calls made (not including retries handled by decorators).</summary>
    public int CallCount => _callCount;

    public Task<ApiResponse> GetAsync(string url)
    {
        Interlocked.Increment(ref _callCount);
        return SimulateCallAsync(url, "GET");
    }

    public Task<ApiResponse> PostAsync(string url, string payload)
    {
        Interlocked.Increment(ref _callCount);
        return SimulateCallAsync(url, "POST", payload);
    }

    private Task<ApiResponse> SimulateCallAsync(string url, string method, string? payload = null)
    {
        var sw = Stopwatch.StartNew();

        // Simulate occasional failures for testing retry decorators
        if (_failureRate > 0 && Random.Shared.NextDouble() < _failureRate)
        {
            sw.Stop();
            return Task.FromResult(new ApiResponse
            {
                RequestUrl = url,
                StatusCode = 503,
                Body = """{"error": "Service Unavailable"}""",
                Duration = sw.Elapsed,
                TimestampUtc = DateTime.UtcNow
            });
        }

        sw.Stop();
        var responseBody = method == "GET"
            ? $$"""{"url": "{{url}}", "data": {"id": 1, "status": "active"}}"""
            : $$"""{"url": "{{url}}", "created": true, "payload": "{{payload ?? ""}}" }""";

        return Task.FromResult(new ApiResponse
        {
            RequestUrl = url,
            StatusCode = 200,
            Body = responseBody,
            Duration = sw.Elapsed,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
