namespace DesignPatterns.Behavioral.Visitor;

/// <summary>
/// Result of a fraud detection analysis on a transaction.
/// </summary>
public sealed record FraudDetectionResult
{
    public required string RuleName { get; init; }
    public required string TransactionId { get; init; }
    public required bool IsSuspicious { get; init; }
    public required RiskLevel Risk { get; init; }
    public required string Reason { get; init; }
    public decimal Confidence { get; init; }
}

public enum RiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}
