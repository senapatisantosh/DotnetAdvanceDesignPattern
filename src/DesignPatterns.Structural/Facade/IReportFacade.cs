using DesignPatterns.Structural.Facade.Models;

namespace DesignPatterns.Structural.Facade;

/// <summary>
/// Facade interface — provides a single, simple entry point for report generation.
/// Hides the complexity of data fetching, aggregation, formatting, and export.
/// </summary>
public interface IReportFacade
{
    /// <summary>
    /// Generates a complete report from a single request object.
    /// Internally coordinates DataFetcher, DataAggregator, ReportFormatter, and ReportExporter.
    /// </summary>
    ReportResult GenerateReport(ReportRequest request);
}
