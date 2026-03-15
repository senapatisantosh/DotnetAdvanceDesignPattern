namespace DesignPatterns.Structural.Proxy;

/// <summary>
/// Domain model for inventory items.
/// </summary>
public sealed record InventoryItem
{
    public required string Sku { get; init; }
    public required string ProductName { get; init; }
    public required int QuantityOnHand { get; init; }
    public required int QuantityReserved { get; init; }
    public required string WarehouseLocation { get; init; }
    public required DateTime LastUpdatedUtc { get; init; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    public bool IsInStock => QuantityAvailable > 0;
}

/// <summary>
/// Result of an inventory reservation attempt.
/// </summary>
public sealed record ReservationResult
{
    public required string Sku { get; init; }
    public required string ReservationId { get; init; }
    public required int QuantityReserved { get; init; }
    public required bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTime ReservedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; init; } = DateTime.UtcNow.AddMinutes(15);
}
