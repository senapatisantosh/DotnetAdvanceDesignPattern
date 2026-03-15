namespace DesignPatterns.Structural.Facade.Models;

/// <summary>
/// The final result returned by the facade to the client.
/// Encapsulates everything the client needs without exposing internal subsystems.
/// </summary>
public sealed record ReportResult
{
    public required string ReportName { get; init; }
    public required byte[] Content { get; init; }
    public required string FileName { get; init; }
    public required string MimeType { get; init; }
    public required int TotalRecords { get; init; }
    public required int PageCount { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required TimeSpan GenerationDuration { get; init; }
    public Dictionary<string, decimal> SummaryMetrics { get; init; } = [];
    public bool IsSuccess { get; init; } = true;
    public string? ErrorMessage { get; init; }
}
