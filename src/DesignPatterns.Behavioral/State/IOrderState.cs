namespace DesignPatterns.Behavioral.State;

/// <summary>
/// State interface for order lifecycle transitions.
/// Each state determines which transitions are valid.
/// </summary>
public interface IOrderState
{
    string Name { get; }
    void Submit(OrderContext context);
    void Approve(OrderContext context);
    void Ship(OrderContext context);
    void Deliver(OrderContext context);
    void Cancel(OrderContext context);
}
