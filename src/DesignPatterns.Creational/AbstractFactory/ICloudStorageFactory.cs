namespace DesignPatterns.Creational.AbstractFactory;

/// <summary>
/// Abstract Factory interface for creating families of related cloud storage clients.
///
/// The key insight: each cloud provider (Azure, AWS) needs a FAMILY of related objects
/// (blob client, queue client, table client) that must work together. You can't mix
/// an AWS blob client with an Azure queue client — they use different authentication,
/// regions, and protocols.
///
/// The Abstract Factory ensures you get a consistent family of objects for one provider.
/// </summary>
public interface ICloudStorageFactory
{
    string ProviderName { get; }
    IBlobClient CreateBlobClient();
    IQueueClient CreateQueueClient();
    ITableClient CreateTableClient();
}
