using DesignPatterns.Enterprise.NullObject;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class NullObjectTests
{
    [Fact]
    public async Task DatabaseAuditLogger_records_entries()
    {
        var logger = new DatabaseAuditLogger();

        await logger.LogAsync("user-1", "PlaceOrder", "Order", "ORD-1", "Total: $50");

        var logs = await logger.GetLogsAsync("user-1");
        logs.Should().HaveCount(1);
        logs[0].Action.Should().Be("PlaceOrder");
        logs[0].Details.Should().Be("Total: $50");
    }

    [Fact]
    public async Task DatabaseAuditLogger_is_enabled()
    {
        var logger = new DatabaseAuditLogger();

        logger.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task NullAuditLogger_does_nothing()
    {
        var logger = NullAuditLogger.Instance;

        // These calls should not throw
        await logger.LogAsync("user-1", "PlaceOrder", "Order", "ORD-1");

        var logs = await logger.GetLogsAsync();
        logs.Should().BeEmpty();
    }

    [Fact]
    public void NullAuditLogger_is_disabled()
    {
        NullAuditLogger.Instance.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task OrderService_works_with_real_logger()
    {
        var logger = new DatabaseAuditLogger();
        var service = new OrderService(logger);

        var orderId = await service.PlaceOrderAsync("CUST-1", 99.99m);

        var logs = await logger.GetLogsAsync("CUST-1");
        logs.Should().HaveCount(1);
        logs[0].EntityId.Should().Be(orderId.ToString());
    }

    [Fact]
    public async Task OrderService_works_with_null_logger()
    {
        var service = new OrderService(NullAuditLogger.Instance);

        // Should not throw — NullAuditLogger silently absorbs calls
        var orderId = await service.PlaceOrderAsync("CUST-1", 99.99m);

        orderId.Should().NotBeEmpty();
        var order = service.GetOrder(orderId);
        order.Should().NotBeNull();
        order!.Total.Should().Be(99.99m);
    }

    [Fact]
    public async Task OrderService_cancel_order()
    {
        var logger = new DatabaseAuditLogger();
        var service = new OrderService(logger);

        var orderId = await service.PlaceOrderAsync("CUST-1", 50m);
        await service.CancelOrderAsync(orderId, "CUST-1");

        var order = service.GetOrder(orderId);
        order!.Status.Should().Be(OrderStatus.Cancelled);

        var logs = await logger.GetLogsAsync("CUST-1");
        logs.Should().HaveCount(2); // place + cancel
    }

    [Fact]
    public async Task OrderService_cancel_nonexistent_throws()
    {
        var service = new OrderService(NullAuditLogger.Instance);

        var act = () => service.CancelOrderAsync(Guid.NewGuid(), "CUST-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DatabaseAuditLogger_filters_by_user()
    {
        var logger = new DatabaseAuditLogger();
        await logger.LogAsync("user-1", "A", "T", "1");
        await logger.LogAsync("user-2", "B", "T", "2");
        await logger.LogAsync("user-1", "C", "T", "3");

        var user1Logs = await logger.GetLogsAsync("user-1");
        var allLogs = await logger.GetLogsAsync();

        user1Logs.Should().HaveCount(2);
        allLogs.Should().HaveCount(3);
    }
}
