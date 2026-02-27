using System.Net;
using FluentAssertions;
using Xunit;

namespace coffee_brewer_api.Tests.Integration;
public class RateLimiterTests
{
    // ── Permit limit ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GET_BrewCoffee_After60RequestsWithinWindow_Returns429()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        for (int i = 0; i < 60; i++)
            await client.GetAsync("/brew-coffee");

        var response = await client.GetAsync("/brew-coffee");

        ((int)response.StatusCode).Should().Be(429);
    }

    [Fact]
    public async Task GET_BrewCoffee_WithinPermitLimit_IsNotRateLimited()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // A single request must never be rate-limited
        var response = await client.GetAsync("/brew-coffee");

        ((int)response.StatusCode).Should().NotBe(429);
    }

    // ── Rejection behaviour

    [Fact]
    public async Task GET_BrewCoffee_WhenRateLimited_ReturnsExactly429NotOtherClientError()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        for (int i = 0; i < 60; i++)
            await client.GetAsync("/brew-coffee");

        var response = await client.GetAsync("/brew-coffee");

        // Verify the RejectionStatusCode was wired up correctly (not 503, not 400…)
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    // ── Helpers

    private static RateLimiterFixture CreateFactory()
    {
        var factory = new RateLimiterFixture();
        factory.SetDate(month: 6, day: 15); // not April 1st


        //Enable only for extra credit use case
        //factory.SetTemperature(35.0);
        return factory;
    }
}
