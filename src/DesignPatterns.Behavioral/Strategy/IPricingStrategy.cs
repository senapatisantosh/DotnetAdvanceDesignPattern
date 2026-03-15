namespace DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Strategy interface for pricing calculations.
/// </summary>
public interface IPricingStrategy
{
    string Name { get; }
    decimal CalculatePrice(OrderDetails order);
}
