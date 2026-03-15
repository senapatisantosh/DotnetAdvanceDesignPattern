namespace DesignPatterns.Structural.Flyweight;

/// <summary>
/// Flyweight — stores the intrinsic (shared) state: tax jurisdiction info
/// and rate that doesn't change per transaction. A single TaxRate instance
/// is shared across millions of transactions for the same jurisdiction.
///
/// Immutable record ensures thread safety for shared instances.
/// </summary>
public sealed record TaxRate
{
    /// <summary>Jurisdiction code (e.g., "US-CA", "US-TX", "DE-BY").</summary>
    public required string JurisdictionCode { get; init; }

    /// <summary>Human-readable jurisdiction name.</summary>
    public required string JurisdictionName { get; init; }

    /// <summary>Tax rate as a decimal (e.g., 0.0725 for 7.25%).</summary>
    public required decimal Rate { get; init; }

    /// <summary>Type of tax (Sales, VAT, GST, etc.).</summary>
    public required TaxType Type { get; init; }

    /// <summary>Effective date of this rate.</summary>
    public required DateTime EffectiveDate { get; init; }

    /// <summary>Calculates tax amount for a given taxable amount (extrinsic state).</summary>
    public decimal CalculateTax(decimal taxableAmount) =>
        Math.Round(taxableAmount * Rate, 2, MidpointRounding.AwayFromZero);

    public override string ToString() =>
        $"{JurisdictionName} ({JurisdictionCode}): {Rate:P2} {Type}";
}

public enum TaxType
{
    Sales,
    ValueAddedTax,
    GoodsAndServicesTax,
    ExciseTax
}
