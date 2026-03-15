namespace DesignPatterns.Structural.Bridge.Channels;

/// <summary>
/// Concrete implementor — delivers notifications via SMS.
/// Truncates messages to 160 characters and formats for mobile.
/// </summary>
public sealed class SmsChannel : INotificationChannel
{
    private const int MaxSmsLength = 160;
    private readonly List<DeliveryReceipt> _sentMessages = [];

    public string ChannelName => "SMS";
    public IReadOnlyList<DeliveryReceipt> SentMessages => _sentMessages.AsReadOnly();

    public Task<DeliveryReceipt> SendAsync(string recipient, string subject, string body, NotificationPriority priority)
    {
        // SMS-specific: combine subject and body, truncate to character limit
        var smsText = $"{subject}: {body}";
        if (smsText.Length > MaxSmsLength)
            smsText = string.Concat(smsText.AsSpan(0, MaxSmsLength - 3), "...");

        var receipt = new DeliveryReceipt
        {
            ChannelName = ChannelName,
            MessageId = $"sms-{Guid.NewGuid():N}",
            Recipient = recipient,
            SentAtUtc = DateTime.UtcNow,
            Succeeded = true
        };

        _sentMessages.Add(receipt);
        return Task.FromResult(receipt);
    }
}
