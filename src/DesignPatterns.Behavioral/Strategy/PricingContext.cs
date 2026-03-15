namespace DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Context that uses a pricing strategy. The strategy can be swapped at runtime.
/// </summary>
public sealed class PricingContext
{
    private IPricingStrategy _strategy;

    public PricingContext(IPricingStrategy strategy)
    {
        _strategy = strategy;
    }

    public string CurrentStrategyName => _strategy.Name;

    public void SetStrategy(IPricingStrategy strategy)
    {
        _strategy = strategy;
    }

    public decimal CalculatePrice(OrderDetails order)
    {
        return _strategy.CalculatePrice(order);
    }
}
