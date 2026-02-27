using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace coffee_brewer_api.Tests.Integration;

public class CoffeeMachineEndpointTests : IClassFixture<CoffeeMachineWebApplicationFactory>
{
    private readonly CoffeeMachineWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CoffeeMachineEndpointTests(CoffeeMachineWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
        _factory.ResetMachine();
    }

    // ── 200 OK — successful brews 

    [Fact]
    public async Task GET_BrewCoffee_WhenMachineOnline_Returns200WithHotCoffeeMessage()
    {

        var response = await _client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ParseBodyAsync(response);
        body.GetProperty("message").GetString()
            .Should().Be("Your piping hot coffee is ready");
    }


    //Enable only for extra credit use case
    //[Fact]
    //public async Task GET_BrewCoffee_WhenMachineOnlineAndTempAbove30_Returns200WithHotCoffeeMessage()
    //{
    //    _factory.SetTemperature(35.0); // > 30°C → hot

    //    var response = await _client.GetAsync("/brew-coffee");

    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var body = await ParseBodyAsync(response);
    //    body.GetProperty("message").GetString()
    //        .Should().Be("Your piping hot coffee is ready");
    //}



    //Enable only for extra credit use case
    //[Fact]
    //public async Task GET_BrewCoffee_WhenMachineOnlineAndTempAtOrBelow30_Returns200WithIcedCoffeeMessage()
    //{
    //    _factory.SetTemperature(30.0); // ≤ 30°C → iced

    //    var response = await _client.GetAsync("/brew-coffee");

    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var body = await ParseBodyAsync(response);
    //    body.GetProperty("message").GetString()
    //        .Should().Be("Your refreshing iced coffee is ready");
    //}

    [Fact]
    public async Task GET_BrewCoffee_SuccessResponse_IncludesValidIso8601PreparedTimestamp()
    {

        //Enable only for extra credit use case
        //_factory.SetTemperature(35.0);

        var response = await _client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ParseBodyAsync(response);
        var prepared = body.GetProperty("prepared").GetString();
        DateTimeOffset.TryParse(prepared, out _)
            .Should().BeTrue($"expected ISO-8601 timestamp but got: {prepared}");
    }

    // ── 418 I'm a Teapot — April 1st easter egg 

    [Fact]
    public async Task GET_BrewCoffee_OnAprilFirst_Returns418()
    {
        _factory.SetDate(month: 4, day: 1);

        var response = await _client.GetAsync("/brew-coffee");

        // 418 I'm a Teapot is not in the HttpStatusCode enum, compare as int
        ((int)response.StatusCode).Should().Be(418);
    }

    [Fact]
    public async Task GET_BrewCoffee_OnAprilFirst_DoesNotDecrementBrewCounter()
    {
        _factory.SetDate(month: 4, day: 1);
        await _client.GetAsync("/brew-coffee"); // 418 — machine untouched

        // Restore a normal date and brew — machine should still have its full 4 brews
        _factory.SetDate(month: 6, day: 15);


        //Enable only for extra credit use case
        //_factory.SetTemperature(35.0);

        for (int i = 0; i < 4; i++)
        {
            var r = await _client.GetAsync("/brew-coffee");
            ((int)r.StatusCode).Should().Be(200,
                $"brew #{i + 1} should succeed because April 1st did not consume capacity");
        }
    }

    // ── 503 Service Unavailable — brew-limit / refill cycle 

    [Fact]
    public async Task GET_BrewCoffee_AfterBrewLimit_Returns503()
    {

        //Enable only for extra credit use case
        //_factory.SetTemperature(35.0);

        // Exhaust the brew limit (4 brews)
        for (int i = 0; i < 4; i++)
            await _client.GetAsync("/brew-coffee");

        var response = await _client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GET_BrewCoffee_RequestAfter503_Returns200BecauseMachineWasRefilled()
    {

        //Enable only for extra credit use case
        //_factory.SetTemperature(35.0);

        for (int i = 0; i < 4; i++)
            await _client.GetAsync("/brew-coffee");

        // 5th request → 503, machine is refilled as a side-effect
        await _client.GetAsync("/brew-coffee");

        // 6th request → 200
        var response = await _client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_BrewCoffee_FullBrewCycle_ProducesExpectedStatusSequence()
    {

        //Enable only for extra credit use case
        //_factory.SetTemperature(35.0);

        // Brews 1-4: all 200
        for (int i = 1; i <= 4; i++)
        {
            var r = await _client.GetAsync("/brew-coffee");
            ((int)r.StatusCode).Should().Be(200, $"brew #{i} should be 200");
        }

        // Brew 5: machine needs refill → 503
        var unavailable = await _client.GetAsync("/brew-coffee");
        ((int)unavailable.StatusCode).Should().Be(503, "5th brew should trigger 503");

        // Brew 6: machine was just refilled → 200 again
        var afterRefill = await _client.GetAsync("/brew-coffee");
        ((int)afterRefill.StatusCode).Should().Be(200, "6th brew should succeed after refill");
    }

    // ── Helper

    private static async Task<JsonElement> ParseBodyAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement;
    }
}
