using DesignPatterns.Structural.Adapter.ThirdParty;

namespace DesignPatterns.Structural.Adapter;

/// <summary>
/// Object Adapter — wraps <see cref="UpsApi"/> via composition.
/// UPS already uses imperial units but has completely different method
/// signatures and naming conventions (underscored property names, service codes).
/// </summary>
public sealed class UpsAdapter : IShippingService
{
    private readonly UpsApi _upsApi;

    public UpsAdapter(UpsApi upsApi)
    {
        _upsApi = upsApi ?? throw new ArgumentNullException(nameof(upsApi));
    }

    public string CarrierName => "UPS";

    public Task<ShippingQuote> GetQuoteAsync(ShippingRequest request)
    {
        var upsRequest = TranslateToUpsRequest(request);
        var response = _upsApi.Rating_RequestRate(upsRequest);

        var quote = new ShippingQuote
        {
            CarrierName = CarrierName,
            ServiceLevel = response.RatedShipment_ServiceDescription,
            Price = response.RatedShipment_TotalCharges,
            EstimatedDaysInTransit = response.RatedShipment_GuaranteedDaysToDelivery,
            EstimatedDeliveryDate = response.RatedShipment_ScheduledDeliveryDate,
            TrackingNumberPrefix = "1Z",
            GuaranteedDelivery = response.RatedShipment_GuaranteedDaysToDelivery <= 3
        };

        return Task.FromResult(quote);
    }

    public Task<string> CreateShipmentAsync(ShippingRequest request)
    {
        var upsRequest = TranslateToUpsRequest(request);
        var response = _upsApi.Shipping_CreateShipment(upsRequest);
        return Task.FromResult(response.PackageResults_TrackingNumber);
    }

    public Task<string> GetTrackingStatusAsync(string trackingNumber)
    {
        var response = _upsApi.Track_GetStatus(trackingNumber);
        return Task.FromResult(response.Activity_Status_Description);
    }

    private static UpsRatingRequest TranslateToUpsRequest(ShippingRequest request) => new()
    {
        Shipper_PostalCode = request.OriginPostalCode,
        ShipTo_PostalCode = request.DestinationPostalCode,
        Shipment_Weight_Lbs = request.WeightInPounds,
        Package_Length_In = request.LengthInInches,
        Package_Width_In = request.WidthInInches,
        Package_Height_In = request.HeightInInches,
        Shipment_ShipDate = request.ShipDate,
        Service_Code = "03", // Ground
        Shipment_DeliveryConfirmation = request.RequiresSignature ? "SIGNATURE" : "NONE",
        Shipment_ResidentialIndicator = request.IsResidential ? "Y" : "N"
    };
}
