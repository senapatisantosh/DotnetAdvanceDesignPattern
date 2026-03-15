using System.Text;
using DesignPatterns.Structural.Facade.Models;

namespace DesignPatterns.Structural.Facade.Subsystems;

/// <summary>
/// Subsystem #4 — exports the formatted report to a byte array (simulating
/// file generation, cloud storage upload, or email attachment).
/// </summary>
public class ReportExporter
{
    public virtual byte[] Export(FormattedReport report, ExportFormat format)
    {
        // In production, this would use libraries like iTextSharp (PDF),
        // ClosedXML (Excel), etc. Here we simulate the export.
        return format switch
        {
            ExportFormat.Pdf => SimulatePdfExport(report),
            ExportFormat.Excel => SimulateExcelExport(report),
            ExportFormat.Csv => Encoding.UTF8.GetBytes(report.Content),
            ExportFormat.Html => Encoding.UTF8.GetBytes(report.Content),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public virtual string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.Pdf => ".pdf",
        ExportFormat.Excel => ".xlsx",
        ExportFormat.Csv => ".csv",
        ExportFormat.Html => ".html",
        _ => ".txt"
    };

    private static byte[] SimulatePdfExport(FormattedReport report)
    {
        // Simulate a PDF by prepending a header to the HTML content
        var pdfHeader = "%PDF-1.4 (simulated)\n"u8.ToArray();
        var content = Encoding.UTF8.GetBytes(report.Content);
        var result = new byte[pdfHeader.Length + content.Length];
        pdfHeader.CopyTo(result, 0);
        content.CopyTo(result, pdfHeader.Length);
        return result;
    }

    private static byte[] SimulateExcelExport(FormattedReport report)
    {
        // Simulate Excel by wrapping CSV content with a header
        var excelHeader = "PK\x03\x04 (simulated xlsx)\n"u8.ToArray();
        var content = Encoding.UTF8.GetBytes(report.Content);
        var result = new byte[excelHeader.Length + content.Length];
        excelHeader.CopyTo(result, 0);
        content.CopyTo(result, excelHeader.Length);
        return result;
    }
}
