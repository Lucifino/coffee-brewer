using FluentAssertions;
using Xunit;

namespace coffee_brewer_api.Tests.Unit.Models;

public class BrewMachineTests
{
    // Real factory — BrewMachine integration with BrewFactory is intentional here;
    // it is the local factory-to-machine seam being tested.
    private readonly BrewFactory    _brewFactory = new();
    private readonly BrewMachineV1  _sut;

    public BrewMachineTests()
    {
        _sut = new BrewMachineV1(_brewFactory);
    }

    // ── Initial state

    [Fact]
    public void InitialState_IsOnline()
    {
        _sut.state.Should().Be(BrewMachineState.ONLINE);
    }

    // ── BrewDrink — drink production

    [Theory]
    [InlineData(DrinkType.TEA,       typeof(Tea))]
    [InlineData(DrinkType.HOTCOFEE,  typeof(HotCoffee))]
    [InlineData(DrinkType.ICEDCOFEE, typeof(IcedCoffee))]
    public void BrewDrink_ReturnsCorrectDrinkType(DrinkType drinkType, Type expectedType)
    {
        var drink = _sut.BrewDrink(drinkType);

        drink.Should().BeOfType(expectedType);
        drink.drinkType.Should().Be(drinkType);
    }

    // ── BrewDrink — brew-limit state transitions

    [Fact]
    public void BrewDrink_AfterBrewLimitReached_SetsStateToNeedRefill()
    {
        BrewTimes(4);

        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);
    }

    [Fact]
    public void BrewDrink_BeforeBrewLimitReached_StateRemainsOnline()
    {
        BrewTimes(3);

        _sut.state.Should().Be(BrewMachineState.ONLINE);
    }

    [Fact]
    public void BrewDrink_ExactlyAtBrewLimit_SetsStateToNeedRefill()
    {
        // First 3 brews keep machine ONLINE
        BrewTimes(3);
        _sut.state.Should().Be(BrewMachineState.ONLINE);

        // 4th brew is the boundary — triggers NEED_REFILL
        _sut.BrewDrink(DrinkType.HOTCOFEE);
        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);
    }

    [Fact]
    public void BrewDrink_StillReturnsDrinkOnLastBrew()
    {
        BrewTimes(3);

        var drink = _sut.BrewDrink(DrinkType.ICEDCOFEE);

        // State has flipped but the drink is still returned
        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);
        drink.Should().BeOfType<IcedCoffee>();
    }

    // ── Refill 

    [Fact]
    public void Refill_AfterBrewLimit_ResetsStateToOnline()
    {
        BrewTimes(4);
        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);

        _sut.Refill();

        _sut.state.Should().Be(BrewMachineState.ONLINE);
    }

    [Fact]
    public void Refill_AllowsAnotherFullBrewCycle()
    {
        BrewTimes(4);
        _sut.Refill();

        // Should survive 3 more brews without flipping to NEED_REFILL
        BrewTimes(3);
        _sut.state.Should().Be(BrewMachineState.ONLINE);

        // And transition again on the 4th
        _sut.BrewDrink(DrinkType.HOTCOFEE);
        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);
    }

    [Fact]
    public void Refill_WhenAlreadyOnline_KeepsStateOnline()
    {
        // Calling Refill on an already-healthy machine should not corrupt state
        _sut.Refill();

        _sut.state.Should().Be(BrewMachineState.ONLINE);
    }

    // ── LastBrew 
    [Fact]
    public void LastBrew_SetsStateToNeedRefill()
    {
        _sut.LastBrew();

        _sut.state.Should().Be(BrewMachineState.NEED_REFILL);
    }

    // ── BrewMachineV1 contract 

    [Fact]
    public void BrewMachineV1_BrewLimit_IsExactlyFour()
    {
        // Brews 1-3: ONLINE
        for (int i = 1; i <= 3; i++)
        {
            _sut.BrewDrink(DrinkType.HOTCOFEE);
            _sut.state.Should().Be(BrewMachineState.ONLINE, $"brew #{i} should keep machine ONLINE");
        }

        // Brew 4: triggers NEED_REFILL
        _sut.BrewDrink(DrinkType.HOTCOFEE);
        _sut.state.Should().Be(BrewMachineState.NEED_REFILL, "brew #4 should trigger NEED_REFILL");
    }

    // ── Helpers

    private void BrewTimes(int count)
    {
        for (int i = 0; i < count; i++)
            _sut.BrewDrink(DrinkType.HOTCOFEE);
    }
}
