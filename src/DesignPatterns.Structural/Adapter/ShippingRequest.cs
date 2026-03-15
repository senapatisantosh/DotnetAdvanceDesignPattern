namespace DesignPatterns.Structural.Adapter;

/// <summary>
/// Unified domain model representing a shipping request,
/// independent of any specific carrier's API format.
/// </summary>
public sealed record ShippingRequest
{
    public required string OriginPostalCode { get; init; }
    public required string DestinationPostalCode { get; init; }
    public required double WeightInPounds { get; init; }
    public required double LengthInInches { get; init; }
    public required double WidthInInches { get; init; }
    public required double HeightInInches { get; init; }
    public required DateTime ShipDate { get; init; }
    public bool RequiresSignature { get; init; }
    public bool IsResidential { get; init; }

    public double VolumeInCubicInches => LengthInInches * WidthInInches * HeightInInches;
}
