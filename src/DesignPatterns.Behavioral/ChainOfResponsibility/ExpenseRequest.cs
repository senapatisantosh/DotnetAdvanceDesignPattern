namespace DesignPatterns.Behavioral.ChainOfResponsibility;

/// <summary>
/// Represents an expense request that flows through the approval chain.
/// </summary>
public sealed record ExpenseRequest
{
    public required Guid Id { get; init; }
    public required string EmployeeName { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required string Department { get; init; }
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Mutable metadata enriched during pipeline processing.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}
