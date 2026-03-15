namespace DesignPatterns.Creational.AbstractFactory.Azure;

/// <summary>
/// Azure Blob Storage client simulation.
/// In production, this would wrap the Azure.Storage.Blobs SDK.
/// </summary>
public sealed class AzureBlobClient : IBlobClient
{
    private readonly Dictionary<string, Dictionary<string, byte[]>> _containers = new();

    public string ProviderName => "Azure";

    public string UploadBlob(string containerName, string blobName, byte[] data)
    {
        if (!_containers.ContainsKey(containerName))
            _containers[containerName] = new Dictionary<string, byte[]>();

        _containers[containerName][blobName] = data;
        return $"https://{containerName}.blob.core.windows.net/{blobName}";
    }

    public byte[] DownloadBlob(string containerName, string blobName)
    {
        if (_containers.TryGetValue(containerName, out var blobs) &&
            blobs.TryGetValue(blobName, out var data))
            return data;

        throw new InvalidOperationException($"Blob '{blobName}' not found in container '{containerName}'.");
    }

    public bool DeleteBlob(string containerName, string blobName)
    {
        return _containers.TryGetValue(containerName, out var blobs) && blobs.Remove(blobName);
    }

    public IReadOnlyList<string> ListBlobs(string containerName)
    {
        return _containers.TryGetValue(containerName, out var blobs)
            ? blobs.Keys.ToList()
            : [];
    }
}
