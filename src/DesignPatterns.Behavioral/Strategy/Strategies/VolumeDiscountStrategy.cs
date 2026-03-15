namespace DesignPatterns.Behavioral.Strategy.Strategies;

/// <summary>
/// Volume discount — tiered discounts based on quantity.
///   1-9 items: no discount
///   10-49 items: 10% off
///   50-99 items: 20% off
///   100+ items: 30% off
/// </summary>
public sealed class VolumeDiscountStrategy : IPricingStrategy
{
    public string Name => "VolumeDiscount";

    public decimal CalculatePrice(OrderDetails order)
    {
        var discountRate = order.Quantity switch
        {
            >= 100 => 0.30m,
            >= 50 => 0.20m,
            >= 10 => 0.10m,
            _ => 0m
        };

        return Math.Round(order.Subtotal * (1 - discountRate), 2);
    }
}
