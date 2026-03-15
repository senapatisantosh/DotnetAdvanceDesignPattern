using DesignPatterns.Behavioral.Visitor.Transactions;

namespace DesignPatterns.Behavioral.Visitor.Visitors;

/// <summary>
/// Checks for high-frequency transaction patterns (velocity abuse).
/// Each transaction type has different velocity thresholds.
/// </summary>
public sealed class VelocityCheckVisitor : IFraudDetectionVisitor
{
    private readonly List<(string TransactionId, DateTime Timestamp)> _recentTransactions = [];
    private readonly TimeSpan _window;
    private readonly int _maxTransactionsInWindow;

    public VelocityCheckVisitor(TimeSpan? window = null, int maxTransactionsInWindow = 5)
    {
        _window = window ?? TimeSpan.FromMinutes(10);
        _maxTransactionsInWindow = maxTransactionsInWindow;
    }

    public FraudDetectionResult Visit(CardTransaction transaction)
    {
        var isSuspicious = CheckVelocity(transaction.TransactionId, transaction.Timestamp);
        return new FraudDetectionResult
        {
            RuleName = "VelocityCheck",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isSuspicious ? RiskLevel.High : RiskLevel.None,
            Reason = isSuspicious
                ? $"Card ending {transaction.CardLastFour}: too many transactions in {_window.TotalMinutes}min window."
                : "Transaction velocity within normal limits.",
            Confidence = isSuspicious ? 0.85m : 0.1m
        };
    }

    public FraudDetectionResult Visit(WireTransfer transaction)
    {
        var isSuspicious = CheckVelocity(transaction.TransactionId, transaction.Timestamp);
        return new FraudDetectionResult
        {
            RuleName = "VelocityCheck",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isSuspicious ? RiskLevel.Critical : RiskLevel.None,
            Reason = isSuspicious
                ? "Multiple wire transfers detected in short window — possible structuring."
                : "Wire transfer velocity acceptable.",
            Confidence = isSuspicious ? 0.90m : 0.1m
        };
    }

    public FraudDetectionResult Visit(CryptoTransaction transaction)
    {
        var isSuspicious = CheckVelocity(transaction.TransactionId, transaction.Timestamp);
        return new FraudDetectionResult
        {
            RuleName = "VelocityCheck",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isSuspicious ? RiskLevel.High : RiskLevel.None,
            Reason = isSuspicious
                ? $"Rapid {transaction.CryptoCurrency} transactions detected from wallet {transaction.WalletAddress[..8]}..."
                : "Crypto transaction velocity within normal limits.",
            Confidence = isSuspicious ? 0.80m : 0.1m
        };
    }

    private bool CheckVelocity(string transactionId, DateTime timestamp)
    {
        // Remove old entries outside the window
        _recentTransactions.RemoveAll(t => timestamp - t.Timestamp > _window);
        _recentTransactions.Add((transactionId, timestamp));

        return _recentTransactions.Count > _maxTransactionsInWindow;
    }
}
