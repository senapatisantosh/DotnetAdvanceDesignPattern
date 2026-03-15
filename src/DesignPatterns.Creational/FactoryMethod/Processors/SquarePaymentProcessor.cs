namespace DesignPatterns.Creational.FactoryMethod.Processors;

/// <summary>
/// Square payment processor implementation.
/// Encapsulates Square-specific logic: lowest fees, no recurring support, different ID format.
/// </summary>
public sealed class SquarePaymentProcessor : IPaymentProcessor
{
    private const decimal PercentageFee = 0.026m;
    private const decimal FixedFee = 0.10m;

    public string GatewayName => "Square";
    public bool SupportsRecurring => false;

    public PaymentResult ProcessPayment(decimal amount, string currency, string customerToken)
    {
        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Amount must be positive.");

        if (string.IsNullOrWhiteSpace(customerToken))
            return PaymentResult.Failure(GatewayName, "Customer token is required.");

        // Square charges 2.6% + $0.10 per transaction
        var fee = amount * PercentageFee + FixedFee;
        var transactionId = $"sq_{Guid.NewGuid():N}";

        return PaymentResult.Success(transactionId, GatewayName, amount + fee, currency);
    }

    public PaymentResult Refund(string transactionId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || !transactionId.StartsWith("sq_"))
            return PaymentResult.Failure(GatewayName, "Invalid Square transaction ID.");

        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Refund amount must be positive.");

        var refundId = $"sq_refund_{Guid.NewGuid():N}";
        return PaymentResult.Success(refundId, GatewayName, amount);
    }
}
