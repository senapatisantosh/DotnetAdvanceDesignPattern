namespace DesignPatterns.Structural.Bridge.Notifications;

/// <summary>
/// Refined Abstraction — formats a fraud alert message.
/// Always sends at Critical priority regardless of channel.
/// </summary>
public sealed class FraudAlertNotification : Notification
{
    public string TransactionId { get; }
    public decimal Amount { get; }
    public string SuspiciousReason { get; }
    public string IpAddress { get; }
    public DateTime OccurredAtUtc { get; }

    public FraudAlertNotification(
        INotificationChannel channel,
        string transactionId,
        decimal amount,
        string suspiciousReason,
        string ipAddress,
        DateTime occurredAtUtc)
        : base(channel)
    {
        TransactionId = transactionId;
        Amount = amount;
        SuspiciousReason = suspiciousReason;
        IpAddress = ipAddress;
        OccurredAtUtc = occurredAtUtc;
    }

    public override NotificationPriority Priority => NotificationPriority.Critical;
    public override string NotificationType => "FraudAlert";

    public override Task<DeliveryReceipt> SendAsync(string recipient)
    {
        var subject = $"FRAUD ALERT — Transaction {TransactionId}";
        var body = $"Suspicious transaction detected! " +
                   $"Transaction ID: {TransactionId}, Amount: {Amount:C}, " +
                   $"Reason: {SuspiciousReason}, Source IP: {IpAddress}, " +
                   $"Time: {OccurredAtUtc:u}. Immediate review required.";

        return Channel.SendAsync(recipient, subject, body, Priority);
    }
}
