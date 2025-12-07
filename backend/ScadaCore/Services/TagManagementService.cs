using Microsoft.EntityFrameworkCore;
using ScadaCore.Data;
using ScadaCore.Models;

namespace ScadaCore.Services;

/// <summary>
/// Service for managing SCADA tags (CRUD operations)
/// Handles tag creation, updates, deletion, and querying
/// </summary>
public class TagManagementService
{
    private readonly ScadaDbContext _dbContext;
    private readonly TagCacheService _cacheService;
    private readonly ILogger<TagManagementService> _logger;

    public TagManagementService(
        ScadaDbContext dbContext,
        TagCacheService cacheService,
        ILogger<TagManagementService> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        // Check for duplicate name
        if (await _dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists");
        }

        tag.CreatedAt = DateTime.UtcNow;
        tag.UpdatedAt = DateTime.UtcNow;

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        // Cache the new tag
        await _cacheService.CacheTagAsync(tag);

        _logger.LogInformation("Created tag: {TagName}", tag.Name);
        return tag;
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    public async Task<Tag?> GetTagByIdAsync(int tagId)
    {
        return await _dbContext.Tags
            .Include(t => t.AlarmRules)
            .FirstOrDefaultAsync(t => t.Id == tagId);
    }

    /// <summary>
    /// Get tag by name (checks cache first, then database)
    /// </summary>
    public async Task<Tag?> GetTagByNameAsync(string tagName)
    {
        // Try cache first
        var cachedTag = await _cacheService.GetTagAsync(tagName);
        if (cachedTag != null)
            return cachedTag;

        // Fall back to database
        var tag = await _dbContext.Tags
            .Include(t => t.AlarmRules)
            .FirstOrDefaultAsync(t => t.Name == tagName);

        if (tag != null)
        {
            // Cache for future requests
            await _cacheService.CacheTagAsync(tag);
        }

        return tag;
    }

    /// <summary>
    /// Get all tags with optional filtering
    /// </summary>
    public async Task<List<Tag>> GetTagsAsync(
        string? site = null,
        string? device = null,
        bool? isEnabled = null,
        int skip = 0,
        int take = 100)
    {
        var query = _dbContext.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(site))
            query = query.Where(t => t.Site == site);

        if (!string.IsNullOrEmpty(device))
            query = query.Where(t => t.Device == device);

        if (isEnabled.HasValue)
            query = query.Where(t => t.IsEnabled == isEnabled.Value);

        return await query
            .OrderBy(t => t.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    public async Task<Tag> UpdateTagAsync(int tagId, Tag updatedTag)
    {
        var existingTag = await _dbContext.Tags.FindAsync(tagId);
        if (existingTag == null)
        {
            throw new KeyNotFoundException($"Tag with ID {tagId} not found");
        }

        // Update properties
        existingTag.Description = updatedTag.Description;
        existingTag.DataType = updatedTag.DataType;
        existingTag.Units = updatedTag.Units;
        existingTag.MinValue = updatedTag.MinValue;
        existingTag.MaxValue = updatedTag.MaxValue;
        existingTag.ScaleFactor = updatedTag.ScaleFactor;
        existingTag.Offset = updatedTag.Offset;
        existingTag.ScanRate = updatedTag.ScanRate;
        existingTag.IsEnabled = updatedTag.IsEnabled;
        existingTag.LogHistory = updatedTag.LogHistory;
        existingTag.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.InvalidateTagAsync(existingTag.Name);

        _logger.LogInformation("Updated tag: {TagName}", existingTag.Name);
        return existingTag;
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    public async Task DeleteTagAsync(int tagId)
    {
        var tag = await _dbContext.Tags.FindAsync(tagId);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID {tagId} not found");
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.InvalidateTagAsync(tag.Name);

        _logger.LogInformation("Deleted tag: {TagName}", tag.Name);
    }

    /// <summary>
    /// Batch create tags (optimized for bulk import)
    /// </summary>
    public async Task<int> CreateTagsBatchAsync(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
        {
            tag.CreatedAt = DateTime.UtcNow;
            tag.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.Tags.AddRangeAsync(tags);
        var count = await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created {Count} tags in batch", count);
        return count;
    }

    /// <summary>
    /// Get tag count by site
    /// </summary>
    public async Task<Dictionary<string, int>> GetTagCountBySiteAsync()
    {
        return await _dbContext.Tags
            .GroupBy(t => t.Site ?? "Unknown")
            .Select(g => new { Site = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Site, x => x.Count);
    }

    /// <summary>
    /// Search tags by name pattern
    /// </summary>
    public async Task<List<Tag>> SearchTagsAsync(string searchPattern)
    {
        return await _dbContext.Tags
            .Where(t => EF.Functions.Like(t.Name, $"%{searchPattern}%"))
            .OrderBy(t => t.Name)
            .Take(50)
            .ToListAsync();
    }
}
