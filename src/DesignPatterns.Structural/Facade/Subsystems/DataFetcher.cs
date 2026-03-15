using DesignPatterns.Structural.Facade.Models;

namespace DesignPatterns.Structural.Facade.Subsystems;

/// <summary>
/// Subsystem #1 — fetches raw data from various data sources based on report type.
/// In production, this would query databases, APIs, or data warehouses.
/// </summary>
public class DataFetcher
{
    public virtual ReportData FetchData(ReportType reportType, DateTime startDate, DateTime endDate, string? department)
    {
        var rows = reportType switch
        {
            ReportType.Sales => GenerateSalesData(startDate, endDate, department),
            ReportType.Inventory => GenerateInventoryData(department),
            ReportType.Financial => GenerateFinancialData(startDate, endDate),
            ReportType.CustomerActivity => GenerateCustomerActivityData(startDate, endDate),
            _ => throw new ArgumentOutOfRangeException(nameof(reportType))
        };

        return new ReportData
        {
            DataSource = $"{reportType}Database",
            Rows = rows,
            FetchedAtUtc = DateTime.UtcNow
        };
    }

    private static List<Dictionary<string, object>> GenerateSalesData(DateTime start, DateTime end, string? dept)
    {
        var rows = new List<Dictionary<string, object>>();
        var current = start;
        while (current <= end)
        {
            rows.Add(new Dictionary<string, object>
            {
                ["Date"] = current,
                ["Department"] = dept ?? "All",
                ["Revenue"] = 1000m + (rows.Count * 150m),
                ["Orders"] = 10 + rows.Count,
                ["AvgOrderValue"] = 100m + (rows.Count * 5m)
            });
            current = current.AddDays(1);
        }
        return rows;
    }

    private static List<Dictionary<string, object>> GenerateInventoryData(string? dept)
    {
        return
        [
            new() { ["SKU"] = "SKU-001", ["Product"] = "Widget A", ["OnHand"] = 150, ["Reserved"] = 30, ["Department"] = dept ?? "Warehouse" },
            new() { ["SKU"] = "SKU-002", ["Product"] = "Widget B", ["OnHand"] = 75, ["Reserved"] = 20, ["Department"] = dept ?? "Warehouse" },
            new() { ["SKU"] = "SKU-003", ["Product"] = "Gadget X", ["OnHand"] = 200, ["Reserved"] = 50, ["Department"] = dept ?? "Warehouse" }
        ];
    }

    private static List<Dictionary<string, object>> GenerateFinancialData(DateTime start, DateTime end)
    {
        return
        [
            new() { ["Period"] = $"{start:yyyy-MM}", ["Revenue"] = 50000m, ["COGS"] = 30000m, ["GrossProfit"] = 20000m, ["NetIncome"] = 12000m },
            new() { ["Period"] = $"{end:yyyy-MM}", ["Revenue"] = 55000m, ["COGS"] = 31000m, ["GrossProfit"] = 24000m, ["NetIncome"] = 15000m }
        ];
    }

    private static List<Dictionary<string, object>> GenerateCustomerActivityData(DateTime start, DateTime end)
    {
        return
        [
            new() { ["CustomerId"] = "C-100", ["Name"] = "Acme Corp", ["OrderCount"] = 12, ["TotalSpent"] = 15000m, ["LastOrder"] = end.AddDays(-3) },
            new() { ["CustomerId"] = "C-200", ["Name"] = "Globex Inc", ["OrderCount"] = 8, ["TotalSpent"] = 9500m, ["LastOrder"] = end.AddDays(-1) }
        ];
    }
}
