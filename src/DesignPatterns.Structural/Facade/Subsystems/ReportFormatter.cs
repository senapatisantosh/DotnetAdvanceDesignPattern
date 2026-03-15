using System.Text;
using DesignPatterns.Structural.Facade.Models;

namespace DesignPatterns.Structural.Facade.Subsystems;

/// <summary>
/// Subsystem #3 — formats aggregated data into a presentable report.
/// Generates content in the requested format (PDF, HTML, CSV, Excel).
/// </summary>
public class ReportFormatter
{
    public virtual FormattedReport Format(AggregatedReportData data, string reportName, ExportFormat format)
    {
        var (content, mimeType) = format switch
        {
            ExportFormat.Html => (FormatAsHtml(data, reportName), "text/html"),
            ExportFormat.Csv => (FormatAsCsv(data), "text/csv"),
            ExportFormat.Excel => (FormatAsCsv(data), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            ExportFormat.Pdf => (FormatAsHtml(data, reportName), "application/pdf"),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        var estimatedPages = Math.Max(1, data.RawData.TotalRowCount / 25 + 1);

        return new FormattedReport
        {
            Title = reportName,
            Content = content,
            MimeType = mimeType,
            GeneratedAtUtc = DateTime.UtcNow,
            PageCount = estimatedPages
        };
    }

    private static string FormatAsHtml(AggregatedReportData data, string title)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<html><head><title>{title}</title></head><body>");
        sb.AppendLine($"<h1>{title}</h1>");
        sb.AppendLine($"<p>Generated: {DateTime.UtcNow:u}</p>");

        // Summary section
        if (data.Summaries.Count > 0)
        {
            sb.AppendLine("<h2>Summary</h2><table border='1'>");
            foreach (var (key, value) in data.Summaries)
            {
                sb.AppendLine($"<tr><td>{key}</td><td>{value:N2}</td></tr>");
            }
            sb.AppendLine("</table>");
        }

        // Data table
        if (data.RawData.Rows.Count > 0)
        {
            sb.AppendLine("<h2>Data</h2><table border='1'><tr>");
            foreach (var header in data.RawData.Rows[0].Keys)
                sb.Append($"<th>{header}</th>");
            sb.AppendLine("</tr>");

            foreach (var row in data.RawData.Rows)
            {
                sb.Append("<tr>");
                foreach (var value in row.Values)
                    sb.Append($"<td>{value}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static string FormatAsCsv(AggregatedReportData data)
    {
        if (data.RawData.Rows.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var headers = data.RawData.Rows[0].Keys;
        sb.AppendLine(string.Join(",", headers));

        foreach (var row in data.RawData.Rows)
        {
            sb.AppendLine(string.Join(",", row.Values.Select(v => $"\"{v}\"")));
        }

        return sb.ToString();
    }
}
