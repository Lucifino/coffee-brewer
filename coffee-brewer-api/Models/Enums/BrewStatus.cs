public enum BrewStatus
{
    /// The drink was brewed successfully. The <see cref="BrewResult.Drink"/> property is populated.

    Success,
    /// The machine was in <see cref="BrewMachineState.NEED_REFILL"/> state.
    /// A refill has been triggered; the next request will succeed.
    /// Maps to HTTP 503 Service Unavailable.
    Unavailable,
    /// The request was made on April 1st (RFC 2324 Easter egg).
    /// Maps to HTTP 418 I'm a Teapot.
    Teapot
}
