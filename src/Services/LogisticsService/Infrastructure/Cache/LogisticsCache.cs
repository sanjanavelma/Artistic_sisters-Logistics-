// Redis cache for GPS coordinates
// Every time agent updates location, it goes to Redis
// Dealer's tracking page reads from Redis — instant response
using StackExchange.Redis;
using System.Text.Json;

namespace LogisticsService.Infrastructure.Cache;

public interface ILogisticsCache
{
    // Save any value to Redis
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    // Get any value from Redis
    Task<T?> GetAsync<T>(string key);

    // Delete a key
    Task RemoveAsync(string key);

    // Save GPS coordinates for an order
    Task SetGPSAsync(Guid orderId, double lat, double lng);

    // Get latest GPS coordinates for an order
    Task<(double Lat, double Lng)?> GetGPSAsync(Guid orderId);
}

public class LogisticsCache : ILogisticsCache
{
    private readonly IConnectionMultiplexer _redis;

    public LogisticsCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private IDatabase GetDb() => _redis.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await GetDb().StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(30));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await GetDb().StringGetAsync(key);
        if (!json.HasValue) return default;
        return JsonSerializer.Deserialize<T>(json.ToString());
    }

    public async Task RemoveAsync(string key)
    {
        await GetDb().KeyDeleteAsync(key);
    }

    // GPS stored as "lat,lng" string in Redis
    // Key format: "gps:order:{orderId}"
    // TTL = 2 hours — GPS data becomes stale after that
    public async Task SetGPSAsync(Guid orderId, double lat, double lng)
    {
        var key = $"gps:order:{orderId}";
        var value = $"{lat},{lng}";
        // 2 hour TTL — auto expires
        await GetDb().StringSetAsync(key, value, TimeSpan.FromHours(2));
    }

    public async Task<(double Lat, double Lng)?> GetGPSAsync(Guid orderId)
    {
        var key = $"gps:order:{orderId}";
        var value = await GetDb().StringGetAsync(key);

        // If no GPS data found return null
        if (!value.HasValue) return null;

        // Parse "lat,lng" string back to doubles
        var parts = value.ToString().Split(',');
        return (double.Parse(parts[0]), double.Parse(parts[1]));
    }
}
