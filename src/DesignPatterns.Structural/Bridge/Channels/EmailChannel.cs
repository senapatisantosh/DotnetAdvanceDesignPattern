namespace DesignPatterns.Structural.Bridge.Channels;

/// <summary>
/// Concrete implementor — delivers notifications via email.
/// Simulates SMTP delivery with HTML formatting.
/// </summary>
public sealed class EmailChannel : INotificationChannel
{
    private readonly List<DeliveryReceipt> _sentMessages = [];

    public string ChannelName => "Email";

    /// <summary>Provides access to sent messages for testing/verification.</summary>
    public IReadOnlyList<DeliveryReceipt> SentMessages => _sentMessages.AsReadOnly();

    public Task<DeliveryReceipt> SendAsync(string recipient, string subject, string body, NotificationPriority priority)
    {
        // Simulate email-specific behavior: high/critical priority adds "URGENT" prefix
        var emailSubject = priority >= NotificationPriority.High
            ? $"[URGENT] {subject}"
            : subject;

        var receipt = new DeliveryReceipt
        {
            ChannelName = ChannelName,
            MessageId = $"email-{Guid.NewGuid():N}",
            Recipient = recipient,
            SentAtUtc = DateTime.UtcNow,
            Succeeded = true
        };

        _sentMessages.Add(receipt);
        return Task.FromResult(receipt);
    }
}
