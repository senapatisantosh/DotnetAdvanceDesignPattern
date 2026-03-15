namespace DesignPatterns.Behavioral.Strategy.Strategies;

/// <summary>
/// Loyalty pricing — converts loyalty points to discounts.
/// Every 100 loyalty points = $1 off, up to a maximum of 25% off.
/// </summary>
public sealed class LoyaltyPricingStrategy : IPricingStrategy
{
    private const decimal PointsPerDollar = 100m;
    private const decimal MaxDiscountPercent = 0.25m;

    public string Name => "Loyalty";

    public decimal CalculatePrice(OrderDetails order)
    {
        var pointDiscount = order.LoyaltyPoints / PointsPerDollar;
        var maxDiscount = order.Subtotal * MaxDiscountPercent;
        var actualDiscount = Math.Min(pointDiscount, maxDiscount);

        return Math.Round(order.Subtotal - actualDiscount, 2);
    }
}
