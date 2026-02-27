using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace coffee_brewer_api.Tests.Unit.Services;

public class WeatherServiceTests : IDisposable
{
    private const string CacheKey = "manila-weather-temp";
    private const string TestApiKey = "test-api-key-12345";

    private readonly Mock<HttpMessageHandler> _handlerMock = new(MockBehavior.Strict);
    private readonly HttpClient               _httpClient;
    private readonly IConfiguration           _configuration;
    private readonly IMemoryCache             _cache;

    public WeatherServiceTests()
    {
        // Strict mocks require a setup for every invocation, including the
        // protected Dispose(bool) called by HttpClient.Dispose().
        _handlerMock.Protected()
            .Setup("Dispose", ItExpr.IsAny<bool>());

        _httpClient = new HttpClient(_handlerMock.Object);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenWeatherMap:ApiKey"] = TestApiKey
            })
            .Build();

        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _cache.Dispose();
    }

    // ── Cache miss → calls API

    [Fact]
    public async Task GetManilaTemperatureAsync_OnCacheMiss_CallsOpenWeatherMapAndReturnsTemp()
    {
        SetupApiResponse(28.5);
        var sut = CreateSut();

        var temp = await sut.GetManilaTemperatureAsync();

        temp.Should().Be(28.5);
        VerifyApiCalledTimes(Times.Once());
    }

    [Fact]
    public async Task GetManilaTemperatureAsync_OnCacheMiss_RequestsCorrectUrlWithApiKey()
    {
        HttpRequestMessage? captured = null;
        SetupApiResponse(25.0, onRequest: req => captured = req);
        var sut = CreateSut();

        await sut.GetManilaTemperatureAsync();

        captured.Should().NotBeNull();
        var url = captured!.RequestUri!.ToString();
        url.Should().Contain("Manila,PH");
        url.Should().Contain(TestApiKey);
        url.Should().Contain("units=metric");
        url.Should().Contain("data/2.5/weather");
    }

    // ── Cache hit → skips API
    [Fact]
    public async Task GetManilaTemperatureAsync_OnCacheHit_ReturnsCachedValueWithoutCallingApi()
    {
        _cache.Set(CacheKey, 31.0);
        // No HTTP handler setup — any network call would throw (MockBehavior.Strict)
        var sut = CreateSut();

        var temp = await sut.GetManilaTemperatureAsync();

        temp.Should().Be(31.0);
    }

    [Fact]
    public async Task GetManilaTemperatureAsync_OnSecondCall_UsesCache_AndCallsApiOnlyOnce()
    {
        SetupApiResponse(29.0);
        var sut = CreateSut();

        var first  = await sut.GetManilaTemperatureAsync();
        var second = await sut.GetManilaTemperatureAsync();

        first.Should().Be(29.0);
        second.Should().Be(29.0);
        VerifyApiCalledTimes(Times.Once());
    }

    // ── Cache population 

    [Fact]
    public async Task GetManilaTemperatureAsync_AfterApiCall_StoresTempInCache()
    {
        SetupApiResponse(33.2);
        var sut = CreateSut();

        await sut.GetManilaTemperatureAsync();

        _cache.TryGetValue(CacheKey, out double cached).Should().BeTrue();
        cached.Should().Be(33.2);
    }

    // ── HTTP error propagation

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetManilaTemperatureAsync_WhenApiReturnsError_ThrowsHttpRequestException(
        HttpStatusCode statusCode)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

        var sut = CreateSut();

        await sut.Invoking(s => s.GetManilaTemperatureAsync())
                 .Should().ThrowAsync<HttpRequestException>();
    }

    // ── Helpers 

    private WeatherService CreateSut() =>
        new(_httpClient, _configuration, _cache);

    private void SetupApiResponse(double temperature, Action<HttpRequestMessage>? onRequest = null)
    {
        var json = JsonSerializer.Serialize(new
        {
            main = new { temp = temperature }
        });

        var setup = _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        if (onRequest is not null)
        {
            setup.Callback<HttpRequestMessage, CancellationToken>((req, _) => onRequest(req))
                 .ReturnsAsync(OkResponse(json));
        }
        else
        {
            setup.ReturnsAsync(OkResponse(json));
        }
    }

    private static HttpResponseMessage OkResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private void VerifyApiCalledTimes(Times times) =>
        _handlerMock.Protected().Verify(
            "SendAsync",
            times,
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
}
