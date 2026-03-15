namespace DesignPatterns.Structural.Facade.Models;

/// <summary>
/// Simple request object the client provides to the facade.
/// Hides all the complexity of data sourcing, aggregation, and formatting.
/// </summary>
public sealed record ReportRequest
{
    public required string ReportName { get; init; }
    public required ReportType Type { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public ExportFormat Format { get; init; } = ExportFormat.Pdf;
    public string? Department { get; init; }
    public bool IncludeSummary { get; init; } = true;
}

public enum ReportType
{
    Sales,
    Inventory,
    Financial,
    CustomerActivity
}

public enum ExportFormat
{
    Pdf,
    Excel,
    Csv,
    Html
}
