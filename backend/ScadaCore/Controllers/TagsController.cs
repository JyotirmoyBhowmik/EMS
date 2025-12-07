using Microsoft.AspNetCore.Mvc;
using ScadaCore.Models;
using ScadaCore.Services;

namespace ScadaCore.Controllers;

/// <summary>
/// API controller for tag management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly TagManagementService _tagService;
    private readonly InfluxDBService _influxService;
    private readonly TagCacheService _cacheService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        TagManagementService tagService,
        InfluxDBService influxService,
        TagCacheService cacheService,
        ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _influxService = influxService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags with optional filtering
    /// </summary>
    /// <param name="site">Filter by site</param>
    /// <param name="device">Filter by device</param>
    /// <param name="isEnabled">Filter by enabled status</param>
    /// <param name="skip">Number of records to skip (pagination)</param>
    /// <param name="take">Number of records to take (pagination)</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags(
        [FromQuery] string? site = null,
        [FromQuery] string? device = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var tags = await _tagService.GetTagsAsync(site, device, isEnabled, skip, take);
        return Ok(tags);
    }

    /// <summary>
    /// Get a specific tag by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    /// <summary>
    /// Get a tag by name
    /// </summary>
    [HttpGet("by-name/{tagName}")]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTagByName(string tagName)
    {
        var tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag([FromBody] Tag tag)
    {
        try
        {
            var createdTag = await _tagService.CreateTagAsync(tag);
            return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag tag)
    {
        try
        {
            var updatedTag = await _tagService.UpdateTagAsync(id, tag);
            return Ok(updatedTag);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Search tags by name pattern
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTags([FromQuery] string query)
    {
        var tags = await _tagService.SearchTagsAsync(query);
        return Ok(tags);
    }

    /// <summary>
    /// Get tag count by site
    /// </summary>
    [HttpGet("stats/count-by-site")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTagCountBySite()
    {
        var stats = await _tagService.GetTagCountBySiteAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get latest tag value
    /// </summary>
    [HttpGet("{tagName}/value/latest")]
    [ProducesResponseType(typeof(TagValue), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestValue(string tagName)
    {
        // Check cache first
        var cachedValue = await _cacheService.GetTagValueAsync(tagName);
        if (cachedValue != null)
            return Ok(cachedValue);

        // Fall back to InfluxDB
        var value = await _influxService.QueryLatestValueAsync(tagName);
        if (value == null)
            return NotFound();

        return Ok(value);
    }

    /// <summary>
    /// Get historical tag values
    /// </summary>
    [HttpGet("{tagName}/history")]
    [ProducesResponseType(typeof(List<TagValue>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        string tagName,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var history = await _influxService.QueryTagHistoryAsync(tagName, start, end);
        return Ok(history);
    }

    /// <summary>
    /// Get aggregated statistics for a tag
    /// </summary>
    [HttpGet("{tagName}/aggregates")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAggregates(
        string tagName,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] string window = "1m")
    {
        var aggregates = await _influxService.QueryAggregatesAsync(tagName, start, end, window);
        return Ok(aggregates);
    }

    /// <summary>
    /// Batch create tags
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTagsBatch([FromBody] List<Tag> tags)
    {
        var count = await _tagService.CreateTagsBatchAsync(tags);
        return CreatedAtAction(nameof(GetTags), new { count });
    }
}
