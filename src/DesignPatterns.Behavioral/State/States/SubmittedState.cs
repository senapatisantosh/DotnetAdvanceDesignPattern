namespace DesignPatterns.Behavioral.State.States;

public sealed class SubmittedState : IOrderState
{
    public string Name => "Submitted";

    public void Submit(OrderContext context) =>
        throw new InvalidOperationException("Order is already submitted.");

    public void Approve(OrderContext context)
    {
        context.TransitionTo(new ApprovedState());
    }

    public void Ship(OrderContext context) =>
        throw new InvalidOperationException("Cannot ship an order that has not been approved.");

    public void Deliver(OrderContext context) =>
        throw new InvalidOperationException("Cannot deliver an order that has not been shipped.");

    public void Cancel(OrderContext context)
    {
        context.TransitionTo(new CancelledState());
    }
}
