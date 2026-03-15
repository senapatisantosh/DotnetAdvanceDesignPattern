namespace DesignPatterns.Behavioral.TemplateMethod;

/// <summary>
/// Result of a document export operation.
/// </summary>
public sealed record ExportResult
{
    public required bool Success { get; init; }
    public required string Format { get; init; }
    public required string Content { get; init; }
    public required int RecordCount { get; init; }
    public List<string> Steps { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
}
