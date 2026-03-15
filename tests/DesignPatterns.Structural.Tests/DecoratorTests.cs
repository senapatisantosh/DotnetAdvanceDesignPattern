using DesignPatterns.Structural.Decorator;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class DecoratorTests
{
    [Fact]
    public async Task BaseApiClient_Get_ReturnsSuccessResponse()
    {
        var client = new BaseApiClient();

        var response = await client.GetAsync("https://api.example.com/data");

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.RequestUrl.Should().Be("https://api.example.com/data");
        response.Body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BaseApiClient_Post_ReturnsSuccessResponse()
    {
        var client = new BaseApiClient();

        var response = await client.PostAsync("https://api.example.com/data", """{"name":"test"}""");

        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("created");
    }

    [Fact]
    public async Task LoggingDecorator_CapturesLogEntries()
    {
        var baseClient = new BaseApiClient();
        var loggingClient = new LoggingApiClientDecorator(baseClient);

        await loggingClient.GetAsync("https://api.example.com/users");

        loggingClient.LogEntries.Should().HaveCount(2);
        loggingClient.LogEntries[0].Message.Should().Contain("Starting");
        loggingClient.LogEntries[1].Message.Should().Contain("Completed");
    }

    [Fact]
    public async Task CachingDecorator_CachesGetRequests()
    {
        var baseClient = new BaseApiClient();
        var cachingClient = new CachingApiClientDecorator(baseClient);
        var url = "https://api.example.com/products";

        // First call: cache miss
        var response1 = await cachingClient.GetAsync(url);
        // Second call: cache hit
        var response2 = await cachingClient.GetAsync(url);

        cachingClient.CacheMisses.Should().Be(1);
        cachingClient.CacheHits.Should().Be(1);
        response2.FromCache.Should().BeTrue();
        baseClient.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task CachingDecorator_DoesNotCachePostRequests()
    {
        var baseClient = new BaseApiClient();
        var cachingClient = new CachingApiClientDecorator(baseClient);

        await cachingClient.PostAsync("https://api.example.com/orders", "{}");
        await cachingClient.PostAsync("https://api.example.com/orders", "{}");

        baseClient.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task CachingDecorator_Invalidate_ForcesRefresh()
    {
        var baseClient = new BaseApiClient();
        var cachingClient = new CachingApiClientDecorator(baseClient);
        var url = "https://api.example.com/products";

        await cachingClient.GetAsync(url);
        cachingClient.Invalidate(url);
        await cachingClient.GetAsync(url);

        cachingClient.CacheMisses.Should().Be(2);
        baseClient.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task RetryDecorator_RetriesOnServerError()
    {
        // Client that always fails
        var failingClient = new BaseApiClient(failureRate: 1.0);
        var retryClient = new RetryApiClientDecorator(
            failingClient, maxRetries: 2, baseDelay: TimeSpan.FromMilliseconds(1));

        var response = await retryClient.GetAsync("https://api.example.com/flaky");

        // Should have retried: original + 2 retries = 3 total calls
        failingClient.CallCount.Should().Be(3);
        retryClient.TotalRetries.Should().Be(2);
        response.RetryCount.Should().Be(2);
    }

    [Fact]
    public async Task RetryDecorator_DoesNotRetrySuccessfulRequests()
    {
        var baseClient = new BaseApiClient(failureRate: 0.0);
        var retryClient = new RetryApiClientDecorator(
            baseClient, maxRetries: 3, baseDelay: TimeSpan.FromMilliseconds(1));

        var response = await retryClient.GetAsync("https://api.example.com/stable");

        baseClient.CallCount.Should().Be(1);
        retryClient.TotalRetries.Should().Be(0);
        response.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ComposedDecorators_WorkTogether()
    {
        // Build the decorator chain: Logging -> Caching -> Base
        var baseClient = new BaseApiClient();
        var cachingClient = new CachingApiClientDecorator(baseClient);
        var loggingClient = new LoggingApiClientDecorator(cachingClient);

        var url = "https://api.example.com/products";

        await loggingClient.GetAsync(url); // miss
        await loggingClient.GetAsync(url); // hit

        // Logging should capture both calls
        loggingClient.LogEntries.Should().HaveCount(4); // 2 starts + 2 completes

        // Caching should have served second from cache
        cachingClient.CacheHits.Should().Be(1);

        // Base should only be called once
        baseClient.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task ApiClientFactory_CreateResilientClient_ReturnsWorkingClient()
    {
        var client = ApiClientFactory.CreateResilientClient(
            retryBaseDelay: TimeSpan.FromMilliseconds(1));

        var response = await client.GetAsync("https://api.example.com/test");

        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApiClientFactory_CreateCachedClient_CachesResults()
    {
        var client = ApiClientFactory.CreateCachedClient();

        var r1 = await client.GetAsync("https://api.example.com/cached");
        var r2 = await client.GetAsync("https://api.example.com/cached");

        r1.FromCache.Should().BeFalse();
        r2.FromCache.Should().BeTrue();
    }

    [Fact]
    public void ApiResponse_IsSuccess_CorrectForVariousStatusCodes()
    {
        var success = new ApiResponse
        {
            RequestUrl = "test", StatusCode = 200, Body = "", Duration = TimeSpan.Zero, TimestampUtc = DateTime.UtcNow
        };
        var notFound = new ApiResponse
        {
            RequestUrl = "test", StatusCode = 404, Body = "", Duration = TimeSpan.Zero, TimestampUtc = DateTime.UtcNow
        };
        var serverError = new ApiResponse
        {
            RequestUrl = "test", StatusCode = 500, Body = "", Duration = TimeSpan.Zero, TimestampUtc = DateTime.UtcNow
        };

        success.IsSuccess.Should().BeTrue();
        notFound.IsSuccess.Should().BeFalse();
        serverError.IsSuccess.Should().BeFalse();
    }
}
