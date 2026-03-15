namespace DesignPatterns.Behavioral.State.States;

public sealed class DraftState : IOrderState
{
    public string Name => "Draft";

    public void Submit(OrderContext context)
    {
        context.TransitionTo(new SubmittedState());
    }

    public void Approve(OrderContext context) =>
        throw new InvalidOperationException("Cannot approve a draft order. Submit it first.");

    public void Ship(OrderContext context) =>
        throw new InvalidOperationException("Cannot ship a draft order.");

    public void Deliver(OrderContext context) =>
        throw new InvalidOperationException("Cannot deliver a draft order.");

    public void Cancel(OrderContext context)
    {
        context.TransitionTo(new CancelledState());
    }
}
