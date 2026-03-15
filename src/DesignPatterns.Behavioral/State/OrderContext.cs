namespace DesignPatterns.Behavioral.State;

/// <summary>
/// Context that delegates behavior to the current state object.
/// The state machine for an e-commerce order.
/// </summary>
public sealed class OrderContext
{
    private IOrderState _currentState;
    private readonly List<string> _transitionLog = [];

    public string CurrentStateName => _currentState.Name;
    public IReadOnlyList<string> TransitionLog => _transitionLog.AsReadOnly();

    public Guid OrderId { get; }
    public string CustomerName { get; }

    public OrderContext(Guid orderId, string customerName, IOrderState? initialState = null)
    {
        OrderId = orderId;
        CustomerName = customerName;
        _currentState = initialState ?? new States.DraftState();
        _transitionLog.Add($"Order created in '{_currentState.Name}' state.");
    }

    internal void TransitionTo(IOrderState newState)
    {
        var previousState = _currentState.Name;
        _currentState = newState;
        _transitionLog.Add($"Transitioned from '{previousState}' to '{newState.Name}'.");
    }

    public void Submit() => _currentState.Submit(this);
    public void Approve() => _currentState.Approve(this);
    public void Ship() => _currentState.Ship(this);
    public void Deliver() => _currentState.Deliver(this);
    public void Cancel() => _currentState.Cancel(this);
}
