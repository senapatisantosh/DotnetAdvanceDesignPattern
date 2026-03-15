using DesignPatterns.Behavioral.Visitor.Transactions;

namespace DesignPatterns.Behavioral.Visitor.Visitors;

/// <summary>
/// Detects geographic anomalies — transactions from high-risk countries
/// or cross-border patterns that indicate potential fraud.
/// </summary>
public sealed class GeoAnomalyVisitor : IFraudDetectionVisitor
{
    private static readonly HashSet<string> HighRiskCountries =
    [
        "XX", // Fictional high-risk
        "YY",
        "ZZ"
    ];

    private readonly string _customerHomeCountry;

    public GeoAnomalyVisitor(string customerHomeCountry = "US")
    {
        _customerHomeCountry = customerHomeCountry;
    }

    public FraudDetectionResult Visit(CardTransaction transaction)
    {
        var isHighRisk = HighRiskCountries.Contains(transaction.OriginCountry);
        var isForeignTransaction = transaction.OriginCountry != _customerHomeCountry;

        var isSuspicious = isHighRisk || (isForeignTransaction && transaction.IsOnline && transaction.Amount > 500);

        return new FraudDetectionResult
        {
            RuleName = "GeoAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = isHighRisk ? RiskLevel.Critical : isSuspicious ? RiskLevel.Medium : RiskLevel.None,
            Reason = isHighRisk
                ? $"Transaction from high-risk country: {transaction.OriginCountry}."
                : isSuspicious
                    ? $"Foreign online card transaction of ${transaction.Amount} from {transaction.OriginCountry}."
                    : "Geographic profile normal.",
            Confidence = isHighRisk ? 0.95m : isSuspicious ? 0.65m : 0.05m
        };
    }

    public FraudDetectionResult Visit(WireTransfer transaction)
    {
        var originHighRisk = HighRiskCountries.Contains(transaction.OriginCountry);
        var destHighRisk = HighRiskCountries.Contains(transaction.DestinationCountry);
        var isCrossBorder = transaction.OriginCountry != transaction.DestinationCountry;

        var isSuspicious = originHighRisk || destHighRisk || (isCrossBorder && transaction.Amount > 10_000);

        return new FraudDetectionResult
        {
            RuleName = "GeoAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isSuspicious,
            Risk = (originHighRisk || destHighRisk) ? RiskLevel.Critical : isSuspicious ? RiskLevel.High : RiskLevel.None,
            Reason = (originHighRisk || destHighRisk)
                ? $"Wire transfer involving high-risk country ({transaction.OriginCountry} → {transaction.DestinationCountry})."
                : isSuspicious
                    ? $"Large cross-border wire transfer: ${transaction.Amount} ({transaction.OriginCountry} → {transaction.DestinationCountry})."
                    : "Geographic profile normal for wire transfer.",
            Confidence = (originHighRisk || destHighRisk) ? 0.90m : isSuspicious ? 0.70m : 0.05m
        };
    }

    public FraudDetectionResult Visit(CryptoTransaction transaction)
    {
        var isHighRisk = HighRiskCountries.Contains(transaction.OriginCountry);

        return new FraudDetectionResult
        {
            RuleName = "GeoAnomaly",
            TransactionId = transaction.TransactionId,
            IsSuspicious = isHighRisk,
            Risk = isHighRisk ? RiskLevel.High : RiskLevel.None,
            Reason = isHighRisk
                ? $"Crypto transaction from high-risk country: {transaction.OriginCountry} on {transaction.ExchangeName}."
                : "Crypto transaction origin acceptable.",
            Confidence = isHighRisk ? 0.80m : 0.05m
        };
    }
}
