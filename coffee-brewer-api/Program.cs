using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<BrewFactory>();
builder.Services.AddSingleton<BrewMachineV1>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMemoryCache();

// Typed HTTP client with standard resilience: retry, circuit breaker, and timeout.
builder.Services.AddHttpClient<IWeatherService, WeatherService>()
                .AddStandardResilienceHandler();

builder.Services.AddTransient<IBrewerService, BrewerService>();

// RFC 7807 Problem Details — used by UseExceptionHandler to produce consistent error bodies.
builder.Services.AddProblemDetails();

// Fixed-window rate limiter: 10 requests per minute per IP address.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("brew-fixed-window", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler — must be first so it wraps the entire pipeline.
// Produces RFC 7807 Problem Details bodies on unhandled exceptions.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the generated Program class to the test project for WebApplicationFactory<Program>.
public partial class Program { }
