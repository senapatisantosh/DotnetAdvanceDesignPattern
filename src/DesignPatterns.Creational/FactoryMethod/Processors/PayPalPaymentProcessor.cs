namespace DesignPatterns.Creational.FactoryMethod.Processors;

/// <summary>
/// PayPal payment processor implementation.
/// Encapsulates PayPal-specific logic: higher fees, different ID format, PayPal conventions.
/// </summary>
public sealed class PayPalPaymentProcessor : IPaymentProcessor
{
    private const decimal PercentageFee = 0.0349m;
    private const decimal FixedFee = 0.49m;

    public string GatewayName => "PayPal";
    public bool SupportsRecurring => true;

    public PaymentResult ProcessPayment(decimal amount, string currency, string customerToken)
    {
        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Amount must be positive.");

        if (string.IsNullOrWhiteSpace(customerToken))
            return PaymentResult.Failure(GatewayName, "Customer token is required.");

        // PayPal charges 3.49% + $0.49 per transaction
        var fee = amount * PercentageFee + FixedFee;
        var transactionId = $"PAYPAL-{Guid.NewGuid():N}".ToUpperInvariant();

        return PaymentResult.Success(transactionId, GatewayName, amount + fee, currency);
    }

    public PaymentResult Refund(string transactionId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || !transactionId.StartsWith("PAYPAL-"))
            return PaymentResult.Failure(GatewayName, "Invalid PayPal transaction ID.");

        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Refund amount must be positive.");

        var refundId = $"PAYPAL-REFUND-{Guid.NewGuid():N}".ToUpperInvariant();
        return PaymentResult.Success(refundId, GatewayName, amount);
    }
}
