using FluentAssertions;
using Xunit;

namespace coffee_brewer_api.Tests.Unit.Factories;

public class BrewFactoryTests
{
    private readonly BrewFactory _sut = new();

    // one test per concrete drink type 

    [Fact]
    public void Brew_WithTea_ReturnsTea()
    {
        var result = _sut.Brew(DrinkType.TEA);

        result.Should().BeOfType<Tea>();
        result.drinkType.Should().Be(DrinkType.TEA);
        result.brewMessage.Should().Be("Your mellow tea is ready");
    }

    [Fact]
    public void Brew_WithHotCoffee_ReturnsHotCoffee()
    {
        var result = _sut.Brew(DrinkType.HOTCOFEE);

        result.Should().BeOfType<HotCoffee>();
        result.drinkType.Should().Be(DrinkType.HOTCOFEE);
        result.brewMessage.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public void Brew_WithIcedCoffee_ReturnsIcedCoffee()
    {
        var result = _sut.Brew(DrinkType.ICEDCOFEE);

        result.Should().BeOfType<IcedCoffee>();
        result.drinkType.Should().Be(DrinkType.ICEDCOFEE);
        result.brewMessage.Should().Be("Your refreshing iced coffee is ready");
    }

    // Parameterised coverage to ensure every enum value is handled
    [Theory]
    [InlineData(DrinkType.TEA,       typeof(Tea))]
    [InlineData(DrinkType.HOTCOFEE,  typeof(HotCoffee))]
    [InlineData(DrinkType.ICEDCOFEE, typeof(IcedCoffee))]
    public void Brew_ReturnsCorrectConcreteType_ForEachDrinkType(DrinkType drinkType, Type expectedType)
    {
        var result = _sut.Brew(drinkType);

        result.Should().BeOfType(expectedType);
        result.drinkType.Should().Be(drinkType);
    }

    // Each call produces a new instance (factory is stateless)
    [Fact]
    public void Brew_CalledTwice_ReturnsSeparateInstances()
    {
        var first  = _sut.Brew(DrinkType.HOTCOFEE);
        var second = _sut.Brew(DrinkType.HOTCOFEE);

        first.Should().NotBeSameAs(second);
    }

    // ── Error pat

    [Fact]
    public void Brew_WithUnknownDrinkType_ThrowsArgumentException()
    {
        Action act = () => _sut.Brew((DrinkType)999);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*No brewer configured for*");
    }
}
