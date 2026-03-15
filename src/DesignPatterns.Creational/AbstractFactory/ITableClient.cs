namespace DesignPatterns.Creational.AbstractFactory;

/// <summary>
/// Abstraction for cloud NoSQL table/key-value storage operations.
/// Azure Table Storage, AWS DynamoDB, GCP Datastore all serve similar purposes.
/// </summary>
public interface ITableClient
{
    string ProviderName { get; }
    string UpsertEntity(string tableName, string partitionKey, string rowKey, Dictionary<string, object> properties);
    Dictionary<string, object>? GetEntity(string tableName, string partitionKey, string rowKey);
    bool DeleteEntity(string tableName, string partitionKey, string rowKey);
    IReadOnlyList<Dictionary<string, object>> QueryEntities(string tableName, string partitionKey);
}
