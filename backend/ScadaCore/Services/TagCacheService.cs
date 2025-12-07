using System.Text.Json;
using StackExchange.Redis;
using ScadaCore.Models;

namespace ScadaCore.Services;

/// <summary>
/// High-performance in-memory cache for tag metadata and latest values
/// Uses Redis for distributed caching across multiple SCADA Core instances
/// </summary>
public class TagCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _cache;
    private const string TAG_PREFIX = "tag:";
    private const string TAG_VALUE_PREFIX = "tagvalue:";
    private const int CACHE_EXPIRY_SECONDS = 3600; // 1 hour

    public TagCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _cache = redis.GetDatabase();
    }

    /// <summary>
    /// Cache tag metadata
    /// </summary>
    public async Task CacheTagAsync(Tag tag)
    {
        var key = $"{TAG_PREFIX}{tag.Name}";
        var json = JsonSerializer.Serialize(tag);
        await _cache.StringSetAsync(key, json, TimeSpan.FromSeconds(CACHE_EXPIRY_SECONDS));
    }

    /// <summary>
    /// Get tag from cache
    /// </summary>
    public async Task<Tag?> GetTagAsync(string tagName)
    {
        var key = $"{TAG_PREFIX}{tagName}";
        var json = await _cache.StringGetAsync(key);
        
        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Tag>(json!);
    }

    /// <summary>
    /// Cache latest tag value for fast retrieval
    /// </summary>
    public async Task CacheTagValueAsync(TagValue tagValue)
    {
        var key = $"{TAG_VALUE_PREFIX}{tagValue.TagName}";
        var json = JsonSerializer.Serialize(tagValue);
        await _cache.StringSetAsync(key, json, TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Get latest cached value for a tag
    /// </summary>
    public async Task<TagValue?> GetTagValueAsync(string tagName)
    {
        var key = $"{TAG_VALUE_PREFIX}{tagName}";
        var json = await _cache.StringGetAsync(key);
        
        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<TagValue>(json!);
    }

    /// <summary>
    /// Batch cache multiple tag values (for high-throughput scenarios)
    /// </summary>
    public async Task CacheBatchAsync(IEnumerable<TagValue> tagValues)
    {
        var batch = _cache.CreateBatch();
        var tasks = new List<Task>();

        foreach (var tagValue in tagValues)
        {
            var key = $"{TAG_VALUE_PREFIX}{tagValue.TagName}";
            var json = JsonSerializer.Serialize(tagValue);
            tasks.Add(batch.StringSetAsync(key, json, TimeSpan.FromMinutes(5)));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Invalidate tag cache
    /// </summary>
    public async Task InvalidateTagAsync(string tagName)
    {
        await _cache.KeyDeleteAsync($"{TAG_PREFIX}{tagName}");
        await _cache.KeyDeleteAsync($"{TAG_VALUE_PREFIX}{tagName}");
    }

    /// <summary>
    /// Get all cached tag names matching a pattern
    /// </summary>
    public async Task<List<string>> GetTagNamesAsync(string pattern = "*")
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{TAG_PREFIX}{pattern}");
        
        return keys.Select(k => k.ToString().Replace(TAG_PREFIX, "")).ToList();
    }
}
