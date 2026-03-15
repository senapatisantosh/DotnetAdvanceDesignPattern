namespace DesignPatterns.Creational.AbstractFactory.Aws;

/// <summary>
/// AWS S3 client simulation.
/// In production, this would wrap the AWSSDK.S3 NuGet package.
/// Note: AWS calls blobs "objects" and containers "buckets" — the abstraction normalizes this.
/// </summary>
public sealed class AwsBlobClient : IBlobClient
{
    private readonly Dictionary<string, Dictionary<string, byte[]>> _buckets = new();

    public string ProviderName => "AWS";

    public string UploadBlob(string containerName, string blobName, byte[] data)
    {
        if (!_buckets.ContainsKey(containerName))
            _buckets[containerName] = new Dictionary<string, byte[]>();

        _buckets[containerName][blobName] = data;
        return $"https://{containerName}.s3.amazonaws.com/{blobName}";
    }

    public byte[] DownloadBlob(string containerName, string blobName)
    {
        if (_buckets.TryGetValue(containerName, out var objects) &&
            objects.TryGetValue(blobName, out var data))
            return data;

        throw new InvalidOperationException($"Object '{blobName}' not found in bucket '{containerName}'.");
    }

    public bool DeleteBlob(string containerName, string blobName)
    {
        return _buckets.TryGetValue(containerName, out var objects) && objects.Remove(blobName);
    }

    public IReadOnlyList<string> ListBlobs(string containerName)
    {
        return _buckets.TryGetValue(containerName, out var objects)
            ? objects.Keys.ToList()
            : [];
    }
}
