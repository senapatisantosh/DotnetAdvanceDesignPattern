namespace DesignPatterns.Structural.Bridge.Channels;

/// <summary>
/// Concrete implementor — delivers notifications via Slack webhook.
/// Formats messages using Slack Block Kit markup.
/// </summary>
public sealed class SlackChannel : INotificationChannel
{
    private readonly List<DeliveryReceipt> _sentMessages = [];

    public string ChannelName => "Slack";
    public IReadOnlyList<DeliveryReceipt> SentMessages => _sentMessages.AsReadOnly();

    public Task<DeliveryReceipt> SendAsync(string recipient, string subject, string body, NotificationPriority priority)
    {
        // Slack-specific: format with emoji indicators based on priority
        var priorityEmoji = priority switch
        {
            NotificationPriority.Critical => ":rotating_light:",
            NotificationPriority.High => ":warning:",
            NotificationPriority.Normal => ":information_source:",
            NotificationPriority.Low => ":memo:",
            _ => ":bell:"
        };

        // Simulate posting to a Slack channel/DM via webhook
        var receipt = new DeliveryReceipt
        {
            ChannelName = ChannelName,
            MessageId = $"slack-{Guid.NewGuid():N}",
            Recipient = recipient,
            SentAtUtc = DateTime.UtcNow,
            Succeeded = true
        };

        _sentMessages.Add(receipt);
        return Task.FromResult(receipt);
    }
}
