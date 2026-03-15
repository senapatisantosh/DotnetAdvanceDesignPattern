namespace DesignPatterns.Behavioral.Strategy.Strategies;

/// <summary>
/// Promotional pricing — applies a configurable promotional discount.
/// Falls back to standard pricing if no promotion is active.
/// </summary>
public sealed class PromotionalPricingStrategy : IPricingStrategy
{
    public string Name => "Promotional";

    public decimal CalculatePrice(OrderDetails order)
    {
        if (!order.IsPromotionActive || order.PromotionDiscountPercent <= 0)
        {
            // No active promotion — charge standard price
            return order.Subtotal;
        }

        var discountRate = Math.Min(order.PromotionDiscountPercent / 100m, 0.50m); // Cap at 50%
        return Math.Round(order.Subtotal * (1 - discountRate), 2);
    }
}
