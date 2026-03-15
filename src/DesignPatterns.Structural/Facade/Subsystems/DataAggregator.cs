using DesignPatterns.Structural.Facade.Models;

namespace DesignPatterns.Structural.Facade.Subsystems;

/// <summary>
/// Subsystem #2 — aggregates and summarizes raw data.
/// Computes totals, averages, groups, and other statistical summaries.
/// </summary>
public class DataAggregator
{
    public virtual AggregatedReportData Aggregate(ReportData rawData, bool includeSummary)
    {
        var summaries = new Dictionary<string, decimal>();
        var grouped = new Dictionary<string, List<Dictionary<string, object>>>();

        if (includeSummary && rawData.Rows.Count > 0)
        {
            // Auto-detect numeric columns and compute summaries
            var sampleRow = rawData.Rows[0];
            foreach (var key in sampleRow.Keys)
            {
                var numericValues = rawData.Rows
                    .Where(r => r.ContainsKey(key) && r[key] is decimal or int or double or float)
                    .Select(r => Convert.ToDecimal(r[key]))
                    .ToList();

                if (numericValues.Count > 0)
                {
                    summaries[$"{key}_Total"] = numericValues.Sum();
                    summaries[$"{key}_Average"] = numericValues.Average();
                    summaries[$"{key}_Min"] = numericValues.Min();
                    summaries[$"{key}_Max"] = numericValues.Max();
                }
            }
        }

        // Group by the first string column found
        var groupKey = rawData.Rows.Count > 0
            ? rawData.Rows[0].Keys.FirstOrDefault(k => rawData.Rows[0][k] is string)
            : null;

        if (groupKey is not null)
        {
            foreach (var row in rawData.Rows)
            {
                var groupValue = row[groupKey]?.ToString() ?? "Unknown";
                if (!grouped.ContainsKey(groupValue))
                    grouped[groupValue] = [];
                grouped[groupValue].Add(row);
            }
        }

        return new AggregatedReportData
        {
            RawData = rawData,
            Summaries = summaries,
            GroupedData = grouped,
            AggregatedAtUtc = DateTime.UtcNow
        };
    }
}
