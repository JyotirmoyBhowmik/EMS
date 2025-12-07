using ScadaCore.Data;
using Microsoft.EntityFrameworkCore;

namespace ScadaCore.Services;

/// <summary>
/// Background service that syncs tag metadata to cache on startup
/// and periodically refreshes the cache to ensure consistency
/// </summary>
public class TagSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TagSyncService> _logger;
    private const int SYNC_INTERVAL_MINUTES = 15;

    public TagSyncService(
        IServiceProvider serviceProvider,
        ILogger<TagSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tag Sync Service starting...");

        // Initial sync on startup
        await SyncTagsToCache();

        // Periodic sync
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(SYNC_INTERVAL_MINUTES));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await SyncTagsToCache();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Tag Sync Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Tag Sync background service");
            }
        }
    }

    private async Task SyncTagsToCache()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ScadaDbContext>();
        var cacheService = scope.ServiceProvider.GetRequiredService<TagCacheService>();

        try
        {
            _logger.LogInformation("Starting tag cache synchronization...");

            var tags = await dbContext.Tags
                .Where(t => t.IsEnabled)
                .ToListAsync();

            foreach (var tag in tags)
            {
                await cacheService.CacheTagAsync(tag);
            }

            _logger.LogInformation("Synchronized {Count} tags to cache", tags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing tags to cache");
        }
    }
}
