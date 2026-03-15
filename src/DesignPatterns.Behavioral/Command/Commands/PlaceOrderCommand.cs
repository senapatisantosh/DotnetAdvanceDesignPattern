namespace DesignPatterns.Behavioral.Command.Commands;

public sealed class PlaceOrderCommand : ICommand
{
    private readonly Order _order;
    private OrderStatus _previousStatus;

    public PlaceOrderCommand(Order order)
    {
        _order = order;
    }

    public string Description => $"Place order for {_order.CustomerName}";

    public void Execute()
    {
        _previousStatus = _order.Status;
        _order.Place();
    }

    public void Undo()
    {
        _order.Restore(_previousStatus);
    }
}
