using DesignPatterns.Structural.Adapter.ThirdParty;

namespace DesignPatterns.Structural.Adapter;

/// <summary>
/// Object Adapter — wraps the <see cref="FedExApi"/> via composition and
/// translates between our unified <see cref="IShippingService"/> interface
/// and FedEx's proprietary API with metric units and different method names.
/// </summary>
public sealed class FedExAdapter : IShippingService
{
    private readonly FedExApi _fedExApi;

    public FedExAdapter(FedExApi fedExApi)
    {
        _fedExApi = fedExApi ?? throw new ArgumentNullException(nameof(fedExApi));
    }

    public string CarrierName => "FedEx";

    public Task<ShippingQuote> GetQuoteAsync(ShippingRequest request)
    {
        var fedExRequest = TranslateToFedExRequest(request);
        var reply = _fedExApi.GetRateEstimate(fedExRequest);

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
        var fedExRequest = TranslateToFedExRequest(request);
        var reply = _fedExApi.ProcessShipment(fedExRequest);
        return Task.FromResult(reply.MasterTrackingNumber);
    }

    public Task<string> GetTrackingStatusAsync(string trackingNumber)
    {
        var reply = _fedExApi.TrackByNumber(trackingNumber);
        return Task.FromResult(reply.StatusDescription);
    }

    /// <summary>
    /// Core adaptation logic: converts our domain model to FedEx's API format,
    /// including unit conversion from imperial (pounds/inches) to metric (kg/cm).
    /// </summary>
    private static FedExRateRequest TranslateToFedExRequest(ShippingRequest request) => new()
    {
        OriginPostalCode = request.OriginPostalCode,
        DestinationPostalCode = request.DestinationPostalCode,
        PackageWeightKg = PoundsToKilograms(request.WeightInPounds),
        LengthCm = InchesToCentimeters(request.LengthInInches),
        WidthCm = InchesToCentimeters(request.WidthInInches),
        HeightCm = InchesToCentimeters(request.HeightInInches),
        ShipTimestamp = request.ShipDate,
        ServiceType = FedExServiceType.Ground,
        SignatureRequired = request.RequiresSignature,
        ResidentialDelivery = request.IsResidential
    };

    private static double PoundsToKilograms(double pounds) => pounds * 0.453592;
    private static double InchesToCentimeters(double inches) => inches * 2.54;
}
