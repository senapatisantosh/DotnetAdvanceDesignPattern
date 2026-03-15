namespace DesignPatterns.Behavioral.Visitor.Transactions;

/// <summary>
/// Credit/debit card transaction.
/// </summary>
public sealed record CardTransaction : ITransaction
{
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string OriginCountry { get; init; }
    public required string CardLastFour { get; init; }
    public required string MerchantCategory { get; init; }
    public bool IsOnline { get; init; }
    public string CardholderName { get; init; } = string.Empty;

    public FraudDetectionResult Accept(IFraudDetectionVisitor visitor) => visitor.Visit(this);
}
