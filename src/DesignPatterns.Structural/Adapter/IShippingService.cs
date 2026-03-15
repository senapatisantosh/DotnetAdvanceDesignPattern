namespace DesignPatterns.Structural.Adapter;

/// <summary>
/// Target interface — the unified shipping API our application expects.
/// All carrier adapters must conform to this contract regardless of
/// the underlying third-party API shape.
/// </summary>
public interface IShippingService
{
    string CarrierName { get; }
    Task<ShippingQuote> GetQuoteAsync(ShippingRequest request);
    Task<string> CreateShipmentAsync(ShippingRequest request);
    Task<string> GetTrackingStatusAsync(string trackingNumber);
}
