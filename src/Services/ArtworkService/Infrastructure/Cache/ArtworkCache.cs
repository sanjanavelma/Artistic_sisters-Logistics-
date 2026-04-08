using StackExchange.Redis;
using System.Text.Json;
namespace ArtworkService.Infrastructure.Cache;
public interface IArtworkCache
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
public class ArtworkCache : IArtworkCache
{
    private readonly IConnectionMultiplexer _redis;
    public ArtworkCache(IConnectionMultiplexer redis) => _redis = redis;
    private IDatabase GetDb() => _redis.GetDatabase();
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await GetDb().StringSetAsync(key, json,
            expiry ?? TimeSpan.FromMinutes(30));
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await GetDb().StringGetAsync(key);
        if (!json.HasValue) return default;
        return JsonSerializer.Deserialize<T>(json.ToString());
    }
    public async Task RemoveAsync(string key)
        => await GetDb().KeyDeleteAsync(key);
    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0) await GetDb().KeyDeleteAsync(keys);
    }
}
