namespace DesignPatterns.Behavioral.State.States;

public sealed class ShippedState : IOrderState
{
    public string Name => "Shipped";

    public void Submit(OrderContext context) =>
        throw new InvalidOperationException("Cannot submit a shipped order.");

    public void Approve(OrderContext context) =>
        throw new InvalidOperationException("Cannot approve a shipped order.");

    public void Ship(OrderContext context) =>
        throw new InvalidOperationException("Order is already shipped.");

    public void Deliver(OrderContext context)
    {
        context.TransitionTo(new DeliveredState());
    }

    public void Cancel(OrderContext context) =>
        throw new InvalidOperationException("Cannot cancel a shipped order. Initiate a return instead.");
}
