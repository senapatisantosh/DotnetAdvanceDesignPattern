namespace DesignPatterns.Creational.Builder;

/// <summary>
/// Classic Builder interface — declares all possible construction steps.
/// Each step returns void; the Director controls the order.
/// </summary>
public interface IInvoiceBuilder
{
    void SetInvoiceNumber(string number);
    void SetDates(DateTimeOffset issued, DateTimeOffset due);
    void SetSeller(string name, string address);
    void SetBuyer(string name, string address);
    void AddLineItem(string description, int quantity, decimal unitPrice, decimal discountPercent = 0, string? taxCategory = null);
    void SetCurrency(string currency);
    void SetTaxRate(decimal taxRate);
    void SetGlobalDiscount(decimal discountPercent);
    void SetHeaderNote(string note);
    void SetFooterNote(string note);
    void SetPaymentTerms(string terms);
    void SetPurchaseOrderNumber(string poNumber);
    Invoice Build();
    void Reset();
}
