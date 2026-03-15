namespace DesignPatterns.Behavioral.Command.Commands;

public sealed class CancelOrderCommand : ICommand
{
    private readonly Order _order;
    private OrderStatus _previousStatus;

    public CancelOrderCommand(Order order)
    {
        _order = order;
    }

    public string Description => $"Cancel order for {_order.CustomerName}";

    public void Execute()
    {
        _previousStatus = _order.Status;
        _order.Cancel();
    }

    public void Undo()
    {
        _order.Restore(_previousStatus);
    }
}
