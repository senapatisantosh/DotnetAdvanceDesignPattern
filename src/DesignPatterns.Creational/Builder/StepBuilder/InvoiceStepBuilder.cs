namespace DesignPatterns.Creational.Builder.StepBuilder;

/// <summary>
/// Step Builder implementation — a single class implements all step interfaces.
/// Each method returns `this` cast to the next step's interface type,
/// ensuring compile-time enforcement of the build order.
/// </summary>
public sealed class InvoiceStepBuilder : IInvoiceNumberStep, IBuyerStep, ILineItemStep, IOptionalStep
{
    private string _invoiceNumber = string.Empty;
    private string _buyerName = string.Empty;
    private string _buyerAddress = string.Empty;
    private string _sellerName = string.Empty;
    private string _sellerAddress = string.Empty;
    private readonly List<InvoiceLineItem> _lineItems = [];
    private string _currency = "USD";
    private decimal _taxRate;
    private decimal _globalDiscountPercent;
    private string? _headerNote;
    private string? _footerNote;
    private string? _paymentTerms;
    private DateTimeOffset _dueDate;

    private InvoiceStepBuilder() { }

    /// <summary>
    /// Entry point — returns the first step interface.
    /// </summary>
    public static IInvoiceNumberStep Create() => new InvoiceStepBuilder();

    // Step 1: Invoice number
    public IBuyerStep WithInvoiceNumber(string number)
    {
        _invoiceNumber = number;
        return this;
    }

    // Step 2: Buyer
    public ILineItemStep WithBuyer(string name, string address)
    {
        _buyerName = name;
        _buyerAddress = address;
        return this;
    }

    // Step 3: Line items (can add multiple)
    public ILineItemStep AddLineItem(string description, int quantity, decimal unitPrice, decimal discountPercent = 0)
    {
        _lineItems.Add(new InvoiceLineItem
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent
        });
        return this;
    }

    // Transition from line items to optional steps
    public IOptionalStep WithTaxRate(decimal taxRate)
    {
        _taxRate = taxRate;
        return this;
    }

    // Optional steps
    public IOptionalStep WithSeller(string name, string address)
    {
        _sellerName = name;
        _sellerAddress = address;
        return this;
    }

    public IOptionalStep WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public IOptionalStep WithGlobalDiscount(decimal discountPercent)
    {
        _globalDiscountPercent = discountPercent;
        return this;
    }

    public IOptionalStep WithHeaderNote(string note)
    {
        _headerNote = note;
        return this;
    }

    public IOptionalStep WithFooterNote(string note)
    {
        _footerNote = note;
        return this;
    }

    public IOptionalStep WithPaymentTerms(string terms)
    {
        _paymentTerms = terms;
        return this;
    }

    public IOptionalStep WithDueDate(DateTimeOffset dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public Invoice Build()
    {
        return new Invoice
        {
            InvoiceNumber = _invoiceNumber,
            IssuedDate = DateTimeOffset.UtcNow,
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
            PaymentTerms = _paymentTerms
        };
    }
}
