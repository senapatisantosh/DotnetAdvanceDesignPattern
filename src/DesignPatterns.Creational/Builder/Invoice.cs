namespace DesignPatterns.Creational.Builder;

/// <summary>
/// The complex object that the Builder constructs.
/// An invoice has many optional parts: header, footer, line items, tax, discounts, notes.
/// A constructor with 15+ parameters would be unwieldy — hence the Builder pattern.
/// </summary>
public sealed class Invoice
{
    // Identity
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTimeOffset IssuedDate { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueDate { get; init; }

    // Parties
    public string SellerName { get; init; } = string.Empty;
    public string SellerAddress { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string BuyerAddress { get; init; } = string.Empty;

    // Line items
    public IReadOnlyList<InvoiceLineItem> LineItems { get; init; } = [];

    // Financial
    public string Currency { get; init; } = "USD";
    public decimal TaxRate { get; init; }
    public decimal GlobalDiscountPercent { get; init; }

    // Extras
    public string? HeaderNote { get; init; }
    public string? FooterNote { get; init; }
    public string? PaymentTerms { get; init; }
    public string? PurchaseOrderNumber { get; init; }

    // Computed properties
    public decimal Subtotal => LineItems.Sum(li => li.Subtotal);

    public decimal DiscountAmount => Subtotal * GlobalDiscountPercent / 100m;

    public decimal TaxableAmount => Subtotal - DiscountAmount;

    public decimal TaxAmount => TaxableAmount * TaxRate / 100m;

    public decimal Total => TaxableAmount + TaxAmount;

    public string FormattedTotal => $"{Currency} {Total:N2}";

    public override string ToString() =>
        $"Invoice {InvoiceNumber} | {BuyerName} | {FormattedTotal} | Due: {DueDate:yyyy-MM-dd}";
}
