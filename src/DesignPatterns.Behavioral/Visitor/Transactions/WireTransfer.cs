namespace DesignPatterns.Behavioral.Visitor.Transactions;

/// <summary>
/// Wire/bank transfer transaction.
/// </summary>
public sealed record WireTransfer : ITransaction
{
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string OriginCountry { get; init; }
    public required string DestinationCountry { get; init; }
    public required string SenderBankCode { get; init; }
    public required string ReceiverBankCode { get; init; }
    public string Purpose { get; init; } = string.Empty;

    public FraudDetectionResult Accept(IFraudDetectionVisitor visitor) => visitor.Visit(this);
}
