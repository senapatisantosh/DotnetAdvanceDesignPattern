using DesignPatterns.Creational.FactoryMethod.Processors;

namespace DesignPatterns.Creational.FactoryMethod.SimpleFactory;

/// <summary>
/// Simple Factory (not a GoF pattern, but widely used in practice).
///
/// Unlike the Factory Method pattern, the Simple Factory is a single class
/// that encapsulates the creation logic. It is NOT a pattern in the GoF book,
/// but it is often the first step toward the Factory Method pattern.
///
/// Pros: Centralizes creation logic in one place, simpler than full Factory Method.
/// Cons: Still uses a switch/mapping — adding gateways requires modifying this class.
///       However, it is much better than scattering switch statements everywhere.
/// </summary>
public sealed class PaymentProcessorSimpleFactory
{
    private static readonly Dictionary<string, Func<IPaymentProcessor>> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["stripe"] = () => new StripePaymentProcessor(),
        ["paypal"] = () => new PayPalPaymentProcessor(),
        ["square"] = () => new SquarePaymentProcessor(),
    };

    /// <summary>
    /// Creates a payment processor by gateway name.
    /// Uses a dictionary-based approach instead of switch to make extension slightly easier.
    /// </summary>
    public IPaymentProcessor Create(string gatewayName)
    {
        if (Registry.TryGetValue(gatewayName, out var factory))
            return factory();

        throw new ArgumentException(
            $"Unknown payment gateway: '{gatewayName}'. Supported: {string.Join(", ", Registry.Keys)}",
            nameof(gatewayName));
    }

    /// <summary>
    /// Returns all supported gateway names.
    /// </summary>
    public IReadOnlyCollection<string> SupportedGateways => Registry.Keys;

    /// <summary>
    /// Allows runtime registration of new gateways (plugin-style extensibility).
    /// </summary>
    public void Register(string gatewayName, Func<IPaymentProcessor> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gatewayName);
        ArgumentNullException.ThrowIfNull(factory);
        Registry[gatewayName.ToLowerInvariant()] = factory;
    }
}
