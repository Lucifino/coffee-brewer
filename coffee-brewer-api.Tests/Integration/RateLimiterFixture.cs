using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace coffee_brewer_api.Tests.Integration;
public sealed class RateLimiterFixture : WebApplicationFactory<Program>
{

    //Enable only for extra credit use case
    //private readonly Mock<IWeatherService> _weatherMock = new();
    private readonly Mock<TimeProvider> _timeMock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {

            //Enable only for extra credit use case
            //var weatherDescriptor = services.SingleOrDefault(
            //    d => d.ServiceType == typeof(IWeatherService));
            //if (weatherDescriptor is not null)
            //    services.Remove(weatherDescriptor);
            //services.AddSingleton(_weatherMock.Object);

            var timeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TimeProvider));
            if (timeDescriptor is not null)
                services.Remove(timeDescriptor);
            services.AddSingleton(_timeMock.Object);
        });
    }

    //Enable only for extra credit use case
    //public void SetTemperature(double celsius) =>
    //    _weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(celsius);

    public void SetDate(int month, int day)
    {
        _timeMock.Setup(t => t.GetUtcNow())
            .Returns(new DateTimeOffset(2025, month, day, 12, 0, 0, TimeSpan.Zero));
        _timeMock.Setup(t => t.LocalTimeZone)
            .Returns(TimeZoneInfo.Utc);
    }
}
