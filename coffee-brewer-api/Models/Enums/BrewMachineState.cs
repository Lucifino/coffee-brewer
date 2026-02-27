/// Represents the operational state of a <see cref="BrewMachine"/>.
public enum BrewMachineState
{
    /// The machine is ready to brew.
    ONLINE,
    /// The machine has reached its brew limit and must be refilled before producing another drink.
    NEED_REFILL
}