using DesignPatterns.Creational.Builder;
using DesignPatterns.Creational.Builder.StepBuilder;
using FluentAssertions;

namespace DesignPatterns.Creational.Tests;

public class BuilderTests
{
    // --- Standard Builder Tests ---

    [Fact]
    public void StandardBuilder_BuildsInvoice_WithAllFields()
    {
        var builder = new StandardInvoiceBuilder();
        builder.SetInvoiceNumber("INV-001");
        builder.SetBuyer("Acme Corp", "123 Main St");
        builder.SetSeller("Contoso LLC", "456 Business Ave");
        builder.AddLineItem("Consulting", 10, 150.00m);
        builder.AddLineItem("Travel Expenses", 1, 500.00m);
        builder.SetTaxRate(8.25m);
        builder.SetCurrency("USD");

        var invoice = builder.Build();

        invoice.InvoiceNumber.Should().Be("INV-001");
        invoice.BuyerName.Should().Be("Acme Corp");
        invoice.SellerName.Should().Be("Contoso LLC");
        invoice.LineItems.Should().HaveCount(2);
        invoice.Currency.Should().Be("USD");
        invoice.Subtotal.Should().Be(2000.00m); // 10*150 + 1*500
        invoice.TaxRate.Should().Be(8.25m);
        invoice.TaxAmount.Should().Be(165.00m); // 2000 * 8.25%
        invoice.Total.Should().Be(2165.00m);
    }

    [Fact]
    public void StandardBuilder_RequiresInvoiceNumber()
    {
        var builder = new StandardInvoiceBuilder();
        builder.SetBuyer("Acme Corp", "123 Main St");

        var act = () => builder.Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invoice number*required*");
    }

    [Fact]
    public void StandardBuilder_RequiresBuyerName()
    {
        var builder = new StandardInvoiceBuilder();
        builder.SetInvoiceNumber("INV-001");

        var act = () => builder.Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Buyer name*required*");
    }

    [Fact]
    public void StandardBuilder_Reset_ClearsState()
    {
        var builder = new StandardInvoiceBuilder();
        builder.SetInvoiceNumber("INV-001");
        builder.SetBuyer("Acme", "Addr");
        builder.AddLineItem("Item", 1, 100);

        builder.Reset();
        builder.SetInvoiceNumber("INV-002");
        builder.SetBuyer("NewCo", "New Addr");
        var invoice = builder.Build();

        invoice.InvoiceNumber.Should().Be("INV-002");
        invoice.BuyerName.Should().Be("NewCo");
        invoice.LineItems.Should().BeEmpty();
    }

    // --- Fluent Builder Tests ---

    [Fact]
    public void FluentBuilder_ChainsMethodCalls()
    {
        var invoice = new FluentInvoiceBuilder()
            .WithInvoiceNumber("INV-100")
            .WithBuyer("MegaCorp", "789 Enterprise Blvd")
            .WithSeller("SmallBiz", "1 Startup Lane")
            .AddLineItem("SaaS License", 12, 99.00m)
            .AddLineItem("Premium Support", 12, 29.99m)
            .WithTaxRate(7.0m)
            .WithGlobalDiscount(10)
            .WithCurrency("EUR")
            .WithPaymentTerms("Net 30")
            .WithHeaderNote("Annual subscription")
            .WithFooterNote("Thank you for your business!")
            .Build();

        invoice.InvoiceNumber.Should().Be("INV-100");
        invoice.BuyerName.Should().Be("MegaCorp");
        invoice.Currency.Should().Be("EUR");
        invoice.LineItems.Should().HaveCount(2);
        invoice.GlobalDiscountPercent.Should().Be(10);
        invoice.PaymentTerms.Should().Be("Net 30");
        invoice.HeaderNote.Should().Be("Annual subscription");
    }

    [Fact]
    public void FluentBuilder_CalculatesDiscountAndTax()
    {
        var invoice = new FluentInvoiceBuilder()
            .WithInvoiceNumber("INV-200")
            .WithBuyer("TestCo", "Addr")
            .AddLineItem("Widget", 10, 100.00m) // Subtotal: 1000
            .WithGlobalDiscount(20) // Discount: 200
            .WithTaxRate(10) // Tax on 800: 80
            .Build();

        invoice.Subtotal.Should().Be(1000.00m);
        invoice.DiscountAmount.Should().Be(200.00m);
        invoice.TaxableAmount.Should().Be(800.00m);
        invoice.TaxAmount.Should().Be(80.00m);
        invoice.Total.Should().Be(880.00m);
    }

    [Fact]
    public void FluentBuilder_LineItem_WithDiscount()
    {
        var invoice = new FluentInvoiceBuilder()
            .WithInvoiceNumber("INV-300")
            .WithBuyer("TestCo", "Addr")
            .AddLineItem("Widget", 10, 100.00m, discountPercent: 15) // 1000 - 150 = 850
            .Build();

        invoice.LineItems[0].Subtotal.Should().Be(850.00m);
        invoice.Subtotal.Should().Be(850.00m);
    }

