namespace DesignPatterns.Behavioral.State.States;

public sealed class CancelledState : IOrderState
{
    public string Name => "Cancelled";

    public void Submit(OrderContext context) =>
        throw new InvalidOperationException("Cannot submit a cancelled order.");

    public void Approve(OrderContext context) =>
        throw new InvalidOperationException("Cannot approve a cancelled order.");

    public void Ship(OrderContext context) =>
        throw new InvalidOperationException("Cannot ship a cancelled order.");

    public void Deliver(OrderContext context) =>
        throw new InvalidOperationException("Cannot deliver a cancelled order.");

    public void Cancel(OrderContext context) =>
        throw new InvalidOperationException("Order is already cancelled.");
}
