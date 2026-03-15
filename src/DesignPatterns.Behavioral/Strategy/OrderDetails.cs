namespace DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Order details used as context for pricing strategy calculations.
/// </summary>
public sealed record OrderDetails
{
    public required decimal BasePrice { get; init; }
    public required int Quantity { get; init; }
    public required string CustomerTier { get; init; }
    public int LoyaltyPoints { get; init; }
    public bool IsPromotionActive { get; init; }
    public decimal PromotionDiscountPercent { get; init; }

    public decimal Subtotal => BasePrice * Quantity;
}
