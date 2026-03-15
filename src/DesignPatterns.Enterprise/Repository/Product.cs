namespace DesignPatterns.Enterprise.Repository;

/// <summary>
/// Represents a product in a multi-tenant catalog.
/// Each product belongs to a specific tenant (store/organization).
/// </summary>
public sealed class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string TenantId { get; init; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, string> Attributes { get; init; } = new();

    public override string ToString() => $"[{TenantId}] {Name} — ${Price:F2} ({StockQuantity} in stock)";
}
