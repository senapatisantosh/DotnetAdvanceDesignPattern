namespace DesignPatterns.Structural.Adapter.ThirdParty;

/// <summary>
/// Simulates FedEx's third-party API with its own naming conventions,
/// data structures, and method signatures — deliberately different from our
/// unified <see cref="IShippingService"/>.
/// </summary>
public class FedExApi
{
    /// <summary>FedEx uses metric weights (kilograms) and dimensions (centimeters).</summary>
    public FedExRateReply GetRateEstimate(FedExRateRequest rateRequest)
    {
        // Simulate FedEx pricing: base rate + per-kg charge + surcharges
        var baseRate = 12.50m;
        var perKgRate = 2.75m;
        var totalWeight = rateRequest.PackageWeightKg;
        var cost = baseRate + (perKgRate * (decimal)totalWeight);

        if (rateRequest.ResidentialDelivery)
            cost += 4.50m;
        if (rateRequest.SignatureRequired)
            cost += 3.00m;

        var transitDays = rateRequest.ServiceType switch
        {
            FedExServiceType.PriorityOvernight => 1,
            FedExServiceType.StandardOvernight => 1,
            FedExServiceType.TwoDay => 2,
            FedExServiceType.Ground => 5,
            _ => 7
        };

        return new FedExRateReply
        {
            TotalNetCharge = cost,
            Currency = "USD",
            ServiceType = rateRequest.ServiceType,
            DeliveryTimestamp = rateRequest.ShipTimestamp.AddDays(transitDays),
            TransitDays = transitDays,
            GuaranteedDelivery = rateRequest.ServiceType != FedExServiceType.Ground
        };
    }

    public FedExShipReply ProcessShipment(FedExRateRequest rateRequest)
    {
        return new FedExShipReply
        {
            MasterTrackingNumber = $"FDX{Random.Shared.Next(100000000, 999999999)}",
            LabelData = Convert.ToBase64String("fake-label-data"u8.ToArray()),
            ServiceType = rateRequest.ServiceType
        };
    }

    public FedExTrackReply TrackByNumber(string trackingNumber)
    {
        return new FedExTrackReply
        {
            TrackingNumber = trackingNumber,
            StatusDescription = "In transit — on FedEx vehicle for delivery",
            EstimatedDelivery = DateTime.UtcNow.AddDays(1),
            StatusCode = "IT"
        };
    }
}

// --- FedEx-specific DTOs ---

public enum FedExServiceType
{
    PriorityOvernight,
    StandardOvernight,
    TwoDay,
    Ground
}

public class FedExRateRequest
{
    public required string OriginPostalCode { get; set; }
    public required string DestinationPostalCode { get; set; }
    public double PackageWeightKg { get; set; }
    public double LengthCm { get; set; }
    public double WidthCm { get; set; }
    public double HeightCm { get; set; }
    public DateTime ShipTimestamp { get; set; }
    public FedExServiceType ServiceType { get; set; } = FedExServiceType.Ground;
    public bool SignatureRequired { get; set; }
    public bool ResidentialDelivery { get; set; }
}

public class FedExRateReply
{
    public decimal TotalNetCharge { get; set; }
    public string Currency { get; set; } = "USD";
    public FedExServiceType ServiceType { get; set; }
    public DateTime DeliveryTimestamp { get; set; }
    public int TransitDays { get; set; }
    public bool GuaranteedDelivery { get; set; }
}

public class FedExShipReply
{
    public string MasterTrackingNumber { get; set; } = string.Empty;
    public string LabelData { get; set; } = string.Empty;
    public FedExServiceType ServiceType { get; set; }
}

public class FedExTrackReply
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime EstimatedDelivery { get; set; }
    public string StatusCode { get; set; } = string.Empty;
}
