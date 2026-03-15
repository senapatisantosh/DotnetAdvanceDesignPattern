using DesignPatterns.Creational.FactoryMethod.Processors;

namespace DesignPatterns.Creational.FactoryMethod.StaticFactory;

/// <summary>
/// Static Factory Method variant — uses static methods to create instances.
///
/// This is the approach used by .NET itself:
///   - TimeSpan.FromMinutes(5)
///   - Task.FromResult(value)
///   - HttpClient.DefaultRequestHeaders
///
/// Pros: Very readable, self-documenting, no factory instance needed.
/// Cons: Cannot be overridden (not polymorphic), harder to mock in tests.
/// </summary>
public static class PaymentProcessorStaticFactory
{
    /// <summary>Creates a Stripe payment processor.</summary>
    public static IPaymentProcessor CreateStripe() => new StripePaymentProcessor();

    /// <summary>Creates a PayPal payment processor.</summary>
    public static IPaymentProcessor CreatePayPal() => new PayPalPaymentProcessor();

    /// <summary>Creates a Square payment processor.</summary>
    public static IPaymentProcessor CreateSquare() => new SquarePaymentProcessor();

    /// <summary>
    /// Creates a processor from a gateway name string.
    /// Combines static factory with pattern matching for flexibility.
    /// </summary>
    public static IPaymentProcessor CreateFromName(string gatewayName) =>
        gatewayName.ToLowerInvariant() switch
        {
            "stripe" => new StripePaymentProcessor(),
            "paypal" => new PayPalPaymentProcessor(),
            "square" => new SquarePaymentProcessor(),
            _ => throw new ArgumentException($"Unknown gateway: '{gatewayName}'", nameof(gatewayName))
        };
}
