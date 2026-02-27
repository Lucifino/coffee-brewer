/// Factory responsible for instantiating <see cref="Drink"/> objects based on a requested <see cref="DrinkType"/>.
public class BrewFactory
{
    /// Creates and returns a <see cref="Drink"/> instance for the specified drink type.
    public Drink Brew(DrinkType drinkType) => drinkType switch
    {
        DrinkType.TEA       => new Tea(),
        DrinkType.HOTCOFEE  => new HotCoffee(),
        DrinkType.ICEDCOFEE => new IcedCoffee(),
        _ => throw new ArgumentException($"No brewer configured for: {drinkType}")
    };
}
