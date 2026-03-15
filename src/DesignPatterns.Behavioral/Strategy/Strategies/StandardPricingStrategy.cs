namespace DesignPatterns.Behavioral.Strategy.Strategies;

/// <summary>
/// Standard pricing — no discounts applied.
/// </summary>
public sealed class StandardPricingStrategy : IPricingStrategy
{
    public string Name => "Standard";

    public decimal CalculatePrice(OrderDetails order)
    {
        return order.Subtotal;
    }
}