    [Fact]
    public void FluentBuilder_FormattedTotal_IncludesCurrency()
    {
        var invoice = new FluentInvoiceBuilder()
            .WithInvoiceNumber("INV-400")
            .WithBuyer("TestCo", "Addr")
            .AddLineItem("Item", 1, 1234.56m)
            .WithCurrency("EUR")
            .Build();

        invoice.FormattedTotal.Should().Be("EUR 1,234.56");
    }

    // --- Step Builder Tests ---

    [Fact]
    public void StepBuilder_EnforcesOrder()
    {
        // The compile-time enforcement means we can ONLY call methods in the right order.
        // This test verifies runtime behavior.
        var invoice = InvoiceStepBuilder.Create()
            .WithInvoiceNumber("STEP-001")
            .WithBuyer("OrderedCo", "1 Sequential St")
            .AddLineItem("Consulting", 8, 200.00m)
            .AddLineItem("Travel", 1, 450.00m)
            .WithTaxRate(6.5m)
            .WithSeller("ExpertCo", "2 Knowledge Way")
            .WithCurrency("GBP")
            .Build();

        invoice.InvoiceNumber.Should().Be("STEP-001");
        invoice.BuyerName.Should().Be("OrderedCo");
        invoice.SellerName.Should().Be("ExpertCo");
        invoice.LineItems.Should().HaveCount(2);
        invoice.TaxRate.Should().Be(6.5m);
        invoice.Currency.Should().Be("GBP");
    }

    [Fact]
    public void StepBuilder_CanBuildAfterLineItems()
    {
        // Build is available from ILineItemStep (after adding at least one item).
        var invoice = InvoiceStepBuilder.Create()
            .WithInvoiceNumber("STEP-002")
            .WithBuyer("MinimalCo", "Addr")
            .AddLineItem("Quick Job", 1, 500.00m)
            .Build();

        invoice.InvoiceNumber.Should().Be("STEP-002");
        invoice.LineItems.Should().HaveCount(1);
        invoice.Total.Should().Be(500.00m);
    }

    // --- Director Tests ---

    [Fact]
    public void Director_BuildsConsultingInvoice()
    {
        var builder = new StandardInvoiceBuilder();
        var director = new InvoiceDirector(builder);

        var invoice = director.BuildConsultingInvoice(
            "CON-001", "BigClient Inc.", "100 Corporate Way", 175.00m, 40);

        invoice.InvoiceNumber.Should().Be("CON-001");
        invoice.BuyerName.Should().Be("BigClient Inc.");
        invoice.SellerName.Should().Be("Contoso Consulting LLC");
        invoice.LineItems.Should().HaveCount(1);
        invoice.LineItems[0].Quantity.Should().Be(40);
        invoice.LineItems[0].UnitPrice.Should().Be(175.00m);
        invoice.Subtotal.Should().Be(7000.00m);
        invoice.TaxRate.Should().Be(0);
        invoice.PaymentTerms.Should().Contain("Net 30");
    }

    [Fact]
    public void Director_BuildsSubscriptionInvoice()
    {
        var builder = new StandardInvoiceBuilder();
        var director = new InvoiceDirector(builder);

        var invoice = director.BuildSubscriptionInvoice(
            "SUB-001", "StartupCo", "1 Innovation Dr",
            "Enterprise Plan", 999.00m, 50, 15.00m);

        invoice.InvoiceNumber.Should().Be("SUB-001");
        invoice.LineItems.Should().HaveCount(2);
        invoice.TaxRate.Should().Be(8.25m);
        invoice.Subtotal.Should().Be(999.00m + (50 * 15.00m));
    }

    [Fact]
    public void Director_BuildsQuickQuote()
    {
        var builder = new StandardInvoiceBuilder();
        var director = new InvoiceDirector(builder);

        var invoice = director.BuildQuickQuote("QQ-001", "ProspectCo", "Website Redesign", 5000.00m);

        invoice.InvoiceNumber.Should().Be("QQ-001");
        invoice.BuyerName.Should().Be("ProspectCo");
        invoice.LineItems.Should().HaveCount(1);
        invoice.Total.Should().Be(5000.00m);
    }

    [Fact]
    public void Director_CanBuildMultipleInvoices_WithReset()
    {
        var builder = new StandardInvoiceBuilder();
        var director = new InvoiceDirector(builder);

        var invoice1 = director.BuildQuickQuote("QQ-001", "Client A", "Job A", 1000.00m);
        var invoice2 = director.BuildQuickQuote("QQ-002", "Client B", "Job B", 2000.00m);

        invoice1.InvoiceNumber.Should().Be("QQ-001");
        invoice2.InvoiceNumber.Should().Be("QQ-002");
        invoice1.BuyerName.Should().NotBe(invoice2.BuyerName);
    }
}
