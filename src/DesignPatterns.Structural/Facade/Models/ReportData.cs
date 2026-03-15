namespace DesignPatterns.Structural.Facade.Models;

/// <summary>
/// Internal data model used by the subsystems to pass data between
/// fetching, aggregation, and formatting stages.
/// </summary>
public sealed class ReportData
{
    public required string DataSource { get; init; }
    public required List<Dictionary<string, object>> Rows { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
    public int TotalRowCount => Rows.Count;
}

/// <summary>
/// Aggregated data after the DataAggregator processes raw data.
/// </summary>
public sealed class AggregatedReportData
{
    public required ReportData RawData { get; init; }
    public required Dictionary<string, decimal> Summaries { get; init; }
    public required Dictionary<string, List<Dictionary<string, object>>> GroupedData { get; init; }
    public required DateTime AggregatedAtUtc { get; init; }
}

/// <summary>
/// Formatted report content ready for export.
/// </summary>
public sealed class FormattedReport
{
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required string MimeType { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required int PageCount { get; init; }
}
