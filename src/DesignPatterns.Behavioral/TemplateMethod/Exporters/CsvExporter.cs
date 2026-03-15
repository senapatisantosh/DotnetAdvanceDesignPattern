namespace DesignPatterns.Behavioral.TemplateMethod.Exporters;

/// <summary>
/// CSV export — transforms data rows into comma-separated values.
/// </summary>
public sealed class CsvExporter : DocumentExporter
{
    protected override string FormatName => "CSV";

    protected override IReadOnlyList<string> Transform(IReadOnlyList<Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        if (data.Count == 0) return lines;

        // Header row from first record's keys
        var headers = data[0].Keys.ToList();
        lines.Add(string.Join(",", headers.Select(EscapeCsvField)));

        // Data rows
        foreach (var row in data)
        {
            var values = headers.Select(h => EscapeCsvField(row.GetValueOrDefault(h, "")));
            lines.Add(string.Join(",", values));
        }

        return lines;
    }

    protected override string Write(IReadOnlyList<string> transformedData)
    {
        return string.Join(Environment.NewLine, transformedData);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
