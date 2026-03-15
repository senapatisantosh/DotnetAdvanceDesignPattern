namespace DesignPatterns.Structural.Adapter.ThirdParty;

/// <summary>
/// Simulates UPS's third-party API — uses completely different naming
/// conventions, imperial units, and a builder-style request pattern.
/// </summary>
public class UpsApi
{
    /// <summary>UPS uses imperial units (pounds, inches) but different object names.</summary>
    public UpsRatingResponse Rating_RequestRate(UpsRatingRequest request)
    {
        var baseCharge = 10.00m;
        var weightCharge = 2.50m * (decimal)request.Shipment_Weight_Lbs;
        var totalCharge = baseCharge + weightCharge;

        if (request.Shipment_ResidentialIndicator == "Y")
            totalCharge += 3.85m;
        if (request.Shipment_DeliveryConfirmation == "SIGNATURE")
            totalCharge += 2.50m;

        var businessDays = request.Service_Code switch
        {
            "01" => 1, // Next Day Air
            "02" => 2, // 2nd Day Air
            "03" => 5, // Ground
            "12" => 3, // 3 Day Select
            _ => 7
        };

        return new UpsRatingResponse
        {
            RatedShipment_TotalCharges = totalCharge,
            RatedShipment_CurrencyCode = "USD",
            RatedShipment_GuaranteedDaysToDelivery = businessDays,
            RatedShipment_ScheduledDeliveryDate = request.Shipment_ShipDate.AddDays(businessDays),
            RatedShipment_ServiceDescription = GetServiceDescription(request.Service_Code)
        };
    }

    public UpsShipmentResponse Shipping_CreateShipment(UpsRatingRequest request)
    {
        return new UpsShipmentResponse
        {
            ShipmentIdentificationNumber = $"1Z{Random.Shared.Next(10000000, 99999999)}",
            PackageResults_TrackingNumber = $"1Z{Random.Shared.Next(10000000, 99999999)}",
            LabelImage_GraphicImage = Convert.ToBase64String("fake-ups-label"u8.ToArray())
        };
    }

    public UpsTrackResponse Track_GetStatus(string inquiryNumber)
    {
        return new UpsTrackResponse
        {
            InquiryNumber = inquiryNumber,
            Activity_Status_Description = "Out For Delivery",
            Activity_Status_Code = "I",
            EstimatedArrival = DateTime.UtcNow.AddHours(4)
        };
    }

    private static string GetServiceDescription(string serviceCode) => serviceCode switch
    {
        "01" => "UPS Next Day Air",
        "02" => "UPS 2nd Day Air",
        "03" => "UPS Ground",
        "12" => "UPS 3 Day Select",
        _ => "UPS Standard"
    };
}

// --- UPS-specific DTOs (intentionally ugly naming to match real UPS API style) ---

public class UpsRatingRequest
{
    public required string Shipper_PostalCode { get; set; }
    public required string ShipTo_PostalCode { get; set; }
    public double Shipment_Weight_Lbs { get; set; }
    public double Package_Length_In { get; set; }
    public double Package_Width_In { get; set; }
    public double Package_Height_In { get; set; }
    public DateTime Shipment_ShipDate { get; set; }
    public string Service_Code { get; set; } = "03";
    public string Shipment_DeliveryConfirmation { get; set; } = "NONE";
    public string Shipment_ResidentialIndicator { get; set; } = "N";
}

public class UpsRatingResponse
{
    public decimal RatedShipment_TotalCharges { get; set; }
    public string RatedShipment_CurrencyCode { get; set; } = "USD";
    public int RatedShipment_GuaranteedDaysToDelivery { get; set; }
    public DateTime RatedShipment_ScheduledDeliveryDate { get; set; }
    public string RatedShipment_ServiceDescription { get; set; } = string.Empty;
}

public class UpsShipmentResponse
{
    public string ShipmentIdentificationNumber { get; set; } = string.Empty;
    public string PackageResults_TrackingNumber { get; set; } = string.Empty;
    public string LabelImage_GraphicImage { get; set; } = string.Empty;
}

public class UpsTrackResponse
{
    public string InquiryNumber { get; set; } = string.Empty;
    public string Activity_Status_Description { get; set; } = string.Empty;
    public string Activity_Status_Code { get; set; } = string.Empty;
    public DateTime EstimatedArrival { get; set; }
}
