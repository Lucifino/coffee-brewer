
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace coffee_brewer_api.Controllers
{
    [ApiController]
    [EnableRateLimiting("brew-fixed-window")]
    public class CoffeeMachineController : ControllerBase
    {
        private readonly IBrewerService _brewerService;

        public CoffeeMachineController(
            IBrewerService brewerService
        )
        {
            _brewerService = brewerService;
        }

        /// Delegates to <see cref="BrewerService.Brew"/> and maps the outcome to an HTTP status:
        ///   200 OK — Coffee brewed successfully. Body contains message, ISO-8601 prepared timestamp.
        ///   418 I'm a Teapot</b> — Request was made on April 1st (RFC 2324 Easter egg)
        ///   503 Service Unavailable — Machine hit its brew limit and is being refilled; retry the next request.
        ///   500 Internal Server Error — Unexpected brew status
        /// An "IActionResult" with the appropriate HTTP status code and, on success,
        /// a JSON body of { message: string, prepared: string }
        [HttpGet("brew-coffee")]
        public async Task<IActionResult> BrewCoffee()
        {
            var result = await _brewerService.BrewAsync();

            return result.Status switch
            {
                BrewStatus.Success     => Ok(new
                {
                    message = result.Drink!.brewMessage,
                    prepared = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                }),
                BrewStatus.Teapot      => StatusCode(StatusCodes.Status418ImATeapot),
                BrewStatus.Unavailable => StatusCode(StatusCodes.Status503ServiceUnavailable),
                _                      => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
