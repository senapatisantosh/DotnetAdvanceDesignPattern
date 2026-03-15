using DesignPatterns.Enterprise.UnitOfWork;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class UnitOfWorkTests
{
    private sealed record OrderEntity(Guid Id, string CustomerId, decimal Total) : IEntity;

    [Fact]
    public async Task Commit_persists_new_entities()
    {
        using var uow = new InMemoryUnitOfWork();
        var order = new OrderEntity(Guid.NewGuid(), "CUST-1", 99.99m);

        uow.RegisterNew(order);
        var count = await uow.CommitAsync();

        count.Should().Be(1);
        uow.TryGet<OrderEntity>(order.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task Commit_persists_updated_entities()
    {
        using var uow = new InMemoryUnitOfWork();
        var order = new OrderEntity(Guid.NewGuid(), "CUST-1", 50m);

        uow.RegisterNew(order);
        await uow.CommitAsync();

        var updated = order with { Total = 75m };
        uow.RegisterDirty(updated);
        await uow.CommitAsync();

        uow.TryGet<OrderEntity>(order.Id)!.Total.Should().Be(75m);
    }

    [Fact]
    public async Task Commit_removes_deleted_entities()
    {
        using var uow = new InMemoryUnitOfWork();
        var order = new OrderEntity(Guid.NewGuid(), "CUST-1", 50m);

        uow.RegisterNew(order);
        await uow.CommitAsync();

        uow.RegisterDeleted(order);
        await uow.CommitAsync();

        uow.TryGet<OrderEntity>(order.Id).Should().BeNull();
    }

    [Fact]
    public void Rollback_discards_pending_changes()
    {
        using var uow = new InMemoryUnitOfWork();
        var order = new OrderEntity(Guid.NewGuid(), "CUST-1", 50m);

        uow.RegisterNew(order);
        uow.Rollback();

        uow.TryGet<OrderEntity>(order.Id).Should().BeNull();
    }

    [Fact]
    public async Task No_changes_returns_zero()
    {
        using var uow = new InMemoryUnitOfWork();

        var count = await uow.CommitAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task Change_log_records_all_committed_operations()
    {
        using var uow = new InMemoryUnitOfWork();
        var o1 = new OrderEntity(Guid.NewGuid(), "C1", 10m);
        var o2 = new OrderEntity(Guid.NewGuid(), "C2", 20m);

        uow.RegisterNew(o1);
        uow.RegisterNew(o2);
        await uow.CommitAsync();

        uow.RegisterDeleted(o1);
        await uow.CommitAsync();

        var log = uow.GetChangeLog();
        log.Should().HaveCount(3);
        log[0].ChangeType.Should().Be(ChangeType.Insert);
        log[2].ChangeType.Should().Be(ChangeType.Delete);
    }

    [Fact]
    public async Task Insert_duplicate_rolls_back_atomically()
    {
        using var uow = new InMemoryUnitOfWork();
        var order = new OrderEntity(Guid.NewGuid(), "CUST-1", 50m);

        uow.RegisterNew(order);
        await uow.CommitAsync();

        // Try to insert same ID again
        uow.RegisterNew(order);
        var act = () => uow.CommitAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Disposed_unit_of_work_throws()
    {
        var uow = new InMemoryUnitOfWork();
        uow.Dispose();

        var order = new OrderEntity(Guid.NewGuid(), "C1", 10m);
        var act = () => uow.RegisterNew(order);

        act.Should().Throw<ObjectDisposedException>();
    }
}
