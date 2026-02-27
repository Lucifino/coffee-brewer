
/// Abstract base class for a stateful brew machine that tracks capacity
/// and delegates drink production to a <see cref="BrewFactory"/>.
public abstract class BrewMachine
{
    private BrewFactory _brewFactory { get; set; }

    public BrewMachine(
        BrewFactory brewFactory
    )
    {
        _brewFactory = brewFactory;
    }

    public BrewMachineState state { get; set; } = BrewMachineState.ONLINE;

    protected abstract int brewLimit { get; init; }

    protected int currentBrew { get; set; }

    /// Resets the brew counter and sets the machine state back to <see cref="BrewMachineState.ONLINE"/>.
    public void Refill()
    {
        currentBrew = 0;
        state = BrewMachineState.ONLINE;
    }

    /// Marks the machine as requiring a refill by setting state to <see cref="BrewMachineState.NEED_REFILL"/>.
    /// Called automatically by <see cref="BrewDrink"/> when <see cref="brewLimit"/> is reached.
    public void LastBrew()
    {
        state = BrewMachineState.NEED_REFILL;
    }


    /// Produces a drink of the specified type, incrementing the brew counter.
    /// Transitions to <see cref="BrewMachineState.NEED_REFILL"/> once the brew limit is reached.
    public Drink BrewDrink(DrinkType drinkType)
    {
        currentBrew++;
        if (currentBrew >= brewLimit)
            LastBrew();

        return _brewFactory.Brew(drinkType);
    }
}


/// Version 1 of the brew machine. Supports up to 4 consecutive brews before requiring a refill.
public class BrewMachineV1 : BrewMachine
{

    public BrewMachineV1(BrewFactory brewFactory) : base(brewFactory)
    {

    }
    
    protected override int brewLimit { get; init; } = 4;
}