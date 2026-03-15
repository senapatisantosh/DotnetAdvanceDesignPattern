namespace DesignPatterns.Enterprise.DomainEvents;

/// <summary>
/// In-process event dispatcher that routes domain events to registered handlers.
/// Supports multiple handlers per event type (fan-out).
/// </summary>
public sealed class DomainEventDispatcher
{
    private readonly Dictionary<Type, List<Func<IDomainEvent, CancellationToken, Task>>> _handlers = new();

    /// <summary>
    /// Registers a handler for a specific event type.
    /// </summary>
    public void Register<TEvent>(IDomainEventHandler<TEvent> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Func<IDomainEvent, CancellationToken, Task>>();

        _handlers[eventType].Add((e, ct) => handler.HandleAsync((TEvent)e, ct));
    }

    /// <summary>
    /// Dispatches an event to all registered handlers for its type.
    /// Returns the number of handlers invoked.
    /// </summary>
    public async Task<int> DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return 0;

        foreach (var handler in handlers)
        {
            await handler(domainEvent, ct);
        }

        return handlers.Count;
    }

    /// <summary>
    /// Dispatches multiple events in order.
    /// </summary>
    public async Task<int> DispatchAllAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        var totalHandled = 0;
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(domainEvent, ct);
                }
                totalHandled += handlers.Count;
            }
        }
        return totalHandled;
    }
}
