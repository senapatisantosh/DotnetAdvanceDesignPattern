using DesignPatterns.Behavioral.Visitor.Transactions;

namespace DesignPatterns.Behavioral.Visitor;

/// <summary>
/// Visitor interface — each fraud detection check implements this to analyze different transaction types.
/// </summary>
public interface IFraudDetectionVisitor
{
    FraudDetectionResult Visit(CardTransaction transaction);
    FraudDetectionResult Visit(WireTransfer transaction);
    FraudDetectionResult Visit(CryptoTransaction transaction);
}
