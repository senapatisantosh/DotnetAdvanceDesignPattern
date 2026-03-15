namespace DesignPatterns.Creational.AbstractFactory;

/// <summary>
/// Consumer/client that uses the Abstract Factory.
///
/// This service has NO knowledge of whether it is talking to Azure or AWS.
/// It receives an ICloudStorageFactory and uses it to create the family of clients
/// it needs. Swapping cloud providers is a single factory change at the composition root.
/// </summary>
public sealed class CloudStorageService
{
    private readonly IBlobClient _blobClient;
    private readonly IQueueClient _queueClient;
    private readonly ITableClient _tableClient;
    private readonly string _providerName;

    public CloudStorageService(ICloudStorageFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _providerName = factory.ProviderName;
        _blobClient = factory.CreateBlobClient();
        _queueClient = factory.CreateQueueClient();
        _tableClient = factory.CreateTableClient();
    }

    public string ProviderName => _providerName;

    /// <summary>
    /// Stores a document and queues a notification — uses blob + queue clients together.
    /// The key point: both clients are from the SAME provider family.
    /// </summary>
    public string StoreDocumentAndNotify(string container, string documentName, byte[] content, string notificationQueue)
    {
        var blobUrl = _blobClient.UploadBlob(container, documentName, content);
        var messageId = _queueClient.SendMessage(notificationQueue, $"New document uploaded: {blobUrl}");
        return blobUrl;
    }

    /// <summary>
    /// Stores metadata in the table store and the file in blob storage.
    /// Both operations go to the same cloud provider.
    /// </summary>
    public string StoreDocumentWithMetadata(string container, string documentName, byte[] content,
        string tableName, Dictionary<string, object> metadata)
    {
        var blobUrl = _blobClient.UploadBlob(container, documentName, content);

        metadata["BlobUrl"] = blobUrl;
        metadata["UploadedAt"] = DateTimeOffset.UtcNow.ToString("O");

        _tableClient.UpsertEntity(tableName, container, documentName, metadata);
        return blobUrl;
    }

    /// <summary>
    /// Retrieves a document by looking up its metadata first, then downloading the blob.
    /// </summary>
    public byte[]? RetrieveDocument(string tableName, string container, string documentName)
    {
        var metadata = _tableClient.GetEntity(tableName, container, documentName);
        if (metadata is null)
            return null;

        return _blobClient.DownloadBlob(container, documentName);
    }
}
