namespace DesignPatterns.Creational.FactoryMethod.Naive;

/// <summary>
/// THE NAIVE APPROACH — demonstrates why the Factory Method pattern is needed.
///
/// Problems with this approach:
/// 1. Open/Closed Principle violation: adding a new gateway requires modifying this class.
/// 2. Single Responsibility Principle violation: one class knows about ALL gateways.
/// 3. Tight coupling: the consuming code is bound to concrete gateway implementations.
/// 4. Hard to test: you cannot mock individual gateways without modifying this class.
/// 5. Switch/if-else chains grow unbounded as gateways are added.
/// </summary>
public class NaivePaymentProcessor
{
    public PaymentResult ProcessPayment(string gateway, decimal amount, string currency, string customerToken)
    {
        // Every new gateway means modifying this method — violates Open/Closed Principle.
        switch (gateway.ToLowerInvariant())
        {
            case "stripe":
                // Imagine dozens of lines of Stripe-specific API calls here...
                var stripeFee = amount * 0.029m + 0.30m;
                return PaymentResult.Success(
                    transactionId: $"stripe_ch_{Guid.NewGuid():N}",
                    gateway: "Stripe",
                    amount: amount + stripeFee,
                    currency: currency);

            case "paypal":
                // PayPal-specific logic, different SDK, different error handling...
                var paypalFee = amount * 0.0349m + 0.49m;
                return PaymentResult.Success(
                    transactionId: $"PAYPAL-{Guid.NewGuid():N}",
                    gateway: "PayPal",
                    amount: amount + paypalFee,
                    currency: currency);

            case "square":
                // Square-specific logic...
                var squareFee = amount * 0.026m + 0.10m;
                return PaymentResult.Success(
                    transactionId: $"sq_{Guid.NewGuid():N}",
                    gateway: "Square",
                    amount: amount + squareFee,
                    currency: currency);

            default:
                throw new NotSupportedException($"Payment gateway '{gateway}' is not supported.");
        }
    }

    public PaymentResult Refund(string gateway, string transactionId, decimal amount)
    {
        // Same problem repeated — every method must switch on gateway type.
        switch (gateway.ToLowerInvariant())
        {
            case "stripe":
                return PaymentResult.Success($"stripe_re_{Guid.NewGuid():N}", "Stripe", amount);
            case "paypal":
                return PaymentResult.Success($"PAYPAL-REFUND-{Guid.NewGuid():N}", "PayPal", amount);
            case "square":
                return PaymentResult.Success($"sq_refund_{Guid.NewGuid():N}", "Square", amount);
            default:
                throw new NotSupportedException($"Payment gateway '{gateway}' is not supported.");
        }
    }
}
