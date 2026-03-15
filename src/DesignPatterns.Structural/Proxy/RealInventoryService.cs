using System.Collections.Concurrent;

namespace DesignPatterns.Structural.Proxy;

/// <summary>
/// Real Subject — the actual inventory service that manages stock.
/// In production, this would connect to a database or warehouse management system.
/// </summary>
public sealed class RealInventoryService : IInventoryService
{
    private readonly ConcurrentDictionary<string, InventoryItem> _inventory = new();
    private readonly ConcurrentDictionary<string, (string Sku, int Quantity)> _reservations = new();
    private int _operationCount;

    public int OperationCount => _operationCount;

    public RealInventoryService()
    {
        // Seed with sample data
        SeedInventory();
    }

    public Task<InventoryItem?> GetItemAsync(string sku)
    {
        Interlocked.Increment(ref _operationCount);
        _inventory.TryGetValue(sku, out var item);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<InventoryItem>> GetAllItemsAsync()
    {
        Interlocked.Increment(ref _operationCount);
        IReadOnlyList<InventoryItem> items = _inventory.Values.ToList().AsReadOnly();
        return Task.FromResult(items);
    }

    public Task<ReservationResult> ReserveAsync(string sku, int quantity)
    {
        Interlocked.Increment(ref _operationCount);

        if (!_inventory.TryGetValue(sku, out var item))
        {
            return Task.FromResult(new ReservationResult
            {
                Sku = sku,
                ReservationId = string.Empty,
                QuantityReserved = 0,
                Success = false,
                FailureReason = $"SKU '{sku}' not found in inventory."
            });
        }

        if (item.QuantityAvailable < quantity)
        {
            return Task.FromResult(new ReservationResult
            {
                Sku = sku,
                ReservationId = string.Empty,
                QuantityReserved = 0,
                Success = false,
                FailureReason = $"Insufficient stock. Available: {item.QuantityAvailable}, Requested: {quantity}"
            });
        }

        var reservationId = $"RES-{Guid.NewGuid():N}"[..16];

        // Update inventory: increase reserved count
        var updated = item with
        {
            QuantityReserved = item.QuantityReserved + quantity,
            LastUpdatedUtc = DateTime.UtcNow
        };
        _inventory[sku] = updated;
        _reservations[reservationId] = (sku, quantity);

        return Task.FromResult(new ReservationResult
        {
            Sku = sku,
            ReservationId = reservationId,
            QuantityReserved = quantity,
            Success = true
        });
    }

    public Task<bool> ReleaseReservationAsync(string reservationId)
    {
        Interlocked.Increment(ref _operationCount);

        if (!_reservations.TryRemove(reservationId, out var reservation))
            return Task.FromResult(false);

        if (_inventory.TryGetValue(reservation.Sku, out var item))
        {
            _inventory[reservation.Sku] = item with
            {
                QuantityReserved = Math.Max(0, item.QuantityReserved - reservation.Quantity),
                LastUpdatedUtc = DateTime.UtcNow
            };
        }

        return Task.FromResult(true);
    }

    private void SeedInventory()
    {
        var items = new[]
        {
            new InventoryItem { Sku = "LAPTOP-001", ProductName = "ThinkPad X1 Carbon", QuantityOnHand = 50, QuantityReserved = 5, WarehouseLocation = "A-1-01", LastUpdatedUtc = DateTime.UtcNow },
            new InventoryItem { Sku = "MONITOR-001", ProductName = "Dell UltraSharp 27\"", QuantityOnHand = 120, QuantityReserved = 15, WarehouseLocation = "B-2-03", LastUpdatedUtc = DateTime.UtcNow },
            new InventoryItem { Sku = "KEYBOARD-001", ProductName = "Mechanical Keyboard", QuantityOnHand = 200, QuantityReserved = 10, WarehouseLocation = "C-1-07", LastUpdatedUtc = DateTime.UtcNow },
            new InventoryItem { Sku = "MOUSE-001", ProductName = "Ergonomic Mouse", QuantityOnHand = 300, QuantityReserved = 25, WarehouseLocation = "C-1-08", LastUpdatedUtc = DateTime.UtcNow },
            new InventoryItem { Sku = "HEADSET-001", ProductName = "Noise-Canceling Headset", QuantityOnHand = 75, QuantityReserved = 70, WarehouseLocation = "D-3-02", LastUpdatedUtc = DateTime.UtcNow }
        };

        foreach (var item in items)
            _inventory[item.Sku] = item;
    }
}
