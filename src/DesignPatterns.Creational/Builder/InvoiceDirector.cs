namespace DesignPatterns.Creational.Builder;

/// <summary>
/// The Director knows HOW to build common invoice configurations.
/// It orchestrates the builder to create predefined invoice types
/// (e.g., a standard consulting invoice, a recurring subscription invoice).
///
/// The Director is optional — clients can use the Builder directly.
/// The Director is useful when you have common "recipes" that should be reusable.
/// </summary>
public sealed class InvoiceDirector
{
    private readonly IInvoiceBuilder _builder;

    public InvoiceDirector(IInvoiceBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Builds a standard consulting invoice with Net-30 payment terms.
    /// </summary>
    public Invoice BuildConsultingInvoice(string invoiceNumber, string clientName,
        string clientAddress, decimal hourlyRate, int hours)
    {
        _builder.Reset();
        _builder.SetInvoiceNumber(invoiceNumber);
        _builder.SetSeller("Contoso Consulting LLC", "456 Business Ave, Suite 200, Seattle, WA 98101");
        _builder.SetBuyer(clientName, clientAddress);
        _builder.SetDates(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        _builder.AddLineItem($"Professional Consulting Services", hours, hourlyRate);
        _builder.SetTaxRate(0); // Services are often tax-exempt
        _builder.SetPaymentTerms("Net 30 — 1.5% late fee per month after due date");
        _builder.SetHeaderNote("Thank you for choosing Contoso Consulting.");
        _builder.SetFooterNote("Please remit payment to the address above.");
        return _builder.Build();
    }

    /// <summary>
    /// Builds a SaaS subscription invoice with multiple line items.
    /// </summary>
    public Invoice BuildSubscriptionInvoice(string invoiceNumber, string clientName,
        string clientAddress, string planName, decimal monthlyFee, int userCount,
        decimal perUserFee)
    {
        _builder.Reset();
        _builder.SetInvoiceNumber(invoiceNumber);
        _builder.SetSeller("CloudSoft Inc.", "789 Tech Park Dr, San Francisco, CA 94105");
        _builder.SetBuyer(clientName, clientAddress);
        _builder.SetDates(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15));
        _builder.AddLineItem($"{planName} — Base Platform Fee", 1, monthlyFee);
        _builder.AddLineItem($"Additional User Licenses ({userCount} users)", userCount, perUserFee);
        _builder.SetTaxRate(8.25m);
        _builder.SetCurrency("USD");
        _builder.SetPaymentTerms("Due upon receipt. Auto-charge enabled.");
        _builder.SetHeaderNote($"Subscription period: {DateTimeOffset.UtcNow:MMM yyyy}");
        return _builder.Build();
    }

    /// <summary>
    /// Builds a minimal invoice with just the essentials — useful for quick quotes.
    /// </summary>
    public Invoice BuildQuickQuote(string invoiceNumber, string clientName, string description, decimal amount)
    {
        _builder.Reset();
        _builder.SetInvoiceNumber(invoiceNumber);
        _builder.SetBuyer(clientName, "Address on file");
        _builder.AddLineItem(description, 1, amount);
        _builder.SetDates(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));
        return _builder.Build();
    }
}
