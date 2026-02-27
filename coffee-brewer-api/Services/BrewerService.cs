
public interface IBrewerService
{
    Task<BrewResult> BrewAsync();
}

/// Orchestrates the coffee brewing workflow, including date-based guards,
/// machine state checks, and drink production.
public class BrewerService : IBrewerService
{
    private readonly BrewMachine _brewMachine;
    private readonly IWeatherService _weatherService;
    private readonly TimeProvider _timeProvider;

    public BrewerService(
        BrewMachineV1 brewMachine,
        IWeatherService weatherService,
        TimeProvider timeProvider
    )
    {
        _brewMachine = brewMachine;
        _weatherService = weatherService;
        _timeProvider = timeProvider;
    }

    /// Attempts to brew a coffee, applying pre-brew guards before delegating to the machine.
    /// The drink type is determined by Manila's current temperature:
    ///   > 30°C → Hot Coffee, ≤ 30°C → Iced Coffee
    public async Task<BrewResult> BrewAsync()
    {
        var now = _timeProvider.GetLocalNow();

        /// April 1st guard — Returns <see cref="BrewStatus.Teapot"/> immediately
        /// without interacting with the machine (RFC 2324 Easter egg).
        if (now.Month == 4 && now.Day == 1)
            return new BrewResult(BrewStatus.Teapot);

        /// Refill guard — If the machine is in <see cref="BrewMachineState.NEED_REFILL"/>,
        /// triggers <see cref="BrewMachine.Refill"/> to reset it and returns <see cref="BrewStatus.Unavailable"/>
        /// The current request fails but the machine is ready for the next call.
        if (_brewMachine.state == BrewMachineState.NEED_REFILL)
        {
            _brewMachine.Refill();
            return new BrewResult(BrewStatus.Unavailable);
        }

        var temperature = await _weatherService.GetManilaTemperatureAsync();
        var drinkType = temperature > 30 ? DrinkType.HOTCOFEE : DrinkType.ICEDCOFEE;

        /// Drink is only populated on <see cref="BrewStatus.Success"/>.
        var drink = _brewMachine.BrewDrink(drinkType);
        return new BrewResult(BrewStatus.Success, drink);
    }
}
