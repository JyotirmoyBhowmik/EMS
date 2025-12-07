using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnergyManagement.Data;
using EnergyManagement.Models;

namespace EnergyManagement.Controllers;

/// <summary>
/// Meter CRUD operations for hierarchical energy meter management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetersController : ControllerBase
{
    private readonly EnergyDbContext _context;
    private readonly ILogger<MetersController> _logger;

    public MetersController(EnergyDbContext context, ILogger<MetersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all meters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EnergyMeter>>> GetAllMeters(
        [FromQuery] string? meterType = null,
        [FromQuery] string? status = null)
    {
        var query = _context.Set<EnergyMeter>().AsQueryable();

        if (!string.IsNullOrEmpty(meterType))
            query = query.Where(m => m.MeterType == meterType);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(m => m.Status == status);

        var meters = await query
            .OrderBy(m => m.Level)
            .ThenBy(m => m.DisplayOrder)
            .ToListAsync();

        return Ok(meters);
    }

    /// <summary>
    /// Get meter by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EnergyMeter>> GetMeter(Guid id)
    {
        var meter = await _context.Set<EnergyMeter>().FindAsync(id);

        if (meter == null)
            return NotFound(new { message = $"Meter with ID {id} not found" });

        return Ok(meter);
    }

    /// <summary>
    /// Get meter hierarchy (tree structure)
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult> GetMeterHierarchy()
    {
        var meters = await _context.Set<EnergyMeter>()
            .OrderBy(m => m.Level)
            .ThenBy(m => m.DisplayOrder)
            .ToListAsync();

        // Build hierarchical structure
        var tree = BuildHierarchy(meters, null);
        return Ok(tree);
    }

    /// <summary>
    /// Get child meters of a parent
    /// </summary>
    [HttpGet("{id}/children")]
    public async Task<ActionResult<List<EnergyMeter>>> GetChildMeters(Guid id)
    {
        var children = await _context.Set<EnergyMeter>()
            .Where(m => m.ParentMeterId == id)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        return Ok(children);
    }

    /// <summary>
    /// Get meter status summary
    /// </summary>
    [HttpGet("status-summary")]
    public async Task<ActionResult> GetStatusSummary()
    {
        var meters = await _context.Set<EnergyMeter>().ToListAsync();

        var summary = new
        {
            total = meters.Count,
            active = meters.Count(m => m.Status == "active"),
            inactive = meters.Count(m => m.Status == "inactive"),
            maintenance = meters.Count(m => m.Status == "maintenance"),
            faulty = meters.Count(m => m.Status == "faulty"),
            online = meters.Count(m => m.CommunicationStatus == "online"),
            offline = meters.Count(m => m.CommunicationStatus == "offline"),
            healthy = meters.Count(m => m.HealthStatus == "good"),
            warning = meters.Count(m => m.HealthStatus == "warning"),
            critical = meters.Count(m => m.HealthStatus == "critical")
        };

        return Ok(summary);
    }

    /// <summary>
    /// Create new meter
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EnergyMeter>> CreateMeter([FromBody] EnergyMeter meter)
    {
        meter.Id = Guid.NewGuid();
        meter.CreatedAt = DateTime.UtcNow;
        meter.UpdatedAt = DateTime.UtcNow;

        // Calculate CT/PT ratios
        if (meter.CtPrimaryAmps.HasValue && meter.CtSecondaryAmps.HasValue)
            meter.CtRatio = $"{meter.CtPrimaryAmps}/{meter.CtSecondaryAmps}";

        if (meter.PtPrimaryVolts.HasValue && meter.PtSecondaryVolts.HasValue)
            meter.PtRatio = $"{meter.PtPrimaryVolts}/{meter.PtSecondaryVolts}";

        _context.Set<EnergyMeter>().Add(meter);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created meter: {MeterName} ({MeterNumber})", meter.MeterName, meter.MeterNumber);

        return CreatedAtAction(nameof(GetMeter), new { id = meter.Id }, meter);
    }

    /// <summary>
    /// Update existing meter
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<EnergyMeter>> UpdateMeter(Guid id, [FromBody] EnergyMeter meter)
    {
        var existing = await _context.Set<EnergyMeter>().FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Meter with ID {id} not found" });

        // Update properties
        existing.MeterNumber = meter.MeterNumber;
        existing.MeterName = meter.MeterName;
        existing.MeterType = meter.MeterType;
        existing.Manufacturer = meter.Manufacturer;
        existing.Model = meter.Model;
        existing.ParentMeterId = meter.ParentMeterId;
        existing.Level = meter.Level;
        existing.Location = meter.Location;
        existing.Building = meter.Building;
        existing.Floor = meter.Floor;
        existing.RatedCapacityKw = meter.RatedCapacityKw;
        existing.RatedVoltage = meter.RatedVoltage;
        existing.RatedCurrent = meter.RatedCurrent;
        existing.CtPrimaryAmps = meter.CtPrimaryAmps;
        existing.CtSecondaryAmps = meter.CtSecondaryAmps;
        existing.PtPrimaryVolts = meter.PtPrimaryVolts;
        existing.PtSecondaryVolts = meter.PtSecondaryVolts;
        existing.Status = meter.Status;
        existing.HealthStatus = meter.HealthStatus;
        existing.CommunicationStatus = meter.CommunicationStatus;
        existing.DisplayOrder = meter.DisplayOrder;
        existing.Icon = meter.Icon;
        existing.Color = meter.Color;
        existing.ModbusAddress = meter.ModbusAddress;
        existing.IpAddress = meter.IpAddress;
        existing.CommunicationProtocol = meter.CommunicationProtocol;
        existing.UpdatedAt = DateTime.UtcNow;

        // Recalculate ratios
        if (existing.CtPrimaryAmps.HasValue && existing.CtSecondaryAmps.HasValue)
            existing.CtRatio = $"{existing.CtPrimaryAmps}/{existing.CtSecondaryAmps}";

        if (existing.PtPrimaryVolts.HasValue && existing.PtSecondaryVolts.HasValue)
            existing.PtRatio = $"{existing.PtPrimaryVolts}/{existing.PtSecondaryVolts}";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated meter: {MeterName}", existing.MeterName);

        return Ok(existing);
    }

    /// <summary>
    /// Delete meter
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMeter(Guid id)
    {
        var meter = await _context.Set<EnergyMeter>().FindAsync(id);
        if (meter == null)
            return NotFound(new { message = $"Meter with ID {id} not found" });

        // Check if meter has children
        var hasChildren = await _context.Set<EnergyMeter>().AnyAsync(m => m.ParentMeterId == id);
        if (hasChildren)
            return BadRequest(new { message = "Cannot delete meter with child meters. Delete children first." });

        _context.Set<EnergyMeter>().Remove(meter);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted meter: {MeterName}", meter.MeterName);

        return NoContent();
    }

    /// <summary>
    /// Get meter readings
    /// </summary>
    [HttpGet("{id}/readings")]
    public async Task<ActionResult> GetMeterReadings(
        Guid id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 1000)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        var readings = await _context.Set<MeterReading>()
            .Where(r => r.MeterId == id && r.Timestamp >= start && r.Timestamp <= end)
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync();

        return Ok(readings);
    }

    /// <summary>
    /// Record meter reading
    /// </summary>
    [HttpPost("{id}/readings")]
    public async Task<ActionResult> RecordReading(Guid id, [FromBody] MeterReading reading)
    {
        var meter = await _context.Set<EnergyMeter>().FindAsync(id);
        if (meter == null)
            return NotFound(new { message = $"Meter with ID {id} not found" });

        reading.Id = Guid.NewGuid();
        reading.MeterId = id;
        reading.CreatedAt = DateTime.UtcNow;

        // Calculate carbon if diesel consumption provided
        if (reading.DieselConsumptionLiters.HasValue && reading.DieselConsumptionLiters > 0)
        {
            const decimal dieselCarbonFactor = 2.68m; // kg CO2 per liter
            reading.CarbonKgCo2 = reading.DieselConsumptionLiters * dieselCarbonFactor;
        }
        // Calculate carbon from grid energy
        else if (reading.EnergyImportKwh.HasValue)
        {
            const decimal gridCarbonFactor = 0.5m; // kg CO2 per kWh
            reading.CarbonKgCo2 = reading.EnergyImportKwh * gridCarbonFactor;
        }

        _context.Set<MeterReading>().Add(reading);
        await _context.SaveChangesAsync();

        // Update meter's last communication
        meter.LastCommunication = DateTime.UtcNow;
        meter.CommunicationStatus = "online";
        await _context.SaveChangesAsync();

        return Ok(reading);
    }

    // Helper method to build hierarchy
    private List<object> BuildHierarchy(List<EnergyMeter> allMeters, Guid? parentId)
    {
        return allMeters
            .Where(m => m.ParentMeterId == parentId)
            .Select(m => new
            {
                m.Id,
                m.MeterNumber,
                m.MeterName,
                m.MeterType,
                m.Level,
                m.Status,
                m.HealthStatus,
                m.CommunicationStatus,
                m.CtRatio,
                m.PtRatio,
                Children = BuildHierarchy(allMeters, m.Id)
            })
            .ToList<object>();
    }
}

