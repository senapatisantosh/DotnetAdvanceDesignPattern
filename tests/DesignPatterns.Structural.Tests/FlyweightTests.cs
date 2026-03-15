using DesignPatterns.Structural.Flyweight;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class FlyweightTests
{
    [Fact]
    public void TaxRate_CalculateTax_ComputesCorrectly()
    {
        var taxRate = new TaxRate
        {
            JurisdictionCode = "US-CA",
            JurisdictionName = "California",
            Rate = 0.0725m,
            Type = TaxType.Sales,
            EffectiveDate = new DateTime(2024, 1, 1)
        };

        var tax = taxRate.CalculateTax(100.00m);

        tax.Should().Be(7.25m);
    }

    [Fact]
    public void TaxRate_CalculateTax_RoundsCorrectly()
    {
        var taxRate = new TaxRate
        {
            JurisdictionCode = "US-NY",
            JurisdictionName = "New York",
            Rate = 0.08m,
            Type = TaxType.Sales,
            EffectiveDate = new DateTime(2024, 1, 1)
        };

        var tax = taxRate.CalculateTax(33.33m);

        tax.Should().Be(2.67m); // 33.33 * 0.08 = 2.6664, rounded to 2.67
    }

    [Fact]
    public void TaxRateFactory_ReturnsSameInstance_ForSameJurisdiction()
    {
        var factory = new TaxRateFactory();

        var rate1 = factory.GetTaxRate("US-CA");
        var rate2 = factory.GetTaxRate("US-CA");

        rate1.Should().BeSameAs(rate2); // Same reference — flyweight sharing
        factory.PoolSize.Should().Be(1);
    }

    [Fact]
    public void TaxRateFactory_ReturnsDifferentInstances_ForDifferentJurisdictions()
    {
        var factory = new TaxRateFactory();

        var ca = factory.GetTaxRate("US-CA");
        var tx = factory.GetTaxRate("US-TX");

        ca.Should().NotBeSameAs(tx);
        ca.Rate.Should().NotBe(tx.Rate);
        factory.PoolSize.Should().Be(2);
    }

    [Fact]
    public void TaxRateFactory_PreloadCommonRates_PopulatesPool()
    {
        var factory = new TaxRateFactory();

        factory.PreloadCommonRates();

        factory.PoolSize.Should().BeGreaterOrEqualTo(8);
        factory.GetAllCachedRates().Should().ContainKey("US-CA");
        factory.GetAllCachedRates().Should().ContainKey("US-TX");
    }

    [Fact]
    public void TaxRateFactory_CustomRate_CanBeProvided()
    {
        var factory = new TaxRateFactory();

        var customRate = factory.GetTaxRate("US-CUSTOM", () => new TaxRate
        {
            JurisdictionCode = "US-CUSTOM",
            JurisdictionName = "Custom Jurisdiction",
            Rate = 0.099m,
            Type = TaxType.Sales,
            EffectiveDate = DateTime.UtcNow
        });

        customRate.Rate.Should().Be(0.099m);
        customRate.JurisdictionName.Should().Be("Custom Jurisdiction");
    }

    [Fact]
    public void TaxRateFactory_UnknownJurisdiction_ReturnsZeroRate()
    {
        var factory = new TaxRateFactory();

        var unknown = factory.GetTaxRate("XX-UNKNOWN");

        unknown.Rate.Should().Be(0.0m);
        unknown.JurisdictionName.Should().Contain("Unknown");
    }

    [Fact]
    public void TaxCalculation_ComputesTaxAndTotal()
    {
        var taxRate = new TaxRate
        {
            JurisdictionCode = "US-CA",
            JurisdictionName = "California",
            Rate = 0.0725m,
            Type = TaxType.Sales,
            EffectiveDate = new DateTime(2024, 1, 1)
        };

        var calculation = new TaxCalculation
        {
            TaxRate = taxRate,
            TaxableAmount = 100.00m,
            TransactionDate = DateTime.UtcNow,
            TransactionId = "TXN-001"
        };

        calculation.TaxAmount.Should().Be(7.25m);
        calculation.TotalAmount.Should().Be(107.25m);
    }

    [Fact]
    public void TaxCalculationEngine_ProcessesBatch_SharesFlyweights()
    {
        var factory = new TaxRateFactory();
        var engine = new TaxCalculationEngine(factory);

        // 1000 transactions across 3 jurisdictions
        var transactions = Enumerable.Range(1, 1000)
            .Select(i =>
            {
                var jurisdiction = (i % 3) switch
                {
                    0 => "US-CA",
                    1 => "US-TX",
                    _ => "US-NY"
                };
                return ($"TXN-{i}", (decimal)(i * 10), jurisdiction, DateTime.UtcNow);
            })
            .ToList();

        var results = engine.ProcessBatch(transactions);

        // 1000 transactions but only 3 flyweight instances
        results.Should().HaveCount(1000);
        engine.GetFlyweightPoolSize().Should().Be(3);

        // Verify flyweight sharing: all CA transactions reference the same TaxRate object
        var caCalculations = results.Where(r => r.TaxRate.JurisdictionCode == "US-CA").ToList();
        caCalculations.Should().NotBeEmpty();
        caCalculations.Select(c => c.TaxRate).Distinct().Should().HaveCount(1);
    }

    [Fact]
    public void TaxCalculationEngine_SummarizeByJurisdiction_AggregatesCorrectly()
    {
        var factory = new TaxRateFactory();
        var engine = new TaxCalculationEngine(factory);

        var calculations = new[]
        {
            engine.CalculateTax("TXN-1", 100m, "US-CA", DateTime.UtcNow),
            engine.CalculateTax("TXN-2", 200m, "US-CA", DateTime.UtcNow),
            engine.CalculateTax("TXN-3", 100m, "US-TX", DateTime.UtcNow)
        };

        var summary = engine.SummarizeByJurisdiction(calculations);

        summary.Should().ContainKey("US-CA");
        summary.Should().ContainKey("US-TX");
        summary["US-CA"].Should().Be(100m * 0.0725m + 200m * 0.0725m);
        summary["US-TX"].Should().Be(100m * 0.0625m);
    }

    [Fact]
    public void TaxRateFactory_InternationalRates_AreAvailable()
    {
        var factory = new TaxRateFactory();

        var germany = factory.GetTaxRate("DE");
        var uk = factory.GetTaxRate("GB");
        var australia = factory.GetTaxRate("AU");

        germany.Rate.Should().Be(0.19m);
        germany.Type.Should().Be(TaxType.ValueAddedTax);

        uk.Rate.Should().Be(0.20m);
        uk.Type.Should().Be(TaxType.ValueAddedTax);

        australia.Rate.Should().Be(0.10m);
        australia.Type.Should().Be(TaxType.GoodsAndServicesTax);
    }

    [Fact]
    public void TaxRate_ZeroRate_CalculatesZeroTax()
    {
        var factory = new TaxRateFactory();
        var oregon = factory.GetTaxRate("US-OR"); // Oregon has no sales tax

        oregon.Rate.Should().Be(0.0m);
        oregon.CalculateTax(500m).Should().Be(0.0m);
    }
}
