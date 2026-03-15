namespace DesignPatterns.Structural.Bridge;

/// <summary>
/// Abstraction — the base notification type that delegates delivery to
/// an <see cref="INotificationChannel"/> implementor.
///
/// The bridge: Notification types (OrderConfirmation, FraudAlert, etc.)
/// can vary independently from delivery channels (Email, SMS, Push, Slack).
/// Adding a new notification type does NOT require changing any channel.
/// Adding a new channel does NOT require changing any notification type.
/// </summary>
public abstract class Notification
{
    protected INotificationChannel Channel { get; }

    protected Notification(INotificationChannel channel)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    /// <summary>
    /// Each notification subclass defines its own content formatting,
    /// then delegates actual delivery to the channel.
    /// </summary>
    public abstract Task<DeliveryReceipt> SendAsync(string recipient);

    /// <summary>The priority level — subclasses set this based on business rules.</summary>
    public abstract NotificationPriority Priority { get; }

    /// <summary>Human-readable notification type name.</summary>
    public abstract string NotificationType { get; }
}
