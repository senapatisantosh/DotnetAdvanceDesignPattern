namespace DesignPatterns.Behavioral.Visitor.Transactions;

/// <summary>
/// Cryptocurrency transaction.
/// </summary>
public sealed record CryptoTransaction : ITransaction
{
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string OriginCountry { get; init; }
    public required string WalletAddress { get; init; }
    public required string CryptoCurrency { get; init; }
    public required string ExchangeName { get; init; }

    public FraudDetectionResult Accept(IFraudDetectionVisitor visitor) => visitor.Visit(this);
}
