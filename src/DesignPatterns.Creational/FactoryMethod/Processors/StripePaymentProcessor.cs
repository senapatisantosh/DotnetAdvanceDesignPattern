namespace DesignPatterns.Creational.FactoryMethod.Processors;

/// <summary>
/// Stripe payment processor implementation.
/// Encapsulates all Stripe-specific logic: fee calculation, transaction ID format, API conventions.
/// </summary>
public sealed class StripePaymentProcessor : IPaymentProcessor
{
    private const decimal PercentageFee = 0.029m;
    private const decimal FixedFee = 0.30m;

    public string GatewayName => "Stripe";
    public bool SupportsRecurring => true;

    public PaymentResult ProcessPayment(decimal amount, string currency, string customerToken)
    {
        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Amount must be positive.");

        if (string.IsNullOrWhiteSpace(customerToken))
            return PaymentResult.Failure(GatewayName, "Customer token is required.");

        // Stripe charges 2.9% + $0.30 per transaction
        var fee = amount * PercentageFee + FixedFee;
        var transactionId = $"stripe_ch_{Guid.NewGuid():N}";

        return PaymentResult.Success(transactionId, GatewayName, amount + fee, currency);
    }

    public PaymentResult Refund(string transactionId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || !transactionId.StartsWith("stripe_"))
            return PaymentResult.Failure(GatewayName, "Invalid Stripe transaction ID.");

        if (amount <= 0)
            return PaymentResult.Failure(GatewayName, "Refund amount must be positive.");

        var refundId = $"stripe_re_{Guid.NewGuid():N}";
        return PaymentResult.Success(refundId, GatewayName, amount);
    }
}
