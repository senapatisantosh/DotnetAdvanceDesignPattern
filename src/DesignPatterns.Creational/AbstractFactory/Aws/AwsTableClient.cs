namespace DesignPatterns.Creational.AbstractFactory.Aws;

/// <summary>
/// AWS DynamoDB client simulation.
/// In production, this would wrap the AWSSDK.DynamoDBv2 NuGet package.
/// </summary>
public sealed class AwsTableClient : ITableClient
{
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, object>>> _tables = new();

    public string ProviderName => "AWS";

    public string UpsertEntity(string tableName, string partitionKey, string rowKey, Dictionary<string, object> properties)
    {
        var compositeKey = $"{partitionKey}#{rowKey}";

        if (!_tables.ContainsKey(tableName))
            _tables[tableName] = new Dictionary<string, Dictionary<string, object>>();

        _tables[tableName][compositeKey] = new Dictionary<string, object>(properties)
        {
            ["pk"] = partitionKey,
            ["sk"] = rowKey
        };

        return $"aws-etag-{Guid.NewGuid():N}";
    }

    public Dictionary<string, object>? GetEntity(string tableName, string partitionKey, string rowKey)
    {
        var compositeKey = $"{partitionKey}#{rowKey}";
        if (_tables.TryGetValue(tableName, out var table) &&
            table.TryGetValue(compositeKey, out var entity))
            return new Dictionary<string, object>(entity);

        return null;
    }

    public bool DeleteEntity(string tableName, string partitionKey, string rowKey)
    {
        var compositeKey = $"{partitionKey}#{rowKey}";
        return _tables.TryGetValue(tableName, out var table) && table.Remove(compositeKey);
    }

    public IReadOnlyList<Dictionary<string, object>> QueryEntities(string tableName, string partitionKey)
    {
        if (!_tables.TryGetValue(tableName, out var table))
            return [];

        return table.Values
            .Where(e => e.TryGetValue("pk", out var pk) && pk.ToString() == partitionKey)
            .Select(e => new Dictionary<string, object>(e))
            .ToList();
    }
}
