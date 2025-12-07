using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarmManagement.Data;
using AlarmManagement.Models;
using AlarmManagement.Services;

namespace AlarmManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly ILogger<AlarmsController> _logger;
    private readonly AlarmDbContext _context;
    private readonly IAlarmService _alarmService;

    public AlarmsController(ILogger<AlarmsController> logger, AlarmDbContext context, IAlarmService alarmService)
    {
        _logger = logger;
        _context = context;
        _alarmService = alarmService;
    }

    /// <summary>
    /// Get all alarm events with filtering
    /// </summary>
    [HttpGet("events")]
    public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetAlarmEvents(
        [FromQuery] string? priority = null,
        [FromQuery] bool? isAcknowledged = null,
        [FromQuery] bool? isCleared = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.AlarmEvents
                .Include(e => e.Rule)
                .ThenInclude(r => r.Tag)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(e => e.Rule.Priority == priority);
            }

            if (isAcknowledged.HasValue)
            {
                query = query.Where(e => e.IsAcknowledged == isAcknowledged.Value);
            }

            if (isCleared.HasValue)
            {
                query = query.Where(e => e.IsCleared == isCleared.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.TriggeredAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.TriggeredAt <= endDate.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var events = await query
                .OrderByDescending(e => e.TriggeredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alarm events");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active alarms (not acknowledged or not cleared)
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetActiveAlarms()
    {
        try
        {
            var activeAlarms = await _context.AlarmEvents
                .Include(e => e.Rule)
                .ThenInclude(r => r.Tag)
                .Where(e => !e.IsAcknowledged || !e.IsCleared)
                .OrderByDescending(e => e.TriggeredAt)
                .ToListAsync();

            return Ok(activeAlarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alarms");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Acknowledge an alarm
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    public async Task<ActionResult> AcknowledgeAlarm(Guid id, [FromBody] AcknowledgeRequest request)
    {
        try
        {
            var alarm = await _context.AlarmEvents.FindAsync(id);

            if (alarm == null)
            {
                return NotFound($"Alarm event {id} not found");
            }

            if (alarm.IsAcknowledged)
            {
                return BadRequest("Alarm already acknowledged");
            }

            alarm.IsAcknowledged = true;
            alarm.AcknowledgedAt = DateTime.UtcNow;
            alarm.AcknowledgedByUserId = request.UserId;
            alarm.AcknowledgmentComment = request.Comment;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Alarm {AlarmId} acknowledged by user {UserId}", id, request.UserId);

            return Ok(alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alarm {AlarmId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get alarm rules
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<IEnumerable<AlarmRule>>> GetAlarmRules([FromQuery] bool? isEnabled = null)
    {
        try
        {
            var query = _context.AlarmRules
                .Include(r => r.Tag)
                .AsQueryable();

            if (isEnabled.HasValue)
            {
                query = query.Where(r => r.IsEnabled == isEnabled.Value);
            }

            var rules = await query.ToListAsync();

            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alarm rules");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create alarm rule
    /// </summary>
    [HttpPost("rules")]
    public async Task<ActionResult<AlarmRule>> CreateAlarmRule([FromBody] AlarmRule rule)
    {
        try
        {
            rule.Id = Guid.NewGuid();
            rule.CreatedAt = DateTime.UtcNow;

            _context.AlarmRules.Add(rule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created alarm rule {RuleName}", rule.Name);

            return CreatedAtAction(nameof(GetAlarmRules), new { id = rule.Id }, rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alarm rule");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update alarm rule
    /// </summary>
    [HttpPut("rules/{id}")]
    public async Task<ActionResult<AlarmRule>> UpdateAlarmRule(Guid id, [FromBody] AlarmRule updatedRule)
    {
        try
        {
            var rule = await _context.AlarmRules.FindAsync(id);

            if (rule == null)
            {
                return NotFound($"Alarm rule {id} not found");
            }

            rule.Name = updatedRule.Name;
            rule.Condition = updatedRule.Condition;
            rule.ThresholdValue = updatedRule.ThresholdValue;
            rule.Priority = updatedRule.Priority;
            rule.Message = updatedRule.Message;
            rule.IsEnabled = updatedRule.IsEnabled;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated alarm rule {RuleId}", id);

            return Ok(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alarm rule {RuleId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete alarm rule
    /// </summary>
    [HttpDelete("rules/{id}")]
    public async Task<ActionResult> DeleteAlarmRule(Guid id)
    {
        try
        {
            var rule = await _context.AlarmRules.FindAsync(id);

            if (rule == null)
            {
                return NotFound($"Alarm rule {id} not found");
            }

            _context.AlarmRules.Remove(rule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted alarm rule {RuleId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alarm rule {RuleId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get alarm statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var stats = new
            {
                totalAlarms = await _context.AlarmEvents
                    .Where(e => e.TriggeredAt >= start && e.TriggeredAt <= end)
                    .CountAsync(),
                activeAlarms = await _context.AlarmEvents
                    .Where(e => !e.IsCleared)
                    .CountAsync(),
                acknowledgedAlarms = await _context.AlarmEvents
                    .Where(e => e.IsAcknowledged && e.TriggeredAt >= start && e.TriggeredAt <= end)
                    .CountAsync(),
                byPriority = await _context.AlarmEvents
                    .Include(e => e.Rule)
                    .Where(e => e.TriggeredAt >= start && e.TriggeredAt <= end)
                    .GroupBy(e => e.Rule.Priority)
                    .Select(g => new { priority = g.Key, count = g.Count() })
                    .ToListAsync(),
                topAlarms = await _context.AlarmEvents
                    .Include(e => e.Rule)
                    .Where(e => e.TriggeredAt >= start && e.TriggeredAt <= end)
                    .GroupBy(e => e.Rule.Name)
                    .Select(g => new { alarmName = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .Take(10)
                    .ToListAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alarm statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record AcknowledgeRequest(Guid UserId, string? Comment);
