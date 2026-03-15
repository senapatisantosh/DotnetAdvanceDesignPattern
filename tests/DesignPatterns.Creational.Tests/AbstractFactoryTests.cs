using DesignPatterns.Creational.AbstractFactory;
using DesignPatterns.Creational.AbstractFactory.Aws;
using DesignPatterns.Creational.AbstractFactory.Azure;
using FluentAssertions;

namespace DesignPatterns.Creational.Tests;

public class AbstractFactoryTests
{
    // --- Azure Factory creates Azure family ---

    [Fact]
    public void AzureFactory_CreatesBlobClient_WithCorrectProvider()
    {
        ICloudStorageFactory factory = new AzureStorageFactory();

        var blob = factory.CreateBlobClient();

        blob.Should().BeOfType<AzureBlobClient>();
        blob.ProviderName.Should().Be("Azure");
    }

    [Fact]
    public void AzureFactory_CreatesQueueClient_WithCorrectProvider()
    {
        ICloudStorageFactory factory = new AzureStorageFactory();

        var queue = factory.CreateQueueClient();

        queue.Should().BeOfType<AzureQueueClient>();
        queue.ProviderName.Should().Be("Azure");
    }

    [Fact]
    public void AzureFactory_CreatesTableClient_WithCorrectProvider()
    {
        ICloudStorageFactory factory = new AzureStorageFactory();

        var table = factory.CreateTableClient();

        table.Should().BeOfType<AzureTableClient>();
        table.ProviderName.Should().Be("Azure");
    }

    // --- AWS Factory creates AWS family ---

    [Fact]
    public void AwsFactory_CreatesBlobClient_WithCorrectProvider()
    {
        ICloudStorageFactory factory = new AwsStorageFactory();

        var blob = factory.CreateBlobClient();

        blob.Should().BeOfType<AwsBlobClient>();
        blob.ProviderName.Should().Be("AWS");
    }

    [Fact]
    public void AwsFactory_CreatesConsistentFamily()
    {
        ICloudStorageFactory factory = new AwsStorageFactory();

        var blob = factory.CreateBlobClient();
        var queue = factory.CreateQueueClient();
        var table = factory.CreateTableClient();

        blob.ProviderName.Should().Be("AWS");
        queue.ProviderName.Should().Be("AWS");
        table.ProviderName.Should().Be("AWS");
    }

    // --- Blob Client Operations ---

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void BlobClient_UploadAndDownload_RoundTrips(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var blob = factory.CreateBlobClient();
        var data = "Hello, Cloud!"u8.ToArray();

        var url = blob.UploadBlob("my-container", "test.txt", data);
        var downloaded = blob.DownloadBlob("my-container", "test.txt");

        url.Should().NotBeNullOrWhiteSpace();
        downloaded.Should().BeEquivalentTo(data);
    }

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void BlobClient_ListBlobs_ReturnsUploadedBlobs(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var blob = factory.CreateBlobClient();

        blob.UploadBlob("container1", "a.txt", [1, 2, 3]);
        blob.UploadBlob("container1", "b.txt", [4, 5, 6]);
        var list = blob.ListBlobs("container1");

        list.Should().HaveCount(2);
        list.Should().Contain("a.txt").And.Contain("b.txt");
    }

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void BlobClient_DeleteBlob_RemovesBlob(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var blob = factory.CreateBlobClient();
        blob.UploadBlob("c", "file.txt", [1]);

        var deleted = blob.DeleteBlob("c", "file.txt");

        deleted.Should().BeTrue();
        blob.ListBlobs("c").Should().BeEmpty();
    }

    // --- Queue Client Operations ---

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void QueueClient_SendAndReceive_Works(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var queue = factory.CreateQueueClient();

        queue.SendMessage("orders", "order-123");
        var received = queue.ReceiveMessage("orders");

        received.Should().Be("order-123");
    }

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void QueueClient_EmptyQueue_ReturnsNull(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var queue = factory.CreateQueueClient();

        var received = queue.ReceiveMessage("empty-queue");

        received.Should().BeNull();
    }

    // --- Table Client Operations ---

    [Theory]
    [InlineData(typeof(AzureStorageFactory))]
    [InlineData(typeof(AwsStorageFactory))]
    public void TableClient_UpsertAndGet_RoundTrips(Type factoryType)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var table = factory.CreateTableClient();
        var props = new Dictionary<string, object> { ["Name"] = "Widget", ["Price"] = 9.99 };

        table.UpsertEntity("Products", "electronics", "widget-1", props);
        var entity = table.GetEntity("Products", "electronics", "widget-1");

        entity.Should().NotBeNull();
        entity!["Name"].Should().Be("Widget");
    }

    // --- CloudStorageService Integration ---

    [Theory]
    [InlineData(typeof(AzureStorageFactory), "Azure")]
    [InlineData(typeof(AwsStorageFactory), "AWS")]
    public void CloudStorageService_StoresDocumentAndNotifies(Type factoryType, string expectedProvider)
    {
        var factory = (ICloudStorageFactory)Activator.CreateInstance(factoryType)!;
        var service = new CloudStorageService(factory);
        var content = "Invoice PDF content"u8.ToArray();

        service.ProviderName.Should().Be(expectedProvider);

        var url = service.StoreDocumentAndNotify("invoices", "inv-001.pdf", content, "notifications");

        url.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CloudStorageService_StoreAndRetrieveWithMetadata()
    {
        var factory = new AzureStorageFactory();
        var service = new CloudStorageService(factory);
        var content = "Report data"u8.ToArray();
        var metadata = new Dictionary<string, object> { ["Author"] = "Jane" };

        service.StoreDocumentWithMetadata("reports", "q4.pdf", content, "ReportMeta", metadata);
        var retrieved = service.RetrieveDocument("ReportMeta", "reports", "q4.pdf");

        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(content);
    }
}
