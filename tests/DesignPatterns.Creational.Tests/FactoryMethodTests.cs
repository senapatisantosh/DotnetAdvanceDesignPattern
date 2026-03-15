using DesignPatterns.Creational.FactoryMethod;
using DesignPatterns.Creational.FactoryMethod.Naive;
using DesignPatterns.Creational.FactoryMethod.Processors;
using DesignPatterns.Creational.FactoryMethod.SimpleFactory;
using DesignPatterns.Creational.FactoryMethod.StaticFactory;
using FluentAssertions;

namespace DesignPatterns.Creational.Tests;

public class FactoryMethodTests
{
    // --- Factory Method Pattern Tests ---

    [Fact]
    public void StripeProcessorFactory_CreatesStripeProcessor()
    {
        var factory = new StripeProcessorFactory();

        var processor = factory.CreateProcessor();

        processor.Should().BeOfType<StripePaymentProcessor>();
        processor.GatewayName.Should().Be("Stripe");
        processor.SupportsRecurring.Should().BeTrue();
    }

    [Fact]
    public void PayPalProcessorFactory_CreatesPayPalProcessor()
    {
        var factory = new PayPalProcessorFactory();

        var processor = factory.CreateProcessor();

        processor.Should().BeOfType<PayPalPaymentProcessor>();
        processor.GatewayName.Should().Be("PayPal");
    }

    [Fact]
    public void SquareProcessorFactory_CreatesSquareProcessor()
    {
        var factory = new SquareProcessorFactory();

        var processor = factory.CreateProcessor();

        processor.Should().BeOfType<SquarePaymentProcessor>();
        processor.GatewayName.Should().Be("Square");
        processor.SupportsRecurring.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(StripeProcessorFactory), "Stripe")]
    [InlineData(typeof(PayPalProcessorFactory), "PayPal")]
    [InlineData(typeof(SquareProcessorFactory), "Square")]
    public void ProcessMerchantPayment_ProcessesSuccessfully(Type factoryType, string expectedGateway)
    {
        var factory = (PaymentProcessorFactory)Activator.CreateInstance(factoryType)!;

        var result = factory.ProcessMerchantPayment(100.00m, "USD", "tok_test123");

        result.IsSuccess.Should().BeTrue();
        result.Gateway.Should().Be(expectedGateway);
        result.AmountCharged.Should().BeGreaterThan(100.00m, "fees are added");
        result.Currency.Should().Be("USD");
        result.TransactionId.Should().NotBeNullOrWhiteSpace();
    }

    // --- Processor-specific Tests ---

    [Fact]
    public void StripeProcessor_CalculatesCorrectFee()
    {
        var processor = new StripePaymentProcessor();

        var result = processor.ProcessPayment(100.00m, "USD", "tok_test");

        // Stripe fee: 2.9% + $0.30 = $2.90 + $0.30 = $3.20
        var expectedTotal = 100.00m + (100.00m * 0.029m + 0.30m);
        result.AmountCharged.Should().Be(expectedTotal);
    }

    [Fact]
    public void Processor_RejectsNegativeAmount()
    {
        var processor = new StripePaymentProcessor();

        var result = processor.ProcessPayment(-50.00m, "USD", "tok_test");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("positive");
    }

    [Fact]
    public void Processor_RejectsEmptyToken()
    {
        var processor = new PayPalPaymentProcessor();

        var result = processor.ProcessPayment(100.00m, "USD", "");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("token");
    }

    [Fact]
    public void StripeProcessor_Refund_ValidatesTransactionId()
    {
        var processor = new StripePaymentProcessor();

        var result = processor.Refund("PAYPAL-wrong", 50.00m);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid Stripe");
    }

    [Fact]
    public void StripeProcessor_Refund_Succeeds()
    {
        var processor = new StripePaymentProcessor();

        var result = processor.Refund("stripe_ch_abc123", 50.00m);

        result.IsSuccess.Should().BeTrue();
        result.TransactionId.Should().StartWith("stripe_re_");
    }

    // --- Simple Factory Tests ---

    [Theory]
    [InlineData("stripe", "Stripe")]
    [InlineData("STRIPE", "Stripe")]
    [InlineData("paypal", "PayPal")]
    [InlineData("square", "Square")]
    public void SimpleFactory_CreatesCorrectProcessor(string gateway, string expectedName)
    {
        var factory = new PaymentProcessorSimpleFactory();

        var processor = factory.Create(gateway);

        processor.GatewayName.Should().Be(expectedName);
    }

    [Fact]
    public void SimpleFactory_ThrowsForUnknownGateway()
    {
        var factory = new PaymentProcessorSimpleFactory();

        var act = () => factory.Create("bitcoin");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown payment gateway*bitcoin*");
    }

    [Fact]
    public void SimpleFactory_SupportsRuntimeRegistration()
    {
        var factory = new PaymentProcessorSimpleFactory();
        factory.Register("custom", () => new StripePaymentProcessor());

        var processor = factory.Create("custom");

        processor.Should().BeOfType<StripePaymentProcessor>();
    }

    // --- Static Factory Tests ---

    [Fact]
    public void StaticFactory_CreatesStripe()
    {
        var processor = PaymentProcessorStaticFactory.CreateStripe();
        processor.GatewayName.Should().Be("Stripe");
    }

    [Fact]
    public void StaticFactory_CreatesFromName()
    {
        var processor = PaymentProcessorStaticFactory.CreateFromName("paypal");
        processor.GatewayName.Should().Be("PayPal");
    }

    [Fact]
    public void StaticFactory_ThrowsForUnknownName()
    {
        var act = () => PaymentProcessorStaticFactory.CreateFromName("venmo");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown gateway*venmo*");
    }

    // --- Naive Approach Tests (showing it works but is not extensible) ---

    [Fact]
    public void NaiveProcessor_ProcessesPayment()
    {
        var naive = new NaivePaymentProcessor();

        var result = naive.ProcessPayment("stripe", 100.00m, "USD", "tok_test");

        result.IsSuccess.Should().BeTrue();
        result.Gateway.Should().Be("Stripe");
    }

    [Fact]
    public void NaiveProcessor_ThrowsForUnsupportedGateway()
    {
        var naive = new NaivePaymentProcessor();

        var act = () => naive.ProcessPayment("bitcoin", 100.00m, "USD", "tok_test");

        act.Should().Throw<NotSupportedException>();
    }
}
