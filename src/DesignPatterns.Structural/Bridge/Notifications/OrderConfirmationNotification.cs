namespace DesignPatterns.Structural.Bridge.Notifications;

/// <summary>
/// Refined Abstraction — formats an order confirmation message and
/// delegates delivery to whatever channel was injected.
/// </summary>
public sealed class OrderConfirmationNotification : Notification
{
    public string OrderId { get; }
    public decimal TotalAmount { get; }
    public string CustomerName { get; }
    public DateTime EstimatedDelivery { get; }

    public OrderConfirmationNotification(
        INotificationChannel channel,
        string orderId,
        decimal totalAmount,
        string customerName,
        DateTime estimatedDelivery)
        : base(channel)
    {
        OrderId = orderId;
        TotalAmount = totalAmount;
        CustomerName = customerName;
        EstimatedDelivery = estimatedDelivery;
    }

    public override NotificationPriority Priority => NotificationPriority.Normal;
    public override string NotificationType => "OrderConfirmation";

    public override Task<DeliveryReceipt> SendAsync(string recipient)
    {
        var subject = $"Order Confirmed — #{OrderId}";
        var body = $"Hi {CustomerName}, your order #{OrderId} for {TotalAmount:C} " +
                   $"has been confirmed. Estimated delivery: {EstimatedDelivery:MMMM dd, yyyy}. " +
                   $"Thank you for your purchase!";

        return Channel.SendAsync(recipient, subject, body, Priority);
    }
}
