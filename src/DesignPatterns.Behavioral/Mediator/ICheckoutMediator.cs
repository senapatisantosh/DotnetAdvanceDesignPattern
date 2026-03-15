namespace DesignPatterns.Behavioral.Mediator;

/// <summary>
/// Mediator interface for coordinating checkout workflow between colleagues.
/// </summary>
public interface ICheckoutMediator
{
    Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request);
    void NotifyStepCompleted(string colleague, string message);
}
