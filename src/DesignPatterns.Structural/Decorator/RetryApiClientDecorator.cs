namespace DesignPatterns.Structural.Decorator;

/// <summary>
/// Concrete Decorator — retries failed requests with exponential backoff.
/// Considers status codes >= 500 and network exceptions as retriable.
/// </summary>
public sealed class RetryApiClientDecorator : IApiClient
{
    private readonly IApiClient _inner;
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryApiClientDecorator(IApiClient inner, int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(100);
    }

    public int TotalRetries { get; private set; }

    public Task<ApiResponse> GetAsync(string url) =>
        ExecuteWithRetryAsync(() => _inner.GetAsync(url));

    public Task<ApiResponse> PostAsync(string url, string payload) =>
        ExecuteWithRetryAsync(() => _inner.PostAsync(url, payload));

    private async Task<ApiResponse> ExecuteWithRetryAsync(Func<Task<ApiResponse>> action)
    {
        ApiResponse? lastResponse = null;

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                lastResponse = await action();

                if (lastResponse.IsSuccess || !IsRetriableStatusCode(lastResponse.StatusCode))
                {
                    if (attempt > 0)
                    {
                        return lastResponse with { RetryCount = attempt };
                    }
                    return lastResponse;
                }
            }
            catch (Exception) when (attempt < _maxRetries)
            {
                // Swallow retriable exceptions, will retry
            }

            if (attempt < _maxRetries)
            {
                TotalRetries++;
                var delay = _baseDelay * Math.Pow(2, attempt);
                await Task.Delay(delay);
            }
        }

        // All retries exhausted — return last response with retry count
        return lastResponse! with { RetryCount = _maxRetries };
    }

    private static bool IsRetriableStatusCode(int statusCode) =>
        statusCode is >= 500 or 408 or 429;
}
