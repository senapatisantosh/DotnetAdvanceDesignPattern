namespace DesignPatterns.Behavioral.TemplateMethod.Exporters;

/// <summary>
/// Simulated PDF export — produces a text representation with PDF-like structure.
/// In production this would use a library like QuestPDF or iTextSharp.
/// </summary>
public sealed class PdfExporter : DocumentExporter
{
    private readonly string _title;

    public PdfExporter(string title = "Document Export")
    {
        _title = title;
    }

    protected override string FormatName => "PDF";

    protected override List<string> Validate(IReadOnlyList<Dictionary<string, string>> data)
    {
        var errors = base.Validate(data);

        // PDF-specific validation: check for maximum page limit
        if (data.Count > 10_000)
            errors.Add("PDF export is limited to 10,000 records.");

        return errors;
    }

    protected override IReadOnlyList<string> Transform(IReadOnlyList<Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        if (data.Count == 0) return lines;

        var headers = data[0].Keys.ToList();

        // Create a text-based table representation
        var columnWidths = headers.Select(h =>
            Math.Max(h.Length, data.Max(r => r.GetValueOrDefault(h, "").Length))
        ).ToList();

        // Header
        var headerLine = string.Join(" | ", headers.Select((h, i) => h.PadRight(columnWidths[i])));
        lines.Add(headerLine);
        lines.Add(new string('-', headerLine.Length));

        // Rows
        foreach (var row in data)
        {
            var values = headers.Select((h, i) =>
                row.GetValueOrDefault(h, "").PadRight(columnWidths[i]));
            lines.Add(string.Join(" | ", values));
        }

        return lines;
    }

    protected override string Write(IReadOnlyList<string> transformedData)
    {
        return string.Join(Environment.NewLine, transformedData);
    }

    protected override string Finalize(string output)
    {
        var header = $"=== {_title} ===";
        var footer = $"=== Generated at {DateTime.UtcNow:yyyy-MM-dd} | Total characters: {output.Length} ===";
        return $"{header}\n{output}\n{footer}";
    }
}
