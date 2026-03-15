namespace DesignPatterns.Creational.AbstractFactory.Azure;

/// <summary>
/// Concrete factory that creates a family of Azure storage clients.
/// All clients share Azure-specific conventions: connection strings, regions, auth.
/// </summary>
public sealed class AzureStorageFactory : ICloudStorageFactory
{
    public string ProviderName => "Azure";

    public IBlobClient CreateBlobClient() => new AzureBlobClient();
    public IQueueClient CreateQueueClient() => new AzureQueueClient();
    public ITableClient CreateTableClient() => new AzureTableClient();
}
