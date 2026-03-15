namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Concrete Decorator — adds structured logging around every API call.
/// Logs the URL, method, duration, and status code. In production
/// you'd inject ILogger{T}; here we capture log entries for testing.
/// </summary>
public sealed class LoggingApiClientDecorator : IApiClient
{
    private readonly IApiClient _inner;
    private readonly List<LogEntry> _logEntries = [];

    public LoggingApiClientDecorator(IApiClient inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();

    public async Task<ApiResponse> GetAsync(string url)
    {
        Log($"GET {url} — Starting request");
        var response = await _inner.GetAsync(url);
        Log($"GET {url} — Completed: {response.StatusCode} in {response.Duration.TotalMilliseconds:F1}ms");
        return response;
    }

    public async Task<ApiResponse> PostAsync(string url, string payload)
    {
        Log($"POST {url} — Starting request (payload: {payload.Length} chars)");
        var response = await _inner.PostAsync(url, payload);
        Log($"POST {url} — Completed: {response.StatusCode} in {response.Duration.TotalMilliseconds:F1}ms");
        return response;
    }

    private void Log(string message)
    {
        _logEntries.Add(new LogEntry(DateTime.UtcNow, message));
    }

    public sealed record LogEntry(DateTime TimestampUtc, string Message);
}
