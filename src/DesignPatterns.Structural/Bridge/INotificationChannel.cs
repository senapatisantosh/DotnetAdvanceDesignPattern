namespace DesignPatterns.Structural.Bridge;

/// <summary>
/// Implementor interface — defines the delivery mechanism for notifications.
/// Each channel knows HOW to deliver a message but not WHAT the message is.
/// This is the "bridge" side that can vary independently from the notification type.
/// </summary>
public interface INotificationChannel
{
    string ChannelName { get; }

    /// <summary>
    /// Sends a message through this channel.
    /// </summary>
    /// <param name="recipient">Channel-specific recipient identifier (email, phone, device token, etc.).</param>
    /// <param name="subject">Short subject/title.</param>
    /// <param name="body">Full message body.</param>
    /// <param name="priority">Urgency level affecting delivery behavior.</param>
    /// <returns>A delivery receipt with channel-specific details.</returns>
    Task<DeliveryReceipt> SendAsync(string recipient, string subject, string body, NotificationPriority priority);
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Confirmation that a message was accepted by the delivery channel.
/// </summary>
public sealed record DeliveryReceipt
{
    public required string ChannelName { get; init; }
    public required string MessageId { get; init; }
    public required string Recipient { get; init; }
    public required DateTime SentAtUtc { get; init; }
    public bool Succeeded { get; init; } = true;
    public string? FailureReason { get; init; }
}
