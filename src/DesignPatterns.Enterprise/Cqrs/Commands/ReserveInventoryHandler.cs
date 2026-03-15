namespace DesignPatterns.Enterprise.Cqrs.Commands;

public sealed class ReserveInventoryHandler(InMemoryInventoryStore store) : ICommandHandler<ReserveInventoryCommand>
{
    public Task HandleAsync(ReserveInventoryCommand command, CancellationToken ct = default)
    {
        var item = store.Get(command.Sku)
            ?? throw new InvalidOperationException($"SKU '{command.Sku}' not found.");

        if (item.AvailableQuantity < command.Quantity)
            throw new InvalidOperationException(
                $"Insufficient stock for '{command.Sku}'. Requested: {command.Quantity}, Available: {item.AvailableQuantity}.");

        item.AvailableQuantity -= command.Quantity;
        item.ReservedQuantity += command.Quantity;

        return Task.CompletedTask;
    }
}
