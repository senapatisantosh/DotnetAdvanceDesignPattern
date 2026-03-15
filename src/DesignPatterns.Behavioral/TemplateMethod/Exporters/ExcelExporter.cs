namespace DesignPatterns.Behavioral.TemplateMethod.Exporters;

/// <summary>
/// Simulated Excel export — produces a tab-separated format with Excel XML header.
/// In production this would use a library like EPPlus or ClosedXML.
/// </summary>
public sealed class ExcelExporter : DocumentExporter
{
    protected override string FormatName => "Excel";

    protected override IReadOnlyList<string> Transform(IReadOnlyList<Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        if (data.Count == 0) return lines;

        var headers = data[0].Keys.ToList();
        lines.Add(string.Join("\t", headers));

        foreach (var row in data)
        {
            var values = headers.Select(h => row.GetValueOrDefault(h, ""));
            lines.Add(string.Join("\t", values));
        }

        return lines;
    }

    protected override string Write(IReadOnlyList<string> transformedData)
    {
        return string.Join(Environment.NewLine, transformedData);
    }

    protected override string Finalize(string output)
    {
        // Add a simulated Excel XML wrapper
        return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Workbook>\n<Sheet name=\"Export\">\n{output}\n</Sheet>\n</Workbook>";
    }
}
