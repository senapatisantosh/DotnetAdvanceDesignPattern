using DesignPatterns.Creational.FactoryMethod.Processors;

namespace DesignPatterns.Creational.FactoryMethod;

/// <summary>
/// The classic Factory Method pattern.
///
/// The base class defines a template method (ProcessMerchantPayment) that calls
/// the abstract factory method (CreateProcessor). Subclasses decide which concrete
/// processor to instantiate.
///
/// This is the GoF Factory Method: a method in a base class that subclasses override
/// to change the type of object created.
/// </summary>
public abstract class PaymentProcessorFactory
{
    /// <summary>
    /// The Factory Method — subclasses override this to return the appropriate processor.
    /// </summary>
    public abstract IPaymentProcessor CreateProcessor();

    /// <summary>
    /// Template method that uses the factory method. The business logic stays in the
    /// base class; only the object creation varies by subclass.
    /// </summary>
    public PaymentResult ProcessMerchantPayment(decimal amount, string currency, string customerToken)
    {
        var processor = CreateProcessor();
        return processor.ProcessPayment(amount, currency, customerToken);
    }
}

/// <summary>Factory that creates Stripe processors.</summary>
public sealed class StripeProcessorFactory : PaymentProcessorFactory
{
    public override IPaymentProcessor CreateProcessor() => new StripePaymentProcessor();
}

/// <summary>Factory that creates PayPal processors.</summary>
public sealed class PayPalProcessorFactory : PaymentProcessorFactory
{
    public override IPaymentProcessor CreateProcessor() => new PayPalPaymentProcessor();
}

/// <summary>Factory that creates Square processors.</summary>
public sealed class SquareProcessorFactory : PaymentProcessorFactory
{
    public override IPaymentProcessor CreateProcessor() => new SquarePaymentProcessor();
}
