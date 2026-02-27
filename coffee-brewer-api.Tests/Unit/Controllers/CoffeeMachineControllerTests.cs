using System.Text.Json;
using coffee_brewer_api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace coffee_brewer_api.Tests.Unit.Controllers;

public class CoffeeMachineControllerTests
{
    private readonly Mock<IBrewerService>     _brewerServiceMock = new();
    private readonly CoffeeMachineController  _sut;

    public CoffeeMachineControllerTests()
    {
        _sut = new CoffeeMachineController(_brewerServiceMock.Object);
    }

    // ── 200 OK 

    [Fact]
    public async Task BrewCoffee_WhenBrewSucceeds_Returns200Ok()
    {
        var drink = new HotCoffee();
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Success, drink));

        var result = await _sut.BrewCoffee();

        result.Should().BeOfType<OkObjectResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task BrewCoffee_WhenBrewSucceeds_ResponseBodyContainsDrinkMessage()
    {
        var drink = new HotCoffee();
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Success, drink));

        var result = (OkObjectResult)await _sut.BrewCoffee();

        // Serialise to JSON and inspect the payload
        var json = JsonSerializer.Serialize(result.Value);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("message").GetString()
           .Should().Be(drink.brewMessage);
    }

    [Fact]
    public async Task BrewCoffee_WhenBrewSucceeds_ResponseBodyContainsPreparedTimestamp()
    {

        //Enable only for extra credit use case
        //var drink = new IcedCoffee();

        //disable for for extra credit use case
        var drink = new HotCoffee();


        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Success, drink));

        var result = (OkObjectResult)await _sut.BrewCoffee();

        var json = JsonSerializer.Serialize(result.Value);
        using var doc = JsonDocument.Parse(json);
        var prepared = doc.RootElement.GetProperty("prepared").GetString();
        prepared.Should().NotBeNullOrWhiteSpace();
        DateTimeOffset.TryParse(prepared, out _).Should().BeTrue("prepared must be a valid ISO-8601 timestamp");
    }

    // ── 418 I'm a Teapot

    [Fact]
    public async Task BrewCoffee_WhenTeapotStatus_Returns418()
    {
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Teapot));

        var result = await _sut.BrewCoffee();

        result.Should().BeOfType<StatusCodeResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status418ImATeapot);
    }

    // ── 503 Service Unavailable

    [Fact]
    public async Task BrewCoffee_WhenUnavailableStatus_Returns503()
    {
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Unavailable));

        var result = await _sut.BrewCoffee();

        result.Should().BeOfType<StatusCodeResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    // ── 500 Internal Server Error (unexpected status)

    [Fact]
    public async Task BrewCoffee_WhenUnexpectedStatus_Returns500()
    {
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult((BrewStatus)999));

        var result = await _sut.BrewCoffee();

        result.Should().BeOfType<StatusCodeResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // ── Delegation

    [Fact]
    public async Task BrewCoffee_AlwaysDelegatesToBrewerService()
    {
        _brewerServiceMock.Setup(s => s.BrewAsync())
            .ReturnsAsync(new BrewResult(BrewStatus.Success, new Tea()));

        await _sut.BrewCoffee();

        _brewerServiceMock.Verify(s => s.BrewAsync(), Times.Once);
    }
}
