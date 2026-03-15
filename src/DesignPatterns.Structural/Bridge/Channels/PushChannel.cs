namespace DesignPatterns.Structural.Bridge.Channels;

/// <summary>
/// Concrete implementor — delivers notifications via mobile push notification.
/// Simulates APNs/FCM behavior with device tokens.
/// </summary>
public sealed class PushChannel : INotificationChannel
{
    private readonly List<DeliveryReceipt> _sentMessages = [];

    public string ChannelName => "Push";
    public IReadOnlyList<DeliveryReceipt> SentMessages => _sentMessages.AsReadOnly();

    public Task<DeliveryReceipt> SendAsync(string recipient, string subject, string body, NotificationPriority priority)
    {
        // Push-specific: critical messages set sound to alarm, others use default
        var receipt = new DeliveryReceipt
        {
            ChannelName = ChannelName,
            MessageId = $"push-{Guid.NewGuid():N}",
            Recipient = recipient,
            SentAtUtc = DateTime.UtcNow,
            Succeeded = true
        };

        _sentMessages.Add(receipt);
        return Task.FromResult(receipt);
    }
}
