namespace DesignPatterns.Creational.AbstractFactory;

/// <summary>
/// Abstraction for cloud blob/object storage operations.
/// Each cloud provider implements this with its own SDK and conventions.
/// </summary>
public interface IBlobClient
{
    string ProviderName { get; }
    string UploadBlob(string containerName, string blobName, byte[] data);
    byte[] DownloadBlob(string containerName, string blobName);
    bool DeleteBlob(string containerName, string blobName);
    IReadOnlyList<string> ListBlobs(string containerName);
}
