namespace DesignPatterns.Creational.AbstractFactory;

/// <summary>
/// Abstraction for cloud message queue operations.
/// AWS SQS, Azure Storage Queues, and GCP Pub/Sub all have different APIs,
/// but the core operations are the same.
/// </summary>
public interface IQueueClient
{
    string ProviderName { get; }
    string SendMessage(string queueName, string messageBody);
    string? ReceiveMessage(string queueName);
    bool DeleteMessage(string queueName, string receiptHandle);
    int GetApproximateMessageCount(string queueName);
}
