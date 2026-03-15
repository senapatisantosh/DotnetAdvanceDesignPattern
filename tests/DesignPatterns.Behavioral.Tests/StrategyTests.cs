using DesignPatterns.Behavioral.Strategy;
using DesignPatterns.Behavioral.Strategy.Strategies;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class StrategyTests
{
    private static OrderDetails CreateOrder(
        decimal basePrice = 100m, int quantity = 1, string tier = "Standard",
        int loyaltyPoints = 0, bool promo = false, decimal promoPercent = 0) => new()
    {
        BasePrice = basePrice,
        Quantity = quantity,
        CustomerTier = tier,
        LoyaltyPoints = loyaltyPoints,
        IsPromotionActive = promo,
        PromotionDiscountPercent = promoPercent
    };

    [Fact]
    public void StandardPricing_ReturnsFullPrice()
    {
        var strategy = new StandardPricingStrategy();
        var order = CreateOrder(basePrice: 50, quantity: 3);

        strategy.CalculatePrice(order).Should().Be(150m);
    }

    [Fact]
    public void PremiumPricing_Applies15PercentDiscount()
    {
        var strategy = new PremiumPricingStrategy();
        var order = CreateOrder(basePrice: 100, quantity: 2);

        strategy.CalculatePrice(order).Should().Be(170m); // 200 * 0.85
    }

    [Theory]
    [InlineData(5, 500)]    // No volume discount
    [InlineData(10, 900)]   // 10% off
    [InlineData(50, 4000)]  // 20% off
    [InlineData(100, 7000)] // 30% off
    public void VolumeDiscount_AppliesTieredDiscount(int quantity, decimal expected)
    {
        var strategy = new VolumeDiscountStrategy();
        var order = CreateOrder(basePrice: 100, quantity: quantity);

        strategy.CalculatePrice(order).Should().Be(expected);
    }

    [Fact]
    public void LoyaltyPricing_ConvertsPointsToDiscount()
    {
        var strategy = new LoyaltyPricingStrategy();
        // 1000 points = $10 off, subtotal = $200
        var order = CreateOrder(basePrice: 100, quantity: 2, loyaltyPoints: 1000);

        strategy.CalculatePrice(order).Should().Be(190m);
    }

    [Fact]
    public void LoyaltyPricing_CapsAt25Percent()
    {
        var strategy = new LoyaltyPricingStrategy();
        // 100000 points = $1000 off, but subtotal = $200, max 25% = $50
        var order = CreateOrder(basePrice: 100, quantity: 2, loyaltyPoints: 100000);

        strategy.CalculatePrice(order).Should().Be(150m); // 200 - 50
    }

    [Fact]
    public void PromotionalPricing_AppliesConfiguredDiscount()
    {
        var strategy = new PromotionalPricingStrategy();
        var order = CreateOrder(basePrice: 100, quantity: 1, promo: true, promoPercent: 20);

        strategy.CalculatePrice(order).Should().Be(80m);
    }

    [Fact]
    public void PromotionalPricing_CapsAt50Percent()
    {
        var strategy = new PromotionalPricingStrategy();
        var order = CreateOrder(basePrice: 100, quantity: 1, promo: true, promoPercent: 75);

        strategy.CalculatePrice(order).Should().Be(50m);
    }

    [Fact]
    public void PromotionalPricing_FallsBackToStandard_WhenNoPromotion()
    {
        var strategy = new PromotionalPricingStrategy();
        var order = CreateOrder(basePrice: 100, quantity: 1);

        strategy.CalculatePrice(order).Should().Be(100m);
    }

    [Fact]
    public void PricingContext_CanSwapStrategies()
    {
        var order = CreateOrder(basePrice: 100, quantity: 1);
        var context = new PricingContext(new StandardPricingStrategy());

        context.CalculatePrice(order).Should().Be(100m);

        context.SetStrategy(new PremiumPricingStrategy());
        context.CalculatePrice(order).Should().Be(85m);
    }

    [Fact]
    public void PricingStrategyFactory_SelectsOptimalStrategy()
    {
        var factory = new PricingStrategyFactory();

        factory.GetStrategy(CreateOrder(promo: true, promoPercent: 20))
            .Should().BeOfType<PromotionalPricingStrategy>();

        factory.GetStrategy(CreateOrder(loyaltyPoints: 600))
            .Should().BeOfType<LoyaltyPricingStrategy>();

        factory.GetStrategy(CreateOrder(tier: "Premium"))
            .Should().BeOfType<PremiumPricingStrategy>();

        factory.GetStrategy(CreateOrder(quantity: 15))
            .Should().BeOfType<VolumeDiscountStrategy>();

        factory.GetStrategy(CreateOrder())
            .Should().BeOfType<StandardPricingStrategy>();
    }

    [Fact]
    public void PricingStrategyFactory_GetBestPrice_ReturnsLowest()
    {
        var factory = new PricingStrategyFactory();
        var order = CreateOrder(basePrice: 100, quantity: 10, tier: "Premium",
            loyaltyPoints: 1000, promo: true, promoPercent: 20);

        var (bestPrice, strategyName) = factory.GetBestPrice(order);

        bestPrice.Should().BeLessThan(order.Subtotal);
        strategyName.Should().NotBeNullOrEmpty();
    }
}
