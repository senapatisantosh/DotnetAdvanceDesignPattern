namespace DesignPatterns.Behavioral.ChainOfResponsibility.Pipeline;

/// <summary>
/// Simulates persisting the expense request to a data store.
/// In a real system this would write to a database.
/// </summary>
public sealed class PersistenceStep : IPipelineStep<ExpenseRequest>
{
    private readonly List<ExpenseRequest> _persistedRequests = [];

    /// <summary>
    /// All requests that have been "persisted" through this step.
    /// Useful for testing and verification.
    /// </summary>
    public IReadOnlyList<ExpenseRequest> PersistedRequests => _persistedRequests.AsReadOnly();

    public async Task<ExpenseRequest> ProcessAsync(ExpenseRequest input, Func<ExpenseRequest, Task<ExpenseRequest>> next)
    {
        var persisted = input with
        {
            Metadata = new Dictionary<string, string>(input.Metadata)
            {
                ["Persisted"] = "true",
                ["PersistedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        _persistedRequests.Add(persisted);

        return await next(persisted);
    }
}
