namespace DesignPatterns.Creational.Builder;

/// <summary>
/// Represents a single line item on an invoice.
/// </summary>
public sealed record InvoiceLineItem
{
    public required string Description { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public string? TaxCategory { get; init; }

    /// <summary>Subtotal before tax: (Quantity * UnitPrice) - discount.</summary>
    public decimal Subtotal
    {
        get
        {
            var gross = Quantity * UnitPrice;
            return gross - (gross * DiscountPercent / 100m);
        }
    }
}
