namespace DesignPatterns.Behavioral.Command.Commands;

public sealed class UpdateShippingCommand : ICommand
{
    private readonly Order _order;
    private readonly string _newAddress;
    private string _previousAddress = string.Empty;

    public UpdateShippingCommand(Order order, string newAddress)
    {
        _order = order;
        _newAddress = newAddress;
    }

    public string Description => $"Update shipping address to '{_newAddress}'";

    public void Execute()
    {
        _previousAddress = _order.UpdateShipping(_newAddress);
    }

    public void Undo()
    {
        _order.UpdateShipping(_previousAddress);
    }
}
