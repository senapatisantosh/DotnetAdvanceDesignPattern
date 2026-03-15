namespace DesignPatterns.Creational.Builder.StepBuilder;

/// <summary>
/// Step Builder pattern — enforces a specific build order at compile time.
/// Each step returns the NEXT step's interface, so the compiler prevents
/// calling methods out of order.
///
/// This is especially useful when certain fields are mandatory and must
/// be set before optional fields.
///
/// Usage:
///   var invoice = InvoiceStepBuilder.Create()
///       .WithInvoiceNumber("INV-001")       // returns IBuyerStep
///       .WithBuyer("Acme", "123 Main St")   // returns ILineItemStep
///       .AddLineItem("Widget", 5, 10.00m)   // returns ILineItemStep
///       .WithTaxRate(8.25m)                  // returns IOptionalStep
///       .Build();                            // returns Invoice
/// </summary>

// Step 1: Must set invoice number first.
public interface IInvoiceNumberStep
{
    IBuyerStep WithInvoiceNumber(string number);
}

// Step 2: Must identify the buyer.
public interface IBuyerStep
{
    ILineItemStep WithBuyer(string name, string address);
}

// Step 3: Must add at least one line item.
public interface ILineItemStep
{
    ILineItemStep AddLineItem(string description, int quantity, decimal unitPrice, decimal discountPercent = 0);
    IOptionalStep WithTaxRate(decimal taxRate);
    Invoice Build();
}

// Step 4: Optional configuration — tax, discounts, notes, then build.
public interface IOptionalStep
{
    IOptionalStep WithSeller(string name, string address);
    IOptionalStep WithCurrency(string currency);
    IOptionalStep WithGlobalDiscount(decimal discountPercent);
    IOptionalStep WithHeaderNote(string note);
    IOptionalStep WithFooterNote(string note);
    IOptionalStep WithPaymentTerms(string terms);
    IOptionalStep WithDueDate(DateTimeOffset dueDate);
    Invoice Build();
}
