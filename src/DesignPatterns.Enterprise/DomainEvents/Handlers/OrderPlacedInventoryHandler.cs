using DesignPatterns.Enterprise.DomainEvents.Events;

namespace DesignPatterns.Enterprise.DomainEvents.Handlers;

/// <summary>
/// Reserves inventory when an order is placed.
/// </summary>
public sealed class OrderPlacedInventoryHandler : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly List<(string Sku, int Quantity)> _reservations = new();

    public IReadOnlyList<(string Sku, int Quantity)> Reservations => _reservations.AsReadOnly();

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
    {
        foreach (var item in domainEvent.Items)
        {
            _reservations.Add((item.Sku, item.Quantity));
        }
        return Task.CompletedTask;
    }
}
