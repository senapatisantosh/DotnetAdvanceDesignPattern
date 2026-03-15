namespace DesignPatterns.Structural.Adapter;

/// <summary>
/// Unified domain model representing a shipping quote returned by any carrier.
/// </summary>
public sealed record ShippingQuote
{
    public required string CarrierName { get; init; }
    public required string ServiceLevel { get; init; }
    public required decimal Price { get; init; }
    public required int EstimatedDaysInTransit { get; init; }
    public required DateTime EstimatedDeliveryDate { get; init; }
    public string? TrackingNumberPrefix { get; init; }
    public bool GuaranteedDelivery { get; init; }
}
