namespace DesignPatterns.Creational.Builder;

/// <summary>
/// Fluent Builder variant — every method returns `this` so calls can be chained.
/// This is the most common builder style in modern C# (think: StringBuilder, HttpRequestMessage).
///
/// Usage:
///   var invoice = new FluentInvoiceBuilder()
///       .WithInvoiceNumber("INV-001")
///       .WithBuyer("Acme Corp", "123 Main St")
///       .AddLineItem("Consulting", 10, 150m)
///       .WithTaxRate(8.25m)
///       .Build();
/// </summary>
public sealed class FluentInvoiceBuilder
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

    public FluentInvoiceBuilder WithInvoiceNumber(string number)
    {
        _invoiceNumber = number;
        return this;
    }

    public FluentInvoiceBuilder WithDates(DateTimeOffset issued, DateTimeOffset due)
    {
        _issuedDate = issued;
        _dueDate = due;
        return this;
    }

    public FluentInvoiceBuilder IssuedOn(DateTimeOffset issued)
    {
        _issuedDate = issued;
        return this;
    }

    public FluentInvoiceBuilder DueOn(DateTimeOffset due)
    {
        _dueDate = due;
        return this;
    }

    public FluentInvoiceBuilder WithSeller(string name, string address)
    {
        _sellerName = name;
        _sellerAddress = address;
        return this;
    }

    public FluentInvoiceBuilder WithBuyer(string name, string address)
    {
        _buyerName = name;
        _buyerAddress = address;
        return this;
    }

    public FluentInvoiceBuilder AddLineItem(string description, int quantity, decimal unitPrice,
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
        return this;
    }

    public FluentInvoiceBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public FluentInvoiceBuilder WithTaxRate(decimal taxRate)
    {
        _taxRate = taxRate;
        return this;
    }

    public FluentInvoiceBuilder WithGlobalDiscount(decimal discountPercent)
    {
        _globalDiscountPercent = discountPercent;
        return this;
    }

    public FluentInvoiceBuilder WithHeaderNote(string note)
    {
        _headerNote = note;
        return this;
    }

    public FluentInvoiceBuilder WithFooterNote(string note)
    {
        _footerNote = note;
        return this;
    }

    public FluentInvoiceBuilder WithPaymentTerms(string terms)
    {
        _paymentTerms = terms;
        return this;
    }

    public FluentInvoiceBuilder WithPurchaseOrderNumber(string poNumber)
    {
        _poNumber = poNumber;
        return this;
    }

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
}
