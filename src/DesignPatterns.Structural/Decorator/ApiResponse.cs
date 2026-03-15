namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Represents the result of an API call, carrying the response data,
/// status information, and metadata about how the response was obtained
/// (e.g., from cache, after retries).
/// </summary>
public sealed record ApiResponse
{
    public required string RequestUrl { get; init; }
    public required int StatusCode { get; init; }
    public required string Body { get; init; }
    public required TimeSpan Duration { get; init; }
    public required DateTime TimestampUtc { get; init; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public bool FromCache { get; init; }
    public int RetryCount { get; init; }
    public Dictionary<string, string> Headers { get; init; } = [];
}
