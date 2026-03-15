namespace DesignPatterns.Behavioral.Visitor;

/// <summary>
/// Element interface — transactions that accept fraud detection visitors.
/// </summary>
public interface ITransaction
{
    string TransactionId { get; }
    decimal Amount { get; }
    DateTime Timestamp { get; }
    string OriginCountry { get; }
    FraudDetectionResult Accept(IFraudDetectionVisitor visitor);
}