// Add missing model classes
public class EnergyMeter
{
    public Guid Id { get; set; }
    public string MeterNumber { get; set; } = string.Empty;
    public string MeterName { get; set; } = string.Empty;
    public string? MeterType { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public Guid? ParentMeterId { get; set; }
    public int Level { get; set; }
    public Guid? SiteId { get; set; }
    public string? Location { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public decimal? RatedCapacityKw { get; set; }
    public decimal? RatedVoltage { get; set; }
    public decimal? RatedCurrent { get; set; }
    public decimal? CtPrimaryAmps { get; set; }
    public decimal? CtSecondaryAmps { get; set; }
    public string? CtRatio { get; set; }
    public decimal? PtPrimaryVolts { get; set; }
    public decimal? PtSecondaryVolts { get; set; }
    public string? PtRatio { get; set; }
    public string Status { get; set; } = "active";
    public string HealthStatus { get; set; } = "good";
    public string CommunicationStatus { get; set; } = "online";
    public DateTime? LastCommunication { get; set; }
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int? ModbusAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? CommunicationProtocol { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? InstallationDate { get; set; }
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MeterReading
{
    public Guid Id { get; set; }
    public Guid MeterId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? VoltageL1 { get; set; }
    public decimal? VoltageL2 { get; set; }
    public decimal? VoltageL3 { get; set; }
    public decimal? CurrentL1 { get; set; }
    public decimal? CurrentL2 { get; set; }
    public decimal? CurrentL3 { get; set; }
    public decimal? ActivePowerKw { get; set; }
    public decimal? ReactivePowerKvar { get; set; }
    public decimal? ApparentPowerKva { get; set; }
    public decimal? PowerFactor { get; set; }
    public decimal? FrequencyHz { get; set; }
    public decimal? TotalEnergyKwh { get; set; }
    public decimal? EnergyImportKwh { get; set; }
    public decimal? EnergyExportKwh { get; set; }
    public decimal? DieselConsumptionLiters { get; set; }
    public decimal? RuntimeHours { get; set; }
    public decimal? EnergyCost { get; set; }
    public decimal? CarbonKgCo2 { get; set; }
    public DateTime CreatedAt { get; set; }
}
