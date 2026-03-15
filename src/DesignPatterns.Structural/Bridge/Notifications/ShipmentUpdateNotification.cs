namespace DesignPatterns.Structural.Bridge.Notifications;

/// <summary>
/// Refined Abstraction — formats a shipment tracking update message.
/// </summary>
public sealed class ShipmentUpdateNotification : Notification
{
    public string OrderId { get; }
    public string TrackingNumber { get; }
    public string CarrierName { get; }
    public string CurrentStatus { get; }
    public string CurrentLocation { get; }

    public ShipmentUpdateNotification(
        INotificationChannel channel,
        string orderId,
        string trackingNumber,
        string carrierName,
        string currentStatus,
        string currentLocation)
        : base(channel)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        CarrierName = carrierName;
        CurrentStatus = currentStatus;
        CurrentLocation = currentLocation;
    }

    public override NotificationPriority Priority => NotificationPriority.Normal;
    public override string NotificationType => "ShipmentUpdate";

    public override Task<DeliveryReceipt> SendAsync(string recipient)
    {
        var subject = $"Shipment Update — Order #{OrderId}";
        var body = $"Your order #{OrderId} shipped via {CarrierName} " +
                   $"(tracking: {TrackingNumber}) is now: {CurrentStatus}. " +
                   $"Current location: {CurrentLocation}.";

        return Channel.SendAsync(recipient, subject, body, Priority);
    }
}
