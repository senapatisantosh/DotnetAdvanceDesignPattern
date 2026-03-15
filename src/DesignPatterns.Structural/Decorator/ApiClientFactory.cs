namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Composes decorators in the correct order to build a fully-featured API client.
/// The decoration order matters:
///   Logging -> Caching -> Retry -> BaseClient
///
/// This means:
/// 1. Logging captures ALL calls (including cache hits)
/// 2. Caching short-circuits before retries when possible
/// 3. Retry wraps only the actual HTTP call
/// </summary>
public static class ApiClientFactory
{
    /// <summary>
    /// Creates an API client with all resilience decorators applied.
    /// </summary>
    public static IApiClient CreateResilientClient(
        double failureRate = 0.0,
        int maxRetries = 3,
        TimeSpan? cacheTtl = null,
        TimeSpan? retryBaseDelay = null)
    {
        IApiClient client = new BaseApiClient(failureRate);
        client = new RetryApiClientDecorator(client, maxRetries, retryBaseDelay);
        client = new CachingApiClientDecorator(client, cacheTtl);
        client = new LoggingApiClientDecorator(client);

        return client;
    }

    /// <summary>
    /// Creates a client with only logging and caching (no retry).
    /// Useful for read-heavy, non-critical endpoints.
    /// </summary>
    public static IApiClient CreateCachedClient(TimeSpan? cacheTtl = null)
    {
        IApiClient client = new BaseApiClient();
        client = new CachingApiClientDecorator(client, cacheTtl);
        client = new LoggingApiClientDecorator(client);

        return client;
    }

    /// <summary>
    /// Creates a minimal client with only logging.
    /// </summary>
    public static IApiClient CreateLoggingClient()
    {
        IApiClient client = new BaseApiClient();
        client = new LoggingApiClientDecorator(client);

        return client;
    }
}
