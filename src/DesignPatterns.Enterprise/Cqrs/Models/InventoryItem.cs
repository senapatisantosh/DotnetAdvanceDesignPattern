namespace DesignPatterns.Enterprise.Cqrs.Models;

/// <summary>
/// Represents an SKU in the warehouse with available and reserved quantities.
/// </summary>
public sealed class InventoryItem
{
    public required string Sku { get; init; }
    public required string ProductName { get; init; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
    public int ReorderThreshold { get; init; } = 10;

    public bool IsLowStock => AvailableQuantity <= ReorderThreshold;

    public override string ToString() =>
        $"{Sku}: {ProductName} — {AvailableQuantity} available, {ReservedQuantity} reserved";
}
