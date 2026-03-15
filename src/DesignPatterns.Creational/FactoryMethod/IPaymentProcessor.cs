namespace DesignPatterns.Creational.FactoryMethod;

/// <summary>
/// Defines the contract for all payment processors.
/// Each gateway implementation handles its own protocol and API specifics.
/// </summary>
public interface IPaymentProcessor
{
    /// <summary>
    /// The name of the payment gateway (e.g., "Stripe", "PayPal").
    /// </summary>
    string GatewayName { get; }

    /// <summary>
    /// Whether this processor supports recurring/subscription billing.
    /// </summary>
    bool SupportsRecurring { get; }

    /// <summary>
    /// Process a one-time payment charge.
    /// </summary>
    PaymentResult ProcessPayment(decimal amount, string currency, string customerToken);

    /// <summary>
    /// Issue a full or partial refund for a previous transaction.
    /// </summary>
    PaymentResult Refund(string transactionId, decimal amount);
}
