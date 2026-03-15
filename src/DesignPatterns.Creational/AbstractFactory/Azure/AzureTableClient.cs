namespace DesignPatterns.Creational.AbstractFactory.Azure;

/// <summary>
/// Azure Table Storage client simulation.
/// In production, this would wrap the Azure.Data.Tables SDK.
/// </summary>
public sealed class AzureTableClient : ITableClient
{
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, object>>> _tables = new();

    public string ProviderName => "Azure";

    public string UpsertEntity(string tableName, string partitionKey, string rowKey, Dictionary<string, object> properties)
    {
        var compositeKey = $"{partitionKey}|{rowKey}";

        if (!_tables.ContainsKey(tableName))
            _tables[tableName] = new Dictionary<string, Dictionary<string, object>>();

        _tables[tableName][compositeKey] = new Dictionary<string, object>(properties)
        {
            ["PartitionKey"] = partitionKey,
            ["RowKey"] = rowKey
        };

        return $"azure-etag-{Guid.NewGuid():N}";
    }

    public Dictionary<string, object>? GetEntity(string tableName, string partitionKey, string rowKey)
    {
        var compositeKey = $"{partitionKey}|{rowKey}";
        if (_tables.TryGetValue(tableName, out var table) &&
            table.TryGetValue(compositeKey, out var entity))
            return new Dictionary<string, object>(entity);

        return null;
    }

    public bool DeleteEntity(string tableName, string partitionKey, string rowKey)
    {
        var compositeKey = $"{partitionKey}|{rowKey}";
        return _tables.TryGetValue(tableName, out var table) && table.Remove(compositeKey);
    }

    public IReadOnlyList<Dictionary<string, object>> QueryEntities(string tableName, string partitionKey)
    {
        if (!_tables.TryGetValue(tableName, out var table))
            return [];

        return table.Values
            .Where(e => e.TryGetValue("PartitionKey", out var pk) && pk.ToString() == partitionKey)
            .Select(e => new Dictionary<string, object>(e))
            .ToList();
    }
}
