namespace DesignPatterns.Behavioral.Mediator.Colleagues;

/// <summary>
/// Sends notifications (email, SMS, etc.) about checkout events.
/// </summary>
public sealed class NotificationColleague
{
    private readonly List<NotificationRecord> _sentNotifications = [];

    public IReadOnlyList<NotificationRecord> SentNotifications => _sentNotifications.AsReadOnly();

    public Task SendOrderConfirmationAsync(string customerId, Guid orderId, string trackingNumber)
    {
        _sentNotifications.Add(new NotificationRecord(
            customerId,
            "OrderConfirmation",
            $"Order {orderId} confirmed. Tracking: {trackingNumber}"));

        return Task.CompletedTask;
    }

    public Task SendPaymentFailureAsync(string customerId, string reason)
    {
        _sentNotifications.Add(new NotificationRecord(
            customerId,
            "PaymentFailure",
            $"Payment failed: {reason}"));

        return Task.CompletedTask;
    }

    public Task SendInventoryFailureAsync(string customerId, string reason)
    {
        _sentNotifications.Add(new NotificationRecord(
            customerId,
            "InventoryFailure",
            $"Order could not be fulfilled: {reason}"));

        return Task.CompletedTask;
    }
}

public sealed record NotificationRecord(
    string CustomerId,
    string Type,
    string Message);
