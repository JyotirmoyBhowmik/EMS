using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyManagement.Models;

/// <summary>
/// Energy consumption record for real-time and historical tracking
/// </summary>
public class EnergyConsumption
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public Guid? SiteId { get; set; }
    public Guid? EquipmentTagId { get; set; }
    
    // Power measurements
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ActivePowerKw { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ReactivePowerKvar { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ApparentPowerKva { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal? PowerFactor { get; set; }
    
    // Energy totals
    [Column(TypeName = "decimal(15,3)")]
    public decimal? EnergyKwh { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? EnergyCost { get; set; }
    
    // Carbon footprint
    [Column(TypeName = "decimal(10,2)")]
    public decimal? CarbonKgCo2 { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Energy targets and baselines for ISO 50001 compliance
/// </summary>
public class EnergyTarget
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid? SiteId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string TargetType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Yearly
    
    [Column(TypeName = "decimal(15,2)")]
    public decimal BaselineKwh { get; set; }
    
    [Column(TypeName = "decimal(15,2)")]
    public decimal TargetKwh { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal? TargetReductionPercent { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Load profile for peak demand tracking
/// </summary>
public class LoadProfile
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid? SiteId { get; set; }
    
    [Required]
    public DateTime ProfileDate { get; set; }
    
    [Range(0, 23)]
    public int HourOfDay { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? PeakDemandKw { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? AverageDemandKw { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal? LoadFactor { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for energy consumption by area
/// </summary>
public class EnergyByArea
{
    public string AreaName { get; set; } = string.Empty;
    public decimal TotalEnergyKwh { get; set; }
    public decimal TotalCost { get; set; }
    public decimal CarbonKgCo2 { get; set; }
    public int MeasurementCount { get; set; }
}

/// <summary>
/// DTO for carbon footprint summary
/// </summary>
public class CarbonFootprint
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCarbonKgCo2 { get; set; }
    public decimal TotalEnergyKwh { get; set; }
    public decimal CarbonIntensity { get; set; } // kg CO2 per kWh
    public List<DailyCarbonData> DailyData { get; set; } = new();
}

public class DailyCarbonData
{
    public DateTime Date { get; set; }
    public decimal CarbonKgCo2 { get; set; }
    public decimal EnergyKwh { get; set; }
}
