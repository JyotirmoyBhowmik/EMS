using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScadaCore.Data;
using ScadaCore.Models;
using ScadaCore.Services;

namespace ScadaCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ILogger<TagsController> _logger;
    private readonly ScadaDbContext _context;
    private readonly ITagService _tagService;

    public TagsController(ILogger<TagsController> logger, ScadaDbContext context, ITagService tagService)
    {
        _logger = logger;
        _context = context;
        _tagService = tagService;
    }

    /// <summary>
    /// Get all tags with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags(
        [FromQuery] string? search = null,
        [FromQuery] string? site = null,
        [FromQuery] string? deviceType = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Tags.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(site))
            {
                query = query.Where(t => t.Site != null && t.Site.Name == site);
            }

            if (!string.IsNullOrEmpty(deviceType))
            {
                query = query.Where(t => t.DeviceType == deviceType);
            }

            if (isEnabled.HasValue)
            {
                query = query.Where(t => t.IsEnabled == isEnabled.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var tags = await query
                .Include(t => t.Site)
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get tag by name
    /// </summary>
    [HttpGet("{name}")]
    public async Task<ActionResult<Tag>> GetTag(string name)
    {
        try
        {
            var tag = await _tagService.GetTagByNameAsync(name);

            if (tag == null)
            {
                return NotFound($"Tag '{name}' not found");
            }

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag {TagName}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get current value of a tag
    /// </summary>
    [HttpGet("{name}/value")]
    public async Task<ActionResult<object>> GetTagValue(string name)
    {
        try
        {
            var value = await _tagService.GetCurrentValueAsync(name);

            if (value == null)
            {
                return NotFound($"No value found for tag '{name}'");
            }

            return Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value for tag {TagName}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get historical data for a tag
    /// </summary>
    [HttpGet("{name}/history")]
    public async Task<ActionResult<IEnumerable<object>>> GetTagHistory(
        string name,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null,
        [FromQuery] int limit = 1000)
    {
        try
        {
            var startTime = start ?? DateTime.UtcNow.AddHours(-24);
            var endTime = end ?? DateTime.UtcNow;

            var history = await _tagService.GetHistoricalDataAsync(name, startTime, endTime, limit);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for tag {TagName}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag([FromBody] Tag tag)
    {
        try
        {
            // Validate tag
            if (string.IsNullOrEmpty(tag.Name))
            {
                return BadRequest("Tag name is required");
            }

            // Check if tag already exists
            var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);
            if (existing != null)
            {
                return Conflict($"Tag '{tag.Name}' already exists");
            }

            tag.Id = Guid.NewGuid();
            tag.CreatedAt = DateTime.UtcNow;
            tag.UpdatedAt = DateTime.UtcNow;

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created tag {TagName}", tag.Name);

            return CreatedAtAction(nameof(GetTag), new { name = tag.Name }, tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{name}")]
    public async Task<ActionResult<Tag>> UpdateTag(string name, [FromBody] Tag updatedTag)
    {
        try
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);

            if (tag == null)
            {
                return NotFound($"Tag '{name}' not found");
            }

            // Update properties
            tag.Description = updatedTag.Description ?? tag.Description;
            tag.Unit = updatedTag.Unit ?? tag.Unit;
            tag.MinValue = updatedTag.MinValue;
            tag.MaxValue = updatedTag.MaxValue;
            tag.IsEnabled = updatedTag.IsEnabled;
            tag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated tag {TagName}", name);

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagName}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{name}")]
    public async Task<ActionResult> DeleteTag(string name)
    {
        try
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);

            if (tag == null)
            {
                return NotFound($"Tag '{name}' not found");
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted tag {TagName}", name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagName}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Bulk import tags from CSV
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<object>> ImportTags([FromBody] List<Tag> tags)
    {
        try
        {
            int created = 0;
            int skipped = 0;
            var errors = new List<string>();

            foreach (var tag in tags)
            {
                try
                {
                    var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);
                    if (existing != null)
                    {
                        skipped++;
                        continue;
                    }

                    tag.Id = Guid.NewGuid();
                    tag.CreatedAt = DateTime.UtcNow;
                    tag.UpdatedAt = DateTime.UtcNow;

                    _context.Tags.Add(tag);
                    created++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Error importing tag {tag.Name}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imported {Count} tags, skipped {Skipped}", created, skipped);

            return Ok(new
            {
                created,
                skipped,
                errors = errors.Count > 0 ? errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing tags");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get tag statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var stats = new
            {
                totalTags = await _context.Tags.CountAsync(),
                enabledTags = await _context.Tags.CountAsync(t => t.IsEnabled),
                disabledTags = await _context.Tags.CountAsync(t => !t.IsEnabled),
                tagsByType = await _context.Tags
                    .GroupBy(t => t.DataType)
                    .Select(g => new { type = g.Key, count = g.Count() })
                    .ToListAsync(),
                tagsByDevice = await _context.Tags
                    .GroupBy(t => t.DeviceType)
                    .Select(g => new { device = g.Key, count = g.Count() })
                    .ToListAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}
