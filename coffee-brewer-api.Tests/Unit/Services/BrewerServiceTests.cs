using FluentAssertions;
using Moq;
using Xunit;

namespace coffee_brewer_api.Tests.Unit.Services;

public class BrewerServiceTests
{
    // Real collaborators where the interaction is the thing under test
    private readonly BrewFactory _brewFactory = new();
    private readonly BrewMachineV1 _brewMachine;

    // Mocked collaborators

    //Enable only for extra credit use case
    //private readonly Mock<IWeatherService> _weatherMock = new();

    private readonly Mock<TimeProvider> _timeProviderMock = new();

    public BrewerServiceTests()
    {
        _brewMachine = new BrewMachineV1(_brewFactory);

        // Default: a normal working day — not April 1st
        SetCurrentDate(month: 6, day: 15);
    }

    // ── April 1st guard

    [Fact]
    public async Task BrewAsync_OnAprilFirst_ReturnsTeapot()
    {
        SetCurrentDate(month: 4, day: 1);

        var result = await CreateSut().BrewAsync();

        result.Status.Should().Be(BrewStatus.Teapot);
        result.Drink.Should().BeNull();
    }

    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_OnAprilFirst_NeverInteractsWithMachineOrWeatherService()
    //{
    //    SetCurrentDate(month: 4, day: 1);

    //    await CreateSut().BrewAsync();

    //    _weatherMock.Verify(w => w.GetManilaTemperatureAsync(), Times.Never);
    //    // Machine state untouched — still ONLINE
    //    _brewMachine.state.Should().Be(BrewMachineState.ONLINE);
    //}

    [Theory]
    [InlineData(3, 31)]  // March 31
    [InlineData(4, 2)]   // April 2
    public async Task BrewAsync_OnDaysAdjacentToAprilFirst_DoesNotReturnTeapot(int month, int day)
    {
        SetCurrentDate(month, day);


        //Enable only for extra credit use case
        //_weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(25.0);

        var result = await CreateSut().BrewAsync();

        result.Status.Should().NotBe(BrewStatus.Teapot);
    }

    // ── Refill guard
    [Fact]
    public async Task BrewAsync_WhenMachineNeedsRefill_ReturnsUnavailable()
    {
        _brewMachine.LastBrew(); // force NEED_REFILL

        var result = await CreateSut().BrewAsync();

        result.Status.Should().Be(BrewStatus.Unavailable);
        result.Drink.Should().BeNull();
    }

    [Fact]
    public async Task BrewAsync_WhenMachineNeedsRefill_RefillsMachineBeforeReturning()
    {
        _brewMachine.LastBrew();

        await CreateSut().BrewAsync();

        // Machine must be ONLINE so the next request can brew
        _brewMachine.state.Should().Be(BrewMachineState.ONLINE);
    }

    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_WhenMachineNeedsRefill_NeverCallsWeatherService()
    //{
    //    _brewMachine.LastBrew();

    //    await CreateSut().BrewAsync();

    //    _weatherMock.Verify(w => w.GetManilaTemperatureAsync(), Times.Never);
    //}


    // ── Temperature-based drink selection

    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_WhenTemperatureAbove30_BrewsHotCoffee()
    //{
    //    _weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(30.1);

    //    var result = await CreateSut().BrewAsync();

    //    result.Status.Should().Be(BrewStatus.Success);
    //    result.Drink.Should().BeOfType<HotCoffee>();
    //}


    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_WhenTemperatureExactly30_BrewsIcedCoffee()
    //{
    //    // Boundary: > 30 is hot, exactly 30 is iced
    //    _weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(30.0);

    //    var result = await CreateSut().BrewAsync();

    //    result.Status.Should().Be(BrewStatus.Success);
    //    result.Drink.Should().BeOfType<IcedCoffee>();
    //}


    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_WhenTemperatureBelow30_BrewsIcedCoffee()
    //{
    //    _weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(20.0);

    //    var result = await CreateSut().BrewAsync();

    //    result.Status.Should().Be(BrewStatus.Success);
    //    result.Drink.Should().BeOfType<IcedCoffee>();
    //}

    // ── Weather service interaction


    //Enable only for extra credit use case
    //[Fact]
    //public async Task BrewAsync_OnSuccessfulBrew_CallsWeatherServiceExactlyOnce()
    //{
    //    _weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(35.0);

    //    await CreateSut().BrewAsync();

    //    _weatherMock.Verify(w => w.GetManilaTemperatureAsync(), Times.Once);
    //}

    // ── Machine state after successful brew


    [Fact]
    public async Task BrewAsync_AfterThreeSuccessfulBrews_MachineRemainsOnline()
    {

        //Enable only for extra credit use case
        //_weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(35.0);
        
        var sut = CreateSut();

        for (int i = 0; i < 3; i++)
            await sut.BrewAsync();

        _brewMachine.state.Should().Be(BrewMachineState.ONLINE);
    }

    [Fact]
    public async Task BrewAsync_AfterFourSuccessfulBrews_MachineSetsNeedRefill()
    {

        //Enable only for extra credit use case
        //_weatherMock.Setup(w => w.GetManilaTemperatureAsync()).ReturnsAsync(35.0);

        var sut = CreateSut();

        for (int i = 0; i < 4; i++)
            await sut.BrewAsync();

        _brewMachine.state.Should().Be(BrewMachineState.NEED_REFILL);
    }

    // ── Helpers
    private BrewerService CreateSut() =>
        new(_brewMachine,
            //Enable only for extra credit use case
            //_weatherMock.Object, 
            _timeProviderMock.Object);

    private void SetCurrentDate(int month, int day)
    {
        _timeProviderMock.Setup(tp => tp.GetUtcNow())
            .Returns(new DateTimeOffset(2024, month, day, 12, 0, 0, TimeSpan.Zero));
        _timeProviderMock.Setup(tp => tp.LocalTimeZone)
            .Returns(TimeZoneInfo.Utc);
    }
}
