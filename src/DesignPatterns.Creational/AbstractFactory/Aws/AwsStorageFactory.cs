namespace DesignPatterns.Creational.AbstractFactory.Aws;

/// <summary>
/// Concrete factory that creates a family of AWS storage clients.
/// All clients share AWS-specific conventions: ARNs, regions, IAM auth.
/// </summary>
public sealed class AwsStorageFactory : ICloudStorageFactory
{
    public string ProviderName => "AWS";

    public IBlobClient CreateBlobClient() => new AwsBlobClient();
    public IQueueClient CreateQueueClient() => new AwsQueueClient();
    public ITableClient CreateTableClient() => new AwsTableClient();
}
