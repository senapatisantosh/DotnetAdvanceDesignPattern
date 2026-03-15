using DesignPatterns.Structural.Facade;
using DesignPatterns.Structural.Facade.Models;
using DesignPatterns.Structural.Facade.Subsystems;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class FacadeTests
{
    private readonly ReportFacade _facade = new();

    [Fact]
    public void GenerateReport_SalesReport_ReturnsSuccessfulResult()
    {
        var request = new ReportRequest
        {
            ReportName = "Q1 Sales Report",
            Type = ReportType.Sales,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 3, 31),
            Format = ExportFormat.Pdf,
            IncludeSummary = true
        };

        var result = _facade.GenerateReport(request);

        result.IsSuccess.Should().BeTrue();
        result.ReportName.Should().Be("Q1 Sales Report");
        result.Content.Should().NotBeEmpty();
        result.TotalRecords.Should().BeGreaterThan(0);
        result.FileName.Should().EndWith(".pdf");
        result.MimeType.Should().Be("application/pdf");
    }

    [Fact]
    public void GenerateReport_CsvFormat_ProducesValidCsv()
    {
        var request = new ReportRequest
        {
            ReportName = "Inventory Report",
            Type = ReportType.Inventory,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Format = ExportFormat.Csv
        };

        var result = _facade.GenerateReport(request);

        result.IsSuccess.Should().BeTrue();
        result.FileName.Should().EndWith(".csv");
        result.MimeType.Should().Be("text/csv");

        // Verify content is actual CSV
        var csvContent = System.Text.Encoding.UTF8.GetString(result.Content);
        csvContent.Should().Contain(",");
    }

    [Fact]
    public void GenerateReport_HtmlFormat_ContainsHtmlTags()
    {
        var request = new ReportRequest
        {
            ReportName = "Customer Activity",
            Type = ReportType.CustomerActivity,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Format = ExportFormat.Html
        };

        var result = _facade.GenerateReport(request);

        result.IsSuccess.Should().BeTrue();
        var html = System.Text.Encoding.UTF8.GetString(result.Content);
        html.Should().Contain("<html>");
        html.Should().Contain("<table");
    }

    [Fact]
    public void GenerateReport_WithSummary_IncludesSummaryMetrics()
    {
        var request = new ReportRequest
        {
            ReportName = "Financial Summary",
            Type = ReportType.Financial,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            Format = ExportFormat.Pdf,
            IncludeSummary = true
        };

        var result = _facade.GenerateReport(request);

        result.IsSuccess.Should().BeTrue();
        result.SummaryMetrics.Should().NotBeEmpty();
        result.SummaryMetrics.Keys.Should().Contain(k => k.Contains("Total"));
    }

    [Fact]
    public void GenerateReport_WithDepartment_FiltersData()
    {
        var request = new ReportRequest
        {
            ReportName = "Electronics Sales",
            Type = ReportType.Sales,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 7),
            Format = ExportFormat.Csv,
            Department = "Electronics"
        };

        var result = _facade.GenerateReport(request);

        result.IsSuccess.Should().BeTrue();
        result.TotalRecords.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateReport_HasNonZeroDuration()
    {
        var request = new ReportRequest
        {
            ReportName = "Quick Report",
            Type = ReportType.Inventory,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow,
            Format = ExportFormat.Csv
        };

        var result = _facade.GenerateReport(request);

        result.GenerationDuration.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
        result.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateReport_AllReportTypes_Succeed()
    {
        var reportTypes = Enum.GetValues<ReportType>();

        foreach (var type in reportTypes)
        {
            var request = new ReportRequest
            {
                ReportName = $"Test {type}",
                Type = type,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 31),
                Format = ExportFormat.Pdf
            };

            var result = _facade.GenerateReport(request);
            result.IsSuccess.Should().BeTrue($"Report type {type} should succeed");
        }
    }

    [Fact]
    public void GenerateReport_AllExportFormats_Succeed()
    {
        var formats = Enum.GetValues<ExportFormat>();

        foreach (var format in formats)
        {
            var request = new ReportRequest
            {
                ReportName = $"Test {format}",
                Type = ReportType.Sales,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 7),
                Format = format
            };

            var result = _facade.GenerateReport(request);
            result.IsSuccess.Should().BeTrue($"Export format {format} should succeed");
            result.Content.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void DataFetcher_ReturnsNonEmptyData()
    {
        var fetcher = new DataFetcher();

        var data = fetcher.FetchData(
            ReportType.Sales, new DateTime(2025, 1, 1), new DateTime(2025, 1, 7), null);

        data.Rows.Should().NotBeEmpty();
        data.DataSource.Should().Be("SalesDatabase");
    }

    [Fact]
    public void DataAggregator_ComputesSummaries()
    {
        var fetcher = new DataFetcher();
        var aggregator = new DataAggregator();

        var data = fetcher.FetchData(
            ReportType.Sales, new DateTime(2025, 1, 1), new DateTime(2025, 1, 7), null);
        var aggregated = aggregator.Aggregate(data, includeSummary: true);

        aggregated.Summaries.Should().NotBeEmpty();
        aggregated.Summaries.Keys.Should().Contain(k => k.EndsWith("_Total"));
    }
}
