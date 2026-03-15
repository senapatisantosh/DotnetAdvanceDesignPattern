namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Component interface — defines the contract for an API client.
/// Both the concrete implementation and all decorators implement this interface.
/// </summary>
public interface IApiClient
{
    Task<ApiResponse> GetAsync(string url);
    Task<ApiResponse> PostAsync(string url, string payload);
}
