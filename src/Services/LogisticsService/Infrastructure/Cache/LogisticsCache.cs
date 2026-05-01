// Redis cache for logistics data
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
}
