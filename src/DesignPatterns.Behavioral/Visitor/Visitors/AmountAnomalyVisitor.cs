using DesignPatterns.Behavioral.Visitor.Transactions;

namespace DesignPatterns.Behavioral.Visitor.Visitors;

/// <summary>
/// Detects suspicious transaction amounts — unusually large, round-number
/// structuring, or amounts just below reporting thresholds.
/// </summary>
public sealed class AmountAnomalyVisitor : IFraudDetectionVisitor
{
    /// <summary>
    /// Amounts just below the $10,000 CTR threshold (structuring indicator).
    /// </summary>
    private const decimal StructuringThreshold = 10_000m;
    private const decimal StructuringBuffer = 500m;

    public FraudDetectionResult Visit(CardTransaction transaction)
    {
        var isSuspicious = transaction.Amount > 5_000 && transaction.IsOnline;

        return new FraudDetectionResult
        {
            RuleName = "AmountAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isSuspicious ? RiskLevel.Medium : RiskLevel.None,
            Reason = isSuspicious
                ? $"Large online card purchase: ${transaction.Amount} at {transaction.MerchantCategory} merchant."
                : "Transaction amount within normal range.",
            Confidence = isSuspicious ? 0.60m : 0.05m
        };
    }

    public FraudDetectionResult Visit(WireTransfer transaction)
    {
        var isStructuring = transaction.Amount >= (StructuringThreshold - StructuringBuffer)
                            && transaction.Amount < StructuringThreshold;
        var isLarge = transaction.Amount >= 50_000;

        var isSuspicious = isStructuring || isLarge;

        return new FraudDetectionResult
        {
            RuleName = "AmountAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isStructuring ? RiskLevel.Critical : isLarge ? RiskLevel.High : RiskLevel.None,
            Reason = isStructuring
                ? $"Potential structuring: wire transfer of ${transaction.Amount} just below ${StructuringThreshold} CTR threshold."
                : isLarge
                    ? $"Large wire transfer: ${transaction.Amount}."
                    : "Wire transfer amount within normal range.",
            Confidence = isStructuring ? 0.88m : isLarge ? 0.55m : 0.05m
        };
    }

    public FraudDetectionResult Visit(CryptoTransaction transaction)
    {
        var isLarge = transaction.Amount >= 25_000;
        var isRoundNumber = transaction.Amount % 1000 == 0 && transaction.Amount >= 5_000;

        var isSuspicious = isLarge || isRoundNumber;

        return new FraudDetectionResult
        {
            RuleName = "AmountAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isLarge ? RiskLevel.High : isRoundNumber ? RiskLevel.Medium : RiskLevel.None,
            Reason = isLarge
                ? $"Large crypto transaction: ${transaction.Amount} in {transaction.CryptoCurrency}."
                : isRoundNumber
                    ? $"Suspicious round-number crypto transaction: ${transaction.Amount} in {transaction.CryptoCurrency}."
                    : "Crypto transaction amount within normal range.",
            Confidence = isLarge ? 0.70m : isRoundNumber ? 0.45m : 0.05m
        };
    }
}
