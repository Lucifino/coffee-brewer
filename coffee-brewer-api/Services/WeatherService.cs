using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

//Enable only for extra credit use case
//public interface IWeatherService
//{
//    Task<double> GetManilaTemperatureAsync();
//}

///// Fetches the current temperature for Manila from the OpenWeatherMap Current Weather API,
///// caching the result for 10 minutes to avoid redundant API calls.
//public class WeatherService : IWeatherService
//{
//    private const string CacheKey = "manila-weather-temp";
//    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

//    private readonly HttpClient _httpClient;
//    private readonly IConfiguration _configuration;
//    private readonly IMemoryCache _cache;

//    public WeatherService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
//    {
//        _httpClient = httpClient;
//        _configuration = configuration;
//        _cache = cache;
//    }

//    /// Returns the current temperature in degrees Celsius for Manila.
//    /// Cached for 10 minutes with absolute expiry — matching OWM's update frequency.
//    public async Task<double> GetManilaTemperatureAsync()
//    {
//        if (_cache.TryGetValue(CacheKey, out double cachedTemp))
//            return cachedTemp;

//        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
//        var url = $"https://api.openweathermap.org/data/2.5/weather?q=Manila,PH&appid={apiKey}&units=metric";

//        var response = await _httpClient.GetAsync(url);
//        response.EnsureSuccessStatusCode();

//        var content = await response.Content.ReadAsStringAsync();
//        var weatherData = JsonSerializer.Deserialize<WeatherApiResponse>(content);
//        var temp = weatherData!.Main.Temp;

//        _cache.Set(CacheKey, temp, new MemoryCacheEntryOptions
//        {
//            AbsoluteExpirationRelativeToNow = CacheTtl
//        });

//        return temp;
//    }
//}
