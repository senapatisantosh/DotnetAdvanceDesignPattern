namespace DesignPatterns.Creational.Builder;

/// <summary>
/// Classic (GoF) Builder implementation.
/// Each method sets one aspect of the invoice. The Director calls methods in the right order.
/// The builder accumulates state, then Build() produces the final immutable Invoice.
/// </summary>
public sealed class StandardInvoiceBuilder : IInvoiceBuilder
{
    private string _invoiceNumber = string.Empty;
    private DateTimeOffset _issuedDate = DateTimeOffset.UtcNow;
    private DateTimeOffset _dueDate;
    private string _sellerName = string.Empty;
    private string _sellerAddress = string.Empty;
    private string _buyerName = string.Empty;
    private string _buyerAddress = string.Empty;
    private readonly List<InvoiceLineItem> _lineItems = [];
    private string _currency = "USD";
    private decimal _taxRate;
    private decimal _globalDiscountPercent;
    private string? _headerNote;
    private string? _footerNote;
    private string? _paymentTerms;
    private string? _poNumber;

    public void SetInvoiceNumber(string number) => _invoiceNumber = number;

    public void SetDates(DateTimeOffset issued, DateTimeOffset due)
    {
        _issuedDate = issued;
        _dueDate = due;
    }

    public void SetSeller(string name, string address)
    {
        _sellerName = name;
        _sellerAddress = address;
    }

    public void SetBuyer(string name, string address)
    {
        _buyerName = name;
        _buyerAddress = address;
    }

    public void AddLineItem(string description, int quantity, decimal unitPrice,
        decimal discountPercent = 0, string? taxCategory = null)
    {
        _lineItems.Add(new InvoiceLineItem
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            TaxCategory = taxCategory
        });
    }

    public void SetCurrency(string currency) => _currency = currency;
    public void SetTaxRate(decimal taxRate) => _taxRate = taxRate;
    public void SetGlobalDiscount(decimal discountPercent) => _globalDiscountPercent = discountPercent;
    public void SetHeaderNote(string note) => _headerNote = note;
    public void SetFooterNote(string note) => _footerNote = note;
    public void SetPaymentTerms(string terms) => _paymentTerms = terms;
    public void SetPurchaseOrderNumber(string poNumber) => _poNumber = poNumber;

    public Invoice Build()
    {
        if (string.IsNullOrWhiteSpace(_invoiceNumber))
            throw new InvalidOperationException("Invoice number is required.");

        if (string.IsNullOrWhiteSpace(_buyerName))
            throw new InvalidOperationException("Buyer name is required.");

        return new Invoice
        {
            InvoiceNumber = _invoiceNumber,
            IssuedDate = _issuedDate,
            DueDate = _dueDate,
            SellerName = _sellerName,
            SellerAddress = _sellerAddress,
            BuyerName = _buyerName,
            BuyerAddress = _buyerAddress,
            LineItems = _lineItems.ToList(),
            Currency = _currency,
            TaxRate = _taxRate,
            GlobalDiscountPercent = _globalDiscountPercent,
            HeaderNote = _headerNote,
            FooterNote = _footerNote,
            PaymentTerms = _paymentTerms,
            PurchaseOrderNumber = _poNumber
        };
    }

    public void Reset()
    {
        _invoiceNumber = string.Empty;
        _issuedDate = DateTimeOffset.UtcNow;
        _dueDate = default;
        _sellerName = string.Empty;
        _sellerAddress = string.Empty;
        _buyerName = string.Empty;
        _buyerAddress = string.Empty;
        _lineItems.Clear();
        _currency = "USD";
        _taxRate = 0;
        _globalDiscountPercent = 0;
        _headerNote = null;
        _footerNote = null;
        _paymentTerms = null;
        _poNumber = null;
    }
}
