namespace DesignPatterns.Behavioral.Strategy.Strategies;

/// <summary>
/// Premium tier pricing — 15% discount for premium customers.
/// </summary>
public sealed class PremiumPricingStrategy : IPricingStrategy
{
    private const decimal DiscountRate = 0.15m;

    public string Name => "Premium";

    public decimal CalculatePrice(OrderDetails order)
    {
        return Math.Round(order.Subtotal * (1 - DiscountRate), 2);
    }
}
