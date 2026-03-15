using DesignPatterns.Behavioral.Strategy.Strategies;

namespace DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Factory that selects the optimal pricing strategy based on order details.
/// Combines Strategy pattern with Factory pattern for runtime strategy selection.
/// </summary>
public sealed class PricingStrategyFactory
{
    public IPricingStrategy GetStrategy(OrderDetails order)
    {
        // Priority: Promotional > Loyalty > Premium > Volume > Standard
        if (order.IsPromotionActive && order.PromotionDiscountPercent > 0)
            return new PromotionalPricingStrategy();

        if (order.LoyaltyPoints >= 500)
            return new LoyaltyPricingStrategy();

        if (order.CustomerTier.Equals("Premium", StringComparison.OrdinalIgnoreCase))
            return new PremiumPricingStrategy();

        if (order.Quantity >= 10)
            return new VolumeDiscountStrategy();

        return new StandardPricingStrategy();
    }

    /// <summary>
    /// Calculate the best price by evaluating all strategies and returning the lowest.
    /// </summary>
    public (decimal BestPrice, string StrategyName) GetBestPrice(OrderDetails order)
    {
        IPricingStrategy[] allStrategies =
        [
            new StandardPricingStrategy(),
            new PremiumPricingStrategy(),
            new VolumeDiscountStrategy(),
            new LoyaltyPricingStrategy(),
            new PromotionalPricingStrategy()
        ];

        var best = allStrategies
            .Select(s => (Price: s.CalculatePrice(order), Strategy: s.Name))
            .MinBy(x => x.Price);

        return (best.Price, best.Strategy);
    }
}
