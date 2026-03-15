namespace DesignPatterns.Creational.AbstractFactory.Azure;

/// <summary>
/// Azure Storage Queue client simulation.
/// In production, this would wrap the Azure.Storage.Queues SDK.
/// </summary>
public sealed class AzureQueueClient : IQueueClient
{
    private readonly Dictionary<string, Queue<(string Id, string Body)>> _queues = new();

    public string ProviderName => "Azure";

    public string SendMessage(string queueName, string messageBody)
    {
        if (!_queues.ContainsKey(queueName))
            _queues[queueName] = new Queue<(string, string)>();

        var messageId = $"azure-msg-{Guid.NewGuid():N}";
        _queues[queueName].Enqueue((messageId, messageBody));
        return messageId;
    }

    public string? ReceiveMessage(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue) && queue.Count > 0)
            return queue.Dequeue().Body;

        return null;
    }

    public bool DeleteMessage(string queueName, string receiptHandle)
    {
        // In a real implementation, this would delete by receipt handle.
        return _queues.ContainsKey(queueName);
    }

    public int GetApproximateMessageCount(string queueName)
    {
        return _queues.TryGetValue(queueName, out var queue) ? queue.Count : 0;
    }
}
