using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnergyManagement.Data;
using EnergyManagement.Models;

namespace EnergyManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly EnergyDbContext _context;
    private readonly ILogger<EnergyController> _logger;

    public EnergyController(EnergyDbContext context, ILogger<EnergyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time energy consumption
    /// </summary>
    [HttpGet("consumption/realtime")]
    public async Task<ActionResult<List<EnergyConsumption>>> GetRealtimeConsumption(
        [FromQuery] Guid? siteId = null,
        [FromQuery] int limit = 100)
    {
        var query = _context.EnergyConsumptions.AsQueryable();

        if (siteId.HasValue)
            query = query.Where(e => e.SiteId == siteId);

        var data = await query
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();

        return Ok(data);
    }

    /// <summary>
    /// Get energy consumption by area/site
    /// </summary>
    [HttpGet("consumption/by-area")]
    public async Task<ActionResult<List<EnergyByArea>>> GetConsumptionByArea(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        var data = await _context.EnergyConsumptions
            .Where(e => e.Timestamp >= start && e.Timestamp <= end && e.SiteId != null)
            .GroupBy(e => e.SiteId)
            .Select(g => new EnergyByArea
            {
                AreaName = g.Key.ToString()!,
                TotalEnergyKwh = g.Sum(e => e.EnergyKwh ?? 0),
                TotalCost = g.Sum(e => e.EnergyCost ?? 0),
                CarbonKgCo2 = g.Sum(e => e.CarbonKgCo2 ?? 0),
                MeasurementCount = g.Count()
            })
            .ToListAsync();

        return Ok(data);
    }

    /// <summary>
    /// Get carbon footprint for date range
    /// </summary>
    [HttpGet("carbon-footprint")]
    public async Task<ActionResult<CarbonFootprint>> GetCarbonFootprint(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var totalData = await _context.EnergyConsumptions
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .GroupBy(e => 1)
            .Select(g => new
            {
                TotalCarbon = g.Sum(e => e.CarbonKgCo2 ?? 0),
                TotalEnergy = g.Sum(e => e.EnergyKwh ?? 0)
            })
            .FirstOrDefaultAsync();

        var dailyData = await _context.EnergyConsumptions
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => new DailyCarbonData
            {
                Date = g.Key,
                CarbonKgCo2 = g.Sum(e => e.CarbonKgCo2 ?? 0),
                EnergyKwh = g.Sum(e => e.EnergyKwh ?? 0)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var result = new CarbonFootprint
        {
            StartDate = start,
            EndDate = end,
            TotalCarbonKgCo2 = totalData?.TotalCarbon ?? 0,
            TotalEnergyKwh = totalData?.TotalEnergy ?? 0,
            CarbonIntensity = totalData?.TotalEnergy > 0 
                ? totalData.TotalCarbon / totalData.TotalEnergy 
                : 0,
            DailyData = dailyData
        };

        return Ok(result);
    }

    /// <summary>
    /// Get load profile (peak demand analysis)
    /// </summary>
    [HttpGet("load-profile")]
    public async Task<ActionResult<List<LoadProfile>>> GetLoadProfile(
        [FromQuery] Guid? siteId = null,
        [FromQuery] DateTime? date = null)
    {
        var profileDate = date ?? DateTime.UtcNow.Date;

        var query = _context.LoadProfiles
            .Where(lp => lp.ProfileDate == profileDate);

        if (siteId.HasValue)
            query = query.Where(lp => lp.SiteId == siteId);

        var data = await query
            .OrderBy(lp => lp.HourOfDay)
            .ToListAsync();

        return Ok(data);
    }

    /// <summary>
    /// Get energy targets
    /// </summary>
    [HttpGet("targets")]
    public async Task<ActionResult<List<EnergyTarget>>> GetTargets(
        [FromQuery] Guid? siteId = null,
        [FromQuery] bool? activeOnly = true)
    {
        var query = _context.EnergyTargets.AsQueryable();

        if (siteId.HasValue)
            query = query.Where(t => t.SiteId == siteId);

        if (activeOnly == true)
            query = query.Where(t => t.IsActive);

        var data = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(data);
    }

    /// <summary>
    /// Create new energy target
    /// </summary>
    [HttpPost("targets")]
    public async Task<ActionResult<EnergyTarget>> CreateTarget([FromBody] EnergyTarget target)
    {
        target.Id = Guid.NewGuid();
        target.CreatedAt = DateTime.UtcNow;

        _context.EnergyTargets.Add(target);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created energy target: {TargetName}", target.Name);
        return CreatedAtAction(nameof(GetTargets), new { }, target);
    }

    /// <summary>
    /// Record energy consumption (typically called by data acquisition service)
    /// </summary>
    [HttpPost("consumption")]
    public async Task<ActionResult<EnergyConsumption>> RecordConsumption([FromBody] EnergyConsumption consumption)
    {
        consumption.Id = Guid.NewGuid();
        consumption.CreatedAt = DateTime.UtcNow;

        // Calculate carbon footprint if not provided (using default emission factor)
        if (consumption.CarbonKgCo2 == null && consumption.EnergyKwh.HasValue)
        {
            // Default: 0.5 kg CO2 per kWh (adjust based on your region's grid)
            const decimal emissionFactor = 0.5m;
            consumption.CarbonKgCo2 = consumption.EnergyKwh * emissionFactor;
        }

        _context.EnergyConsumptions.Add(consumption);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRealtimeConsumption), new { }, consumption);
    }
}
