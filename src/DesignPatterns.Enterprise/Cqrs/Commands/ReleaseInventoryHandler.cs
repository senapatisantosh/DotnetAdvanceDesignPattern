namespace DesignPatterns.Enterprise.Cqrs.Commands;

public sealed class ReleaseInventoryHandler(InMemoryInventoryStore store) : ICommandHandler<ReleaseInventoryCommand>
{
    public Task HandleAsync(ReleaseInventoryCommand command, CancellationToken ct = default)
    {
        var item = store.Get(command.Sku)
            ?? throw new InvalidOperationException($"SKU '{command.Sku}' not found.");

        if (item.ReservedQuantity < command.Quantity)
            throw new InvalidOperationException(
                $"Cannot release {command.Quantity} of '{command.Sku}'. Only {item.ReservedQuantity} reserved.");

        item.ReservedQuantity -= command.Quantity;
        item.AvailableQuantity += command.Quantity;

        return Task.CompletedTask;
    }
}
