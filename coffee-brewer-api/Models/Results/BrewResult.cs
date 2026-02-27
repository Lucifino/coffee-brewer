
/// Represents the outcome of a brew attempt returned by <see cref="BrewerService.Brew"/>.
/// Encapsulates brewing logic from the controller
public record BrewResult(BrewStatus Status, Drink? Drink = null);
