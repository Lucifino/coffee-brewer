using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace coffee_brewer_api.Tests.Integration;
public class CoffeeMachineWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IWeatherService> WeatherMock { get; } = new();
    public Mock<TimeProvider> TimeProviderMock { get; } = new();

    public CoffeeMachineWebApplicationFactory()
    {
        // Sensible defaults: a normal working day, warm Manila weather
        SetDate(month: 6, day: 15);
        SetTemperature(35.0);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace IWeatherService
            var weatherDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IWeatherService));
            if (weatherDescriptor is not null)
                services.Remove(weatherDescriptor);
            services.AddSingleton(WeatherMock.Object);

            // Replace TimeProvider
            var timeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TimeProvider));
            if (timeDescriptor is not null)
                services.Remove(timeDescriptor);
            services.AddSingleton(TimeProviderMock.Object);

            // Disable the rate limiter so business-logic tests are not affected
            // by the shared per-IP bucket accumulating across the fixture.
            // AddPolicy throws if the key already exists, so we must remove the
            // IConfigureOptions registered by Program.cs before adding our own.
            var rateLimiterConfig = services.Where(
                d => d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>)).ToList();
            foreach (var d in rateLimiterConfig)
                services.Remove(d);

            services.Configure<RateLimiterOptions>(options =>
                options.AddPolicy("brew-fixed-window", _ =>
                    RateLimitPartition.GetNoLimiter("test")));
        });
    }

    // ── Convenience helpers
    public void SetDate(int month, int day)
    {
        TimeProviderMock.Setup(t => t.GetUtcNow())
            .Returns(new DateTimeOffset(2024, month, day, 12, 0, 0, TimeSpan.Zero));
        TimeProviderMock.Setup(t => t.LocalTimeZone)
            .Returns(TimeZoneInfo.Utc);
    }

    public void SetTemperature(double celsius) =>
        WeatherMock.Setup(w => w.GetManilaTemperatureAsync())
            .ReturnsAsync(celsius);

    /// Resets the <see cref="BrewMachineV1"/> singleton to its initial state
    /// between tests so each test starts from a clean machine.
    public void ResetMachine() =>
        Services.GetRequiredService<BrewMachineV1>().Refill();
}
