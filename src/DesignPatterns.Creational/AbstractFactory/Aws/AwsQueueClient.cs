namespace DesignPatterns.Creational.AbstractFactory.Aws;

/// <summary>
/// AWS SQS client simulation.
/// In production, this would wrap the AWSSDK.SQS NuGet package.
/// </summary>
public sealed class AwsQueueClient : IQueueClient
{
    private readonly Dictionary<string, Queue<(string Id, string Body)>> _queues = new();

    public string ProviderName => "AWS";

    public string SendMessage(string queueName, string messageBody)
    {
        if (!_queues.ContainsKey(queueName))
            _queues[queueName] = new Queue<(string, string)>();

        var messageId = $"aws-sqs-{Guid.NewGuid():N}";
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
        return _queues.ContainsKey(queueName);
    }

    public int GetApproximateMessageCount(string queueName)
    {
        return _queues.TryGetValue(queueName, out var queue) ? queue.Count : 0;
    }
}
