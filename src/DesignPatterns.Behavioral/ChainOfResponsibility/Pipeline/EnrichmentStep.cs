namespace DesignPatterns.Behavioral.ChainOfResponsibility.Pipeline;

/// <summary>
/// Enriches the expense request with additional metadata.
/// Simulates looking up cost center, department info, etc.
/// </summary>
public sealed class EnrichmentStep : IPipelineStep<ExpenseRequest>
{
    private static readonly Dictionary<string, string> DepartmentCostCenters = new()
    {
        ["Engineering"] = "CC-1001",
        ["Marketing"] = "CC-2001",
        ["Sales"] = "CC-3001",
        ["HR"] = "CC-4001",
        ["Finance"] = "CC-5001"
    };

    public async Task<ExpenseRequest> ProcessAsync(ExpenseRequest input, Func<ExpenseRequest, Task<ExpenseRequest>> next)
    {
        var costCenter = DepartmentCostCenters.GetValueOrDefault(input.Department, "CC-9999");
        var fiscalQuarter = GetFiscalQuarter(input.SubmittedAt);

        var enriched = input with
        {
            Metadata = new Dictionary<string, string>(input.Metadata)
            {
                ["CostCenter"] = costCenter,
                ["FiscalQuarter"] = fiscalQuarter,
                ["EnrichedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        return await next(enriched);
    }

    private static string GetFiscalQuarter(DateTime date) =>
        $"FY{date.Year}-Q{(date.Month - 1) / 3 + 1}";
}
