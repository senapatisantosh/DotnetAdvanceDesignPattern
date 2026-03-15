using DesignPatterns.Structural.Adapter.ThirdParty;

namespace DesignPatterns.Structural.Adapter.ClassAdapter;

/// <summary>
/// Class Adapter variant — inherits from <see cref="FedExApi"/> directly
/// rather than holding a reference via composition (the object adapter approach).
///
/// Trade-offs vs. Object Adapter:
/// - Tighter coupling to the adaptee (cannot swap implementations)
/// - No additional indirection (direct method calls, no delegate)
/// - Cannot adapt multiple adaptees simultaneously
/// - In C# (single inheritance), you can only class-adapt one type
///
/// Generally the object adapter (<see cref="FedExAdapter"/>) is preferred
/// in C# because it favors composition over inheritance.
/// </summary>
public sealed class FedExClassAdapter : FedExApi, IShippingService
{
    public string CarrierName => "FedEx (Class Adapter)";

    public Task<ShippingQuote> GetQuoteAsync(ShippingRequest request)
    {
        // Call inherited method directly — no composition indirection
        var reply = GetRateEstimate(TranslateRequest(request));

        var quote = new ShippingQuote
        {
            CarrierName = CarrierName,
            ServiceLevel = reply.ServiceType.ToString(),
            Price = reply.TotalNetCharge,
            EstimatedDaysInTransit = reply.TransitDays,
            EstimatedDeliveryDate = reply.DeliveryTimestamp,
            TrackingNumberPrefix = "FDX",
            GuaranteedDelivery = reply.GuaranteedDelivery
        };

        return Task.FromResult(quote);
    }

    public Task<string> CreateShipmentAsync(ShippingRequest request)
    {
        var reply = ProcessShipment(TranslateRequest(request));
        return Task.FromResult(reply.MasterTrackingNumber);
    }

    public Task<string> GetTrackingStatusAsync(string trackingNumber)
    {
        var reply = TrackByNumber(trackingNumber);
        return Task.FromResult(reply.StatusDescription);
    }

    private static FedExRateRequest TranslateRequest(ShippingRequest request) => new()
    {
        OriginPostalCode = request.OriginPostalCode,
        DestinationPostalCode = request.DestinationPostalCode,
        PackageWeightKg = request.WeightInPounds * 0.453592,
        LengthCm = request.LengthInInches * 2.54,
        WidthCm = request.WidthInInches * 2.54,
        HeightCm = request.HeightInInches * 2.54,
        ShipTimestamp = request.ShipDate,
        ServiceType = FedExServiceType.Ground,
        SignatureRequired = request.RequiresSignature,
        ResidentialDelivery = request.IsResidential
    };
}
