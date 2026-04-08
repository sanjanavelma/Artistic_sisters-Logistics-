using StackExchange.Redis;
using System.Text.Json;

namespace OrderService.Infrastructure.Cache;

public class OrderCache
{
    private readonly IConnectionMultiplexer _redis;
    public OrderCache(IConnectionMultiplexer redis) => _redis = redis;
    private IDatabase Db() => _redis.GetDatabase();
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await Db().StringSetAsync(key, JsonSerializer.Serialize(value), expiry ?? TimeSpan.FromMinutes(30));
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await Db().StringGetAsync(key);
        if (!val.HasValue) return default;
        return JsonSerializer.Deserialize<T>(val.ToString());
    }
}
