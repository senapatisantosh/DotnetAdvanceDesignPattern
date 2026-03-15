using DesignPatterns.Enterprise.Outbox;
using FluentAssertions;

namespace DesignPatterns.Enterprise.Tests;

public class OutboxTests
{
    private sealed record TestEvent(string OrderId, decimal Amount);

    private sealed class FakePublisher : IMessagePublisher
    {
        public List<(string EventType, string Payload)> Published { get; } = new();
        public bool ShouldFail { get; set; }

        public Task PublishAsync(string eventType, string payload, CancellationToken ct = default)
        {
            if (ShouldFail) throw new Exception("Broker unavailable");
            Published.Add((eventType, payload));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task OutboxPublisher_saves_message_to_store()
    {
        var store = new InMemoryOutboxStore();
        var publisher = new OutboxPublisher(store);

        await publisher.PublishAsync(new TestEvent("ORD-1", 99.99m));

        (await store.GetPendingCountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task OutboxProcessor_publishes_pending_messages()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher();
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher);

        await outboxPublisher.PublishAsync(new TestEvent("ORD-1", 50m));
        await outboxPublisher.PublishAsync(new TestEvent("ORD-2", 75m));

        var count = await processor.ProcessBatchAsync();

        count.Should().Be(2);
        fakePublisher.Published.Should().HaveCount(2);
        (await store.GetPendingCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task OutboxProcessor_marks_failed_messages()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher { ShouldFail = true };
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher);

        await outboxPublisher.PublishAsync(new TestEvent("ORD-1", 50m));
        var count = await processor.ProcessBatchAsync();

        count.Should().Be(0);
        (await store.GetPendingCountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task OutboxProcessor_retries_up_to_max()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher { ShouldFail = true };
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher, maxRetries: 2);

        await outboxPublisher.PublishAsync(new TestEvent("ORD-1", 50m));

        // Process twice to exhaust retries
        await processor.ProcessBatchAsync();
        await processor.ProcessBatchAsync();

        // Third time should skip the message
        var count = await processor.ProcessBatchAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task OutboxProcessor_recovers_after_broker_comes_back()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher { ShouldFail = true };
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher);

        await outboxPublisher.PublishAsync(new TestEvent("ORD-1", 50m));

        // First attempt fails
        await processor.ProcessBatchAsync();
        fakePublisher.Published.Should().BeEmpty();

        // Broker comes back
        fakePublisher.ShouldFail = false;
        var count = await processor.ProcessBatchAsync();

        count.Should().Be(1);
        fakePublisher.Published.Should().HaveCount(1);
    }

    [Fact]
    public async Task Batch_size_limits_processing()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher();
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher);

        for (int i = 0; i < 5; i++)
            await outboxPublisher.PublishAsync(new TestEvent($"ORD-{i}", i * 10m));

        var count = await processor.ProcessBatchAsync(batchSize: 2);

        count.Should().Be(2);
        (await store.GetPendingCountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Message_payload_contains_serialized_event()
    {
        var store = new InMemoryOutboxStore();
        var fakePublisher = new FakePublisher();
        var outboxPublisher = new OutboxPublisher(store);
        var processor = new OutboxProcessor(store, fakePublisher);

        await outboxPublisher.PublishAsync(new TestEvent("ORD-1", 42m));
        await processor.ProcessBatchAsync();

        fakePublisher.Published[0].Payload.Should().Contain("ORD-1");
        fakePublisher.Published[0].Payload.Should().Contain("42");
    }
}
