using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Flyweight;

/// <summary>
/// Flyweight Factory — manages the pool of shared <see cref="TaxRate"/> instances.
/// Ensures that only one TaxRate object exists per jurisdiction, even when
/// processing millions of transactions.
///
/// Without flyweight: 1M transactions * ~80 bytes per TaxRate = ~80 MB
/// With flyweight: ~50 unique jurisdictions * ~80 bytes = ~4 KB
/// </summary>
public sealed class TaxRateFactory
{
    private readonly ConcurrentDictionary<string, TaxRate> _taxRates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a shared TaxRate instance for the given jurisdiction.
    /// Creates one if it doesn't exist (using the provided factory function).
    /// </summary>
    public TaxRate GetTaxRate(string jurisdictionCode, Func<TaxRate> factory)
    {
        return _taxRates.GetOrAdd(jurisdictionCode, _ => factory());
    }

    /// <summary>
    /// Returns a shared TaxRate for the given jurisdiction code,
    /// using a built-in lookup of well-known US state tax rates.
    /// </summary>
    public TaxRate GetTaxRate(string jurisdictionCode)
    {
        return _taxRates.GetOrAdd(jurisdictionCode, code => CreateWellKnownRate(code));
    }

    /// <summary>Number of unique TaxRate flyweight instances in the pool.</summary>
    public int PoolSize => _taxRates.Count;

    /// <summary>Returns all cached flyweight instances (for diagnostics).</summary>
    public IReadOnlyDictionary<string, TaxRate> GetAllCachedRates() =>
        _taxRates.AsReadOnly();

    /// <summary>Preloads common tax rates to avoid lazy creation under load.</summary>
    public void PreloadCommonRates()
    {
        var commonCodes = new[] { "US-CA", "US-TX", "US-NY", "US-FL", "US-WA", "US-IL", "US-PA", "US-OH" };
        foreach (var code in commonCodes)
        {
            GetTaxRate(code);
        }
    }

    private static TaxRate CreateWellKnownRate(string jurisdictionCode)
    {
        return jurisdictionCode.ToUpperInvariant() switch
        {
            "US-CA" => new TaxRate { JurisdictionCode = "US-CA", JurisdictionName = "California", Rate = 0.0725m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-TX" => new TaxRate { JurisdictionCode = "US-TX", JurisdictionName = "Texas", Rate = 0.0625m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-NY" => new TaxRate { JurisdictionCode = "US-NY", JurisdictionName = "New York", Rate = 0.08m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-FL" => new TaxRate { JurisdictionCode = "US-FL", JurisdictionName = "Florida", Rate = 0.06m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-WA" => new TaxRate { JurisdictionCode = "US-WA", JurisdictionName = "Washington", Rate = 0.065m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-IL" => new TaxRate { JurisdictionCode = "US-IL", JurisdictionName = "Illinois", Rate = 0.0625m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-PA" => new TaxRate { JurisdictionCode = "US-PA", JurisdictionName = "Pennsylvania", Rate = 0.06m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-OH" => new TaxRate { JurisdictionCode = "US-OH", JurisdictionName = "Ohio", Rate = 0.0575m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "US-OR" => new TaxRate { JurisdictionCode = "US-OR", JurisdictionName = "Oregon", Rate = 0.0m, Type = TaxType.Sales, EffectiveDate = new DateTime(2024, 1, 1) },
            "DE" => new TaxRate { JurisdictionCode = "DE", JurisdictionName = "Germany", Rate = 0.19m, Type = TaxType.ValueAddedTax, EffectiveDate = new DateTime(2024, 1, 1) },
            "GB" => new TaxRate { JurisdictionCode = "GB", JurisdictionName = "United Kingdom", Rate = 0.20m, Type = TaxType.ValueAddedTax, EffectiveDate = new DateTime(2024, 1, 1) },
            "AU" => new TaxRate { JurisdictionCode = "AU", JurisdictionName = "Australia", Rate = 0.10m, Type = TaxType.GoodsAndServicesTax, EffectiveDate = new DateTime(2024, 1, 1) },
            _ => new TaxRate { JurisdictionCode = jurisdictionCode, JurisdictionName = $"Unknown ({jurisdictionCode})", Rate = 0.0m, Type = TaxType.Sales, EffectiveDate = DateTime.UtcNow }
        };
    }
}
