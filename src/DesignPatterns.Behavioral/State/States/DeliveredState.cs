namespace DesignPatterns.Behavioral.State.States;

public sealed class DeliveredState : IOrderState
{
    public string Name => "Delivered";

    public void Submit(OrderContext context) =>
        throw new InvalidOperationException("Cannot submit a delivered order.");

    public void Approve(OrderContext context) =>
        throw new InvalidOperationException("Cannot approve a delivered order.");

    public void Ship(OrderContext context) =>
        throw new InvalidOperationException("Cannot ship a delivered order.");

    public void Deliver(OrderContext context) =>
        throw new InvalidOperationException("Order is already delivered.");

    public void Cancel(OrderContext context) =>
        throw new InvalidOperationException("Cannot cancel a delivered order. Initiate a return instead.");
}
