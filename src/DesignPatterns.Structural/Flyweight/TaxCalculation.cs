namespace DesignPatterns.Structural.Flyweight;

/// <summary>
/// Context / extrinsic state — represents a single tax calculation for a
/// specific transaction. Each transaction is unique (amount, date, line items)
/// but the <see cref="TaxRate"/> flyweight is shared across many transactions
/// in the same jurisdiction.
/// </summary>
public sealed record TaxCalculation
{
    /// <summary>Reference to the shared flyweight (intrinsic state).</summary>
    public required TaxRate TaxRate { get; init; }

    /// <summary>Transaction-specific taxable amount (extrinsic state).</summary>
    public required decimal TaxableAmount { get; init; }

    /// <summary>Transaction date (extrinsic state).</summary>
    public required DateTime TransactionDate { get; init; }

    /// <summary>Transaction or line item ID (extrinsic state).</summary>
    public required string TransactionId { get; init; }

    /// <summary>Computed tax amount using the shared flyweight's rate.</summary>
    public decimal TaxAmount => TaxRate.CalculateTax(TaxableAmount);

    /// <summary>Total amount including tax.</summary>
    public decimal TotalAmount => TaxableAmount + TaxAmount;

    public override string ToString() =>
        $"Transaction {TransactionId}: {TaxableAmount:C} + {TaxAmount:C} tax " +
        $"({TaxRate.JurisdictionCode} @ {TaxRate.Rate:P2}) = {TotalAmount:C}";
}
