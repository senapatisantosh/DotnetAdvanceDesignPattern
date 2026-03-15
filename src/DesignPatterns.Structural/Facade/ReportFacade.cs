using System.Diagnostics;
using DesignPatterns.Structural.Facade.Models;
using DesignPatterns.Structural.Facade.Subsystems;

namespace DesignPatterns.Structural.Facade;

/// <summary>
/// Concrete Facade — orchestrates four subsystems behind a single method call.
///
/// Without the facade, the client would need to:
/// 1. Know about DataFetcher, DataAggregator, ReportFormatter, ReportExporter
/// 2. Call them in the correct order
/// 3. Pass intermediate results between them
/// 4. Handle errors at each stage
///
/// The facade simplifies this to a single <see cref="GenerateReport"/> call.
/// </summary>
public sealed class ReportFacade : IReportFacade
{
    private readonly DataFetcher _dataFetcher;
    private readonly DataAggregator _dataAggregator;
    private readonly ReportFormatter _reportFormatter;
    private readonly ReportExporter _reportExporter;

    public ReportFacade(
        DataFetcher dataFetcher,
        DataAggregator dataAggregator,
        ReportFormatter reportFormatter,
        ReportExporter reportExporter)
    {
        _dataFetcher = dataFetcher;
        _dataAggregator = dataAggregator;
        _reportFormatter = reportFormatter;
        _reportExporter = reportExporter;
    }

    /// <summary>Default constructor with standard subsystem implementations.</summary>
    public ReportFacade()
        : this(new DataFetcher(), new DataAggregator(), new ReportFormatter(), new ReportExporter())
    {
    }

    public ReportResult GenerateReport(ReportRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Fetch raw data from the appropriate data source
            var rawData = _dataFetcher.FetchData(
                request.Type, request.StartDate, request.EndDate, request.Department);

            // Step 2: Aggregate and summarize the data
            var aggregated = _dataAggregator.Aggregate(rawData, request.IncludeSummary);

            // Step 3: Format the report content
            var formatted = _reportFormatter.Format(aggregated, request.ReportName, request.Format);

            // Step 4: Export to the final byte format
            var content = _reportExporter.Export(formatted, request.Format);
            var extension = _reportExporter.GetFileExtension(request.Format);

            stopwatch.Stop();

            return new ReportResult
            {
                ReportName = request.ReportName,
                Content = content,
                FileName = $"{SanitizeFileName(request.ReportName)}{extension}",
                MimeType = formatted.MimeType,
                TotalRecords = rawData.TotalRowCount,
                PageCount = formatted.PageCount,
                GeneratedAtUtc = DateTime.UtcNow,
                GenerationDuration = stopwatch.Elapsed,
                SummaryMetrics = aggregated.Summaries,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new ReportResult
            {
                ReportName = request.ReportName,
                Content = [],
                FileName = string.Empty,
                MimeType = string.Empty,
                TotalRecords = 0,
                PageCount = 0,
                GeneratedAtUtc = DateTime.UtcNow,
                GenerationDuration = stopwatch.Elapsed,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string SanitizeFileName(string name) =>
        string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
}
