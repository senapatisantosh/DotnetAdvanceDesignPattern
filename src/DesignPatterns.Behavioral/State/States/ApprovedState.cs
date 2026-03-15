namespace DesignPatterns.Behavioral.State.States;

public sealed class ApprovedState : IOrderState
{
    public string Name => "Approved";

    public void Submit(OrderContext context) =>
        throw new InvalidOperationException("Cannot submit an already approved order.");

    public void Approve(OrderContext context) =>
        throw new InvalidOperationException("Order is already approved.");

    public void Ship(OrderContext context)
    {
        context.TransitionTo(new ShippedState());
    }

    public void Deliver(OrderContext context) =>
        throw new InvalidOperationException("Cannot deliver an order that has not been shipped.");

    public void Cancel(OrderContext context)
    {
        context.TransitionTo(new CancelledState());
    }
}
