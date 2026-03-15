using DesignPatterns.Behavioral.TemplateMethod;
using DesignPatterns.Behavioral.TemplateMethod.Exporters;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class TemplateMethodTests
{
    private static IReadOnlyList<Dictionary<string, string>> CreateSampleData() =>
    [
        new() { ["Name"] = "Widget A", ["Price"] = "10.00", ["Category"] = "Tools" },
        new() { ["Name"] = "Widget B", ["Price"] = "25.50", ["Category"] = "Electronics" },
        new() { ["Name"] = "Widget C", ["Price"] = "7.99", ["Category"] = "Tools" }
    ];

    [Fact]
    public void CsvExporter_ProducesValidCsv()
    {
        var exporter = new CsvExporter();
        var result = exporter.Export(CreateSampleData());

        result.Success.Should().BeTrue();
        result.Format.Should().Be("CSV");
        result.RecordCount.Should().Be(3);

        var lines = result.Content.Split(Environment.NewLine);
        lines[0].Should().Be("Name,Price,Category");
        lines[1].Should().Contain("Widget A");
    }

    [Fact]
    public void ExcelExporter_WrapsInXmlStructure()
    {
        var exporter = new ExcelExporter();
        var result = exporter.Export(CreateSampleData());

        result.Success.Should().BeTrue();
        result.Format.Should().Be("Excel");
        result.Content.Should().StartWith("<?xml");
        result.Content.Should().Contain("<Workbook>");
        result.Content.Should().Contain("Widget A");
    }

    [Fact]
    public void PdfExporter_IncludesHeaderAndFooter()
    {
        var exporter = new PdfExporter("Sales Report");
        var result = exporter.Export(CreateSampleData());

        result.Success.Should().BeTrue();
        result.Format.Should().Be("PDF");
        result.Content.Should().StartWith("=== Sales Report ===");
        result.Content.Should().Contain("Widget A");
    }

    [Fact]
    public void AllExporters_ExecuteAllSteps()
    {
        var data = CreateSampleData();
        var exporters = new DocumentExporter[] { new CsvExporter(), new ExcelExporter(), new PdfExporter() };

        foreach (var exporter in exporters)
        {
            var result = exporter.Export(data);

            result.Steps.Should().BeEquivalentTo(
                ["LoadData", "Validate", "Transform", "Write", "Finalize"],
                options => options.WithStrictOrdering());
        }
    }

    [Fact]
    public void EmptyData_FailsValidation()
    {
        var exporter = new CsvExporter();
        var result = exporter.Export([]);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No data");
    }

    [Fact]
    public void CsvExporter_EscapesCommasInFields()
    {
        var data = new List<Dictionary<string, string>>
        {
            new() { ["Name"] = "Widget, Deluxe", ["Price"] = "15.00" }
        };

        var exporter = new CsvExporter();
        var result = exporter.Export(data);

        result.Content.Should().Contain("\"Widget, Deluxe\"");
    }

    [Fact]
    public void ExportResult_TracksDuration()
    {
        var exporter = new CsvExporter();
        var result = exporter.Export(CreateSampleData());

        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
