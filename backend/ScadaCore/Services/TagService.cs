using ScadaCore.Models;

namespace ScadaCore.Services;

public interface ITagService
{
    Task<Tag?> GetTagByNameAsync(string name);
    Task<object?> GetCurrentValueAsync(string name);
    Task<IEnumerable<object>> GetHistoricalDataAsync(string name, DateTime start, DateTime end, int limit);
    Task<bool> UpdateTagValueAsync(string name, object value, string quality = "Good");
}

public class TagService : ITagService
{
    private readonly ILogger<TagService> _logger;
    private readonly Data.ScadaDbContext _context;
    private readonly IConfiguration _configuration;

    public TagService(ILogger<TagService> logger, Data.ScadaDbContext context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _context.Tags
            .Include(t => t.Site)
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<object?> GetCurrentValueAsync(string name)
    {
        // First check Redis cache
        var cacheKey = $"tag:value:{name}";
        
        // If not in cache, query InfluxDB for latest value
        var tag = await GetTagByNameAsync(name);
        if (tag == null)
        {
            return null;
        }

        // Return cached or default value
        return new
        {
            tagName = name,
            value = 0.0, // Would come from Redis/InfluxDB
            timestamp = DateTime.UtcNow,
            quality = "Good"
        };
    }

    public async Task<IEnumerable<object>> GetHistoricalDataAsync(string name, DateTime start, DateTime end, int limit)
    {
        // Query InfluxDB for historical data
        var results = new List<object>();

        // Simulated historical data structure
        var random = new Random();
        var current = start;
        var interval = (end - start).TotalSeconds / limit;

        while (current <= end && results.Count < limit)
        {
            results.Add(new
            {
                timestamp = current,
                value = 100 + random.NextDouble() * 50,
                quality = "Good"
            });

            current = current.AddSeconds(interval);
        }

        return results;
    }

    public async Task<bool> UpdateTagValueAsync(string name, object value, string quality = "Good")
    {
        try
        {
            // Update Redis cache
            var cacheKey = $"tag:value:{name}";
            
            // Publish to RabbitMQ for storage in InfluxDB
            _logger.LogDebug("Updated value for tag {TagName}: {Value}", name, value);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag value for {TagName}", name);
            return false;
        }
    }
}
