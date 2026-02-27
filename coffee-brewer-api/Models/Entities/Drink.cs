/// Abstract base class representing a brewed drink produced by the <see cref="BrewFactory"/>.
public abstract class Drink
{
    public abstract DrinkType drinkType { get; }

    public abstract string brewMessage { get; }
}

/// Represents a brewed cup of tea.
public class Tea : Drink
{
    public override DrinkType drinkType => DrinkType.TEA;

    public override string brewMessage => "Your mellow tea is ready";
}

/// Represents a brewed cup of hot coffee.
public class HotCoffee : Drink
{
    public override DrinkType drinkType => DrinkType.HOTCOFEE;
    public override string brewMessage => "Your piping hot coffee is ready";
}



//Enable only for extra credit use case
/// Represents a brewed cup of hot coffee.
//public class IcedCoffee : Drink
//{
//    public override DrinkType drinkType => DrinkType.ICEDCOFEE;
//    public override string brewMessage => "Your refreshing iced coffee is ready";
//}