using DesignPatterns.Structural.Bridge;
using DesignPatterns.Structural.Bridge.Channels;
using DesignPatterns.Structural.Bridge.Notifications;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class BridgeTests
{
    [Fact]
    public async Task OrderConfirmation_ViaEmail_SendsSuccessfully()
    {
        var emailChannel = new EmailChannel();
        var notification = new OrderConfirmationNotification(
            emailChannel, "ORD-12345", 149.99m, "John Doe", DateTime.UtcNow.AddDays(5));

        var receipt = await notification.SendAsync("john@example.com");

        receipt.Succeeded.Should().BeTrue();
        receipt.ChannelName.Should().Be("Email");
        receipt.Recipient.Should().Be("john@example.com");
        emailChannel.SentMessages.Should().HaveCount(1);
    }

    [Fact]
    public async Task OrderConfirmation_ViaSms_SendsSuccessfully()
    {
        var smsChannel = new SmsChannel();
        var notification = new OrderConfirmationNotification(
            smsChannel, "ORD-12345", 149.99m, "John", DateTime.UtcNow.AddDays(5));

        var receipt = await notification.SendAsync("+15551234567");

        receipt.Succeeded.Should().BeTrue();
        receipt.ChannelName.Should().Be("SMS");
    }

    [Fact]
    public async Task FraudAlert_HasCriticalPriority()
    {
        var slackChannel = new SlackChannel();
        var notification = new FraudAlertNotification(
            slackChannel, "TXN-999", 5000m, "Velocity check failed",
            "192.168.1.100", DateTime.UtcNow);

        notification.Priority.Should().Be(NotificationPriority.Critical);
        notification.NotificationType.Should().Be("FraudAlert");

        var receipt = await notification.SendAsync("#fraud-alerts");
        receipt.Succeeded.Should().BeTrue();
        receipt.ChannelName.Should().Be("Slack");
    }

    [Fact]
    public async Task ShipmentUpdate_ViaPush_SendsSuccessfully()
    {
        var pushChannel = new PushChannel();
        var notification = new ShipmentUpdateNotification(
            pushChannel, "ORD-12345", "FDX123456789", "FedEx",
            "Out for delivery", "Local distribution center");

        var receipt = await notification.SendAsync("device-token-xyz");

        receipt.Succeeded.Should().BeTrue();
        receipt.ChannelName.Should().Be("Push");
        pushChannel.SentMessages.Should().HaveCount(1);
    }

    [Fact]
    public async Task SameNotificationType_DifferentChannels_AllSucceed()
    {
        // Bridge pattern power: same notification type, multiple delivery channels
        var channels = new INotificationChannel[]
        {
            new EmailChannel(),
            new SmsChannel(),
            new PushChannel(),
            new SlackChannel()
        };

        var receipts = new List<DeliveryReceipt>();

        foreach (var channel in channels)
        {
            var notification = new OrderConfirmationNotification(
                channel, "ORD-999", 75.00m, "Jane", DateTime.UtcNow.AddDays(3));
            receipts.Add(await notification.SendAsync("recipient"));
        }

        receipts.Should().HaveCount(4);
        receipts.Should().OnlyContain(r => r.Succeeded);
        receipts.Select(r => r.ChannelName).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task DifferentNotificationTypes_SameChannel_AllSucceed()
    {
        // Bridge: multiple notification types through same channel
        var emailChannel = new EmailChannel();

        var orderNotification = new OrderConfirmationNotification(
            emailChannel, "ORD-1", 100m, "Alice", DateTime.UtcNow.AddDays(5));
        var shipmentNotification = new ShipmentUpdateNotification(
            emailChannel, "ORD-1", "1Z999", "UPS", "Delivered", "Front door");
        var fraudNotification = new FraudAlertNotification(
            emailChannel, "TXN-1", 999m, "Card not present", "10.0.0.1", DateTime.UtcNow);

        await orderNotification.SendAsync("alice@example.com");
        await shipmentNotification.SendAsync("alice@example.com");
        await fraudNotification.SendAsync("security@example.com");

        emailChannel.SentMessages.Should().HaveCount(3);
    }

    [Fact]
    public void DeliveryReceipt_HasUniqueMessageId()
    {
        var receipt1 = new DeliveryReceipt
        {
            ChannelName = "Test",
            MessageId = $"test-{Guid.NewGuid():N}",
            Recipient = "test@example.com",
            SentAtUtc = DateTime.UtcNow
        };

        var receipt2 = new DeliveryReceipt
        {
            ChannelName = "Test",
            MessageId = $"test-{Guid.NewGuid():N}",
            Recipient = "test@example.com",
            SentAtUtc = DateTime.UtcNow
        };

        receipt1.MessageId.Should().NotBe(receipt2.MessageId);
    }
}
