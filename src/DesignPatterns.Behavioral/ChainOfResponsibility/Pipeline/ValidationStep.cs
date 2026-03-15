namespace DesignPatterns.Behavioral.ChainOfResponsibility.Pipeline;

/// <summary>
/// Validates the expense request before further processing.
/// Throws if validation fails, otherwise passes to the next step.
/// </summary>
public sealed class ValidationStep : IPipelineStep<ExpenseRequest>
{
    public async Task<ExpenseRequest> ProcessAsync(ExpenseRequest input, Func<ExpenseRequest, Task<ExpenseRequest>> next)
    {
        if (input.Amount <= 0)
            throw new ArgumentException("Expense amount must be positive.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.EmployeeName))
            throw new ArgumentException("Employee name is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.Description))
            throw new ArgumentException("Description is required.", nameof(input));

        // Add validation metadata
        var enriched = input with
        {
            Metadata = new Dictionary<string, string>(input.Metadata)
            {
                ["Validated"] = "true",
                ["ValidatedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        return await next(enriched);
    }
}
